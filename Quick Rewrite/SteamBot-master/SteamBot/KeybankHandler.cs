using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Timers;
using SteamKit2;
using SteamTrade;
using ChatterBotAPI;
using System.Text.RegularExpressions;
using SteamTrade.TradeWebAPI;

namespace SteamBot
{
    public class KeybankHandler : UserHandler
    {
        private ChatterBot chatterBot;

        private ChatterBotSession chatterBotsession;

        private int MAX_RESERVE = clsFunctions.KEYBANK_BUY_KEY_MAX;//used to globalize code

        private int MAX_RESERVE_DONATOR = clsFunctions.KEYBANK_BUY_DONATOR_KEY_MAX;

        private string Item_Type = "keys";

        private string Default_Currency = "ref";
        
        public FriendAddedHandler FriendAddedHandler;

        //public ItemReserveHandler ItemReserveHandler;

        ////currency of the bot. Used in tracking trade values.
        //private TF2Currency Bot.myCurrency = new TF2Currency();

        ////currency of the user. Used in tracking trade values.
        //private TF2Currency Bot.userCurrency = new TF2Currency();  

        private int InventoryRec, InventoryRef, InventoryScrap, InventoryMetal, InventoryKey, UnknownItems, MetalAdded, ScrapAdded, RecAdded, RefAdded, UserRefAdded, UserRecAdded, UserScrapAdded, KeyBotAdded, UserKeyAdded, InventoryBlacklistItem;

        private bool ChooseDonate = false;

        private bool UserAddingMetal = false;

        private List<ulong> KeyIDS = new List<ulong>();

        /// <summary>
        /// Called when a new user handler is created.
        /// </summary>
        /// <param name="bot">Bot assigned to</param>
        /// <param name="sid">SteamID of the other user</param>
        public KeybankHandler(Bot bot, SteamID sid)
            : base(bot, sid) 
        {
            chatterBot = clsFunctions.factory.Create(ChatterBotType.PANDORABOTS, "b0dafd24ee35a477");
            chatterBotsession = chatterBot.CreateSession();
            this.FriendAddedHandler = new SteamBot.FriendAddedHandler(bot, bot.BotControlClass.Substring(9), OtherSID);
            bot.GetInventory();
        
        }//KeybankHandler()


        public override void OnLoginCompleted()
        {

        }

        public override void OnTradeSuccess()
        {

        }

        public override void OnTradeTimeout()
        {

        }

        //GLOBALIZED
        /// <summary>
        /// Triggered when the bot gets a Group Invite
        /// </summary>
        /// <returns>To accept or not</returns>
        public override bool OnGroupAdd()
        {
            return clsFunctions.DealWithGroupAdd(OtherSID, this.Bot.BotControlClass);
        }//OnClanAdd()

        //GLOBALIZED
        /// <summary>
        /// Triggers when a user adds the bot
        /// </summary>
        /// <returns>true to accept invite, false if not.</returns>
        public override bool OnFriendAdd()
        {
            try
            {
                if (SteamRepStatus.GetSteamRepStatus(OtherSID.ConvertToUInt64().ToString()).steamrep.Reputation.SummaryRep.Contains("SCAMMER"))
                    return false;
                if (clsFunctions.ScammerList.Contains(OtherSID.ConvertToUInt64()))
                    return false;
            }
            catch
            {
                Bot.log.Success(Bot.SteamFriends.GetFriendPersonaName(OtherSID) + " (" + OtherSID.ConvertToUInt64() + ") added me!");//show someone added the bot
                this.FriendAddedHandler.Start();
                return true;
            }
            Bot.log.Success(Bot.SteamFriends.GetFriendPersonaName(OtherSID) + " (" + OtherSID.ConvertToUInt64() + ") added me!");//show someone added the bot
            this.FriendAddedHandler.Start();
            return true;//accept friend
        }//OnFriendAdd()

        //GLOBALIZED
        /// <summary>
        /// Triggers when a user removes the bot from friends list
        /// </summary>
        public override void OnFriendRemove()
        {
            if (Bot.TalkingWith == OtherSID)
            {
                Bot.TalkingWith = null;//set to null as it cannot talk with the user anymore.
            }//if (Bot.TalkingWith == OtherSID)
            Bot.log.Warn(String.Format("{0} removed me from friends list", Bot.SteamFriends.GetFriendPersonaName(OtherSID)));//show someone removed me
        }//OnFriendRemove()

