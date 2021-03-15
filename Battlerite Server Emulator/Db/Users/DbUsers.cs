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
using SKYNET.Steam;
using System.Net;
using StunShared;

namespace SKYNET.Db
{
    public class DbUsers
    {
        DbManager Manager;
        private readonly MongoDbCollection<User> DbUser;
        public event EventHandler<User> OnNewAccount;
        public event EventHandler<User> OnAccountDeleted;
        public event EventHandler<User> OnAccountBaned;
        public event EventHandler<User> OnAccountUnbanned;


        public DbUsers(DbManager dbManager)
        {
            this.DbUser = new MongoDbCollection<User>(dbManager, "SKYNET_users");
            Manager = dbManager;
        }


        private uint CreateAccountId()
        {
            uint num = this.DbUser.Collection.Find(FilterDefinition<User>.Empty, null).SortByDescending((User u) => (object)u.AccountId).Project((User u) => u.AccountId).FirstOrDefault(default(CancellationToken));
            if (num <= 0U)
            {
                return 1000U;
            }
            return num + 1U;
        }


        public List<User> Users(int offset, int count)
        {
            return this.DbUser.Collection.Find(FilterDefinition<User>.Empty, null).Skip(new int?(offset)).Limit(new int?(count)).ToList(default(CancellationToken));
        }

        public List<User> AllUsers()
        {
            return this.DbUser.Collection.Find(FilterDefinition<User>.Empty, null).ToList();
        }

        public List<User> AllBannedUsers()
        {
            return this.DbUser.Collection.Find((User usr) => usr.Banned, null).ToList(default(CancellationToken));
        }

        public List<User> FindUsers(string pattern)
        {
            string p = (pattern ?? "").ToLower();
            if (!string.IsNullOrEmpty(pattern))
            {
                return this.AllUsers().FindAll((User usr) => (usr.AccountName.ToLower().Contains(p)) || usr.PersonaName.ToLower().Contains(p));
            }
            return this.AllUsers();
        }
        public List<User> FindUsersExcept(string pattern, ulong steamid)
        {
            string p = (pattern ?? "").ToLower();
            if (!string.IsNullOrEmpty(p))
            {
                return this.AllUsers().FindAll((User usr) => usr.SteamId != steamid && (usr.AccountName.ToLower().Contains(p) || usr.PersonaName.ToLower().Contains(p)));
            }
            return this.AllUsers().FindAll((User usr) => usr.SteamId != steamid);
        }

        internal void SetPicture(uint accountId, int picture)
        {
            this.DbUser.Collection.FindOneAndUpdate((User user) => user.AccountId == accountId, this.DbUser.Ub.Set<int>((User user) => user.Account.profile.picture, picture), null, default(CancellationToken));
        }

        public bool Create(User user)
        {
            if (user.AccountName == null)
            {
                return false;
            }
            if (this.DbUser.Collection.CountDocuments((User usr) => usr.AccountName.ToLower() == user.AccountName.ToLower(), null, default(CancellationToken)) != 0L)
            {
                return false;
            }
            user.AccountId = this.CreateAccountId();
            user.SteamId = new SteamID(user.AccountId, EUniverse.Public, EAccountType.Individual).ConvertToUInt64();
            this.DbUser.Collection.InsertOne(user, null, default(CancellationToken));
            this.OnNewAccount?.Invoke(this, user);
            return true;
        }
        public User CreateandGet(User user)
        {
            //if (this.DbUser.Collection.CountDocuments((User usr) => usr.AccountName.ToLower() == user.AccountName.ToLower(), null, default(CancellationToken)) != 0L)
            //{
            //    throw new Exception("User '" + user.AccountName + "' already exists.");
            //}
            //user.AccountId = this.CreateAccountId();
            //user.SteamId = new SteamID(user.AccountId, EUniverse.Public, EAccountType.Individual).ConvertToUInt64();
            //this.DbUser.Collection.InsertOne(user, null, default(CancellationToken));
            //this.OnNewAccount?.Invoke(this, user);
            return user;
        }

        public bool Delete(uint accountId)
        {
            User user = GetByAccountId(accountId);
            if (user != null)
            {
                var flag2 = DbUser.Collection.DeleteMany((User a) => a.AccountId == accountId, default(CancellationToken));
                if (flag2.DeletedCount > 0)
                {
                    this.OnAccountDeleted?.Invoke(this, user);
                    return true;
                }
            }
            return false;
        }

