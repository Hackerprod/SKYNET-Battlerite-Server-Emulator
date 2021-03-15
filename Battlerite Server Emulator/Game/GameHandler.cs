using Lidgren.Network;

namespace SKYNET
{
    public class GameHandler
    {
        public IdManager idMan = new IdManager();
        public NetServer server;
        public ILog ilog => BattleriteServer.ilog;
        public PlayerConnectionLookup connections = new PlayerConnectionLookup();
    }
}