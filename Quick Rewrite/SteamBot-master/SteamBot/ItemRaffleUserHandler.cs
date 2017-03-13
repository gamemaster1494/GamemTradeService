//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Timers;
//using SteamKit2;
//using SteamTrade;

//namespace SteamBot
//{
//    public class ItemRaffleUserHandler : UserHandler
//    {
//        ///When adding an item to raffle, enter a command, it trades
//        ///then it stores the item it got in a .txt file, along with 
//        ///cost it takes to enter, and slots
//        ///so something like .addraffle <cost in .33 form> <slots>
//        ///when it loads it loads the file, that also has entries in it.
//        ///so a new file config.cs thing needs to be made
//        private bool bPickWinner = false;

//        private bool ChooseDonate = false;

//        private TF2Currency userCurrency = new TF2Currency();//Users currency

//        public FriendAddedHandler FriendAddedHandler;//Friend Added Handler

//        public ItemRaffleUserHandler(Bot bot, SteamID sid):base(bot,sid)
//        {
//            this.FriendAddedHandler = new SteamBot.FriendAddedHandler(bot, bot.BotControlClass.Substring(9), OtherSID);
//            bot.GetInventory();
//        }

//        //GLOBALIZED
//        /// <summary>
//        /// Triggered when the bot gets a Group Invite
//        /// </summary>
//        /// <returns>To accept or not</returns>
//        public override bool OnGroupAdd()
//        {
//            return false;//don't accept
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
//            if (SteamRepStatus.GetSteamRepStatus(OtherSID.ConvertToUInt64().ToString()).steamrep.Reputation.SummaryRep.Contains("SCAMMER"))
//                return false;
//            if (clsFunctions.ScammerList.Contains(OtherSID.ConvertToUInt64()))
//                return false;

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
//                Bot.SteamFriends.SendChatMessage(OtherSID, type, "You have no reserved items from me.... I do not sell items.");
//            }//else if (MessageHandled == clsFunctions.UserClearCMD)
//            else if (MessageHandled == clsFunctions.UserDonateCMD)
//            {
//                ChooseDonate = true;//user is donating
//                return;//stop code
//            }//else if (MessageHandled == clsFunctions.UserDonateCMD)
//            else if (MessageHandled.StartsWith(Bot.BackpackUrl) && MessageHandled.Length > Bot.BackpackUrl.Length)
//            {

//                string[] sLines = MessageHandled.Split('_');//get item information

//                if (!Bot.dReserved.ContainsKey(Convert.ToUInt64(sLines[2])))
//                {
//                    Bot.SteamFriends.SendChatMessage(OtherSID, type, "You cannot reserve items from me. I do not sell items. I raffle them.");
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
//            if (SteamRepStatus.GetSteamRepStatus(OtherSID.ConvertToUInt64().ToString()).steamrep.Reputation.SummaryRep.Contains("SCAMMER"))
//            {
//                Bot.SteamFriends.RemoveFriend(OtherSID);
//                return false;
//            }
//            if (clsFunctions.ScammerList.Contains(OtherSID.ConvertToUInt64()))
//                return false;
//            if (Bot.craftHandler.InGame)
//            {
//                Bot.SteamFriends.SendChatMessage(OtherSID, EChatEntryType.ChatMsg, "Sorry. I'm not available to trade. I'm currently crafting weapons. Sorry!");//show error
//                return false;//making metal can't trade
//            }//if (Bot.craftHandler.InGame)

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

//            Trade.SendMessage("Inventory loaded! The price for the current raffle is " + clsFunctions.itemraffleData.price.ToMetal() + " ref.");
//        }//OnTradeInit()