        public User Get(string accountName)
        {
            accountName = accountName.ToLower();
            return this.DbUser.Collection.Find((User usr) => usr.AccountName.ToLower() == accountName, null).FirstOrDefault(default(CancellationToken));
        }

        public List<uint> GetAccountFriends(ulong steamId)
        {
            List<uint> friends = new List<uint>();
            Friend friend = Manager.Friends.GetBySteamId(steamId);
            if (friend != null)
            {
                friends.AddRange(friend.friends);
            }
            return friends;
        }

        public User GetBySteamId(ulong steamId)
        {
            return this.DbUser.Collection.Find((User usr) => usr.SteamId == steamId, null).FirstOrDefault(default(CancellationToken));
        }
        public User GetByRemoteAddress(IPAddress Address)
        {
            return this.DbUser.Collection.Find((User usr) => usr.ConnectionAddress == Address, null).FirstOrDefault(default(CancellationToken));
        }
        public void SetRemoteAddress(uint accountId, IPAddress Address)
        {
            this.DbUser.Collection.FindOneAndUpdate((User user) => user.AccountId == accountId, this.DbUser.Ub.Set<IPAddress>((User user) => user.ConnectionAddress, Address), null, default(CancellationToken));
        }

        public User GetByAccountId(ulong accountId)
        {
            return this.DbUser.Collection.Find((User usr) => usr.AccountId == accountId, null).FirstOrDefault(default(CancellationToken));
        }

        public User GetByAccountName(string accountName)
        {
            return this.DbUser.Collection.Find((User usr) => usr.AccountName.ToLower() == accountName.ToLower(), null).FirstOrDefault(default(CancellationToken));
        }

        public bool TryGetSteamId(uint uniqueId, out ulong steamId)
        {
            steamId = this.DbUser.Collection.Find((User usr) => usr.AccountId == uniqueId, null).Project((User u) => u.SteamId).FirstOrDefault(default(CancellationToken));
            return steamId > 0UL;
        }

        public string GetPersonaName(uint acccountId)
        {
            return this.DbUser.Collection.Find((User usr) => usr.AccountId == acccountId, null).Project((User u) => u.PersonaName).FirstOrDefault(default(CancellationToken));
        }
        public string GetPersonaName(ulong steamId)
        {
            string personaname = this.DbUser.Collection.Find((User usr) => usr.SteamId == steamId, null).Project((User u) => u.PersonaName).FirstOrDefault(default(CancellationToken));
            return string.IsNullOrEmpty(personaname) ? "" : personaname;
        }
        public bool TryGetUser(ulong steamId, out User commender)
        {
            commender = GetBySteamId(steamId);
            return commender != null;
        }

        public bool AccountExists(string accountName)
        {
            string st = accountName.ToLower();
            return this.DbUser.Collection.CountDocuments((User usr) => usr.AccountName == st, null, default(CancellationToken)) == 1L;
        }

        public bool IsAccountAvailable(string accountName)
        {
            return this.DbUser.Collection.CountDocuments((User usr) => usr.AccountName.ToLower() == accountName.ToLower(), null, default(CancellationToken)) == 0L;
        }

        public bool IsPersonaNameAvailable(string personaName)
        {
            string st = personaName.ToLower();
            return this.DbUser.Collection.CountDocuments((User usr) => usr.PersonaName == st, null, default(CancellationToken)) == 0L;
        }

        public void SetLastLogOff(uint accountId, ulong lastLogOff)
        {
            this.DbUser.Collection.FindOneAndUpdate((User user) => user.AccountId == accountId, this.DbUser.Ub.Set<ulong>((User user) => user.LastLogoff, lastLogOff), null, default(CancellationToken));
        }

        public void SetLastLogOn(uint accountId, ulong lastLogOn)
        {
            this.DbUser.Collection.FindOneAndUpdate((User user) => user.AccountId == accountId, this.DbUser.Ub.Set<ulong>((User user) => user.LastLogon, lastLogOn), null, default(CancellationToken));
        }

        public void SetPassword(uint accountId, string password)
        {
            this.DbUser.Collection.FindOneAndUpdate((User user) => user.AccountId == accountId, this.DbUser.Ub.Set<string>((User user) => user.Password, User.HashPassword(password)), null, default(CancellationToken));
        }

