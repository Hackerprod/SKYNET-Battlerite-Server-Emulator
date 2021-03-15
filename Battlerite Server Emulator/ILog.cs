using System;

namespace SKYNET
{
    public class ILog
    {

        public event EventHandler<MessageLog> OnNewMessage;

        public void Info(object message)
        {
            MessageLog log = new MessageLog()
            {
                Type = MessageType.INFO,
                Message = message?.ToString()
            };
            OnNewMessage?.Invoke(this, log);
        }
        public void Warn(object message)
        {
            MessageLog log = new MessageLog()
            {
                Type = MessageType.WARN,
                Message = message?.ToString()
            };
            OnNewMessage?.Invoke(this, log);
        }
        public void Debug(object message)
        {
            MessageLog log = new MessageLog()
            {
                Type = MessageType.DEBUG,
                Message = message?.ToString()
            };
            OnNewMessage?.Invoke(this, log);
        }
        public void Error(object message)
        {
            MessageLog log = new MessageLog()
            {
                Type = MessageType.ERROR,
                Message = message?.ToString()
            };
            OnNewMessage?.Invoke(this, log);
        }
        public void Error(string message, Exception ex)
        {
            MessageLog log = new MessageLog()
            {
                Type = MessageType.ERROR,
                Message = message?.ToString() + " " + ex.StackTrace
            };
            OnNewMessage?.Invoke(this, log);
        }
        public void Error(Exception ex)
        {
            MessageLog log = new MessageLog()
            {
                Type = MessageType.ERROR,
                Message = ex.Message + " " + ex.StackTrace
            };
            OnNewMessage?.Invoke(this, log);
        }

        internal void ErrorFormat(string v1, string v2)
        {
            MessageLog log = new MessageLog()
            {
                Type = MessageType.ERROR,
                Message = v1?.ToString() + " " + v2?.ToString()
            };
            OnNewMessage?.Invoke(this, log);
        }
        public class MessageLog
        {
            public string Message { get; set; }
            public MessageType Type { get; set; }
        }
        public enum MessageType
        {
            INFO,
            WARN,
            ERROR,
            DEBUG
        }
    }
}