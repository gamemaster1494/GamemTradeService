﻿//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Timers;
//using SteamKit2;
//using SteamTrade;

//namespace SteamBot
//{
//    public class OneScrapUserHandler : UserHandler
//    {
//        private int MAX_RESERVE = clsFunctions.ONESCRAPBANK_MAX_RESERVE;//used to globalize code

//        private int MAX_RESERVE_DONATOR = clsFunctions.ONESCRAPBANK_DONATOR_RESERVE;//used to globalize code        

//        private string Item_Type = "items";//Used to globalize code

//        private string Default_Currency = "scrap";//used to globalize code

//        public FriendAddedHandler FriendAddedHandler;//Friend Added Handler

//        public ItemReserveHandler ItemReserveHandler;//Item reserve handler

//        private List<Inventory.Item> donatedItems = new List<Inventory.Item>();//items donates

//        //private TF2Currency Bot.myCurrency = new TF2Currency();//Bots currency

//        //private TF2Currency Bot.userCurrency = new TF2Currency();//Users currency

//        private int InventoryRec, InventoryRef, InventoryScrap, InventoryMetal, InventoryItems, BotItemsAdded;

//        private int ItemsReserved = 0;

//        private int ItemsBotAdded = 0;

//        private bool ChooseDonate = false;

//        /// <summary>
//        /// called when bot is made with this handle
//        /// </summary>
//        /// <param name="bot">bot assigned to</param>
//        /// <param name="sid">SteamID of the other user</param>
//        public OneScrapUserHandler(Bot bot, SteamID sid)
//            : base(bot, sid)
//        {
//            this.FriendAddedHandler = new SteamBot.FriendAddedHandler(bot, bot.BotControlClass.Substring(9), OtherSID);//Initialize
//            this.ItemReserveHandler = new SteamBot.ItemReserveHandler(bot, bot.BotControlClass.Substring(9), OtherSID);//Initialize
//            this.ItemReserveHandler.OnElapsed += this.OnReserveElapsed;//Set trigger method
//            bot.GetInventory();//Get Inventory
//        }//OneScrapUserHandler()

//        //GLOBALIZED
//        /// <summary>
//        /// Triggered when the bot gets a Group Invite
//        /// </summary>
//        /// <returns>True to accept invite, False to decline invite.</returns>
//        public override bool OnGroupAdd()
//        {
//            return clsFunctions.DealWithGroupAdd(OtherSID, this.Bot.BotControlClass);
//        }//OnClanAdd()


//        public override void OnLoginCompleted()
//        {

//        }

//        public override void OnTradeSuccess()
//        {

//        }

//        public override void OnTradeTimeout()
//        {

//        }

//        //GLOBALIZED
//        /// <summary>
//        /// Triggers when a user adds the bot
//        /// </summary>
//        /// <returns>true to accept invite, false if not.</returns>
//        public override bool OnFriendAdd()
//        {
//            try
//            {
//                if (SteamRepStatus.GetSteamRepStatus(OtherSID.ConvertToUInt64().ToString()).steamrep.Reputation.SummaryRep.Contains("SCAMMER"))
//                    return false;
//                if (clsFunctions.ScammerList.Contains(OtherSID.ConvertToUInt64()))
//                    return false;
//            }
//            catch
//            {
//                Bot.log.Success(Bot.SteamFriends.GetFriendPersonaName(OtherSID) + " (" + OtherSID.ConvertToUInt64() + ") added me!");//show someone added the bot
//                this.FriendAddedHandler.Start();
//                return true;
//            }
//            Bot.log.Success(Bot.SteamFriends.GetFriendPersonaName(OtherSID) + " (" + OtherSID.ConvertToUInt64() + ") added me!");//show someone added the bot
//            this.FriendAddedHandler.Start();
//            return true;//accept friend
//        }//OnFriendAdd()

//        //GLOBALIZED
//        /// <summary>
//        /// Triggers when a user removes the bot from friends list
//        /// </summary>
//        public override void OnFriendRemove()
//        {
//            if (ItemsReserved > 0)
//            {
//                Bot.UnReserveAllByUser(OtherSID);//Remove items user had reserved
//                ItemsReserved = 0;//reset weapons reserved
//            }//if (ItemsReserved > 0)
//            if (Bot.TalkingWith == OtherSID)
//            {
//                Bot.TalkingWith = null;//set to null as it cannot talk with the user anymore.
//            }//if (Bot.TalkingWith == OtherSID)
//            Bot.log.Warn(String.Format("{0} removed me from friends list", Bot.SteamFriends.GetFriendPersonaName(OtherSID)));//show someone removed me
//        }//OnFriendRemove()

//        //GLOBALIZED
//        /// <summary>
//        /// Called when user sends bot a ItemRemovedMsg
//        /// </summary>
//        /// <param name="ItemRemovedMsg">ItemRemovedMsg sent</param>
//        /// <param name="type">type of ItemRemovedMsg</param>
//        public override void OnMessage(string message, EChatEntryType type)
//        {
//            string BackupMessage = message;
//            if (message == "removed_item")
//            {
//                if (ItemsReserved > 0)
//                {
//                    ItemsReserved--;//remove weapon
//                }//if (ItemsReserved > 0)
//                return;//stop going due to this being a bot internal command
//            }//if (ItemRemovedMsg == "removed_item")

