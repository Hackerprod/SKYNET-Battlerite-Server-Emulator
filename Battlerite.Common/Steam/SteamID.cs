using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;

namespace SKYNET.Steam
{
	[DebuggerDisplay("{Render()}, {ConvertToUInt64()}")]
	public class SteamID
	{
		[Flags]
		public enum ChatInstanceFlags : uint
		{
			Clan = 0x80000,
			Lobby = 0x40000,
			MMSLobby = 0x20000
		}

		private BitVector64 steamid;

		private static Regex Steam2Regex = new Regex("STEAM_(?<universe>[0-4]):(?<authserver>[0-1]):(?<accountid>\\d+)", RegexOptions.IgnoreCase | RegexOptions.Compiled);

		private static Regex Steam3Regex = new Regex("\\[(?<type>[AGMPCgcLTIUai]):(?<universe>[0-4]):(?<account>\\d+)(:(?<instance>\\d+))?\\]", RegexOptions.Compiled);

		private static Regex Steam3FallbackRegex = new Regex("\\[(?<type>[AGMPCgcLTIUai]):(?<universe>[0-4]):(?<account>\\d+)(\\((?<instance>\\d+)\\))?\\]", RegexOptions.Compiled);

		private static Dictionary<EAccountType, char> AccountTypeChars = new Dictionary<EAccountType, char>
		{
			{
				EAccountType.AnonGameServer,
				'A'
			},
			{
				EAccountType.GameServer,
				'G'
			},
			{
				EAccountType.Multiseat,
				'M'
			},
			{
				EAccountType.Pending,
				'P'
			},
			{
				EAccountType.ContentServer,
				'C'
			},
			{
				EAccountType.Clan,
				'g'
			},
			{
				EAccountType.Chat,
				'T'
			},
			{
				EAccountType.Invalid,
				'I'
			},
			{
				EAccountType.Individual,
				'U'
			},
			{
				EAccountType.AnonUser,
				'a'
			}
		};

		private const char UnknownAccountTypeChar = 'i';

		public const uint AllInstances = 0u;

		public const uint DesktopInstance = 1u;

		public const uint ConsoleInstance = 2u;

		public const uint WebInstance = 4u;

		public const uint AccountIDMask = uint.MaxValue;

		public const uint AccountInstanceMask = 1048575u;

		public bool IsBlankAnonAccount
		{
			get
			{
				if (AccountID == 0 && IsAnonAccount)
				{
					return AccountInstance == 0;
				}
				return false;
			}
		}

		public bool IsGameServerAccount
		{
			get
			{
				if (AccountType != EAccountType.GameServer)
				{
					return AccountType == EAccountType.AnonGameServer;
				}
				return true;
			}
		}

		public bool IsPersistentGameServerAccount => AccountType == EAccountType.GameServer;

		public bool IsAnonGameServerAccount => AccountType == EAccountType.AnonGameServer;

		public bool IsContentServerAccount => AccountType == EAccountType.ContentServer;

		public bool IsClanAccount => AccountType == EAccountType.Clan;

		public bool IsChatAccount => AccountType == EAccountType.Chat;

		public bool IsLobby
		{
			get
			{
				if (AccountType == EAccountType.Chat)
				{
					return (AccountInstance & 0x40000) != 0;
				}
				return false;
			}
		}

		public bool IsIndividualAccount
		{
			get
			{
				if (AccountType != EAccountType.Individual)
				{
					return AccountType == EAccountType.ConsoleUser;
				}
				return true;
			}
		}

		public bool IsAnonAccount
		{
			get
			{
				if (AccountType != EAccountType.AnonUser)
				{
					return AccountType == EAccountType.AnonGameServer;
				}
				return true;
			}
		}

		public bool IsAnonUserAccount => AccountType == EAccountType.AnonUser;

		public bool IsConsoleUserAccount => AccountType == EAccountType.ConsoleUser;