        //GLOBALIZED
        /// <summary>
        /// Called when user sends bot a ItemRemovedMsg
        /// </summary>
        /// <param name="ItemRemovedMsg">ItemRemovedMsg sent</param>
        /// <param name="type">type of ItemRemovedMsg</param>
        public override void OnMessage(string message, EChatEntryType type)
        {
            string BackupMessage = message;
            if (!message.StartsWith(".") && !message.StartsWith("enter"))
            {
                message = message.ToLower();//lowercase ItemRemovedMsg
            }//if (!ItemRemovedMsg.StartsWith(".") && !ItemRemovedMsg.StartsWith("enter"))

            string MessageHandled;//the sConversionResult of dealing with the ItemRemovedMsg

            #region AdminCommands

            // ADMIN commands
            if (IsAdmin)
            {
                MessageHandled = clsFunctions.DealWithAdminCommand(this.Bot, OtherSID, message);//Deal with the ItemRemovedMsg, or receive something back if the bot needs specific things to do.
                if (MessageHandled == String.Empty)
                {
                    return;//message was handled like it should of been, so stop code.
                }//if (MessageHandled == String.Empty)

                if (MessageHandled == clsFunctions.AdvertiseCMD)
                {
                    if (Bot.AdvertiseHandler.bStarted)
                    {
                        Bot.SteamFriends.SendChatMessage(OtherSID, type, "Stopping Advertisements...");
                        Bot.log.Success("Stopping Advertisements.");
                        Bot.AdvertiseHandler.Stop();
                    }
                    else
                    {
                        Bot.SteamFriends.SendChatMessage(OtherSID, type, "Starting Advertisements.");
                        Bot.log.Success("Starting Advertisements.");
                        Bot.AdvertiseHandler.Start();
                    }
                    return;
                }
                else if (MessageHandled == clsFunctions.MetalCountCMD)
                {
                    Bot.informHandler.Start();//count bots inventory
                    Bot.SteamFriends.SendChatMessage(OtherSID, type, Bot.informHandler.AdminStatsMessage());//Send the results
                    return;//stop code.
                }// else if (MessageHandled == clsFunctions.MetalCountCMD)
            }//if (IsAdmin)

            #endregion AdminCommands

            #region Responces

            //since admin commands were not handled or user is not an admin, check regular commands.
            MessageHandled = clsFunctions.DealWithCommand(Bot, OtherSID, message);//Get command results

            if (MessageHandled == String.Empty)
            {
                return;//message was handled in clsFunctions, so we can stop the code.
            }//if (MessageHandled == String.Empty)
            else if (MessageHandled == clsFunctions.UserClearCMD)
            {
                Bot.SteamFriends.SendChatMessage(OtherSID, type, "You don't have any items reserved...");
                return;
            }//else if (MessageHandled == clsFunctions.UserClearCMD)
            else if (MessageHandled == clsFunctions.UserDonateCMD)
            {
                ChooseDonate = true;//user is donating
                return;//stop code
            }//else if (MessageHandled == clsFunctions.UserDonateCMD)
            else if (MessageHandled.StartsWith(Bot.BackpackUrl) && MessageHandled.Length > Bot.BackpackUrl.Length)
            {
                Bot.SteamFriends.SendChatMessage(OtherSID, type, "You don't have to reserve items! Just invite me to trade and add keys or metal to get started.");
                return;
            }//else if (MessageHandled.StartsWith(Bot.BackpackUrl) && MessageHandled.Length > Bot.BackpackUrl.Length)

            else if (MessageHandled == clsFunctions.UserStatusCMD)
            {
                Bot.informHandler.Start();//Start counting inventory
                Bot.SteamFriends.SendChatMessage(OtherSID, type, Bot.informHandler.UserStatsMessage());//Send the stats ItemRemovedMsg for users.
                return;
            }//else if (MessageHandled == clsFunctions.UserStatusCMD)

            else
            {
                if (MessageHandled != String.Empty)
                {
                    string MsgToSend = clsFunctions.DealWithGenericMessage(BackupMessage, OtherSID, Bot);//See if there is a response to the general ItemRemovedMsg
                    //Bot.SteamFriends.SendChatMessage(OtherSID, type, MsgToSend);//Send ItemRemovedMsg response
                    if (MsgToSend == "I'm sorry. I'm a nub bot and haven't been taught to respond to that =C")
                    {
                        try
                        {
                            string BotResponceMessage = chatterBotsession.Think(BackupMessage);
                            int icount = 0;
                            if (BotResponceMessage.Contains("<a href="))
                            {
                                BotResponceMessage = Regex.Replace(BotResponceMessage, @"<a\b[^>]+>([^<]*(?:(?!</a)<[^<]*)*)</a>", "$1");
                            }
                            if (BotResponceMessage.Contains("click here!"))
                            {
                                BotResponceMessage = "Uhhh what?";
                            }
                            if (BotResponceMessage.Contains("http://tinyurl.com/comskybot"))
                            {
                                BotResponceMessage = "By being yourself!";
                            }
                            if (BotResponceMessage.Contains("Please click here to help protect"))
                            {
                                BotResponceMessage = "Bye bye!";
                            }
                            if (BotResponceMessage.Contains("<br>"))
                            {
                                string[] splitVars = { "<br>" };
                                string[] newBotMsg = BotResponceMessage.Split(splitVars, StringSplitOptions.None);
                                BotResponceMessage = newBotMsg[0];
                            }
                            Bot.SteamFriends.SendChatMessage(OtherSID, type, BotResponceMessage);
                        }
                        catch (Exception ex)
                        {
                            Bot.SteamFriends.SendChatMessage(OtherSID, type, "What...?");
                        }
                        //Bot.log.Success("Send bot response message of: " + BotResponceMessage);

                    }//if (MsgToSend == "I'm sorry. I'm a nub bot and haven't been taught to respond to that =C")
                }//if (MessageHandled != String.Empty)
            }//else

            #endregion Responces
        }//OnMessage()

