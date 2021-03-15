using System;

namespace CodeProject.ObjectPool.Core
{
	[Flags]
	public enum PooledObjectState : byte
	{
		Available = 0x0,
		Reserved = 0x1,
		Disposed = 0x2
	}
}