//            if (!message.StartsWith(".") && !message.StartsWith("enter"))
//            {
//                message = message.ToLower();//lowercase ItemRemovedMsg
//            }//if (!ItemRemovedMsg.StartsWith(".") && !ItemRemovedMsg.StartsWith("enter"))

//            string MessageHandled;//the sConversionResult of dealing with the ItemRemovedMsg

//            #region AdminCommands

//            // ADMIN commands
//            if (IsAdmin)
//            {
//                MessageHandled = clsFunctions.DealWithAdminCommand(this.Bot, OtherSID, message);//Deal with the ItemRemovedMsg, or receive something back if the bot needs specific things to do.
//                if (MessageHandled == String.Empty)
//                {
//                    return;//message was handled like it should of been, so stop code.
//                }//if (MessageHandled == String.Empty)

//                if (MessageHandled == clsFunctions.AdvertiseCMD)
//                {
//                    if (Bot.AdvertiseHandler.bStarted)
//                    {
//                        Bot.SteamFriends.SendChatMessage(OtherSID, type, "Stopping Advertisements...");
//                        Bot.log.Success("Stopping Advertisements.");
//                        Bot.AdvertiseHandler.Stop();
//                    }
//                    else
//                    {
//                        Bot.SteamFriends.SendChatMessage(OtherSID, type, "Starting Advertisements.");
//                        Bot.log.Success("Starting Advertisements.");
//                        Bot.AdvertiseHandler.Start();
//                    }
//                    return;
//                }
//                else if (MessageHandled == clsFunctions.MetalCountCMD)
//                {
//                    Bot.informHandler.Start();//count bots inventory
//                    Bot.SteamFriends.SendChatMessage(OtherSID, type, Bot.informHandler.AdminStatsMessage());//Send the results
//                    return;//stop code.
//                }// else if (MessageHandled == clsFunctions.MetalCountCMD)
//            }//if (IsAdmin)

//            #endregion AdminCommands

//            #region Responces

//            //since admin commands were not handled or user is not an admin, check regular commands.
//            MessageHandled = clsFunctions.DealWithCommand(Bot, OtherSID, message);//Get command results

//            if (MessageHandled == String.Empty)
//            {
//                return;//message was handled in clsFunctions, so we can stop the code.
//            }//if (MessageHandled == String.Empty)
//            else if (MessageHandled == clsFunctions.UserClearCMD)
//            {
//                Bot.UnReserveAllByUser(OtherSID);//Remove all reserved items from the user
//                ItemsReserved = 0;//reset weapons
//                Bot.SteamFriends.SendChatMessage(OtherSID, type, "Reserved items cleared!");//tell user of success
//                Bot.log.Success(String.Format("{0} cleared their reserved items.", Bot.SteamFriends.GetFriendPersonaName(OtherSID)));//log weapons cleared
//                this.ItemReserveHandler.Stop();
//                return;
//            }//else if (MessageHandled == clsFunctions.UserClearCMD)
//            else if (MessageHandled == clsFunctions.UserDonateCMD)
//            {
//                ChooseDonate = true;//user is donating
//                return;//stop code
//            }//else if (MessageHandled == clsFunctions.UserDonateCMD)
//            else if (MessageHandled.StartsWith(Bot.BackpackUrl) && MessageHandled.Length > Bot.BackpackUrl.Length)
//            {
//                if (!clsFunctions.IsPremiumDonator(OtherSID.ConvertToUInt64().ToString()))
//                {
//                    if (MAX_RESERVE == ItemsReserved)
//                    {
//                        Bot.SteamFriends.SendChatMessage(OtherSID, type, String.Format("Sorry. You can currently reserve {0} {1} at a time. Donation in the future will increase this.)", MAX_RESERVE, Item_Type));//show max they can reserve
//                        return;//stop code
//                    }//if (clsFunctions.SCRAPBANK_MAX_RESERVE == ItemsReserved)
//                }

//                else
//                {
//                    if (MAX_RESERVE_DONATOR == ItemsReserved)
//                    {
//                        Bot.SteamFriends.SendChatMessage(OtherSID, type, String.Format("Sorry. As a donator,  you can only reserve {0} {1}.", MAX_RESERVE_DONATOR, Item_Type));
//                        return;//stop code
//                    }//if (clsFunctions.SCRAPBANK_MAX_RESERVE == ItemsReserved)
//                }
//                string[] sLines = MessageHandled.Split('_');//get item information

//                if (!Bot.dReserved.ContainsKey(Convert.ToUInt64(sLines[2])))
//                {

//                    if (Bot.MyInventory == null)
//                    {
//                        Bot.GetInventory();
//                        if (Bot.MyInventory == null)
//                        {
//                            Bot.SteamFriends.SendChatMessage(OtherSID, type, "I'm sorry. SteamAPI may be down. I cannot seem to retrieve my backpack. Please try again later.");
//                            return;
//                        }
//                    }

//                    Inventory.Item item = Bot.MyInventory.GetItem(Convert.ToUInt64(sLines[2]));//get the item from the bots inventory

//                    if (item == null)
//                    {
//                        Bot.GetInventory();//reload the inventory to make sure
//                        item = Bot.MyInventory.GetItem(Convert.ToUInt64(sLines[2]));//Try to get the item again with the new inventory
//                        if (item == null)
//                        {
//                            Bot.SteamFriends.SendChatMessage(OtherSID, type, "Sorry. I do not have that item anymore. Please refresh my inventory.");//Item already traded. Tell user to refresh.
//                            return;//stop code
//                        }//if (item == null)
//                    }//if (item == null)