        //GLOBALIZED
        /// <summary>
        /// Triggered when an error occurs in trade.
        /// </summary>
        /// <param name="error">Error description</param>
        public override void OnTradeError(string error)
        {
            Bot.log.Error(error);//log trade error
        }//OnTradeError()

        //GLOBALIZED
        /// <summary>
        /// Triggered when someone invites the bot to trade
        /// </summary>
        /// <returns>True to accept request, false if not</returns>
        public override bool OnTradeRequest()
        {
            try
            {
                if (SteamRepStatus.GetSteamRepStatus(OtherSID.ConvertToUInt64().ToString()).steamrep.Reputation.SummaryRep.Contains("SCAMMER"))
                {
                    Bot.SteamFriends.RemoveFriend(OtherSID);
                    return false;
                }
            }
            catch { }
            if (clsFunctions.ScammerList.Contains(OtherSID.ConvertToUInt64()))
                return false;
            if (Bot.craftHandler.InGame)
            {
                Bot.SteamFriends.SendChatMessage(OtherSID, EChatEntryType.ChatMsg, "Sorry. I'm not available to trade. I'm currently crafting metal. Sorry!");//show error
                return false;//making metal can't trade
            }//if (Bot.craftHandler.InGame)

            Bot.log.Success("(" + clsFunctions.GetFriendIndex(OtherSID, this.Bot) + ") " + Bot.SteamFriends.GetFriendPersonaName(OtherSID) + " (" + OtherSID.ConvertToUInt64() + ") has requested to trade with me!");//show log someone traded
            return true;//start trade!
        }//OnTradeRequest()

        //GLOBALIZED
        /// <summary>
        /// Called when the trade initializes
        /// </summary>
        public override void OnTradeInit()
        {
            Bot.SteamFriends.SetPersonaState(EPersonaState.Busy);//set status to busy
            Bot.log.Success("Trade started!");//log trade started
            Trade.SendMessage("Please wait while I load inventories...");//tell user to wait

            ReInit();//Reset vars
            TradeCountInventory(ChooseDonate);//count trade

            if (ChooseDonate && IsAdmin)
            {
                OnTradeMessage("add blacklist");//add blacklisted items
                OnTradeMessage("add unknown");//add unknown items
            }//if (ChooseDonate && IsAdmin)
        }//OnTradeInit()

        public override void OnTradeAddItem(Schema.Item schemaItem, Inventory.Item inventoryItem)
        {
            if (schemaItem == null || inventoryItem == null)
            {
                Trade.CancelTrade();
                Bot.SteamFriends.SendChatMessage(OtherSID, EChatEntryType.ChatMsg, "I'm sorry. I believe SteamAPI is down. Please try again to trade in a few minutes.");
                Bot.log.Warn("Issue getting inventory item. API down? Closing trade.");
                return;
            }
            if (schemaItem.ItemName == "Mann Co. Supply Crate Key" || inventoryItem.Defindex == 5021)
            {
                UserKeyAdded++;
                Bot.log.Success("User added a key");
            }                                
            else if (schemaItem.CraftMaterialType == "craft_bar")
            {
                switch (inventoryItem.Defindex)
                {
                    case 5000:
                        Bot.log.Success("User added a scrap metal.");
                        UserScrapAdded++;
                        Bot.userCurrency.AddScrap();
                        break;

                    case 5001:
                        Bot.log.Success("User added a reclaimed metal.");
                        UserRecAdded++;
                        Bot.userCurrency.AddRec();
                        break;

                    case 5002:
                        Bot.log.Success("User added a refined metal.");
                        UserRefAdded++;
                        Bot.userCurrency.AddRef();
                        break;
                }
            }
            else
            {
                Trade.SendMessage(String.Format("{0} is not a hat I will pay for!", schemaItem.ItemName));
                Bot.log.Warn(String.Format("User added non hat item: {0} {1} {2} {3}", inventoryItem.IsNotCraftable.ToString().ToLower() == "true" ? "Uncraftable" : "Craftable", clsFunctions.ConvertQualityToString(schemaItem.ItemQuality), schemaItem.ItemName, schemaItem.CraftMaterialType));
            }
            Test();
        }

