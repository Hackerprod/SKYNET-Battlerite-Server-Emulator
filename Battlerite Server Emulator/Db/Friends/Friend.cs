using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace SKYNET
{
    [Serializable]
    public class Friend
    {
        public ulong SteamId
        {
            get;
            set;
        }

        public List<uint> friends
        {
            get;
            set;
        }

        public Friend()
        {
            friends = new List<uint>();
        }
    }
}
