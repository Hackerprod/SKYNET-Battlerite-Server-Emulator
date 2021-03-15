namespace SKYNET.Steam
{
	internal class BitVector64
	{
		private ulong data;

		public ulong Data
		{
			get
			{
				return data;
			}
			set
			{
				data = value;
			}
		}

		public ulong this[uint bitoffset, ulong valuemask]
		{
			get
			{
				return (data >> (int)(ushort)bitoffset) & valuemask;
			}
			set
			{
				data = ((data & ~(valuemask << (int)(ushort)bitoffset)) | ((value & valuemask) << (int)(ushort)bitoffset));
			}
		}

		public BitVector64()
		{
		}

		public BitVector64(ulong value)
		{
			data = value;
		}
	}
}
