using System;

namespace KinectPlayerTracker.Sockets
{
    /// <summary>
    /// 异常处理器
    /// </summary>
    public static class ExceptionHandler
    {
        private static readonly object handlerLocker = new object();

        /// <summary>
        /// 处理异常
        /// </summary>
        /// <param name="exception">异常</param>
        public static void Handle(Exception exception)
        {
            lock (handlerLocker)
            {
                Console.WriteLine(exception.Message);
            }
        }

        /// <summary>
        /// 处理异常
        /// </summary>
        /// <param name="message">异常信息</param>
        /// <param name="exception">异常</param>
        public static void Handle(string message, Exception exception)
        {
            lock (handlerLocker)
            {
                Console.WriteLine(message);
                Console.WriteLine(exception.Message);
            }
        }
    }
}