//        //NON GLOBALIZED
//        /// <summary>
//        /// Triggered when the user adds an item
//        /// </summary>
//        /// <param name="schemaItem">schema version of the item</param>
//        /// <param name="inventoryItem">Inventory version of the item</param>
//        public override void OnTradeAddItem(Schema.Item schemaItem, Inventory.Item inventoryItem)
//        {
//            //Maybe try to globalize by having every bot accept weapons and such when the vault is implemented.
//            string ItemAddedMsg = String.Format("User added {0} {1} {2} {3}", inventoryItem.IsNotCraftable ? "NonCraftable" : "Craftable", clsFunctions.ConvertQualityToString(schemaItem.ItemQuality), schemaItem.ItemName, schemaItem.CraftMaterialType); //ready ItemRemovedMsg
//            if (!ChooseDonate)
//            {
//                if (schemaItem.CraftMaterialType == "craft_bar" && clsFunctions.bTakingSlots)
//                {
//                    switch (schemaItem.Defindex)
//                    {
//                        case 5000:
//                            Bot.log.Success("User added a scrap metal.");//log metal added
//                            userCurrency.AddScrap();//add metal
//                            break;//5000
//                        case 5001:
//                            Bot.log.Success("User added a reclaimed metal.");//log metal added
//                            userCurrency.AddRec();//add metal
//                            break;//5001
//                        case 5002:
//                            Bot.log.Success("User added a refined metal.");//log metal added
//                            userCurrency.AddRef();//add metal
//                            break;//5002
//                    }//switch (schemaItem.defindex)
//                }//else if (schemaItem.CraftMaterialType == "craft_bar" && ItemsBotAdded > 0)
//                else
//                {
//                    Trade.SendMessage(String.Format("I will not accept the item {0} but if you wish to get rid of it I will take it.", schemaItem.ItemName));//show user it wont accept it
//                    Bot.log.Warn("Unaccepted Item Added: " + ItemAddedMsg);//user added item unaccepted
//                }//else
//                CheckMetalCountInTrade();//check metal logic
//            }//if (!ChooseDonate)
//            else
//            {
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
//            string ItemRemovedMsg = String.Format("{0} {1} {2} {3}", inventoryItem.IsNotCraftable ? "NonCraftable" : "Craftable", clsFunctions.ConvertQualityToString(schemaItem.ItemQuality), schemaItem.ItemName, schemaItem.CraftMaterialType);

//            if (!ChooseDonate)
//            {
//                if (schemaItem.CraftMaterialType == "craft_bar" && clsFunctions.bTakingSlots)
//                {
//                    switch (schemaItem.Defindex)
//                    {
//                        case 5000:
//                            Bot.log.Warn("User removed a scrap metal.");//show metal removed
//                            userCurrency.RemoveScrap();//remove metal
//                            break;//5000
//                        case 5001:
//                            Bot.log.Warn("User removed a reclaimed metal.");//show metal removed
//                            userCurrency.RemoveRec();//remove metal
//                            break;//5001
//                        case 5002:
//                            Bot.log.Warn("User removed a refined metal.");//show metal removed
//                            userCurrency.RemoveRef();//remove metal
//                            break;//5002
//                    }//switch (schemaItem.defindex)
//                }//else if (schemaItem.CraftMaterialType == "craft_bar" && ItemsBotAdded > 0)
//                else
//                {
//                    Bot.log.Warn("Unaccepted Item Removed: " + ItemRemovedMsg);//log item removed
//                }//else
//                CheckMetalCountInTrade();//check metal
//            }//if (!ChooseDonate)
//            else
//            {
//                Bot.log.Warn("Donation removed: " + ItemRemovedMsg);//log donation removed
//            }//else
//        }//OnTradeRemoveItem();

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