//                    if (Bot.dDonated.ContainsKey(item.OriginalId))
//                    {
//                        Bot.SteamFriends.SendChatMessage(OtherSID, type, "I'm sorry. But that item is a donation. Donations cannot be taken until the owner deals with it.");//tell user they can't reserve donation items
//                        Bot.log.Warn("User tried to reserve a donated item.");//log they tried to reserve a donated item
//                        return;//stop code as they can't reserve this item
//                    }//if (Bot.dDonated.Keys.Contains(item.OriginalId))

//                    string sResults;//used to get the sConversionResult ItemRemovedMsg of checking the item

//                    if (CheckBlackList(item, out sResults))
//                    {
//                        Bot.dReserved.Add(Convert.ToUInt64(sLines[2]), OtherSID);//add item to reserved list
//                        ItemsReserved++;//add to weapons reserved
//                        Bot.SteamFriends.SendChatMessage(OtherSID, type, "Item Reserved! Trade when ready.");//tell user item was reserved
//                        Bot.log.Success(String.Format("{0} reserved {1}", Bot.SteamFriends.GetFriendPersonaName(OtherSID), clsFunctions.schema.GetItem(item.Defindex).Name));//log item reserved
//                        this.ItemReserveHandler.Start();
//                    }//if (CheckBlackList(item, out sResults))
//                    else
//                    {
//                        Bot.SteamFriends.SendChatMessage(OtherSID, type, sResults);//send the sConversionResult of checking the item
//                    }//else
//                }//if (!Bot.dReserved.ContainsKey(Convert.ToUInt64(sLines[2])))
//                else
//                {
//                    if (Bot.dReserved[Convert.ToUInt64(sLines[2])].ConvertToUInt64() == OtherSID.ConvertToUInt64())
//                    {
//                        Bot.SteamFriends.SendChatMessage(OtherSID, type, "You have already reserved this item silly!");//they already reserved this item
//                    }//if (Bot.dReserved[Convert.ToUInt64(sLines[2])].ConvertToUInt64() == OtherSID.ConvertToUInt64())
//                    else
//                    {
//                        Bot.SteamFriends.SendChatMessage(OtherSID, type, "Sorry! Someone else has reserved this item...");//someone already reserved this item
//                    }//else
//                }//else
//                return;
//            }//else if (MessageHandled.StartsWith(Bot.BackpackUrl) && MessageHandled.Length > Bot.BackpackUrl.Length)

//            else if (MessageHandled == clsFunctions.UserStatusCMD)
//            {
//                Bot.informHandler.Start();//Start counting inventory
//                Bot.SteamFriends.SendChatMessage(OtherSID, type, Bot.informHandler.UserStatsMessage());//Send the stats ItemRemovedMsg for users.
//                return;
//            }//else if (MessageHandled == clsFunctions.UserStatusCMD)

//            else
//            {
//                if (MessageHandled != String.Empty)
//                {
//                    string MsgToSend = clsFunctions.DealWithGenericMessage(BackupMessage, OtherSID, Bot);//See if there is a response to the general ItemRemovedMsg
//                    Bot.SteamFriends.SendChatMessage(OtherSID, type, MsgToSend);//Send ItemRemovedMsg response
//                    if (MsgToSend == "I'm sorry. I'm a nub bot and haven't been taught to respond to that =C")
//                    {
//                        Bot.log.Warn(String.Format("({0}) {1} sent the unhandled message: {2}", clsFunctions.GetFriendIndex(OtherSID, this.Bot), Bot.SteamFriends.GetFriendPersonaName(OtherSID), MessageHandled));//log unhandled ItemRemovedMsg
//                        clsFunctions.AddUnhandledMessageToList(MessageHandled);
//                    }//if (MsgToSend == "I'm sorry. I'm a nub bot and haven't been taught to respond to that =C")
//                }//if (MessageHandled != String.Empty)
//            }//else

//            #endregion Responces
//        }//OnMessage()

//        //GLOBALIZED
//        /// <summary>
//        /// Triggered when an error occurs in trade.
//        /// </summary>
//        /// <param name="error">Error description</param>
//        public override void OnTradeError(string error)
//        {
//            Bot.log.Error(error);//log trade error
//        }//OnTradeError()

//        //GLOBALIZED
//        /// <summary>
//        /// Triggered when someone invites the bot to trade
//        /// </summary>
//        /// <returns>True to accept request, false if not</returns>
//        public override bool OnTradeRequest()
//        {
//            try
//            {
//                if (SteamRepStatus.GetSteamRepStatus(OtherSID.ConvertToUInt64().ToString()).steamrep.Reputation.SummaryRep.Contains("SCAMMER"))
//                {
//                    Bot.SteamFriends.RemoveFriend(OtherSID);
//                    return false;
//                }
//            }
//            catch { }
//            if (clsFunctions.ScammerList.Contains(OtherSID.ConvertToUInt64()))
//                return false;
//            if (Bot.craftHandler.InGame)
//            {
//                Bot.SteamFriends.SendChatMessage(OtherSID, EChatEntryType.ChatMsg, "Sorry. I'm not available to trade. I'm currently crafting weapons. Sorry!");//show error
//                return false;//making metal can't trade
//            }//if (Bot.craftHandler.InGame)

