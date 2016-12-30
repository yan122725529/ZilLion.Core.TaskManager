/** Copyright 2010-2012 Twitter, Inc.*/

/**
 * An object that generates IDs.
 * This is broken into a separate class in case
 * we ever want to support multiple worker threads
 * per process
 */

using System;
using System.Configuration;

namespace Snowflake
{
	public class IdWorker
	{
		public const long Twepoch = 1069142674L;//2000-01-01 时间戳

		protected const int WorkerIdBits = 7;
		protected const int SequenceBits = 10;
		protected const long MaxWorkerId = -1L ^ (-1L << WorkerIdBits);

		protected const int WorkerIdShift = SequenceBits;
		protected const int TimestampLeftShift = SequenceBits + WorkerIdBits;

		protected const long SequenceMask = -1L ^ (-1L << SequenceBits);

		protected long _sequence = 0L;
		protected long _lastTimestamp = -1L;


		public IdWorker()
		{
			long workerId = 0;

			if (GetExceptionEnabled())
				workerId = long.Parse(ConfigurationManager.AppSettings["WorkId"]);

			WorkerId = workerId;
			long sequence = 0L;
			_sequence = sequence;

			// sanity check for workerId
			if (workerId > MaxWorkerId || workerId < 0)
			{
				throw new ArgumentException(String.Format("worker Id can't be greater than {0} or less than 0", MaxWorkerId));
			}

			//log.info(
			//    String.Format("worker starting. timestamp left shift {0}, datacenter id bits {1}, worker id bits {2}, sequence bits {3}, workerid {4}",
			//                  TimestampLeftShift, DatacenterIdBits, WorkerIdBits, SequenceBits, workerId)
			//    );	
		}

		private static bool GetExceptionEnabled()
		{
			bool result;
			if (!Boolean.TryParse(ConfigurationManager.AppSettings["WorkId"], out result))
			{
				return false;
			}
			return result;
		}

		public long WorkerId { get; protected set; }
		public long DatacenterId { get; protected set; }

		public long Sequence
		{
			get { return _sequence; }
			internal set { _sequence = value; }
		}

		// def get_timestamp() = System.currentTimeMillis

		protected readonly object _lock = new Object();

		public virtual long NextId()
		{
			lock (_lock)
			{
				var timestamp = TimeGen();

				if (timestamp < _lastTimestamp)
				{
					//exceptionCounter.incr(1);
					//log.Error("clock is moving backwards.  Rejecting requests until %d.", _lastTimestamp);
					throw new InvalidSystemClock(String.Format(
						"Clock moved backwards.  Refusing to generate id for {0} milliseconds", _lastTimestamp - timestamp));
				}

				if (_lastTimestamp == timestamp)
				{
					_sequence = (_sequence + 1) & SequenceMask;
					if (_sequence == 0)
					{
						timestamp = TilNextMillis(_lastTimestamp);
					}
				}
				else
				{
					_sequence = 0;
				}

				_lastTimestamp = timestamp;
				var id = ((timestamp - Twepoch) << TimestampLeftShift) |

						 (WorkerId << WorkerIdShift) | _sequence;

				return id;
			}
		}

		protected virtual long TilNextMillis(long lastTimestamp)
		{
			var timestamp = TimeGen();
			while (timestamp <= lastTimestamp)
			{
				timestamp = TimeGen();
			}
			return timestamp;
		}

		protected virtual long TimeGen()
		{
			return System.CurrentTimeMillis();
		}
	}
}