//                if (ChooseDonate && !IsAdmin)
//                {
//                }//if (ChooseDonate && !IsAdmin)
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
//                        int iSlots = Convert.ToInt32(userCurrency.ToMetal() / Bot.itemRaffleData.price.ToMetal());
//                        Bot.SteamFriends.SendChatMessage(OtherSID, EChatEntryType.ChatMsg, "Thank you for your entry! You will be notified if you won or lost when all slots are filled. Please keep me added. Good luck! =)");//send trade message
//                        if (!Bot.itemRaffleData.Entries.ContainsKey(OtherSID.ConvertToUInt64()))
//                        {
//                            Bot.itemRaffleData.Entries.Add(OtherSID.ConvertToUInt64(), iSlots);
//                        }
//                        else
//                        {
//                            Bot.itemRaffleData.Entries[OtherSID.ConvertToUInt64()] += iSlots;
//                        }
//                        clsFunctions.itemraffleData.SlotsLeft -= iSlots;
//                        ItemRaffleData.SaveItemRaffleData(Bot.itemRaffleData, Bot.sItemRaffleSaveFile);
//                    }//if (!ChooseDonate)
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
//            if (Bot.itemRaffleData.SlotsLeft <= 0)
//            {
//                if (Bot.itemRaffleData.Winner == null)
//                {
//                    Bot.itemRaffleData.Winner = ItemRaffleData.PickWinner(Bot.itemRaffleData);
//                    ItemRaffleData.SaveItemRaffleData(Bot.itemRaffleData, Bot.sItemRaffleSaveFile);
//                }
//            }
            
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
//                Trade.SendMessage("Please type donate in the regular chat window. Thank you!");//tell user to type that elsewhere.
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
//        /// The main logic of the trade. Calculates the number of scrap needed to remove or add in trades.
//        /// </summary>
//        /// <param name="error">only set if there is an internal error</param>
//        public void CheckMetalCountInTrade()
//        {

//        }//CheckMetalInTrade

//        //NON GLOBALIZED
//        /// <summary>
//        /// Used to reinitialize variables used in trades.
//        /// </summary>
//        public void ReInit()
//        {
//            userCurrency.Clear();//reset
//        }//ReInit()


//        //NON GLOBALIZED
//        /// <summary>
//        /// Called to validate the trade
//        /// </summary>
//        /// <returns>If trade is okay or not</returns>
//        public bool Validate()
//        {
//            if (ChooseDonate)
//            {
//                return true;//trade is fine
//            }//if (ChooseDonate)

//            List<string> errors = new List<string>();//List of errors

//            int iCheckedScrap = 0;

//            List<Inventory.Item> items = new List<Inventory.Item>();//holds items user has offered

//            foreach (ulong id in Trade.OtherOfferedItems)
//            {
//                items.Add(Trade.OtherInventory.GetItem(id));//add item to inventory list
//            }//foreach (ulong id in Trade.OtherOfferedItems)

//            foreach (Inventory.Item item in items)
//            {
//                Schema.Item newitem = clsFunctions.schema.GetItem(item.Defindex);//get schema item information

//                if (newitem.Defindex == 5000)
//                {
//                    iCheckedScrap += 1;//add to checked weapons
//                }//else if (newitem.defindex == 5000)

//                else if (newitem.Defindex == 5001)
//                {
//                    iCheckedScrap += 3;//add to checked weapons
//                }//else if (newitem.defindex == 5001 && ItemsReserved > 0)

//                else if (newitem.Defindex == 5002)
//                {
//                    iCheckedScrap += 9;//add to checked weapons
//                }//else if (newitem.defindex == 5002 && ItemsReserved > 0)
//                else if (newitem.ItemName.Contains("Mann Co. Supply Crate") && !newitem.ItemName.Contains("Key"))
//                {
//                    errors.Add("" + newitem.ItemName + " will not be accepted!");
//                }
//            }//foreach (Inventory.Item item in items)

//            if (Bot.itemRaffleData.price.ToMetal() > userCurrency.ToMetal())
//            {
//                errors.Add(String.Format("You must put up {0} metal to enter this raffle.", Bot.itemRaffleData.price.ToMetal()));//add error
//                Bot.log.Warn("User has not put up enough items to enter raffle.");//log error
//            }//if (UserWepsAdded < ItemsBotAdded && ItemsReserved > 0)

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
//    }//class
//}//namespace
