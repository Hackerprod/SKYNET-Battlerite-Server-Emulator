using BloodGUI_Binding.Web;
using MongoDB.Bson;
using StunShared;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace SKYNET
{
    [Serializable]
    public class Team
    {
        public ObjectId Id { get; set; }
        public ulong TeamId { get; set; }
        public PublicTeamData TeamData { get; set; }
        public PublicTeamDataV2 GetPublicTeamDataV2()
        {
            PublicTeamDataV2 Data = new PublicTeamDataV2()
            {
                avatar = TeamData.avatar,
                freeNameChangesOwned = TeamData.freeNameChangesOwned,
                members = TeamData.members,
                name = TeamData.name,
                rankingTypeDatas = TeamData.rankingTypeDatas,
                teamID = TeamData.teamID,
                teamVersion = TeamData.teamVersion,
            };
            return Data;
        }
        public void SetPublicTeamDataV2(PublicTeamDataV2 Data)
        {
            TeamData = new PublicTeamData()
            {
                avatar = Data.avatar,
                freeNameChangesOwned = Data.freeNameChangesOwned,
                members = Data.members,
                name = Data.name,
                rankingTypeDatas = Data.rankingTypeDatas,
                teamID = Data.teamID,
                teamVersion = Data.teamVersion,
            };
        }
    }
}
