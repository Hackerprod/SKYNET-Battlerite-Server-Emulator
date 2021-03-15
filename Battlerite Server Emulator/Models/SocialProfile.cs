using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SKYNET
{
    [Serializable]
    public class SocialProfile
    {
        public ulong userId;
        public string name;
        public int picture;
        public int title;
        public long numPosts;
        public long numFollowing;
        public long numFollowers;
        public bool following;
        public bool followedBy;
    }
}
