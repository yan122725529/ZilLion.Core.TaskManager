using System;



namespace Caliburn.Micro
{
    /// <summary>
    ///     Used to manage logging.
    /// </summary>
    public static class LogManager
    {
#if DEBUG
        private static readonly ILog NullLogInstance = new NullLog();
#else
         static readonly ILog NullLogInstance = new NullLog();
#endif

        /// <summary>
        ///     Creates an <see cref="ILog" /> for the provided type.
        /// </summary>
        public static Func<Type, ILog> GetLog = type => NullLogInstance;

        private class NullLog : ILog
        {
            public void Info(string format, params object[] args)
            {
            }

            public void Warn(string format, params object[] args)
            {
            }

            public void Error(Exception exception)
            {
            }
        }

       
    }
}