		public bool IsValid
		{
			get
			{
				if (AccountType <= EAccountType.Invalid || AccountType >= EAccountType.Max)
				{
					return false;
				}
				if (AccountUniverse <= EUniverse.Invalid || AccountUniverse >= EUniverse.Max)
				{
					return false;
				}
				if (AccountType == EAccountType.Individual && (AccountID == 0 || AccountInstance > 4))
				{
					return false;
				}
				if (AccountType == EAccountType.Clan && (AccountID == 0 || AccountInstance != 0))
				{
					return false;
				}
				if (AccountType == EAccountType.GameServer && AccountID == 0)
				{
					return false;
				}
				return true;
			}
		}

		public uint AccountID
		{
			get
			{
				return (uint)steamid[0u, 4294967295uL];
			}
			set
			{
				steamid[0u, 4294967295uL] = value;
			}
		}

		public uint AccountInstance
		{
			get
			{
				return (uint)steamid[32u, 1048575uL];
			}
			set
			{
				steamid[32u, 1048575uL] = value;
			}
		}

		public EAccountType AccountType
		{
			get
			{
				return (EAccountType)steamid[52u, 15uL];
			}
			set
			{
				steamid[52u, 15uL] = (ulong)value;
			}
		}

		public EUniverse AccountUniverse
		{
			get
			{
				return (EUniverse)steamid[56u, 255uL];
			}
			set
			{
				steamid[56u, 255uL] = (ulong)value;
			}
		}

		public SteamID()
			: this(0uL)
		{
		}

		public SteamID(uint unAccountID, EUniverse eUniverse, EAccountType eAccountType)
			: this()
		{
			Set(unAccountID, eUniverse, eAccountType);
		}

		public SteamID(uint unAccountID, uint unInstance, EUniverse eUniverse, EAccountType eAccountType)
			: this()
		{
			InstancedSet(unAccountID, unInstance, eUniverse, eAccountType);
		}

		public SteamID(ulong id)
		{
			steamid = new BitVector64(id);
		}

		public SteamID(string steamId)
			: this(steamId, EUniverse.Public)
		{
		}

		public SteamID(string steamId, EUniverse eUniverse)
			: this()
		{
			SetFromString(steamId, eUniverse);
		}

		public void Set(uint unAccountID, EUniverse eUniverse, EAccountType eAccountType)
		{
			AccountID = unAccountID;
			AccountUniverse = eUniverse;
			AccountType = eAccountType;
			if (eAccountType == EAccountType.Clan || eAccountType == EAccountType.GameServer)
			{
				AccountInstance = 0u;
			}
			else
			{
				AccountInstance = 1u;
			}
		}

		public void InstancedSet(uint unAccountID, uint unInstance, EUniverse eUniverse, EAccountType eAccountType)
		{
			AccountID = unAccountID;
			AccountUniverse = eUniverse;
			AccountType = eAccountType;
			AccountInstance = unInstance;
		}

		public bool SetFromString(string steamId, EUniverse eUniverse)
		{
			if (string.IsNullOrEmpty(steamId))
			{
				return false;
			}
			Match match = Steam2Regex.Match(steamId);
			if (!match.Success)
			{
				return false;
			}
			if (!uint.TryParse(match.Groups["accountid"].Value, out uint result) || !uint.TryParse(match.Groups["authserver"].Value, out uint result2))
			{
				return false;
			}
			AccountUniverse = eUniverse;
			AccountInstance = 1u;
			AccountType = EAccountType.Individual;
			AccountID = ((result << 1) | result2);
			return true;
		}