//            if (ItemsReserved == 0 && clsFunctions.OPERATION_FIRE_STORM)
//            {
//                Bot.SteamFriends.SendChatMessage(OtherSID, EChatEntryType.ChatMsg, "I'm sorry. Due to Operation Fire Storm, I am currently not buying hats.");
//                return false;
//            }

//            Bot.log.Success("(" + clsFunctions.GetFriendIndex(OtherSID, this.Bot) + ") " + Bot.SteamFriends.GetFriendPersonaName(OtherSID) + " (" + OtherSID.ConvertToUInt64() + ") has requested to trade with me!");//show log someone traded
//            return true;//start trade!
//        }//OnTradeRequest()

//        //GLOBALIZED
//        /// <summary>
//        /// Called when the trade initializes
//        /// </summary>
//        public override void OnTradeInit()
//        {
//            Bot.SteamFriends.SetPersonaState(EPersonaState.Busy);//set status to busy
//            Bot.log.Success("Trade started!");//log trade started
//            Trade.SendMessage("Please wait while I load inventories...");//tell user to wait

//            ReInit();//Reset vars
//            TradeCountInventory(ItemsReserved > 0 ? false : true);//count trade
//            if (Trade != null && !ChooseDonate && ItemsReserved > 0)
//            {
//                int iItemSuccess = 0;//items added
//                foreach (KeyValuePair<ulong, SteamID> pair in Bot.dReserved)
//                {
//                    if (pair.Value == Trade.OtherSID)
//                    {
//                        bool ok = Trade.AddItem(pair.Key);//try to add item
//                        if (ok)
//                        {                           
//                            iItemSuccess++;//weapon added
//                            AddDefaultCurrency();//add default currency for this bot
//                        }//if (ok)
//                    }//if (pair.value == Trade.OtherSID)
//                }//foreach(KeyValuePair<ulong,SteamID> pair in Bot.dReserved)
//                ItemsBotAdded = iItemSuccess;//set vars
//                if (ItemsReserved == ItemsBotAdded)
//                {
//                    Trade.SendMessage(String.Format("Success. I have added {0} items. You must put up {1} {2} or equivalent in {3}.", ItemsReserved, CalculatePrice(), Default_Currency, Item_Type));//tell user price
//                    Bot.log.Success(ItemsBotAdded + " Items added!");//log items added
//                }//if (ItemsReserved == ItemsBotAdded)
//                else
//                {
//                    Trade.SendMessage(String.Format("Semi-Success. I have added {0} items of the {1} you reserved... You can close trade and try to trade again or put up {2} {3} or equivalent in {4}.", ItemsBotAdded, ItemsReserved, CalculatePrice(), Default_Currency, Item_Type));//tell user there was error but allow them to pay # it added
//                    Bot.log.Success(ItemsBotAdded + " Items added out of " + ItemsReserved);//show items added out of # it should
//                }//else
//            }//if (Trade != null && !ChooseDonate)

//            if (ChooseDonate && IsAdmin)
//            {
//                OnTradeMessage("add blacklist");//add blacklisted items
//                OnTradeMessage("add unknown");//add unknown items
//            }//if (ChooseDonate && IsAdmin)
//        }//OnTradeInit()

//        //GLOBALIZED
//        /// <summary>
//        /// Triggered when user sets ready status
//        /// </summary>
//        /// <param name="ready">Ready status</param>
//        public override void OnTradeReady(bool ready)
//        {
//            if (!ready)
//            {
//                Trade.SetReady(false);//don't ready up
//            }//if (!ready)
//            else
//            {
//                Bot.log.Success("User is ready to trade!");//user is ready to trade
//                if (Validate())
//                {
//                    Bot.log.Success("Readying trade.");//log
//                    Trade.SetReady(true);//set ready
//                }//if(Validate())
//                else
//                {
//                    Bot.log.Warn("Invalid trade!");//log
//                    Trade.SendMessage("Invalid trade!");//send error
//                }//else
//            }//else
//        }//OnTradeReady()

//        //GLOBALIZED
//        /// <summary>
//        /// Triggered when user accepts trade
//        /// </summary>
//        public override void OnTradeAccept()
//        {
//            if (Validate())
//            {
//                Bot.log.Success("Accepting trade...");//log bot is accepting the trade

//                bool success;//success of the trade variable
//                lock (this)
//                {
//                    success = Trade.AcceptTrade();//see if trade went through
//                }//lock (this)
//                if (success)
//                {
//                    Log.Success("Trade was successful!");//log
//                    if (!ChooseDonate)
//                    {
//                        Bot.SteamFriends.SendChatMessage(OtherSID, EChatEntryType.ChatMsg, clsFunctions.ONESCRAPBANK_TRADE_COMPLETED_MESSAGE);//send trade message
//                        clsFunctions.AddToTradeNumber(this.Bot);
//                    }//if (!ChooseDonate)
//                    Bot.UnReserveAllByUser(OtherSID);//remove reserved weapons from user
//                    ItemsReserved = 0;//reset weapons reserved
//                    ItemReserveHandler.Stop();
//                }//if (success)
//                else
//                {
//                    Log.Warn("Trade might have failed.");//log
//                    Bot.SteamFriends.SendChatMessage(OtherSID, EChatEntryType.ChatMsg, "Did something go wrong with the trade? =C");//send ItemRemovedMsg
//                }//else
//            }//if (Validate())
//            OnTradeClose();//close the trade
//        }//OnTradeAccept()

