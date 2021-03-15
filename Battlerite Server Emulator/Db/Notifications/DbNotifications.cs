using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace SKYNET.Db
{
	public class DbNotifications
	{
		private readonly MongoDbCollection<UserNotifications> DB_notifications;

		public DbNotifications(DbManager dbAccess)
		{
            DB_notifications = new MongoDbCollection<UserNotifications>(dbAccess, "SKYNET_notifications");
		}

		public List<UserNotifications> GetNotifications(ulong steamId)
		{
			return DB_notifications.Collection.Find((UserNotifications n) => n.TargetSteamId == steamId).ToList();
		}

		public void AddNotification(UserNotifications notification)
		{
            UserNotifications NewNotification = notification;
            DB_notifications.Collection.InsertOne(NewNotification);
        }

        private uint GetRandomId()
        {
            return (uint)modCommon.RandomID();
        }

        public void RemoveNotifications(ulong steamId)
		{
			DB_notifications.Collection.DeleteMany((UserNotifications n) => n.TargetSteamId == steamId);
		}
        public void RemoveNotification(uint id)
        {
            DB_notifications.Collection.DeleteMany((UserNotifications n) => n.notificationid == id);
        }

        public bool FriendshipRequested(ulong toUser, ulong fromUser)
        {
            return DB_notifications.Collection.Find((UserNotifications n) => n.type == NotificationType.FriendshipRequest && n.TargetSteamId == toUser && n.SourceSteamId == fromUser).FirstOrDefault(default(CancellationToken)) != null;
        }

        public bool Exist(UserNotifications notification)
        {
            List<UserNotifications> Notifications = GetNotifications(notification.TargetSteamId);
            
            if (Notifications.Count > 0)
            {
                List<UserNotifications> someNotifications = Notifications.FindAll((UserNotifications n) => n.type == notification.type && n.TargetSteamId == notification.TargetSteamId && n.SourceSteamId == notification.SourceSteamId);
               
                if (someNotifications.Count > 0)
                {
                    foreach (var item in someNotifications)
                    {
                        if (!string.IsNullOrEmpty(item.message))
                        {
                            if (item.message == notification.message)
                            {
                                return true;
                            }
                        }
                    }
                    return false;
                }
                return false;
            }
            else
                return false;
        }

        public void RemoveFriendshipRequest(ulong SourceSteamId, ulong TargetSteamId)
        {
            DB_notifications.Collection.DeleteMany((UserNotifications n) => n.type == NotificationType.FriendshipRequest && n.SourceSteamId == SourceSteamId && n.TargetSteamId == TargetSteamId);
        }
    }
}