        public void SetPersonaName(uint accountId, string personaName)
        {
            User byAccountId = this.GetByAccountId(accountId);
            this.DbUser.Collection.FindOneAndUpdate((User user) => user.AccountId == accountId, this.DbUser.Ub.Set<string>((User user) => user.PersonaName, personaName), null, default(CancellationToken));
        }
        public User DiscountOnWallet(User user, int money)
        {
            if (money <= 0)
            {
                return user;
            }
            if (user == null)
            {
                return user;
            }
            int usercoins = GetCoins(user);
            int num = usercoins - money;
            if (num < 0)
            {
                num = 0;
            }
            foreach (var item in user.Account.inventory.stackables)
            {
                if (item.type == 100)
                {
                    item.amount = num;
                }
            }
            this.DbUser.Collection.FindOneAndUpdate((User u) => u.AccountId == user.AccountId, this.DbUser.Ub.Set<List<PlayerStackableData>>((User u) => u.Account.inventory.stackables, user.Account.inventory.stackables), null, default(CancellationToken));
            return user;
        }

        private int GetCoins(User user)
        {
            foreach (var item in user.Account.inventory.stackables)
            {
                if (item.type == 100)
                {
                    return item.amount;
                }
            }
            return 0;
        }

        public int AddToWallet(User user, int money)
        {
            int count = money;
            foreach (var item in user.Account.inventory.stackables)
            {
                if (item.type == 100)
                {
                    count = item.amount + money;
                    item.amount = count;
                }
            }
            this.DbUser.Collection.FindOneAndUpdate((User u) => u.AccountId == user.AccountId, this.DbUser.Ub.Set<List<PlayerStackableData>>((User u) => u.Account.inventory.stackables, user.Account.inventory.stackables), null, default(CancellationToken));

            return count;
        }
        public void AddStackableData(User user)
        {
            this.DbUser.Collection.FindOneAndUpdate((User u) => u.AccountId == user.AccountId, this.DbUser.Ub.Set<List<PlayerStackableData>>((User us) => us.Account.inventory.stackables, user.Account.inventory.stackables), null, default(CancellationToken));
        }

        public int Count()
        {
            return (int)this.DbUser.Collection.CountDocuments(FilterDefinition<User>.Empty, null, default(CancellationToken));
        }

        public int CountActive()
        {
            return Count();
        }
        public int CountPlaying()
        {
            return (int)this.DbUser.Collection.CountDocuments((User user) => user.IsPlaying, null, default(CancellationToken));
        }
        public List<User> GetPlaying()
        {
            return this.AllUsers().FindAll((User usr) => (usr.IsPlaying));
        }

        public uint BansCount()
        {
            return (uint)this.DbUser.Collection.CountDocuments((User u) => u.Banned, null, default(CancellationToken));
        }



        private FilterDefinition<User> Predicate(string search)
        {
            FilterDefinitionBuilder<User> filter = Builders<User>.Filter;
            BsonRegularExpression regex = BsonRegularExpression.Create(new Regex(Regex.Escape(search), RegexOptions.IgnoreCase));

            return filter.Or(new FilterDefinition<User>[]
            {
                    filter.Regex((User u) => u.AccountName, regex),
                    filter.Regex((User u) => u.PersonaName, regex),
                    filter.Regex((User u) => (object)u.SteamId, regex)
            });
        }

        public int CountPaginated(string filter)
        {
            return (int)(string.IsNullOrEmpty(filter.Trim()) ? this.DbUser.Collection.CountDocuments(FilterDefinition<User>.Empty, null, default(CancellationToken)) : this.DbUser.Collection.CountDocuments(this.Predicate(filter), null, default(CancellationToken)));
        }

        public List<User> GetPaginated(string filter, int offset, int pageSize)
        {
            if (string.IsNullOrEmpty(filter.Trim()))
            {
                return  this.DbUser.Collection.Find(FilterDefinition<User>.Empty, null).Skip(new int?(offset)).Limit(new int?(pageSize)).SortBy((User user) => (object)user.AccountId).ToList(default(CancellationToken));
            }
            else
            {
                return this.DbUser.Collection.Find(this.Predicate(filter), null).Skip(new int?(offset)).Limit(new int?(pageSize)).SortBy((User user) => (object)user.AccountId).ToList(default(CancellationToken));
            }
        }

    }
}