        public override void OnTradeRemoveItem(Schema.Item schemaItem, Inventory.Item inventoryItem)
        {
            if (schemaItem.ItemName == "Mann Co. Supply Crate Key" || inventoryItem.Defindex == 5021)
            {
                UserKeyAdded--;
                Bot.log.Warn("User removed a key");
            }
            else if (schemaItem.CraftMaterialType == "craft_bar")
            {
                switch (inventoryItem.Defindex)
                {
                    case 5000:
                        Bot.log.Warn("User removed a scrap metal.");
                        UserScrapAdded--;
                        Bot.userCurrency.RemoveScrap();
                        break;

                    case 5001:
                        Bot.log.Warn("User removed a reclaimed metal.");
                        UserRecAdded--;
                        Bot.userCurrency.RemoveRec();
                        break;

                    case 5002:
                        Bot.log.Warn("User removed a refined metal.");
                        UserRefAdded--;
                        Bot.userCurrency.RemoveRef();
                        break;
                }
            }
            else
            {
                Trade.SendMessage(String.Format("{0} is not a item I will accept!", schemaItem.ItemName));
                Bot.log.Warn(String.Format("User added non key or metal item: {0} {1} {2} {3}", inventoryItem.IsNotCraftable.ToString().ToLower() == "true" ? "Uncraftable" : "Craftable", clsFunctions.ConvertQualityToString(schemaItem.ItemQuality), schemaItem.ItemName, schemaItem.CraftMaterialType));
            }
            Test();
        }

        public override void OnTradeClose()
        {
            ChooseDonate = false;
            base.OnTradeClose();//close the trade
            Bot.SteamFriends.SetPersonaState(EPersonaState.Online);
            Bot.CheckBackpack();
            Bot.informHandler.Start();
            
        }//OnTradeClose()

        public override void OnTradeMessage(string message)
        {
            Bot.log.Warn("[TRADE MESSAGE] " + message);

            message = message.ToLower();

            if (message == "donate" && IsAdmin)
            {
                ChooseDonate = true;
                Trade.SendMessage("donating");
                return;
            }

            else if (message.Contains("backpack"))
            {
                Trade.SendMessage("No need to reserve keys! Just add enough metal!");
                return;
            }

            if (IsAdmin && message == "redo")
            {
                Test();
            }

            if (IsAdmin && message == "info")
            {
                Trade.SendMessage(UserKeyAdded.ToString());
            }
            if (IsAdmin)
            {
                clsFunctions.ProcessTradeMessage(message, this.Trade, this.Bot);
            }//if (ItemRemovedMsg.StartsWith("give") && IsAdmin && ItemRemovedMsg.Length > 5)
        }

        public override void OnTradeReady(bool ready)
        {
            if (!ready)
            {
                Trade.SetReady(false);//don't ready up
            }//if (!ready)
            else
            {
                Bot.log.Success("User is ready to trade!");//user is ready to trade
                if (Validate())
                {
                    Bot.log.Success("Readying trade.");
                    Trade.SetReady(true);
                }
                else
                {
                    Bot.log.Warn("Invalid trade!");
                }
            }
        }

        public override void OnTradeAccept()
        {
            if (Validate())
            {
                Bot.log.Success("Accepting trade...");
                bool success;
                lock (this)
                {
                    success = Trade.AcceptTrade();//see if trade went through
                }
                if (success)
                {
                    Log.Success("Trade was successful!");//log
                    Bot.SteamFriends.SendChatMessage(OtherSID, EChatEntryType.ChatMsg, clsFunctions.KEYBANK_TRADE_COMPLETED_MESSAGE);
                    clsFunctions.AddToTradeNumber(this.Bot);
                    //Random rnd = new Random();
                    //if (rnd.Next(1, 10) <= 2)
                    //{
                    //    Bot.SteamFriends.SendChatMessage(OtherSID, EChatEntryType.ChatMsg, "If you are part of the Gamem Trading Services group, code: KeysMate may mean something to you... PLEASE DO NOT SHARE THIS!");
                    //}
                }//if (success)
                else
                {
                    Log.Warn("Trade might have failed.");//log
                    Bot.SteamFriends.SendChatMessage(OtherSID, EChatEntryType.ChatMsg, "Did something go wrong with the trade? =C");//send ItemRemovedMsg
                }//else
            }//if (Validate())
            OnTradeClose();
        }

