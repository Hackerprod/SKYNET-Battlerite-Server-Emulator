using MongoDB.Bson;
using System.Runtime.CompilerServices;

namespace SKYNET
{
    public class UserNotifications
    {
        public ObjectId Id { get; set; }

        public ulong SourceSteamId { get; set; }

        public ulong TargetSteamId { get; set; }

        public uint notificationid { get; set; }

        public NotificationType type { get; set; }

        public uint timestamp { get; set; }

        public string message { get; set; }

        public bool FriendshipResult { get; set; }

        public UserNotifications()
        {


        }
    }

    public enum NotificationType
    {
        Message,
        FriendshipRequest,
        FriendshipResponse,
        FriendshipCancel,
        ServerNotification
    }

}
