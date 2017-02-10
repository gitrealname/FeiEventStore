namespace PDEventStore.Store.Logger
{
    public class NullLoger : ILogger
    {

        #region IEventStoreLogger Members

        public bool IsDebugEnabled
        {
            get { return true; }
        }

        public bool IsErrorEnabled
        {
            get { return true; }
        }

        public bool IsFatalEnabled
        {
            get { return true; }
        }

        public bool IsInfoEnabled
        {
            get { return true; }
        }

        public bool IsTraceEnabled
        {
            get { return true; }
        }

        public bool IsWarnEnabled
        {
            get { return true; }
        }

        public void Debug ( System.Exception exception )
        {
        }

        public void Debug ( string format, params object [] args )
        {
        }

        public void Debug ( System.Exception exception, string format, params object [] args )
        {
        }

        public void Error ( System.Exception exception )
        {
        }

        public void Error ( string format, params object [] args )
        {
        }

        public void Error ( System.Exception exception, string format, params object [] args )
        {
        }

        public void Fatal ( System.Exception exception )
        {
        }

        public void Fatal ( string format, params object [] args )
        {
        }

        public void Fatal ( System.Exception exception, string format, params object [] args )
        {
        }

        public void Info ( System.Exception exception )
        {
        }

        public void Info ( string format, params object [] args )
        {
        }

        public void Info ( System.Exception exception, string format, params object [] args )
        {
        }

        public void Trace ( System.Exception exception )
        {
        }

        public void Trace ( string format, params object [] args )
        {
        }

        public void Trace ( System.Exception exception, string format, params object [] args )
        {
        }

        public void Warn ( System.Exception exception )
        {
        }

        public void Warn ( string format, params object [] args )
        {
        }

        public void Warn ( System.Exception exception, string format, params object [] args )
        {
        }

        #endregion
    }
}