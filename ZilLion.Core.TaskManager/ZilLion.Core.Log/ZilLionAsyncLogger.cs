using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace ZilLion.Core.Log
{
    public class ZilLionAsyncLogger<TMessage> : IZilLionLog<TMessage>, IDisposable
    {
        private readonly ConcurrentQueue<TMessage> _logQueue = new ConcurrentQueue<TMessage>();

        /// <summary>
        /// 是否正在处理日志
        /// </summary>
        private bool _logging;
        public Action<TMessage> HandleAction { get; set; }

        public void Dispose()
        {
           
        }

        public ZilLionAsyncLogger()
        {
            _logging = false;
            Task.Factory.StartNew(() =>
            {
                
                while (true)
                {
                    Thread.Sleep(5000);
                    if (!_logging)
                    HandleLog();
                }
            });
        }

        public void LogInfo(TMessage message)
        {
            PushLog(message);
        }

        private void PushLog(TMessage message)
        {
            _logQueue.Enqueue(message);
        }

        private void HandleLog()
        {
            _logging = true;
          
            while (true)
            {
                TMessage todolog;
                var isok = _logQueue.TryDequeue(out todolog);
                if (isok)
                {
                    if (HandleAction != null)
                        HandleAction(todolog);
                }
                else
                {
                    _logging = false;
                    return;
                }
            }
         
        }
    }
}