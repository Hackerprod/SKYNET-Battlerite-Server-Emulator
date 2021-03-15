using BloodGUI_Binding.Web;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SKYNET.Managements
{
    public class LobbySystem : SystemManagament
    {
        public ConcurrentDictionary<string, MatchLobbyData> Lobbies;
        public LobbySystem()
        {
            Lobbies = new ConcurrentDictionary<string, MatchLobbyData>();
            CreateTestLobby();
        }
        public void CreateTestLobby()
        {
            MatchLobbyData lobby = new MatchLobbyData()
            {
                id = "privatelobby_06a7c73db09e44228d7fb9c988c7fb56_1",
                isInMatch = false,
                observers = new List<PublicMatchLobbyObserver>(),
                players = new List<PublicMatchLobbyPlayer>()
                        {
                            new PublicMatchLobbyPlayer()
                            {
                                isBot = false,
                                slot = 1,
                                team = 2,
                                userId = 1001
                            },
                        },
                version = 102,
                session = new PublicMatchLobbySession()
                {
                    teamOneScore = 15854,
                    teamTwoScore = 12542
                },
                settings = new PublicMatchLobbySettings()
                {
                    allowDuplicateChampions = true,
                    armoryEnabled = true,
                    cooldownModifier = 1.0f,
                    gameType = 1733162751,
                    map = 1608885260,
                    maxObservers = 6,
                    name = "SKYNET lobby",
                    overloadEnabled = true,
                    region = "cuba",
                    roundTime = 90,
                    soulOrbEnabled = true,
                    suddenDeathEnabled = true,
                    teamCount = (int)TeamCount.Arena,
                    teamSize = (int)TeamSize.ThreeVsThree,
                    winScore = 3
                }
            };
            AddOrUpdate(lobby.id, lobby);
        }

        internal List<MatchLobbyData> GetAll()
        {
            return Lobbies.Values.ToList();
        }

        public void AddOrUpdate(string lobbyId, MatchLobbyData conn)
        {
            if (lobbyId == "") return;

            if (Lobbies.TryGetValue(lobbyId, out MatchLobbyData cmConnection))
            {
                this.Lobbies[lobbyId] = cmConnection;
            }
            else
            {
                Lobbies.TryAdd(lobbyId, conn);
            }
        }
        public MatchLobbyData Get(string lobbyId)
        {
            this.Lobbies.TryGetValue(lobbyId, out MatchLobbyData result);
            return result;
        }
        public void Remove(MatchLobbyData LobbyData)
        {
            string num = (from c in this.Lobbies where object.Equals(c.Value.id, LobbyData.id) select c.Key).FirstOrDefault<string>();
            if (num == "")
            {
                return;
            }
            if (this.Lobbies.TryRemove(num, out MatchLobbyData conn))
            {
                ilog.Info(string.Format("The team {0} was removed successfully.", num));
            }
            else
            {
                ilog.Info(string.Format("Team {0} was not found.", num));
            }
        }
        public void Remove(string lobbyId)
        {
            if (lobbyId == "") return;
            MatchLobbyData cmConnection;
            bool flag = this.Lobbies.TryRemove(lobbyId, out cmConnection);
            ilog.Info(flag ? string.Format("The team {0} was removed successfully.", lobbyId) : string.Format("The team {0} was not found.", lobbyId));
        }

    }
}