//        //GLOBALIZED
//        /// <summary>
//        /// Triggers when the trade has closed.
//        /// </summary>
//        public override void OnTradeClose()
//        {
//            ChooseDonate = false;//reset variable
//            base.OnTradeClose();//close the trade
//            Bot.SteamFriends.SetPersonaState(EPersonaState.Online);//Set status
//            Bot.CheckBackpack();//Check backpack to see if items are there. Do this in case accepting lagged out, but it still went through.
//            Bot.informHandler.Start();//Start the information handler.
//        }//OnTradeClose()

//        //GLOBALIZED
//        /// <summary>
//        /// Triggered when a message is received from the trade chat.
//        /// </summary>
//        /// <param name="message">message received</param>
//        public override void OnTradeMessage(string message)
//        {
//            Bot.log.Warn("[TRADE MESSAGE] " + message);//Log user sent a trade message

//            message = message.ToLower();//lowercase the message to deal with it easier

//            if (message.Contains("backpack"))
//            {
//                Trade.SendMessage("Please type backpack in the regular chat window. Thank you!");//tell user to type that elsewhere.
//                return;//stop code
//            }//if (message.Contains("backpack"))

//            else if (message.Contains("donate"))
//            {
//                Trade.SendMessage("Please type donate to a Scrapbanking Bot. Thank you!");//tell user to type that elsewhere.
//                return;//stop code
//            }//else if (message.Contains("donate"))

//            if (IsAdmin)
//            {
//                if (message == "donate")
//                {
//                    ChooseDonate = true;//admin override to donate
//                    Trade.SendMessage("You are now donating");//tell admin status has been changed
//                }//if (message == "donate")

//                else
//                {
//                    clsFunctions.ProcessTradeMessage(message, this.Trade, this.Bot);//See if the message from the admin was a trade command.
//                }//else
//            }//if (ItemRemovedMsg.StartsWith("give") && IsAdmin && ItemRemovedMsg.Length > 5)
//        }//OnTradeMessage()

//        //NON GLOBALIZED
//        /// <summary>
//        /// Triggered when the user adds an item
//        /// </summary>
//        /// <param name="schemaItem">schema version of the item</param>
//        /// <param name="inventoryItem">Inventory version of the item</param>
//        public override void OnTradeAddItem(Schema.Item schemaItem, Inventory.Item inventoryItem)
//        {
//            string ItemAddedMsg = String.Format("User added {0} {1} {2} {3}", inventoryItem.IsNotCraftable.ToString().ToLower() == "true" ? "NonCraftable" : "Craftable", clsFunctions.ConvertQualityToString(schemaItem.ItemQuality), schemaItem.ItemName, schemaItem.CraftMaterialType); //ready ItemRemovedMsg

//            if (!ChooseDonate)
//            {
//                if (schemaItem.CraftMaterialType == "craft_bar")
//                {
//                    switch (inventoryItem.Defindex)
//                    {
//                        case 5000:
//                            Bot.log.Success("User added a scrap metal.");//log user added scrap                            
//                            Bot.userCurrency.AddScrap();//Add scrap
//                            break;//case 5000

//                        case 5001:
//                            Bot.log.Success("User added a reclaimed metal.");//
//                            Bot.userCurrency.AddRec();//add rec
//                            break;//case 5001

//                        case 5002:
//                            Bot.log.Success("User added a refined metal.");//
//                            Bot.userCurrency.AddRef();//add ref
//                            break;//case 5002
//                    }//switch (inventoryItem.defindex)
//                }//if (schemaItem.CraftMaterialType == "craft_bar")
//                else
//                {
//                    Trade.SendMessage(String.Format("I will not accept the item {0} but if you wish to get rid of it I will take it.", schemaItem.ItemName));//show user it wont accept it
//                    Bot.log.Warn("Unaccepted Item Added: " + ItemAddedMsg);//user added item unaccepted
//                }//else
//                CheckMetalCountInTrade();//add or remove metal
//            }//if (!ChooseDonate)
//            else
//            {
//                donatedItems.Add(inventoryItem);//add item to donation
//                Bot.log.Success("Donation: " + ItemAddedMsg);//log donation
//            }//else
//        }//OnTradeAddItem()

//        //NON GLOBALIZED
//        /// <summary>
//        /// Triggered when a user removes an item from the trade
//        /// </summary>
//        /// <param name="schemaItem">Schham version of item</param>
//        /// <param name="inventoryItem">Inventory version of item</param>
//        public override void OnTradeRemoveItem(Schema.Item schemaItem, Inventory.Item inventoryItem)
//        {
//            string ItemRemovedMsg = String.Format("{0} {1} {2} {3}", inventoryItem.IsNotCraftable.ToString().ToLower() == "true" ? "NonCraftable" : "Craftable", clsFunctions.ConvertQualityToString(schemaItem.ItemQuality), schemaItem.ItemName, schemaItem.CraftMaterialType);

