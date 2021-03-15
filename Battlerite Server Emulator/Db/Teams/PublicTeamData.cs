using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SKYNET
{
    [Serializable]
    public class PublicTeamData
    {
        public long teamID;
        public List<ulong> members;
        public string name;
        public int avatar;
        public int freeNameChangesOwned;
        public int teamVersion;
        public List<PublicTeamRankingTypeDataV1> rankingTypeDatas;

        public PublicTeamData TeamData { get; set; }
        public PublicTeamDataV2 Get()
        {
            PublicTeamDataV2 Data = new PublicTeamDataV2()
            {
                avatar = avatar,
                freeNameChangesOwned = freeNameChangesOwned,
                members = members,
                name = name,
                rankingTypeDatas = rankingTypeDatas,
                teamID = teamID,
                teamVersion = teamVersion,
            };
            return Data;
        }
        public void Set(PublicTeamDataV2 Data)
        {
            avatar = Data.avatar;
            freeNameChangesOwned = Data.freeNameChangesOwned;
            members = Data.members;
            name = Data.name;
            rankingTypeDatas = Data.rankingTypeDatas;
            teamID = Data.teamID;
            teamVersion = Data.teamVersion;
        }

    }
}
