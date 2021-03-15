using SKYNET.Db;
using StunShared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SKYNET
{
    public static class UserHelper
    {
        public static bool CanBuy(this User user, int expectedPrice)
        {
            bool CanBuy = false;

            if (user == null) return false;

            foreach (var item in user.Account.inventory.stackables)
            {
                if (item.type == 100 && item.amount > expectedPrice)
                {
                    CanBuy = true;
                }
            }

            return CanBuy;
        }
        public static void AddStackableData(this User user, PlayerStackableData stackableData)
        {
            if (user == null) return;

            if (user.Account.inventory.stackables == null)
                user.Account.inventory.stackables = new List<StunShared.PlayerStackableData>();
            user.Account.inventory.stackables.Add(stackableData);
            BattleriteServer.DbManager.Users.AddStackableData(user);
        }
        public static User DiscountOnWallet(this User user, int price)
        {
            return BattleriteServer.DbManager.Users.DiscountOnWallet(user, price);
        }
        
    }
}