//            if (!ChooseDonate)
//            {
//                if (schemaItem.CraftMaterialType == "craft_bar")
//                {
//                    switch (schemaItem.Defindex)
//                    {
//                        case 5000:
//                            Bot.log.Warn("User removed a scrap metal.");//show metal removed
//                            Bot.userCurrency.RemoveScrap();//remove metal
//                            break;//5000
//                        case 5001:
//                            Bot.log.Warn("User removed a reclaimed metal.");//show metal removed
//                            Bot.userCurrency.RemoveRec();//remove metal
//                            break;//5001
//                        case 5002:
//                            Bot.log.Warn("User removed a refined metal.");//show metal removed
//                            Bot.userCurrency.RemoveRef();//remove metal
//                            break;//5002
//                    }//switch (schemaItem.defindex)
//                }//if (schemaItem.CraftMaterialType == "craft_bar")
//                else
//                {
//                    Bot.log.Warn("Unaccepted Item Removed: " + ItemRemovedMsg);//log item removed
//                }//else
//                CheckMetalCountInTrade();//add or remove metal
//            }//if (!ChooseDonate)
//            else
//            {
//                donatedItems.Remove(inventoryItem);//Remove item from donation
//                Bot.log.Warn("Donation removed: " + ItemRemovedMsg);//log donation removed
//            }//else
//        }//OnTradeRemoveItem()

//        //NON GLOBALIZED
//        /// <summary>
//        /// Used to reset all variables before trade starts
//        /// </summary>
//        public void ReInit()
//        {
//            donatedItems.Clear();//Clear
//            Bot.myCurrency.Clear();//reset
//            Bot.userCurrency.Clear();//reset
//            InventoryRec = 0;//reset
//            InventoryRef = 0;//reset
//            InventoryScrap = 0;//reset
//            BotItemsAdded = 0;//reset
//            InventoryItems = 0;//reset
//        }//ReInit()

//        //NON GLOBALIZED
//        /// <summary>
//        /// Used to count the bots inventory levels in trade.
//        /// </summary>
//        /// <param name="message">To send a message or not</param>
//        public void TradeCountInventory(bool message)
//        {            
//            //Bot.MyInventory = Trade.MyInventory;//set inventory
//            foreach (Inventory.Item item in Bot.MyInventory.Items)
//            {
//                if (item.IsNotTradeable)
//                {

//                }//if (item.isnottradeable)
//                else if (item.Defindex == 5000)
//                {
//                    InventoryMetal++;//add metal
//                    InventoryScrap++;//add scrap
//                }//else if (item.defindex == 5000)
//                else if (item.Defindex == 5001)
//                {
//                    InventoryMetal += 3;//add metal
//                    InventoryRec++;//add rec
//                }//else if (item.defindex == 5001)
//                else if (item.Defindex == 5002)
//                {
//                    InventoryMetal += 9;//add metal
//                    InventoryRef++;//add ref
//                }//else if (item.defindex == 5002)                
//                else
//                {
//                    InventoryItems++;//add to total items
//                }//else
//            }//foreach (Inventory.Item item in Bot.MyInventory.items)
//            if (message)
//            {
//                if (!ChooseDonate)
//                {

//                }//if (!ChooseDonate)
//                else
//                {
//                    Trade.SendMessage("Success! Please add the items you wish to donate. Thank you ^^.");//tell user to add items to donate
//                }//else
//            }//if (message)
//        }//TradeCountInventory()

//        //NON GLOBALIZED
//        /// <summary>
//        /// Used to validate the trade
//        /// </summary>
//        /// <returns>True if everything is good in the trade, false if not</returns>
//        public bool Validate()
//        {
//            List<string> errors = new List<string>();//list of errors
//            Schema schema = Trade.CurrentSchema;//get schema
//            int iCheckedScrap = 0;//scrap var
//            int iCheckedRec = 0;//rec var
//            int iCheckedRef = 0;//ref var
//            int iCheckedMetal = 0;//metal var

//            if (ChooseDonate)
//            {
//                return true;//trade is fine
//            }//if (ChooseDonate)

//            List<Inventory.Item> items = new List<Inventory.Item>();//Holds offered items

//            Bot.OtherInventory = Trade.OtherInventory;//set inventory

//            foreach (ulong id in Trade.OtherOfferedItems)
//            {
//                items.Add(Bot.OtherInventory.GetItem(id));//add to list
//            }//foreach (ulong id in Trade.OtherOfferedItems)

//            foreach (Inventory.Item item in items)
//            {
//                Schema.Item newitem = schema.GetItem(item.Defindex);//get schema version of item
//                if (newitem.Defindex == 5000 && ItemsReserved > 0)
//                {
//                    iCheckedScrap++;//add to scrap
//                    iCheckedMetal++;//add to metal
//                }//if (newitem.defindex == 5000 && ItemsReserved > 0)
//                else if (newitem.Defindex == 5001 && ItemsReserved > 0)
//                {
//                    iCheckedRec++;//add to rec
//                    iCheckedMetal += 3;//add to metal
//                }//else if (newitem.defindex == 5001 && ItemsReserved > 0)
//                else if (newitem.Defindex == 5002 && ItemsReserved > 0)
//                {
//                    iCheckedRef++;//add to ref
//                    iCheckedMetal += 9;//add to metal
//                }//else if (newitem.defindex == 5002 && ItemsReserved > 0)
//                else if (newitem.ItemName.Contains("Mann Co. Supply Crate") && !newitem.ItemName.Contains("Key"))
//                {
//                    errors.Add("" + newitem.ItemName + " will not be accepted!");
//                }//else if (newitem.ItemName.Contains("Mann Co. Supply Crate") && !newitem.ItemName.Contains("Key"))
//            }//foreach (Inventory.Item item in items)

