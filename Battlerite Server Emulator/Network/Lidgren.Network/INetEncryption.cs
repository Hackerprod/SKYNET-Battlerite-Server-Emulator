namespace Lidgren.Network
{
	public interface INetEncryption
	{
		bool Encrypt(NetOutgoingMessage msg);

		bool Decrypt(NetIncomingMessage msg);
	}
}
