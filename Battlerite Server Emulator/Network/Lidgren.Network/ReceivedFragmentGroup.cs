namespace Lidgren.Network
{
	internal class ReceivedFragmentGroup
	{
		public float LastReceived;

		public byte[] Data;

		public NetBitVector ReceivedChunks;
	}
}
