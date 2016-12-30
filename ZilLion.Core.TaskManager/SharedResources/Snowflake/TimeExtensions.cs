using System;

namespace Snowflake
{
    public static class System
    {
        public static Func<long> currentTimeFunc = InternalCurrentTimeMillis;
 
        public static long CurrentTimeMillis()
        {
            return currentTimeFunc();
        }

        public static IDisposable StubCurrentTime(Func<long> func)
        {
            currentTimeFunc = func;
            return new DisposableAction(() =>
            {
                currentTimeFunc = InternalCurrentTimeMillis;
            });  
        }

        public static IDisposable StubCurrentTime(long millis)
        {
            currentTimeFunc = () => millis;
            return new DisposableAction(() =>
            {
                currentTimeFunc = InternalCurrentTimeMillis;
            });
        }

        private static readonly DateTime Jan1St2000 = new 
           DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        private static long InternalCurrentTimeMillis()
        {

            long time = (long)(DateTime.UtcNow - Jan1St2000).TotalMilliseconds;
            return (long)Math.Ceiling(Convert.ToDecimal(time / 500));


        }        
    }
}
