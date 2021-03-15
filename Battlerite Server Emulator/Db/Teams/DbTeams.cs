using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading;
using MongoDB.Bson.IO;
using System.IO;
using MongoDB.Bson.Serialization;
using System.Net;

namespace SKYNET.Db
{
    public class DbTeams
    {
        DbManager Manager;
        private readonly MongoDbCollection<Team> DbTeam;
        public event EventHandler<Team> OnNewAccount;
        public event EventHandler<Team> OnAccountDeleted;
        public event EventHandler<Team> OnAccountBaned;
        public event EventHandler<Team> OnAccountUnbanned;


        public DbTeams(DbManager dbManager)
        {
            this.DbTeam = new MongoDbCollection<Team>(dbManager, "SKYNET_teams");
            Manager = dbManager;
        }


        private ulong CreateTeamId()
        {
            ulong num = this.DbTeam.Collection.Find(FilterDefinition<Team>.Empty, null).SortByDescending((Team u) => (object)u.TeamId).Project((Team u) => u.TeamId).FirstOrDefault(default(CancellationToken));
            if (num <= 0U)
            {
                return 100000U;
            }
            return num + 1U;
        }


        public List<Team> GetTeams(int offset, int count)
        {
            return this.DbTeam.Collection.Find(FilterDefinition<Team>.Empty, null).Skip(new int?(offset)).Limit(new int?(count)).ToList(default(CancellationToken));
        }

        public List<Team> AllTeams()
        {
            return this.DbTeam.Collection.Find(FilterDefinition<Team>.Empty, null).ToList();
        }


        public List<Team> FindTeamsFromAccountId(ulong AccountId)
        {
            return this.AllTeams().FindAll((Team usr) => (usr.TeamData.members.Contains(AccountId)));
        }
        public List<Team> FindTeamsExcept(ulong TeamId)
        {
            return this.AllTeams().FindAll((Team usr) => usr.TeamId != TeamId);
        }

        internal void SetPicture(uint TeamId, int picture)
        {
            this.DbTeam.Collection.FindOneAndUpdate((Team Team) => Team.TeamId == TeamId, this.DbTeam.Ub.Set<int>((Team Team) => Team.TeamData.avatar, picture), null, default(CancellationToken));
        }
        internal void AddPlayer(ulong TeamId, ulong AccountId)
        {
            Team team = GetByTeamId(TeamId);
            if (team == null) return;
            List<ulong> members = team.TeamData.members;
            members.Add(AccountId);
            this.DbTeam.Collection.FindOneAndUpdate((Team Team) => Team.TeamId == TeamId, this.DbTeam.Ub.Set<List<ulong>>((Team Team) => Team.TeamData.members, members), null, default(CancellationToken));
        }

        internal void RemovePlayer(ulong TeamId, ulong AccountId)
        {
            Team team = GetByTeamId(TeamId);
            if (team == null) return;
            List<ulong> members = team.TeamData.members;
            members.Remove(AccountId);
            this.DbTeam.Collection.FindOneAndUpdate((Team Team) => Team.TeamId == TeamId, this.DbTeam.Ub.Set<List<ulong>>((Team Team) => Team.TeamData.members, members), null, default(CancellationToken));
        }



        public Team CreateandGet(Team Team)
        {
            if (this.DbTeam.Collection.CountDocuments((Team usr) => usr.TeamId == Team.TeamId, null, default(CancellationToken)) != 0L)
            {
                throw new Exception("Team '" + Team.TeamId + "' already exists.");
            }
            Team.TeamId = this.CreateTeamId();
            this.DbTeam.Collection.InsertOne(Team, null, default(CancellationToken));
            this.OnNewAccount?.Invoke(this, Team);
            return Team;
        }

        public bool Delete(uint TeamId)
        {
            Team Team = GetByTeamId(TeamId);
            if (Team != null)
            {
                var flag2 = DbTeam.Collection.DeleteMany((Team a) => a.TeamId == TeamId, default(CancellationToken));
                if (flag2.DeletedCount > 0)
                {
                    this.OnAccountDeleted?.Invoke(this, Team);
                    return true;
                }
            }
            return false;
        }

        internal void AddOrUpdate(ulong teamID, PublicTeamDataV2 team)
        {
            if (TeamExists(teamID))
            {
                PublicTeamData TeamData = new PublicTeamData();
                TeamData.Set(team);
                this.DbTeam.Collection.FindOneAndUpdate((Team Team) => Team.TeamId == teamID, this.DbTeam.Ub.Set<PublicTeamData>((Team Team) => Team.TeamData, TeamData), null, default(CancellationToken));
            }
            else
                Create(teamID, team);
        }

        private void Create(ulong teamID, PublicTeamDataV2 teamData)
        {
            Team team = new Team();
            team.TeamId = this.CreateTeamId();
            team.SetPublicTeamDataV2(teamData);
            this.DbTeam.Collection.InsertOne(team, null, default(CancellationToken)); this.DbTeam.Collection.InsertOne(team, null, default(CancellationToken));
        }

        public Team Get(ulong teamid)
        {
            return this.DbTeam.Collection.Find((Team usr) => usr.TeamId == teamid, null).FirstOrDefault(default(CancellationToken));
        }
        
        public Team GetByTeamId(ulong TeamId)
        {
            return this.DbTeam.Collection.Find((Team usr) => usr.TeamId == TeamId, null).FirstOrDefault(default(CancellationToken));
        }
        public Team GetByTeamName(string TeamName)
        {
            return this.DbTeam.Collection.Find((Team usr) => usr.TeamData.name.ToLower() == TeamName.ToLower(), null).FirstOrDefault(default(CancellationToken));
        }

        public bool TryGetTeam(uint uniqueId, out Team TeamId)
        {
            TeamId = this.DbTeam.Collection.Find((Team usr) => usr.TeamId == uniqueId, null).Project((Team u) => u).FirstOrDefault(default(CancellationToken));
            return TeamId != null;
        }
        public bool TryGetTeam(ulong TeamId, out Team commender)
        {
            commender = GetByTeamId(TeamId);
            return commender != null;
        }

        public bool TeamExists(string TeamName)
        {
            return this.DbTeam.Collection.CountDocuments((Team usr) => usr.TeamData.name.ToLower() == TeamName.ToLower(), null, default(CancellationToken)) == 1L;
        }
        public bool TeamExists(ulong teamID)
        {
            return this.DbTeam.Collection.CountDocuments((Team usr) => usr.TeamId == teamID, null, default(CancellationToken)) == 1L;
        }

        public int Count()
        {
            return (int)this.DbTeam.Collection.CountDocuments(FilterDefinition<Team>.Empty, null, default(CancellationToken));
        }

    }
}
