using BloodGUI_Binding.Web;
using Newtonsoft.Json;
using SKYNET;
using SKYNET.Db;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Script.Serialization;

public static class Helpers
{
    public static UserSocialProfile GetUserSocialProfile(this User user)
    {
        return new UserSocialProfile()
        {
            userId = user.SocialProfile.userId,
            name = user.SocialProfile.name,
            picture = user.SocialProfile.picture,
            title = user.SocialProfile.title,
            numPosts = user.SocialProfile.numPosts,
            numFollowing = user.SocialProfile.numFollowing,
            numFollowers = user.SocialProfile.numFollowers,
            following = user.SocialProfile.following,
            followedBy = user.SocialProfile.followedBy,
        };
    }
    public static T DeserializeQuery<T>(this string queryString)
    {
        var dict = HttpUtility.ParseQueryString(queryString);
        string JSON = JsonConvert.SerializeObject(dict.Cast<string>().ToDictionary(k => k, v => dict[v]));
        T request = JsonConvert.DeserializeObject<T>(JSON);
        return request;
    }
    public static T Deserialize<T>(this string json)
    {
        try
        {
            return new JavaScriptSerializer().Deserialize<T>(json);
        }
        catch
        {
            return (T)default;
        }
    }
}