//            if (iCheckedScrap != Bot.userCurrency.Scrap)
//            {
//                Bot.log.Warn("Scrap count in trade didn't' match ones in the trade vars. Setting. " + Bot.userCurrency.Scrap + " before " + iCheckedScrap + " after.");//log error
//                Bot.userCurrency.RemoveScrap(Bot.userCurrency.Scrap);//clear scrap
//                Bot.userCurrency.AddScrap(iCheckedScrap);//reset var
//                CheckMetalCountInTrade();//recheck metal
//            }//if (iCheckedScrap != UserScrapAdded)
//            if (iCheckedRec != Bot.userCurrency.Reclaimed)
//            {
//                Bot.log.Warn("Rec count in trade didn't' match ones in the trade vars. Setting. " + Bot.userCurrency.Reclaimed + " before " + iCheckedRec + " after.");//log error
//                Bot.userCurrency.RemoveRec(Bot.userCurrency.Reclaimed);//clear rec
//                Bot.userCurrency.AddRec(iCheckedRec);//reset var
//                CheckMetalCountInTrade();//recheck metal
//            }//if (iCheckedRec != UserRecAdded)
//            if (iCheckedRef != Bot.userCurrency.Refined)
//            {
//                Bot.log.Warn("Ref count in trade didn't match ones in the trade vars. Setting. " + Bot.userCurrency.Refined + " before " + iCheckedRef + " after.");//log error
//                Bot.userCurrency.RemoveRef(Bot.userCurrency.Refined);//clear ref
//                Bot.userCurrency.AddRef(iCheckedRef);//reset var
//                CheckMetalCountInTrade();//recheck metal
//            }// if (iCheckedRef != UserRefAdded)

//            if (Bot.myCurrency.ToMetal() < BotItemsAdded && ItemsReserved > 0)
//            {
//                errors.Add(String.Format("You must put up {0} scrap!", BotItemsAdded));//show user error
//                Bot.log.Warn("User has not put up enough scrap to get items chosen.");//log error
//            }//if (UserMetalAdded < BotItemsAdded && ItemsReserved > 0)

//            CheckMetalCountInTrade();//recheck to make sure

//            if (errors.Count > 0)
//            {
//                Trade.SendMessage("There are errors in your trade:");//tell user errors are present
//            }//if (errors.Count > 0)

//            foreach (string error in errors)
//            {
//                Trade.SendMessage(error);//send error
//                Bot.log.Warn(String.Format("Trade Validation error: {0} with user {1}", error, Bot.SteamFriends.GetFriendPersonaName(Trade.OtherSID)));//log error
//            }//foreach (string error in errors)

//            return errors.Count == 0;//return if there are errors or not
//        }//Validate()

//        public void CheckMetalCountInTrade()
//        {
//            TF2Currency change = Bot.userCurrency.GetChange(Bot.myCurrency);

//            if (change.Neutral())
//            {
//                return;//no change to remove or add
//            }//if (change.Neutral())
//            if (change.Positive())
//            {
//                int iChange = (int)change.ToScrap();
//                if (InventoryScrap - Bot.myCurrency.Scrap >= iChange)
//                {
//                    uint ScrapAdded = Trade.AddAllItemsByDefindex(5000, (uint)iChange);
//                    Bot.myCurrency.AddScrap((int)ScrapAdded);
//                    if (ScrapAdded == iChange)
//                    {
//                        Bot.log.Success("Added " + iChange + " scrap.!");
//                    }
//                }
//                else
//                {
//                    Trade.SendMessage("Sorry, I do not have enough change!");
//                }
//            }
//            else if (change.Negative())
//            {
//                change.MakePositive();
//                int iChange = (int)change.ToScrap() - ItemsBotAdded;
//                if (Bot.myCurrency.Scrap >= iChange)
//                {
//                    uint ScrapRemoved = Trade.RemoveAllItemsByDefindex(5000, (uint)iChange);
//                    Bot.myCurrency.RemoveScrap((int)ScrapRemoved);
//                    if (ScrapRemoved == iChange)
//                    {
//                        Bot.log.Success("Removed " + iChange + " scrap.");
//                    }
//                    else
//                    {
//                        Trade.CancelTrade();
//                        Bot.SteamFriends.SendChatMessage(OtherSID, EChatEntryType.ChatMsg, "I'm sorry. Something went wrong in trade. Please try again.");
//                    }
//                }
//                else
//                {
//                    Trade.CancelTrade();
//                    Bot.SteamFriends.SendChatMessage(OtherSID, EChatEntryType.ChatMsg, "I'm sorry. Something went wrong in trade. Please try again.");
//                    return;
//                }
//            }
//        }//CheckMetalCountInTrade()

//        //public void CheckMetalCountInTrade()
//        //{
//        //    int SetsToAdd = UserMetalAdded - (BotItemsAdded + MetalAdded);