        private void OnReserveElapsed(object source, ElapsedEventArgs e)
        {
            Bot.SteamFriends.SendChatMessage(OtherSID, EChatEntryType.ChatMsg, "I'm sorry. You haven't traded for your items in 10 minutes. I have removed them.");
            Bot.UnReserveAllByUser(OtherSID);
        }

        public void ReInit()
        {
            Bot.myCurrency.Clear();
            Bot.userCurrency.Clear();
            KeyIDS.Clear();
            InventoryRec = 0;
            InventoryRef = 0;            
            InventoryScrap = 0;
            RefAdded = 0;
            RecAdded = 0;
            UserScrapAdded = 0;
            UserRecAdded = 0;
            UserRefAdded = 0;
            KeyBotAdded = 0;
            UserKeyAdded = 0;
            InventoryKey = 0;
            MetalAdded = 0;
        }

        public void TradeCountInventory(bool message)
        {
            // Let's count our inventory
            Schema schema = Trade.CurrentSchema;

            //Bot.MyInventory = Trade.MyInventory;

            foreach (Inventory.Item item in Bot.MyInventory.Items) 
            {
                if (item.IsNotCraftable && !schema.GetItem(item.Defindex).ItemName.Contains("Mann Co. Supply Crate Key") || item.IsNotTradeable)
                {

                }
                else if (item.Defindex == 5000)
                {
                    InventoryMetal++;
                    InventoryScrap++;
                   
                }
                else if (item.Defindex == 5001)
                {
                    InventoryMetal += 3;
                    InventoryRec++;
                }
                else if (item.Defindex == 5002)
                {
                    InventoryMetal += 9;
                    InventoryRef++;
                }
                else if (schema.GetItem(item.Defindex).ItemName.Contains("Mann Co. Supply Crate Key") || item.Defindex == 5021)
                {
                    InventoryKey++;
                    KeyIDS.Add(item.Id);
                }
            }
            TF2Currency tempCurn = new TF2Currency(0, InventoryRef, InventoryRec, InventoryScrap);
            
            if (!ChooseDonate)
            {
                Trade.SendMessage("Success! I can currently buy " + tempCurn.ToKeys() + " keys, and have " + InventoryKey + " keys for sale.");
            }
            else
            {
                Trade.SendMessage("Success! Please add the items you wish to donate. Thank you ^^.");
            }
            Bot.log.Success(String.Format("I have {0} ref {1} rec {2} scrap and {3} keys.", InventoryRef, InventoryRec, InventoryScrap, InventoryKey));
            
        }

