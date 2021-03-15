using System;

namespace SKYNET.Network
{
    public class ExceptionEventArgs : EventArgs
    {
        public Exception Exception
        {
            get;
            set;
        }

        public ExceptionEventArgs(Exception ex)
        {
            Exception = ex;
        }
    }
}
