using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;
using Snowflake;
using static System.String;

namespace Snowflake
{
    public class SnoWorKer : IdWorker
    {
        private long _workerId;
        private long sequence = 0;
        private int snoposition = 10;
        private readonly string _prefix;
        private readonly int _snoposition;
        private readonly char[] _rule = new char[] {'0','1','2','3','4','5','6','7','8','9',
                              'A','B','C','D','E','F','G','H','J','K',
                              'L','M','N','P','Q','R','S','T','U','V',
                              'W','X','Y','Z',};

        private readonly string _seamsnoposition = string.Empty;//补位
        /// <summary>
        /// 生成单据流水号
        /// </summary>
        /// <param name="prefix">单据前缀</param>
        public SnoWorKer(string prefix)
        {
            if (GetExceptionEnabled())
                _workerId = long.Parse(ConfigurationManager.AppSettings["WorkId"]);
            _prefix = prefix;
            _snoposition = snoposition;
            for (var i = 0; i < snoposition; i++)
            {
                _seamsnoposition = _seamsnoposition + "0";
            }

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

        public virtual string GetNewSno()
        {
            lock (_lock)
            {
                var timestamp = TimeGen();

                if (timestamp < _lastTimestamp)
                {
                    //exceptionCounter.incr(1);
                    //log.Error("clock is moving backwards.  Rejecting requests until %d.", _lastTimestamp);
                    throw new InvalidSystemClock(
                        $"Clock moved backwards.  Refusing to generate id for {_lastTimestamp - timestamp} milliseconds");
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




                return _prefix + (Change(id) + _seamsnoposition).Substring(0, _snoposition); /*id.ToString("x8");*/
            }
        }

        private string Change(long num)
        {
            if (num < 0)
            {
                return Empty;
            }
            //保存除34后的余数
            var list = new List<long>();
            while (num >= 34)
            {
                var a = num % 34;
                num /= 34;
                list.Add(a);
            }
            list.Add(num);
            var sb = new StringBuilder();
            //结果要从后往前排
            for (var i = list.Count - 1; i >= 0; i--)
            {
                sb.Append(_rule[list[i]]);
            }
            return sb.ToString();
        }

    }
}