        public bool Validate()
        {
            List<string> errors = new List<string>();
            Schema schema = Trade.CurrentSchema;
            int iCheckedKeys = 0;
            int iCheckedScrap = 0;
            int iCheckedRec = 0;
            int iCheckedRef = 0;
            if (ChooseDonate)
            {
                return true;//trade is fine
            }//if (ChooseDonate)

            List<Inventory.Item> items = new List<Inventory.Item>();

            foreach (TradeUserAssets id in Trade.OtherOfferedItems)
            {
                if (id.appid == 440)
                    items.Add(Trade.OtherInventory.GetItem(id.assetid));//Get item
            }//foreach (ulong id in Trade.OtherOfferedItems)

            foreach (Inventory.Item item in items)
            {
                Schema.Item newitem = schema.GetItem(item.Defindex);
                if (newitem.Defindex == 5000 && KeyBotAdded > 0)
                {
                    iCheckedScrap++;
                }
                else if (newitem.Defindex == 5001 && KeyBotAdded > 0)
                {
                    iCheckedRec++;
                }
                else if (newitem.Defindex == 5002 && KeyBotAdded > 0)
                {
                    iCheckedRef++;
                }
                else if (newitem.ItemName == "Mann Co. Supply Crate Key")
                {
                    iCheckedKeys++;
                }
                else if (newitem.ItemName.Contains("Crate") && !newitem.ItemName.Contains("Key"))
                {
                    errors.Add("" + newitem.ItemName + " will not be accepted!");
                }
            }

            if (iCheckedScrap != UserScrapAdded)
            {
                Bot.log.Warn("Scrap count in trade didn't match ones in the trade vars.");
                Bot.SteamFriends.SendChatMessage(OtherSID, EChatEntryType.ChatMsg, "Sorry. Something went wrong in trade. Please trade me again.");
                Trade.CancelTrade();
                return false;
            }
            if (iCheckedRec != UserRecAdded)
            {
                Bot.log.Warn("Rec count in trade didn't match ones in the trade vars.");
                Bot.SteamFriends.SendChatMessage(OtherSID, EChatEntryType.ChatMsg, "Sorry. Something went wrong in trade. Please trade me again.");
                Trade.CancelTrade();
                return false;
            }
            if (iCheckedRef != UserRefAdded)
            {
                Bot.log.Warn("Ref count in trade didn't match ones in the trade vars.");
                Bot.SteamFriends.SendChatMessage(OtherSID, EChatEntryType.ChatMsg, "Sorry. Something went wrong in trade. Please trade me again.");
                Trade.CancelTrade();
                return false;                
            }
            if (iCheckedKeys != UserKeyAdded)
            {
                Bot.log.Warn("Key count in trade didn't match ones in the trade vars.");
                Bot.SteamFriends.SendChatMessage(OtherSID, EChatEntryType.ChatMsg, "Sorry. Something went wrong in trade. Please trade me again.");
                Trade.CancelTrade();
                return false;  
            }

            TF2Currency change = Bot.userCurrency.GetChange(Bot.myCurrency);

            Test();
            if (errors.Count > 0)
            {
                Trade.SendMessage("There are errors in your trade:");
                foreach (string error in errors)
                {
                    Trade.SendMessage(error);
                    Bot.log.Warn(String.Format("Trade Validation error: {0} with user {1}", error, Bot.SteamFriends.GetFriendPersonaName(Trade.OtherSID)));
                }
            }

            return errors.Count == 0;
        }

