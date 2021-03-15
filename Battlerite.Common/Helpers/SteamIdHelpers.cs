using SKYNET.Steam;
using System;
using System.Collections.Generic;

public static class SteamIdHelpers
{
    [Flags]
    public enum ChatInstanceFlags : uint
    {
        Clan = 0x80000,
        Lobby = 0x40000,
        MMSLobby = 0x20000
    }

    private const uint WebInstance = 4u;

    private const char UnknownAccountTypeChar = 'i';

    private const uint AccountInstanceMask = 1048575u;

    public const uint DesktopInstance = 1u;

    private static readonly Dictionary<EAccountType, char> AccountTypeChars = new Dictionary<EAccountType, char>
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

    public static uint GetAccountId(this ulong steamId)
    {
        return (uint)(steamId & uint.MaxValue);
    }

    public static uint GetAccountInstanceId(this ulong steamId)
    {
        return (uint)((steamId >> 32) & 0xFFFFF);
    }

    public static EAccountType GetAccountType(this ulong steamId)
    {
        return (EAccountType)((steamId >> 52) & 0xF);
    }

    public static EUniverse GetAccountUniverse(this ulong steamId)
    {
        return (EUniverse)((steamId >> 56) & 0xFF);
    }

    public static bool IsAnonAccount(this ulong steamId)
    {
        EAccountType accountType = steamId.GetAccountType();
        if (EAccountType.AnonUser != accountType)
        {
            return EAccountType.AnonGameServer == accountType;
        }
        return true;
    }

    public static bool IsAnonUserAccount(this ulong steamId)
    {
        EAccountType accountType = steamId.GetAccountType();
        return EAccountType.AnonUser == accountType;
    }

    public static bool IsIndividualAccount(this ulong steamId)
    {
        EAccountType accountType = steamId.GetAccountType();
        if (EAccountType.Individual != accountType)
        {
            return EAccountType.ConsoleUser == accountType;
        }
        return true;
    }

    public static bool IsAnonGameServerAccount(this ulong steamId)
    {
        EAccountType accountType = steamId.GetAccountType();
        return EAccountType.AnonGameServer == accountType;
    }

    public static bool IsGameServerAccount(this ulong steamId)
    {
        EAccountType accountType = steamId.GetAccountType();
        if (EAccountType.GameServer != accountType)
        {
            return EAccountType.AnonGameServer == accountType;
        }
        return true;
    }

    public static bool IsLobbyAccount(this ulong steamId)
    {
        if (steamId.GetAccountType() == EAccountType.Chat)
        {
            return (steamId.GetAccountInstanceId() & 0x40000) != 0;
        }
        return false;
    }

    public static bool IsValid(this ulong steamId)
    {
        uint accountId = steamId.GetAccountId();
        EAccountType accountType = steamId.GetAccountType();
        EUniverse accountUniverse = steamId.GetAccountUniverse();
        uint accountInstanceId = steamId.GetAccountInstanceId();
        if (accountType <= EAccountType.Invalid || accountType >= EAccountType.Max)
        {
            return false;
        }
        if (accountUniverse <= EUniverse.Invalid || accountUniverse >= EUniverse.Max)
        {
            return false;
        }
        if (EAccountType.Individual == accountType && (accountId == 0 || accountInstanceId > 4))
        {
            return false;
        }
        if (EAccountType.Clan == accountType && (accountId == 0 || accountInstanceId != 0))
        {
            return false;
        }
        if (EAccountType.GameServer != accountType)
        {
            return true;
        }
        return accountId != 0;
    }

    public static string RenderSteam3(this ulong steamId)
    {
        EAccountType accountType = steamId.GetAccountType();
        uint accountInstanceId = steamId.GetAccountInstanceId();
        EUniverse accountUniverse = steamId.GetAccountUniverse();
        uint accountId = steamId.GetAccountId();
        if (!AccountTypeChars.TryGetValue(accountType, out char value))
        {
            value = 'i';
        }
        if (accountType == EAccountType.Chat)
        {
            if (((ChatInstanceFlags)accountInstanceId).HasFlag(ChatInstanceFlags.Clan))
            {
                value = 'c';
            }
            else if (((ChatInstanceFlags)accountInstanceId).HasFlag(ChatInstanceFlags.Lobby))
            {
                value = 'L';
            }
        }
        bool flag = false;
        switch (accountType)
        {
            case EAccountType.Multiseat:
            case EAccountType.AnonGameServer:
                flag = true;
                break;
            case EAccountType.Individual:
                flag = (accountInstanceId != 1);
                break;
        }
        if (!flag)
        {
            return $"[{value}:{(uint)accountUniverse}:{accountId}]";
        }
        return $"[{value}:{(uint)accountUniverse}:{accountId}:{accountInstanceId}]";
    }
}