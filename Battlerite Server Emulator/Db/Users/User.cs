using BloodGUI_Binding.Web;
using MongoDB.Bson;
using StunShared;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace SKYNET
{
    public class User
    {
        public ObjectId Id { get; set; }
        public System.Net.IPAddress ConnectionAddress { get; set; }
        public ulong SteamId { get; set; }
        public string AccountName { get; set; }
        public string Password { get; set; }
        public string PersonaName { get; set; }
        public GetAccountResponse Account { get; set; }
        public SocialProfile SocialProfile { get; set; }
        public List<ulong> Following { get; set; }
        public List<ulong> Followers { get; set; }
        public string sessionID { get; set; }
        public ulong LastLogon { get; set; }
        public ulong LastLogoff { get; set; }
        public bool Banned { get; set; }
        public uint BannedUntil { get; set; }
        public bool IsPlaying { get; set; }
        public bool IsPublicProfile { get; set; }
        public uint AccountId
        {
            get => (uint)Account.profile.userId;
            set
            {
                Account.profile.userId = value;
                Account.inventory.userId = value;
                SocialProfile.userId = value;
            }
        }

        public static string HashPassword(string password)
        {
            return password;// Utils.EncodeHexString(Crypto.ShaHash(password));
        }
        public User(string accountName, string password, string personaName)
        {
            this.AccountName = accountName;
            this.Password = User.HashPassword(password);
            this.PersonaName = (personaName ?? accountName);
            this.IsPlaying = false;
            this.LastLogoff = 0UL;
            this.LastLogon = 0UL;
            this.sessionID = Guid.NewGuid().ToString();
            this.IsPublicProfile = true;
            this.ConnectionAddress = System.Net.IPAddress.Parse("127.0.0.1");
            this.Following = new List<ulong>();
            this.Followers = new List<ulong>();

            Account = new GetAccountResponse()
            {
                createTime = DateTimeExtensions.ToJavaTime(DateTime.Now),
                emailVerificationState = (int)EmailVerificationState.Verified,
                hasDoneAccountTransfer = true,
                newQuests = new List<int>(),
                inventory = new InventoryData()
                {
                    stackables = new List<PlayerStackableData>()
                    {
                        new PlayerStackableData()   //AccountXP 0 - 399 per level
                        {
                            type = 25,
                            amount = 0
                        },
                        new PlayerStackableData()   //AccountLevel amount + 1 (amount 8 = level 9)
                        {
                            type = 26,
                            amount = 0
                        },

                        new PlayerStackableData()   //Bloodcoins (money)
                        {
                            type = 100,
                            amount = 50
                        },
                        new PlayerStackableData()   //Dustcoins (green money)
                        {
                            type = 101,
                            amount = 0
                        },
                        new PlayerStackableData()   //Gems
                        {
                            type = 102,
                            amount = 0
                        },

                        new PlayerStackableData()   //Lootboxes
                        {
                            type = 400001,
                            amount = 0
                        },
                        new PlayerStackableData()   //EmailVerificationReward
                        {
                            type = 91,
                            amount = 1612403765
                        },
                        new PlayerStackableData()   //PrivacyPolicyAccepted
                        {
                            type = 93,
                            amount = 1
                        },
                        new PlayerStackableData()   //TermsOfServiceVersionExcepted
                        {
                            type = 20,
                            amount = 1
                        },
                        new PlayerStackableData()   //CountryCode
                        {
                            type = 199,
                            amount = 192
                        },
                        new PlayerStackableData()   //RoyaleEmailVerificationReward
                        {
                            type = 201,
                            amount = 1612403765
                        },
                        new PlayerStackableData()   //LastUpdateCountryCodeTime
                        {
                            type = 205,
                            amount = 1612403596
                        },
                        new PlayerStackableData()   //Cofre de plata
                        {
                            type = 400001,
                            amount = 0
                        },
                        new PlayerStackableData()   //Cofre de oro
                        {
                            type = 400002,
                            amount = 0
                        },
                        // 400003 Cofre legendario
                        // 400004 Cofre Espeluznante
                        // 400005 Cofre de Ezmo
                        // 400006 Cofre de Helado
                        // 400007 Cofre de Pestilus
                        // 400008 Cofre de Raigon
                        // 400009 Cofre de Atuendo legendario
                        // 400010 Cofre tutorial de Jade
                        // 400011 Cofre de Campeon
                        // 400012 Cofre de duelo
                        // 400013 Cofre de atuendo Rooktober
                        //...
                        

                        //11000 Characters exp (Example: 1500 = hero lvl 5 )
                        //12000 CharacterWins 
                        //13000 CharacterLosses

                        //400001 Cofre de plata
                        //400002 Cofre de oro
                    }
                },
                profile = new AccountProfileData()
                {
                    displayFlags = (uint)DisplayFlag.ShowLeagueFrame,
                    name = personaName,
                    picture = 30000,
                    title = 504,
                },
            };

            SocialProfile = new SocialProfile()
            {
                name = personaName,
                picture = 30000,
                title = 504,
            };
            
        }

    }
}