        public void Test()
        {
            if (UserKeyAdded > 0)
            {
                //they are selling keys
                //check amount of keys user added (make into scrap)
                //see if currencies are the same
                //+/- if not
                TF2Currency tempCurn = new TF2Currency(0,0,0,(int)clsFunctions.KEY_BUY_VALUE.ToScrap() * UserKeyAdded);

                string TempCurnDebug = String.Format("UserStuff Ref Change {0} Rec Change {1} Scrap Change {2} Key Buy Price {3} Key Sell Price {4}", tempCurn.Refined, tempCurn.Reclaimed, tempCurn.Scrap, clsFunctions.KEY_BUY_VALUE.ToPrice(), clsFunctions.KEY_SELL_VALUE.ToPrice());

                TF2Currency change = tempCurn.GetChange(Bot.myCurrency);

                string DebugFormat = String.Format("Ref Change {0} Rec Change {1} Scrap Change {2} Key Buy Price {3} Key Sell Price {4}", change.Refined, change.Reclaimed, change.Scrap, clsFunctions.KEY_BUY_VALUE.ToPrice(), clsFunctions.KEY_SELL_VALUE.ToPrice());

                Bot.log.Success(DebugFormat);

                if (change.Neutral())
                {

                }
                else if (change.Positive())
                {
                    //+ change means user is overpaying and bot needs to add correct change.
                    #region Refined Adding

                    if (change.Refined > 0)
                    {
                        if (InventoryRef - Bot.myCurrency.Refined >= change.Refined)
                        {
                            uint RefAdded = Trade.AddAllItemsByDefindex(5002, (uint)change.Refined);
                            Bot.myCurrency.AddRef((int)RefAdded);
                            if (RefAdded == change.Refined)
                            {
                                Bot.log.Success(String.Format("Added {0} refined.", RefAdded));
                            }
                        }
                        else
                        {
                            Trade.SendMessage("I'm sorry. I don't seem to have enough refined!");
                        }
                    }

                    #endregion

                    #region Reclaimed Adding

                    if (change.Reclaimed > 0)
                    {
                        if (InventoryRec - Bot.myCurrency.Reclaimed >= change.Reclaimed)
                        {
                            if (Bot.myCurrency.Reclaimed == 2)
                            {
                                if (InventoryRef - Bot.myCurrency.Refined >= 1)
                                {
                                    uint rectoremove = 2;
                                    if (change.Reclaimed == 2)
                                        rectoremove = 1;
                                    uint RecRemoved = Trade.RemoveAllItemsByDefindex(5001, rectoremove);
                                    Bot.myCurrency.RemoveRec((int)RecRemoved);
                                    if (RecRemoved == rectoremove)
                                    {
                                        Bot.log.Success(String.Format("Removed {0} reclaimed", RecRemoved));
                                        uint RefAdded = Trade.AddAllItemsByDefindex(5002, 1);

                                        if (RefAdded == 1)
                                        {
                                            Bot.myCurrency.AddRef();
                                            Bot.log.Success("Added 1 refined.");
                                            change.RemoveRec();
                                        }
                                        else
                                        {
                                            Trade.CancelTrade();
                                            Bot.SteamFriends.SendChatMessage(OtherSID, EChatEntryType.ChatMsg, "Sorry. Something went wrong. Please trade me again.");
                                            Bot.log.Error("Something went wrong removing 2 rec and adding a ref(adding ref part)");
                                        }
                                    }
                                }
                                else
                                {
                                    uint RecAdded = Trade.AddAllItemsByDefindex(5001, (uint)change.Reclaimed);
                                    Bot.myCurrency.AddRec((int)RecAdded);
                                    if (RecAdded == change.Reclaimed)
                                    {
                                        Bot.log.Success(String.Format("Added {0} rec", RecAdded));

                                    }
                                    else
                                    {
                                        Trade.CancelTrade();
                                        Bot.SteamFriends.SendChatMessage(OtherSID, EChatEntryType.ChatMsg, "Sorry. Something went wrong. Please trade me again.");
                                        Bot.log.Error("Couldn't add reclaimed");
                                    }
                                }
                            }
                            else
                            {
                                uint RecAdded = Trade.AddAllItemsByDefindex(5001, (uint)change.Reclaimed);
                                Bot.myCurrency.AddRec((int)RecAdded);
                                if (RecAdded == change.Reclaimed)
                                {
                                    Bot.log.Success(String.Format("Added {0} rec", RecAdded));
                                }
                                else
                                {
                                    Trade.CancelTrade();
                                    Bot.SteamFriends.SendChatMessage(OtherSID, EChatEntryType.ChatMsg, "Sorry. Something went wrong. Please trade me again.");
                                    Bot.log.Error("Couldn't add reclaimed");
                                }
                            }
                        }
                        else
                        {
                            Trade.SendMessage("Sorry. I do not have enough reclaimed!");
                        }
                    }
                    #endregion

                    #region Scrap Adding
                    if (change.Scrap > 0)
                    {
                        uint ScrapAdded = Trade.AddAllItemsByDefindex(5000, (uint)change.Scrap);
                        Bot.myCurrency.AddScrap((int)ScrapAdded);
                        if (ScrapAdded == change.Scrap)
                        {
                            Bot.log.Success(String.Format("Added {0} scrap", ScrapAdded));
                        }
                        else
                        {
                            Trade.CancelTrade();
                            Bot.SteamFriends.SendChatMessage(OtherSID, EChatEntryType.ChatMsg, "Sorry. Something went wrong. Please trade me again.");
                            Bot.log.Error("Couldn't add scrap");
                        }
                    }
                    #endregion
                }
                else if (change.Negative())
                {
                    change.MakePositive();
                    //- change means bot needs to remove items.

                    #region Refined Removing
                    if (change.Refined > 0)
                    {
                        if (Bot.myCurrency.Refined >= change.Refined)
                        {
                            uint RefRemoved = Trade.RemoveAllItemsByDefindex(5002, (uint)change.Refined);
                            Bot.myCurrency.RemoveRef((int)RefRemoved);
                            if (RefRemoved == change.Refined)
                            {
                                Bot.log.Success(String.Format("Removed {0} refined", RefRemoved));
                            }
                            else
                            {
                                Trade.CancelTrade();
                                Bot.SteamFriends.SendChatMessage(OtherSID, EChatEntryType.ChatMsg, "Sorry. Something went wrong. Please trade me again.");
                                Bot.log.Error("Something went wrong removing ref");
                            }
                        }
                        else
                        {
                            Trade.CancelTrade();
                            Bot.SteamFriends.SendChatMessage(OtherSID, EChatEntryType.ChatMsg, "Sorry. Something went wrong. Please trade me again.");
                            Bot.log.Error("Something went wrong removing ref");
                        }
                    }
                    #endregion

                    #region Reclaimed Removeing
                    if (change.Reclaimed > 0)
                    {
                        if (Bot.myCurrency.Reclaimed >= change.Reclaimed)
                        {
                            uint RecRemoved = Trade.RemoveAllItemsByDefindex(5001, (uint)change.Reclaimed);
                            Bot.myCurrency.RemoveRec((int)RecRemoved);
                            if (RecRemoved == change.Reclaimed)
                            {
                                Bot.log.Success(String.Format("Removed {0} rec", RecRemoved));
                            }
                            else
                            {
                                Trade.CancelTrade();
                                Bot.SteamFriends.SendChatMessage(OtherSID, EChatEntryType.ChatMsg, "Sorry. Something went wrong. Please trade me again.");
                                Bot.log.Error("Something went wrong removing rec");
                            }
                        }
                        else if (Bot.myCurrency.Refined >= change.Reclaimed / 3)
                        {
                            double reftoremove;
                            if (change.Reclaimed > 3)
                            {
                                reftoremove = Math.Abs(change.Reclaimed) / 3;
                            }
                            else
                            {
                                reftoremove = 1;
                            }
                            reftoremove = Math.Round(reftoremove, 0, MidpointRounding.AwayFromZero);
                            if (Bot.myCurrency.Refined >= reftoremove)
                            {
                                uint RefRemoved = Trade.RemoveAllItemsByDefindex(5002, (uint)reftoremove);
                                Bot.myCurrency.RemoveRef((int)RefRemoved);
                                if (RefRemoved == reftoremove)
                                {
                                    Bot.log.Success(String.Format("Removed {0} ref", RefRemoved));                                    
                                    int rectoadd = ((int)RefRemoved * 3) - change.Reclaimed;
                                    uint recadded = Trade.AddAllItemsByDefindex(5001, (uint)rectoadd);
                                    Bot.myCurrency.AddRec(rectoadd);
                                    if (recadded == rectoadd)
                                    {
                                        Bot.log.Success(String.Format("Added {0} rec.", recadded));
                                    }
                                }
                                else
                                {
                                    Trade.CancelTrade();
                                    Bot.SteamFriends.SendChatMessage(OtherSID, EChatEntryType.ChatMsg, "Sorry. Something went wrong. Please trade me again.");
                                    Bot.log.Error("Something went wrong removing ref");
                                }
                            }
                        }
                    }
                    #endregion

                    #region Scrap Removing
                    
                    if (change.Scrap > 0)
                    {
                        uint ScrapRemoved = Trade.RemoveAllItemsByDefindex(5000, (uint)change.Scrap);
                        Bot.myCurrency.RemoveScrap((int)ScrapRemoved);
                        if (ScrapRemoved == change.Scrap)
                        {
                            Bot.log.Warn(String.Format("Removed {0} scrap", ScrapRemoved));
                        }
                        else
                        {
                            Trade.CancelTrade();
                            Bot.SteamFriends.SendChatMessage(OtherSID, EChatEntryType.ChatMsg, "Sorry. Something went wrong. Please trade me again.");
                            Bot.log.Error("Couldn't remove scrap");
                        }
                    }
                    #endregion
                }
            }
            else if (UserKeyAdded == 0 && (Bot.myCurrency.Refined > 0 || Bot.myCurrency.Reclaimed > 0 || Bot.myCurrency.Scrap > 0))
            {
                uint MetalRemoved = Trade.RemoveAllItemsByDefindex(5002);
                Bot.myCurrency.RemoveRef((int)MetalRemoved);
                MetalRemoved = Trade.RemoveAllItemsByDefindex(5001);
                Bot.myCurrency.RemoveRec((int)MetalRemoved);
                MetalRemoved = Trade.RemoveAllItemsByDefindex(5000);
                Bot.myCurrency.RemoveScrap((int)MetalRemoved);
                Bot.log.Warn("Removed all metal as all keys were removed");
            }
            else
            {
                //they are buying keys
                //check metal they have added
                //divide by key selling price
                //if they have enough for one add (after checking # added

                int iUserKeyAfford = Bot.userCurrency.ToKeys(false);
                int iKeysToAdd = iUserKeyAfford - KeyBotAdded;

                if (iKeysToAdd == 0)
                {
                    
                }
                else if (iKeysToAdd > 0)
                {
                    while (iKeysToAdd > 0)
                    {
                        try
                        {
                            if (Trade.AddItem(KeyIDS[KeyBotAdded]))
                            {
                                KeyBotAdded++;
                                Bot.log.Success("Added a key.");
                                iKeysToAdd--;
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                            Trade.SendMessage("I don't have enough keys!");
                            iKeysToAdd = 0;
                        }
                    }
                }
                else if (iKeysToAdd < 0)
                {
                    while (iKeysToAdd < 0)
                    {
                        try
                        {
                            if (Trade.RemoveItem(KeyIDS[KeyBotAdded--]))
                            {
                                KeyBotAdded--;
                                Bot.log.Success("Removed a key");
                                iKeysToAdd++;
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                            Trade.CancelTrade();
                            Bot.SteamFriends.SendChatMessage(OtherSID, EChatEntryType.ChatMsg, "I'm sorry. Something went wrong. Please try again.");
                            iKeysToAdd = 0;
                        }
                    }
                }
            }
        }
    }
}