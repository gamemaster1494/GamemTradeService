//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading;
//using System.Timers;
//using SteamKit2;
//using SteamTrade;

//namespace SteamBot
//{
//    public class UnusualUserHandler : UserHandler
//    {

//        private int InventoryKeys = 0;

//        private int KeysPutUp = 0;

//        private Thread FriendInviteMessage;

//        private const string FRIEND_ADDED_MESSAGE = "Hello! I am an Unusual Bot! I pay keys for quicksell unusuals! Invite me to trade to see how much I would pay for yours!";

//        public UnusualUserHandler(Bot bot, SteamID sid)
//            : base(bot, sid)
//        {
//            FriendInviteMessage = new Thread(new ThreadStart(SendFriendMessage));
//        }

//        /// <summary>
//        /// Triggered when the bot receives a clan invite
//        /// </summary>
//        /// <returns>True to accept invite, False to decline invite</returns>
//        public override bool OnClanAdd()
//        {
//            return false;//Decline, as the bot doesn't need to join groups
//        }//OnClanAdd()

//        /// <summary>
//        /// Triggered when a user adds the bot as a friend.
//        /// </summary>
//        /// <returns>True to accept invite, False to decline invite</returns>
//        public override bool OnFriendAdd()
//        {
//            if (SteamRepStatus.GetSteamRepStatus(OtherSID.ConvertToUInt64().ToString()).steamrep.Reputation.SummaryRep.Contains("SCAMMER"))
//            {
//                Bot.log.Warn(String.Format("{0} tried to add me, but is a scammer!", OtherSID.ConvertToUInt64()));//Log scammer
//                return false;//Decline invite
//            }//If user is a scammer
//            Bot.log.success(Bot.SteamFriends.GetFriendPersonaName(OtherSID) + " (" + OtherSID.ConvertToUInt64() + ") added me!");//Log someone added
//            FriendInviteMessage.Start();//start message thread
//            return true;//Accept invite
//        }//OnFriendAdd()

//        /// <summary>
//        /// Triggered when a user removes the bot from their friends list.
//        /// </summary>
//        public override void OnFriendRemove()
//        {
//            Bot.log.Warn(String.Format("{0} removed me from friends list", Bot.SteamFriends.GetFriendPersonaName(OtherSID)));//log friend removed
//        }//OnFriendRemoved()

//        /// <summary>
//        /// Triggers when a user invites the bot to trade.
//        /// </summary>
//        /// <returns>True to accept the invite, False to decline the invite.</returns>
//        public override bool OnTradeRequest()
//        {
//            if (SteamRepStatus.GetSteamRepStatus(OtherSID.ConvertToUInt64().ToString()).steamrep.Reputation.SummaryRep.Contains("SCAMMER"))
//            {
//                Bot.SteamFriends.RemoveFriend(OtherSID);//remove friend
//                return false;//Don't accept the trade
//            }//If user is a scammer
//            Bot.log.success(Bot.SteamFriends.GetFriendPersonaName(OtherSID) + " (" + OtherSID.ConvertToUInt64() + ") has requested to trade with me!");//show log someone traded            
//            return true;//start trade!
//        }//OnTradeRequest()

//        /// <summary>
//        /// Triggers when an error occurs in trade.
//        /// </summary>
//        /// <param name="error">Error description.</param>
//        public override void OnTradeError(string error)
//        {
//            Bot.log.Error(error);//Log error 
//        }//OnTradeError()

//        /// <summary>
//        /// Triggers when the user adds an item to the trade.
//        /// </summary>
//        /// <param name="schemaItem">Schema version of the item added.</param>
//        /// <param name="inventoryItem">Inventory.Item version of the item added.</param>
//        public override void OnTradeAddItem(Schema.Item schemaItem, Inventory.Item inventoryItem)
//        {
//            throw new NotImplementedException();
//        }//OnTradeAddItem()

//        /// <summary>
//        /// Triggered when the user removes an item from trade.
//        /// </summary>
//        /// <param name="schemaItem">Schema version of the item removed.</param>
//        /// <param name="inventoryItem">Inventory.Item version of the item removed.</param>
//        public override void OnTradeRemoveItem(Schema.Item schemaItem, Inventory.Item inventoryItem)
//        {
//            throw new NotImplementedException();
//        }//OnTradeRemoveItem()

//        /// <summary>
//        /// Triggered when the trade has been initialized.
//        /// </summary>
//        public override void OnTradeInit()
//        {
//            Bot.SteamFriends.SetPersonaState(EPersonaState.Busy);//Set state as busy
//            Bot.log.success("Trade started!");//Log trade started
//            Trade.SendMessage("Please wait while I load our inventories.");//Tell user to wait while bot loads inventory

//            TradeCountInventory();//Load inventory
//        }//OnTradeInit()


//        public override void OnTradeMessage(string message)
//        {
            
//        }


//        /// <summary>
//        /// Triggered when the trade closes.
//        /// </summary>
//        public override void OnTradeClose()
//        {
//            base.OnTradeClose();//Close the trade vars in UserHandler
//            Bot.SteamFriends.SetPersonaState(EPersonaState.Online);//Set state to online.
//        }//OnTradeClose()

//        /// <summary>
//        /// Triggered when the user sends the bot a chat message.
//        /// </summary>
//        /// <param name="message">message received from the user</param>
//        /// <param name="type">Type of message sent.</param>
//        public override void OnMessage(string message, EChatEntryType type)
//        {
//            throw new NotImplementedException();
//        }//OnMessage()


















//        /// <summary>
//        /// Used to send the friend a message when they add the bot.
//        /// </summary>
//        public void SendFriendMessage()
//        {
//            Thread.Sleep(4000);//wait 4 seconds
//            Bot.SteamFriends.SendChatMessage(OtherSID, EChatEntryType.ChatMsg, FRIEND_ADDED_MESSAGE);//Send message
//            Bot.log.success("Sent welcome message.");//log message sent
//        }//SendFriendMessage()

//        /// <summary>
//        /// Used to count the items the bot has in their inventory before each trade.
//        /// </summary>
//        public void TradeCountInventory()
//        {
//            Schema schema = Trade.CurrentSchema;//get schema

//            Inventory inventory = Trade.MyInventory;//get inventory

//            InventoryKeys = 0;//reset values

//            foreach (Inventory.Item item in inventory.items)
//            {
//                if (schema.GetItem(item.defindex).ItemName == "Mann Co. Supply Crate Key")
//                {
//                    InventoryKeys++;//add key
//                }//if name is a key
//            }//foreach item in inventory

//            if (InventoryKeys == 0)
//            {
//                Trade.SendMessage("I'm sorry.... I'm currently out of keys. Trade back later to see if I get any in stock!");//Send message
//            }//if bot has no keys
//            else
//            {
//                Trade.SendMessage("success! I currently have " + InventoryKeys + " keys.");//send message
//            }//else

//            Bot.log.success("I have " + InventoryKeys + " keys.");//Log inventory status
//        }//TradeCountInventory()

//    }
//}
