using System;

namespace CodeProject.ObjectPool
{
	public class EvictionSettings
	{
		public static EvictionSettings Default
		{
			get;
		} = new EvictionSettings();


		public bool Enabled
		{
			get;
			set;
		}

		public TimeSpan Delay
		{
			get;
			set;
		} = TimeSpan.Zero;


		public TimeSpan Period
		{
			get;
			set;
		} = TimeSpan.FromMinutes(1.0);

	}
}
