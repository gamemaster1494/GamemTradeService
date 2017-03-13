//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading;
//using System.Timers;
//using SteamKit2;
//using SteamTrade;

//namespace SteamBot
//{
//    public class RaffleDonationUserHandler : UserHandler
//    {
//        private System.Timers.Timer inviteMsgTimer = new System.Timers.Timer(clsFunctions.FriendAddedMessageInterval);

//        private List<Inventory.Item> donatedItems = new List<Inventory.Item>();

//        public RaffleDonationUserHandler(Bot bot, SteamID sid)
//            : base(bot, sid)
//        {

//        }

//        #region Overrides of UserHandler

//        /// <summary>
//        /// Called when a the user adds the bot as a friend.
//        /// </summary>
//        /// <returns>
//        /// Whether to accept.
//        /// </returns>
//        public override bool OnFriendAdd()
//        {
//            try
//            {
//                if (SteamRepStatus.GetSteamRepStatus(OtherSID.ConvertToUInt64().ToString()).steamrep.Reputation.SummaryRep.Contains("SCAMMER"))
//                    return false;
//                if (clsFunctions.ScammerList.Contains(OtherSID.ConvertToUInt64()))
//                    return false;
//                Bot.log.Success(Bot.SteamFriends.GetFriendPersonaName(OtherSID) + " (" + OtherSID.ConvertToUInt64() + ") added me!");//Log someone added
//                inviteMsgTimer.Elapsed += (sender, e) => FriendAddMessage(sender, e);
//                inviteMsgTimer.Enabled = true;
//                return true;
//            }
//            catch (Exception ex)
//            {
//                Console.WriteLine(ex.Message);
//                return true;
//            }
//        }

//        public override bool OnClanAdd()
//        {
//            return clsFunctions.DealWithGroupAdd(OtherSID, this.Bot.BotControlClass);
//        }//OnClanAdd()

//        public override void OnFriendRemove()
//        {
//            Bot.log.Warn(String.Format("{0} removed me from friends list", Bot.SteamFriends.GetFriendPersonaName(OtherSID)));
//        }

//        /// <summary>
//        /// Called when user sends bot a message
//        /// </summary>
//        /// <param name="message">message sent</param>
//        /// <param name="type">type of chat used</param>
//        public override void OnMessage(string message, EChatEntryType type)
//        {
//            string BackupMessage = message;
//            if (Bot.TalkingWith != null && !message.StartsWith(".cunlock"))
//            {
//                Bot.SteamFriends.SendChatMessage(Bot.TalkingWith, EChatEntryType.ChatMsg, message);
//                Bot.log.Success("Message sent to " + Bot.SteamFriends.GetFriendPersonaName(Bot.TalkingWith));
//                return;
//            }

//            if (!message.StartsWith(".") && !message.StartsWith("enter"))
//                message = message.ToLower();

//            string messagehandled;

//            #region AdminCommands

//            // ADMIN commands
//            if (IsAdmin)
//            {
//                messagehandled = clsFunctions.DealWithAdminCommand(this.Bot, OtherSID, message);
//                if (messagehandled == String.Empty)
//                {
//                    return;
//                }
//            }//if (IsAdmin)

//            #endregion AdminCommands

//            #region Responces

//            //REGULAR chat commands

//            messagehandled = clsFunctions.DealWithCommand(Bot, OtherSID, message);

//            if (messagehandled == String.Empty)
//            {
//                return;
//            }
//            else
//            {
//                if (Bot.TalkingWith == OtherSID)
//                {
//                    Bot.SteamFriends.SendChatMessage(clsFunctions.BotsOwnerID, type, "Message from " + Bot.SteamFriends.GetFriendPersonaName(OtherSID) + ": " + message);
//                    return;
//                }
//                if (messagehandled != String.Empty)
//                {
//                    string MsgToSend = clsFunctions.DealWithGenericMessage(BackupMessage, OtherSID, Bot);//See if there is a response to the general ItemRemovedMsg
//                    Bot.SteamFriends.SendChatMessage(OtherSID, type, MsgToSend);
//                    if (MsgToSend == "I'm sorry. I'm a nub bot and haven't been taught to respond to that =C")
//                        Bot.log.Warn(String.Format("({0}) {1} sent the unhandled message: {2}", clsFunctions.GetFriendIndex(OtherSID, this.Bot), Bot.SteamFriends.GetFriendPersonaName(OtherSID), messagehandled));
//                }
//            }

//            #endregion Responces
//        }//OnMessage()


//        /// <summary>
//        /// Called whenever a user requests a trade.
//        /// </summary>
//        /// <returns>
//        /// Whether to accept the request.
//        /// </returns>
//        public override bool OnTradeRequest()
//        {
//            if (SteamRepStatus.GetSteamRepStatus(OtherSID.ConvertToUInt64().ToString()).steamrep.Reputation.SummaryRep.Contains("SCAMMER"))
//            {
//                Bot.SteamFriends.RemoveFriend(OtherSID);
//                return false;
//            }
//            if (clsFunctions.ScammerList.Contains(OtherSID.ConvertToUInt64()))
//                return false;

//            Bot.log.Success(Bot.SteamFriends.GetFriendPersonaName(OtherSID) + " (" + OtherSID.ConvertToUInt64() + ") has requested to trade with me!");//show log someone traded            
//            return true;//start trade!

//        }

//        public override void OnTradeError(string error)
//        {
//            Log.Error(error);
//        }