		public bool SetFromSteam3String(string steamId)
		{
			if (string.IsNullOrEmpty(steamId))
			{
				return false;
			}
			Match match = Steam3Regex.Match(steamId);
			if (!match.Success)
			{
				match = Steam3FallbackRegex.Match(steamId);
				if (!match.Success)
				{
					return false;
				}
			}
			if (!uint.TryParse(match.Groups["account"].Value, out uint result))
			{
				return false;
			}
			if (!uint.TryParse(match.Groups["universe"].Value, out uint result2))
			{
				return false;
			}
			string value = match.Groups["type"].Value;
			if (value.Length != 1)
			{
				return false;
			}
			char type = value[0];
			Group group = match.Groups["instance"];
			uint num;
			if (group == null || string.IsNullOrEmpty(group.Value))
			{
				switch (type)
				{
				case 'L':
				case 'T':
				case 'c':
				case 'g':
					num = 0u;
					break;
				default:
					num = 1u;
					break;
				}
			}
			else
			{
				num = uint.Parse(group.Value);
			}
			if (type == 'c')
			{
				num |= 0x80000;
				AccountType = EAccountType.Chat;
			}
			else if (type == 'L')
			{
				num |= 0x40000;
				AccountType = EAccountType.Chat;
			}
			else if (type == 'i')
			{
				AccountType = EAccountType.Invalid;
			}
			else
			{
				AccountType = AccountTypeChars.First((KeyValuePair<EAccountType, char> x) => x.Value == type).Key;
			}
			AccountUniverse = (EUniverse)result2;
			AccountInstance = num;
			AccountID = result;
			return true;
		}

		public void SetFromUInt64(ulong ulSteamID)
		{
			steamid.Data = ulSteamID;
		}

		public ulong ConvertToUInt64()
		{
			return steamid.Data;
		}

		public ulong GetStaticAccountKey()
		{
			return (ulong)(((long)AccountUniverse << 56) + ((long)AccountType << 52) + AccountID);
		}

		public string Render(bool steam3 = false)
		{
			if (steam3)
			{
				return RenderSteam3();
			}
			return RenderSteam2();
		}

		private string RenderSteam2()
		{
			EAccountType accountType = AccountType;
			if ((uint)accountType <= 1u)
			{
				string arg = (AccountUniverse <= EUniverse.Public) ? "0" : Enum.Format(typeof(EUniverse), AccountUniverse, "D");
				return $"STEAM_{arg}:{AccountID & 1}:{AccountID >> 1}";
			}
			return Convert.ToString(this);
		}

		private string RenderSteam3()
		{
			if (!AccountTypeChars.TryGetValue(AccountType, out char value))
			{
				value = 'i';
			}
			if (AccountType == EAccountType.Chat)
			{
				if (((ChatInstanceFlags)AccountInstance).HasFlag(ChatInstanceFlags.Clan))
				{
					value = 'c';
				}
				else if (((ChatInstanceFlags)AccountInstance).HasFlag(ChatInstanceFlags.Lobby))
				{
					value = 'L';
				}
			}
			bool flag = false;
			switch (AccountType)
			{
			case EAccountType.Multiseat:
			case EAccountType.AnonGameServer:
				flag = true;
				break;
			case EAccountType.Individual:
				flag = (AccountInstance != 1);
				break;
			}
			if (flag)
			{
				return $"[{value}:{(uint)AccountUniverse}:{AccountID}:{AccountInstance}]";
			}
			return $"[{value}:{(uint)AccountUniverse}:{AccountID}]";
		}

		public override string ToString()
		{
			return Render();
		}

		public static implicit operator ulong(SteamID sid)
		{
			return sid.steamid.Data;
		}

		public static implicit operator SteamID(ulong id)
		{
			return new SteamID(id);
		}

		public override bool Equals(object obj)
		{
			if (obj == null)
			{
				return false;
			}
			SteamID steamID = obj as SteamID;
			if ((object)steamID == null)
			{
				return false;
			}
			return steamid.Data == steamID.steamid.Data;
		}

		public bool Equals(SteamID sid)
		{
			if ((object)sid == null)
			{
				return false;
			}
			return steamid.Data == sid.steamid.Data;
		}

		public static bool operator ==(SteamID a, SteamID b)
		{
			if ((object)a == b)
			{
				return true;
			}
			if ((object)a == null || (object)b == null)
			{
				return false;
			}
			return a.steamid.Data == b.steamid.Data;
		}

		public static bool operator !=(SteamID a, SteamID b)
		{
			return !(a == b);
		}

		public override int GetHashCode()
		{
			return steamid.Data.GetHashCode();
		}
	}
}
