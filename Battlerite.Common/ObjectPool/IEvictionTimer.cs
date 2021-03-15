using System;

namespace CodeProject.ObjectPool
{
    public interface IEvictionTimer : IDisposable
    {
        Guid Schedule(Action action, TimeSpan delay, TimeSpan period);

        void Cancel(Guid actionTicket);
    }
}