//        public override void OnTradeInit()
//        {
//            Bot.SteamFriends.SetPersonaState(EPersonaState.Busy);
//            Bot.log.Success("Trade started!");
//            donatedItems.Clear();
//            Trade.SendMessage("Success! Please add the items you wish to add to the raffle! Thank you!");
//        }

//        public override void OnTradeAddItem(Schema.Item schemaItem, Inventory.Item inventoryItem)
//        {
//            if (schemaItem.ItemName.Equals("Mann Co. Supply Crate") || schemaItem.ItemName.Equals("Red Summer 2013 Cooler") || schemaItem.ItemName.Equals("Blue Summer 2013 Cooler") || schemaItem.ItemName.Equals("Yellow Summer 2013 Cooler") || schemaItem.ItemName.Equals("Orange Summer 2013 Cooler") || schemaItem.ItemName.Equals("Brown Summer 2013 Cooler") || schemaItem.ItemName.Equals("Black Summer 2013 Cooler") || schemaItem.ItemName.Equals("Summer Claim Check") || schemaItem.ItemName.Equals("Green Summer 2013 Cooler") || schemaItem.ItemName.Equals("Summer Appetizer Crate") || schemaItem.ItemName.Equals("RoboCrate"))
//            {
//                OnTradeClose();
//                Bot.SteamFriends.SendChatMessage(OtherSID, EChatEntryType.ChatMsg, "I will not accept crates.");
//                Bot.log.Warn("User tried to donate crates.");
//                return;
//            }
//            donatedItems.Add(inventoryItem);
//            Bot.log.Success(String.Format("User added the donation of {0} {1} {2} {3}", inventoryItem.IsNotCraftable.ToString().ToLower() == "true" ? "Uncraftable" : "Craftable", clsFunctions.ConvertQualityToString(schemaItem.ItemQuality), schemaItem.ItemName, schemaItem.CraftMaterialType));

//        }

//        public override void OnTradeRemoveItem(Schema.Item schemaItem, Inventory.Item inventoryItem)
//        {
//            donatedItems.Remove(inventoryItem);
//            Bot.log.Warn(String.Format("User removed the donation of {0} {1} {2} {3}", inventoryItem.IsNotCraftable.ToString().ToLower() == "true" ? "Uncraftable" : "Craftable", clsFunctions.ConvertQualityToString(schemaItem.ItemQuality), schemaItem.ItemName, schemaItem.CraftMaterialType));

//        }

//        public override void OnTradeMessage(string message)
//        {
//            Bot.log.Warn("[TRADE MESSAGE] " + message);

//            message = message.ToLower();


//            if (IsAdmin)
//            {
//                clsFunctions.ProcessTradeMessage(message, this.Trade, this.Bot);
//            }//if (ItemRemovedMsg.StartsWith("give") && IsAdmin && ItemRemovedMsg.Length > 5)
//            else
//            {
//                Trade.SendMessage("?");
//            }
//        }

//        public override void OnTradeReady(bool ready)
//        {
//            if (!ready)
//            {
//                Trade.SetReady(false);
//            }
//            else
//            {
//                Trade.SetReady(true);
//                Bot.log.Success("Readying trade...");
//            }
//        }

//        public override void OnTradeAccept()
//        {            
//            bool ok = Trade.AcceptTrade();

//            if (ok)
//            {                
//                Bot.log.Success("Trade was successful!");                
//                Bot.SteamFriends.SendChatMessage(OtherSID, EChatEntryType.ChatMsg, "Thank you for your donation!");
//                clsFunctions.AddRaffleDonations(OtherSID.ConvertToUInt64().ToString(), donatedItems, Bot.GetAPIKey());
//                foreach (Inventory.Item item in donatedItems)
//                {
//                    clsFunctions.DealWithAdminCommand(this.Bot, clsFunctions.BotsOwnerID, String.Format("{0} {1} {2} {3}",clsFunctions.AddRafflePrize, item.IsNotCraftable ? "Uncraftable" : "Craftable", clsFunctions.ConvertQualityToString(Convert.ToInt32(item.Quality)),clsFunctions.schema.GetItem(item.Defindex).ItemName));
//                }                
//            }
//            else
//            {
//                Log.Warn("Trade might have failed.");
//                Bot.SteamFriends.SendChatMessage(OtherSID, EChatEntryType.ChatMsg, "Did something go wrong with the trade? =C");//send ItemRemovedMsg
//            }
//            OnTradeClose();
//        }

//        public override void OnTradeClose()
//        {
//            Bot.SteamFriends.SetPersonaState(EPersonaState.Online);
//            base.OnTradeClose();
//        }

//        #endregion Overrides of UserHandler

//        /// <summary>
//        /// Triggered when timer goes off to send welcome ItemRemovedMsg
//        /// </summary>
//        /// <param name="source">What called it</param>
//        /// <param name="e">Arguments</param>
//        private void FriendAddMessage(object source, ElapsedEventArgs e)
//        {
//            Bot.SteamFriends.SendChatMessage(OtherSID, EChatEntryType.ChatMsg, clsFunctions.RAFFLEDONATION_FRIEND_ADD_BASE_MESSAGE + " " + clsFunctions.RAFFLEDONATION_CURRENT_RAFFLE_DESCRIPTION);//Show welcome ItemRemovedMsg
//            Bot.log.Success("Sent welcome message.");
//            inviteMsgTimer.Enabled = false;//disable timer
//        }//FriendAddMessage()


//    }
//}