//        //    if (SetsToAdd > 0)
//        //    {
//        //        if (InventoryScrap > SetsToAdd)
//        //        {
//        //            uint scrapadded = 0;
//        //            scrapadded = Trade.AddAllItemsByDefindex(5000, (uint)SetsToAdd);
//        //            ScrapAdded += (int)scrapadded;
//        //            MetalAdded += (int)scrapadded;
//        //            if (scrapadded == SetsToAdd)
//        //            {
//        //                Bot.log.success(String.Format("{0} scrap added!", scrapadded));
//        //            }
//        //            else
//        //            {
//        //                Bot.log.Warn("error occurred adding metal...");
//        //                Trade.CancelTrade();
//        //                Bot.SteamFriends.SendChatMessage(OtherSID, EChatEntryType.ChatMsg, "Sorry. Something went wrong in trade. Please try again.");
//        //            }
//        //        }
//        //        else
//        //        {
//        //            Trade.SendMessage("Sorry! I don't have enough change.");
//        //        }
//        //    }
//        //    else if (SetsToAdd < 0)
//        //    {
//        //        if (BotItemsAdded > 0 && MetalAdded == 0)
//        //        {
//        //            Trade.SendMessage(String.Format("Error. You must put up {0} scrap. You have {1} scrap showing.", BotItemsAdded, UserMetalAdded));
//        //            Bot.log.Warn("User has not put up enough metal to get items chosen.");
//        //            return;
//        //        }
//        //        if (ScrapAdded > (Math.Abs(SetsToAdd)))
//        //        {
//        //            uint scrapremoved = 0;
//        //            scrapremoved = Trade.RemoveAllItemsByDefindex(5000, (uint)(Math.Abs(SetsToAdd)));
//        //            ScrapAdded -= (int)scrapremoved;
//        //            MetalAdded -= (int)scrapremoved;
//        //            if (scrapremoved == (Math.Abs(SetsToAdd)))
//        //            {
//        //                Bot.log.success(String.Format("Removed {0} scrap.", scrapremoved));
//        //            }
//        //            else
//        //            {
//        //                Bot.log.Warn("error occurred removing metal...");
//        //                Trade.CancelTrade();
//        //                Bot.SteamFriends.SendChatMessage(OtherSID, EChatEntryType.ChatMsg, "Sorry. Something went wrong in trade. Please try again.");
//        //            }
//        //        }
//        //        else
//        //        {
//        //            Trade.CancelTrade();
//        //            Bot.SteamFriends.SendChatMessage(OtherSID, EChatEntryType.ChatMsg, "Something went horribly wrong. Please try to trade me again!");
//        //        }
//        //    }
//        //}

//        //NON GLOBALIZED

//        //NON GLOBALIZED
//        /// <summary>
//        /// Used to check if the hat is on the blacklist
//        /// </summary>
//        /// <param name="item">Inventory.Item item version</param>
//        /// <param name="sReason">Reason the hat is not able to be sold</param>
//        /// <returns>True if blacklisted false if not.</returns>
//        public bool CheckBlackList(Inventory.Item item, out string sReason)
//        {
//            bool bBlacklisted = true;//storage
//            sReason = "";//default

//            if (clsFunctions.schema.GetItem(item.Defindex).CraftMaterialType == "craft_bar")
//            {
//                sReason = "You cannot reserve metal...";//they can't reserve metal
//                bBlacklisted = false;//can't reserve metal
//            }//if (clsFunctions.schema.GetItem(item.defindex).CraftMaterialType == "craft_bar")

//            if (clsFunctions.schema.GetItem(item.Defindex).ItemName.Contains("Tux"))
//            {
//                sReason = "That item is not for sale.";
//                bBlacklisted = false;//can't reserve the tux
//            }//if (clsFunctions.schema.GetItem(item.defindex).ItemName.Contains("Tux"))

//            return bBlacklisted;//return sConversionResult           
//        }//CheckBlackList()

//        //GLOBALIZED
//        /// <summary>
//        /// Triggered when the user has not traded for their items in 6 minutes
//        /// </summary>
//        public void OnReserveElapsed()
//        {
//            Bot.SteamFriends.SendChatMessage(OtherSID, EChatEntryType.ChatMsg, "I'm sorry. You haven't traded for your items in 6 minutes. I have removed them.");//send message.
//            Bot.UnReserveAllByUser(OtherSID);//remove all reserved items from user            
//        }//OnReserveElapsed()

//        /// <summary>
//        /// Used to add the default currency to the bots currency var
//        /// </summary>
//        public void AddDefaultCurrency()
//        {
//            switch (Item_Type)
//            {
//                case "weapons":
//                    Bot.myCurrency.AddWeapon();//add a weapon
//                    break;//case "weapons"
//                case "hats":
//                    Bot.myCurrency.AddHat();//add a hat
//                    break;//case "hats"
//                case "items":
//                    Bot.myCurrency.AddScrap();//add a scrap
//                    break;//case "items"
//            }//switch (Item_Type)
//        }//AddDefaultCurrency()

//        /// <summary>
//        /// Used to calculate the price in trades
//        /// </summary>
//        /// <returns>Price the user has to pay</returns>
//        public double CalculatePrice()
//        {
//            double dePrice = 0;//price variable
//            switch (Item_Type)
//            {
//                case "weapons":
//                    dePrice = ItemsBotAdded / 2;
//                    break;//case "weapons"
//                case "hats":
//                    dePrice = clsFunctions.ConvertHatToMetal(ItemsBotAdded);
//                    break;//case "hats"
//                case "items":
//                    TF2Currency tempCurn = new TF2Currency();
//                    tempCurn.AddScrap(ItemsBotAdded);
//                    dePrice = tempCurn.ToPrice();
//                    break;//case "items"
//            }//switch (Item_Type)
//            return dePrice;
//        }//CalculatePrice()
//    }//class
//}//namespace