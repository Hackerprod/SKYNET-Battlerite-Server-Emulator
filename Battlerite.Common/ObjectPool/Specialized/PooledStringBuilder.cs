using CodeProject.ObjectPool.Core;
using CodeProject.ObjectPool.Logging;
using System;
using System.Text;

namespace CodeProject.ObjectPool.Specialized
{
	public class PooledStringBuilder : PooledObject
	{
        public StringBuilder StringBuilder
		{
			get;
		}

		public PooledStringBuilder(int capacity)
		{
			StringBuilder = new StringBuilder(capacity);
			base.OnValidateObject = (Func<PooledObjectValidationContext, bool>)Delegate.Combine(base.OnValidateObject, (Func<PooledObjectValidationContext, bool>)delegate(PooledObjectValidationContext ctx)
			{
				if (ctx.Direction == PooledObjectDirection.Outbound)
				{
					return true;
				}
				IStringBuilderPool stringBuilderPool = base.PooledObjectInfo.Handle as IStringBuilderPool;
				if (StringBuilder.Capacity > stringBuilderPool.MaximumStringBuilderCapacity)
				{
					return false;
				}
				return true;
			});
			base.OnResetState = (Action)Delegate.Combine(base.OnResetState, new Action(ClearStringBuilder));
			base.OnReleaseResources = (Action)Delegate.Combine(base.OnReleaseResources, new Action(ClearStringBuilder));
		}

		public override string ToString()
		{
			return StringBuilder.ToString();
		}

		protected void ClearStringBuilder()
		{
			StringBuilder.Clear();
		}
	}
}
