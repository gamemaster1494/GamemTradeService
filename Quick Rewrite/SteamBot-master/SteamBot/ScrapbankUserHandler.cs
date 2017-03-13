using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using SteamKit2;
using SteamTrade;
using ChatterBotAPI;
using System.Text.RegularExpressions;
using SteamTrade.TradeWebAPI;

namespace SteamBot
{
    public class ScrapbankUserHandler : UserHandler
    {        

        private ChatterBot chatterBot;

        private ChatterBotSession chatterBotsession;

        private int MAX_RESERVE = clsFunctions.SCRAPBANK_MAX_RESERVE;//Used to globalize code

        private int MAX_RESERVE_DONATOR = clsFunctions.SCRAPBANK_DOANTOR_RESERVE;//Used to globalize code

        private string Item_Type = "weapons";//Used to globalize code

        private string Default_Currency = "scrap";//used to globalize code

        public FriendAddedHandler FriendAddedHandler;//Friend Added Handler

        public ItemReserveHandler ItemReserveHandler;//Item Reserve Handler

        //public TF2Currency Bot.myCurrency = new TF2Currency();//Bots currency

        //public TF2Currency Bot.userCurrency = new TF2Currency();//Users currency

        private int InventoryMetal, InventoryScrap, InventoryRec, InventoryRef, InventoryWeps, InventoryBlacklistItem, UnknownItems;
         
        private bool ChooseDonate = false;

        private int UserWepsAdded, UserMetalAdded, BotScrapAdded, BotRecAdded, BotRefAdded, BotMetalAdded;

        private int ItemsBotAdded = 0;

        private int ItemsReserved = 0;

        /// <summary>
        /// called when bot is made with this handle
        /// </summary>
        /// <param name="bot">bot assigned to</param>
        /// <param name="sid">SteamID of the other user</param>
        public ScrapbankUserHandler(Bot bot, SteamID sid)
            : base(bot, sid)
        {
            chatterBot = clsFunctions.factory.Create(ChatterBotType.PANDORABOTS, "b0dafd24ee35a477");
            chatterBotsession = chatterBot.CreateSession();
            this.FriendAddedHandler = new SteamBot.FriendAddedHandler(bot, bot.BotControlClass.Substring(9), OtherSID);//Initialize
            this.ItemReserveHandler = new SteamBot.ItemReserveHandler(bot, bot.BotControlClass.Substring(9), OtherSID);//Initialize
            this.ItemReserveHandler.OnElapsed += this.OnReserveElapsed;//Set trigger method
            bot.GetInventory();//Get Inventory
        }//ScrapbankUserHandler()

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
            if (ItemsReserved > 0)
            {
                Bot.UnReserveAllByUser(OtherSID);//Remove items user had reserved
                ItemsReserved = 0;//reset weapons reserved
            }//if (ItemsReserved > 0)
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
            if (message == "removed_item")
            {
                if (ItemsReserved > 0)
                {
                    ItemsReserved--;//remove weapon
                }//if (ItemsReserved > 0)
                return;//stop going due to this being a bot internal command
            }//if (ItemRemovedMsg == "removed_item")

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
                Bot.UnReserveAllByUser(OtherSID);//Remove all reserved items from the user
                ItemsReserved = 0;//reset weapons
                Bot.SteamFriends.SendChatMessage(OtherSID, type, "Reserved items cleared!");//tell user of success
                Bot.log.Success(String.Format("{0} cleared their reserved items.", Bot.SteamFriends.GetFriendPersonaName(OtherSID)));//log weapons cleared
                this.ItemReserveHandler.Stop();
                return;
            }//else if (MessageHandled == clsFunctions.UserClearCMD)
            else if (MessageHandled == clsFunctions.UserDonateCMD)
            {
                ChooseDonate = true;//user is donating
                return;//stop code
            }//else if (MessageHandled == clsFunctions.UserDonateCMD)
            else if (MessageHandled.StartsWith(Bot.BackpackUrl) && MessageHandled.Length > Bot.BackpackUrl.Length)
            {
                if (!clsFunctions.IsPremiumDonator(OtherSID.ConvertToUInt64().ToString()))
                {
                    if (MAX_RESERVE == ItemsReserved)
                    {
                        Bot.SteamFriends.SendChatMessage(OtherSID, type, String.Format("Sorry. You can currently reserve {0} {1} at a time. Donation in the future will increase this.)", MAX_RESERVE,Item_Type));//show max they can reserve
                        return;//stop code
                    }//if (clsFunctions.SCRAPBANK_MAX_RESERVE == ItemsReserved)
                }

                else
                {
                    if (MAX_RESERVE_DONATOR == ItemsReserved)
                    {
                        Bot.SteamFriends.SendChatMessage(OtherSID, type, String.Format("Sorry. As a donator,  you can only reserve {0} {1}.", MAX_RESERVE_DONATOR,Item_Type));
                        return;//stop code
                    }//if (clsFunctions.SCRAPBANK_MAX_RESERVE == ItemsReserved)
                }
                string[] sLines = MessageHandled.Split('_');//get item information
                if (!Bot.dReserved.ContainsKey(Convert.ToUInt64(sLines[2])))
                {
                    if (Bot.MyInventory == null)
                    {
                        Bot.GetInventory();
                        if (Bot.MyInventory == null)
                        {
                            Bot.SteamFriends.SendChatMessage(OtherSID, type, "I'm sorry. SteamAPI may be down. I cannot seem to retrieve my backpack. Please try again later.");
                            return;
                        }
                    }
                    Inventory.Item item = Bot.MyInventory.GetItem(Convert.ToUInt64(sLines[2]));//get the item from the bots inventory
                    string testvar = sLines[2];
                    if (item == null)
                    {
                        Bot.GetInventory();//reload the inventory to make sure
                        item = Bot.MyInventory.GetItem(Convert.ToUInt64(sLines[2]));//Try to get the item again with the new inventory
                        if (item == null)
                        {
                            Bot.SteamFriends.SendChatMessage(OtherSID, type, "Sorry. I do not have that item anymore. Please refresh my inventory.");//Item already traded. Tell user to refresh.
                            return;//stop code
                        }//if (item == null)
                    }//if (item == null)
                    if (Bot.dDonated.ContainsKey(item.OriginalId))
                    {
                        Bot.SteamFriends.SendChatMessage(OtherSID, type, "I'm sorry. But that item is a donation. Donations cannot be taken until the owner deals with it.");//tell user they can't reserve donation items
                        Bot.log.Warn("User tried to reserve a donated item.");//log they tried to reserve a donated item
                        return;//stop code as they can't reserve this item
                    }//if (Bot.dDonated.Keys.Contains(item.OriginalId))
                    string sResults;//used to get the sConversionResult ItemRemovedMsg of checking the item

                    if (CheckBlackList(item, out sResults))
                    {
                        Bot.dReserved.Add(Convert.ToUInt64(sLines[2]), OtherSID);//add item to reserved list
                        ItemsReserved++;//add to weapons reserved
                        Bot.SteamFriends.SendChatMessage(OtherSID, type, "Item Reserved! Trade when ready.");//tell user item was reserved
                        Bot.log.Success(String.Format("{0} reserved {1}", Bot.SteamFriends.GetFriendPersonaName(OtherSID), clsFunctions.schema.GetItem(item.Defindex).Name));//log item reserved
                        this.ItemReserveHandler.Start();
                    }//if (CheckBlackList(item, out sResults))
                    else
                    {
                        Bot.SteamFriends.SendChatMessage(OtherSID, type, sResults);//send the sConversionResult of checking the item
                    }//else
                }//if (!Bot.dReserved.ContainsKey(Convert.ToUInt64(sLines[2])))
                else
                {
                    if (Bot.dReserved[Convert.ToUInt64(sLines[2])].ConvertToUInt64() == OtherSID.ConvertToUInt64())
                    {
                        Bot.SteamFriends.SendChatMessage(OtherSID, type, "You have already reserved this item silly!");//they already reserved this item
                    }//if (Bot.dReserved[Convert.ToUInt64(sLines[2])].ConvertToUInt64() == OtherSID.ConvertToUInt64())
                    else
                    {
                        Bot.SteamFriends.SendChatMessage(OtherSID, type, "Sorry! Someone else has reserved this item...");//someone already reserved this item
                    }//else
                }//else
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
                Bot.SteamFriends.SendChatMessage(OtherSID, EChatEntryType.ChatMsg, "Sorry. I'm not available to trade. I'm currently crafting weapons. Sorry!");//show error
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
            TradeCountInventory(ItemsReserved > 0 ? false : true);//count trade
            if (Trade != null && !ChooseDonate && ItemsReserved > 0)
            {
                int iItemSuccess = 0;//items added
                foreach (KeyValuePair<ulong, SteamID> pair in Bot.dReserved)
                {
                    if (pair.Value == Trade.OtherSID)
                    {
                        bool ok = Trade.AddItem(pair.Key);//try to add item
                        if (ok)
                        {
                            iItemSuccess++;//weapon added
                            AddDefaultCurrency();//add default currency for this bot
                        }//if (ok)
                    }//if (pair.value == Trade.OtherSID)
                }//foreach(KeyValuePair<ulong,SteamID> pair in Bot.dReserved)
                ItemsBotAdded = iItemSuccess;//set vars
                if (ItemsReserved == ItemsBotAdded)
                {
                    Trade.SendMessage(String.Format("Success. I have added {0} items. You must put up {1} {2} or equivalent in {3}.", ItemsReserved, CalculatePrice(), Default_Currency, Item_Type));//tell user price
                    Bot.log.Success(ItemsBotAdded + " Items added!");//log items added
                }//if (ItemsReserved == ItemsBotAdded)
                else
                {
                    Trade.SendMessage(String.Format("Semi-Success. I have added {0} items of the {1} you reserved... You can close trade and try to trade again or put up {2} {3} or equivalent in {4}.", ItemsBotAdded, ItemsReserved, CalculatePrice(), Default_Currency, Item_Type));//tell user there was error but allow them to pay # it added
                    Bot.log.Success(ItemsBotAdded + " Items added out of " + ItemsReserved);//show items added out of # it should
                }//else
            }//if (Trade != null && !ChooseDonate)

            if (ChooseDonate && IsAdmin)
            {
                OnTradeMessage("add blacklist");//add blacklisted items
                OnTradeMessage("add unknown");//add unknown items
            }//if (ChooseDonate && IsAdmin)
        }//OnTradeInit()

        //NON GLOBALIZED
        /// <summary>
        /// Triggered when the user adds an item
        /// </summary>
        /// <param name="schemaItem">schema version of the item</param>
        /// <param name="inventoryItem">Inventory version of the item</param>
        public override void OnTradeAddItem(Schema.Item schemaItem, Inventory.Item inventoryItem)
        {
            if (schemaItem == null || inventoryItem == null)
            {
                Trade.CancelTrade();
                Bot.SteamFriends.SendChatMessage(OtherSID, EChatEntryType.ChatMsg, "I'm sorry. I believe SteamAPI is down. Please try again to trade in a few minutes.");
                Bot.log.Warn("Issue getting inventory item. API down? Closing trade.");
                return;
            }

            //Maybe try to globalize by having every bot accept weapons and such when the vault is implemented.
            string ItemAddedMsg = String.Format("User added {0} {1} {2} {3}", inventoryItem.IsNotCraftable.ToString().ToLower() == "true" ? "NonCraftable" : "Craftable", clsFunctions.ConvertQualityToString(schemaItem.ItemQuality), schemaItem.ItemName, schemaItem.CraftMaterialType); //ready ItemRemovedMsg
            if (!ChooseDonate)
            {
                if (schemaItem.CraftMaterialType == "weapon" || clsFunctions.WepBlackList.Contains(inventoryItem.Defindex))
                {
                    if (!inventoryItem.IsNotCraftable)
                    {
                        Bot.log.Success(ItemAddedMsg);//log weapon added
                        UserWepsAdded++;//add weapon
                        Bot.userCurrency.AddWeapon();//Add Weapon to user currency
                    }//item is Craftable
                    else
                    {
                        Bot.log.Warn(ItemAddedMsg);//log info ItemRemovedMsg
                        Trade.SendMessage(schemaItem.ItemName + " is not Craftable!");//say item isn't Craftable
                    }//else
                }//if (schemaItem.CraftMaterialType == "weapon" || CheckBlackList(schemaItem.defindex))
                else if (schemaItem.CraftMaterialType == "craft_bar" && ItemsBotAdded > 0)
                {
                    switch (schemaItem.Defindex)
                    {
                        case 5000:
                            Bot.log.Success("User added a scrap metal.");//log metal added
                            UserWepsAdded += 2;//add weapon
                            UserMetalAdded++;//add metal
                            Bot.userCurrency.AddScrap();//add metal
                            break;//5000
                        case 5001:
                            Bot.log.Success("User added a reclaimed metal.");//log metal added
                            UserWepsAdded += 6;//add weapon
                            UserMetalAdded += 3;//add metal
                            Bot.userCurrency.AddRec();//add metal
                            break;//5001
                        case 5002:
                            Bot.log.Success("User added a refined metal.");//log metal added
                            UserWepsAdded += 18;//add weapon
                            UserMetalAdded += 9;//add metal
                            Bot.userCurrency.AddRef();//add metal
                            break;//5002
                    }//switch (schemaItem.defindex)
                }//else if (schemaItem.CraftMaterialType == "craft_bar" && ItemsBotAdded > 0)
                else
                {
                    Trade.SendMessage(String.Format("I will not accept the item {0} but if you wish to get rid of it I will take it.", schemaItem.ItemName));//show user it wont accept it
                    Bot.log.Warn("Unaccepted Item Added: " + ItemAddedMsg);//user added item unaccepted
                    Bot.log.Warn(schemaItem.ItemName);
                }//else
                CheckMetalCountInTrade();//check metal logic
            }//if (!ChooseDonate)
            else
            {
                Bot.log.Success("Donation: " + ItemAddedMsg);//log donation
            }//else
        }//OnTradeAddItem()

        //NON GLOBALIZED
        /// <summary>
        /// Triggered when a user removes an item from the trade
        /// </summary>
        /// <param name="schemaItem">Schham version of item</param>
        /// <param name="inventoryItem">Inventory version of item</param>
        public override void OnTradeRemoveItem(Schema.Item schemaItem, Inventory.Item inventoryItem)
        {
            string ItemRemovedMsg = String.Format("{0} {1} {2} {3}", inventoryItem.IsNotCraftable.ToString().ToLower() == "true" ? "NonCraftable" : "Craftable", clsFunctions.ConvertQualityToString(schemaItem.ItemQuality), schemaItem.ItemName, schemaItem.CraftMaterialType);

            if (!ChooseDonate)
            {
                if (schemaItem.CraftMaterialType == "weapon" || clsFunctions.WepBlackList.Contains(inventoryItem.Defindex))
                {
                    if (!inventoryItem.IsNotCraftable)
                    {
                        UserWepsAdded--;//remove weapon
                        Bot.log.Warn("User removed " + ItemRemovedMsg);//log item removed
                        Bot.userCurrency.RemoveWeapon();//remove weapon
                    }//if (!inventoryItem.IsNotCraftable)
                    else
                    {
                        Bot.log.Warn("Uncraftable item removed: " + ItemRemovedMsg);//log item removed
                    }//else
                }//if (schemaItem.CraftMaterialType == "weapon" || CheckBlackList(inventoryItem.defindex))
                else if (schemaItem.CraftMaterialType == "craft_bar" && ItemsBotAdded > 0)
                {
                    switch (schemaItem.Defindex)
                    {
                        case 5000:
                            Bot.log.Warn("User removed a scrap metal.");//show metal removed
                            UserWepsAdded -= 2;//remove weapon
                            UserMetalAdded--;//remove metal
                            Bot.userCurrency.RemoveScrap();//remove metal
                            break;//5000
                        case 5001:
                            Bot.log.Warn("User removed a reclaimed metal.");//show metal removed
                            UserWepsAdded -= 6;//remove weapon
                            UserMetalAdded -= 3;//remove metal
                            Bot.userCurrency.RemoveRec();//remove metal
                            break;//5001
                        case 5002:
                            Bot.log.Warn("User removed a refined metal.");//show metal removed
                            UserWepsAdded -= 18;//remove weapon
                            UserMetalAdded -= 9;//remove metal
                            Bot.userCurrency.RemoveRef();//remove metal
                            break;//5002
                    }//switch (schemaItem.defindex)
                }//else if (schemaItem.CraftMaterialType == "craft_bar" && ItemsBotAdded > 0)
                else
                {
                    Bot.log.Warn("Unaccepted Item Removed: " + ItemRemovedMsg);//log item removed
                }//else
                CheckMetalCountInTrade();//check metal
            }//if (!ChooseDonate)
            else
            {
                Bot.log.Warn("Donation removed: " + ItemRemovedMsg);//log donation removed
            }//else
        }//OnTradeRemoveItem();

        //GLOBALIZED
        /// <summary>
        /// Triggered when user sets ready status
        /// </summary>
        /// <param name="ready">Ready status</param>
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
                    Bot.log.Success("Readying trade.");//log
                    Trade.SetReady(true);//set ready
                }//if(Validate())
                else
                {
                    Bot.log.Warn("Invalid trade!");//log
                    Trade.SendMessage("Invalid trade!");//send error
                }//else
            }//else
        }//OnTradeReady()

        //GLOBALIZED
        /// <summary>
        /// Triggered when user accepts trade
        /// </summary>
        public override void OnTradeAccept()
        {
            if (Validate())
            {
                Bot.log.Success("Accepting trade...");//log bot is accepting the trade

                bool success;//success of the trade variable
                lock (this)
                {
                    success = Trade.AcceptTrade();//see if trade went through
                }//lock (this)
                if (success)
                {
                    Log.Success("Trade was successful!");//log
                    if (!ChooseDonate)
                    {
                        Bot.SteamFriends.SendChatMessage(OtherSID, EChatEntryType.ChatMsg, clsFunctions.SCRAPBANK_TRADE_COMPLETED_MESSAGE);//send trade message
                        clsFunctions.AddToTradeNumber(this.Bot);
                        
                        //Random rnd = new Random();
                        //if (rnd.Next(1, 10) <= 2)
                        //{
                        //    Bot.SteamFriends.SendChatMessage(OtherSID, EChatEntryType.ChatMsg, "If you are part of the Gamem Trading Services group, code: ScrapWoot may mean something to you... PLEASE DO NOT SHARE THIS!");
                        //}
                    }//if (!ChooseDonate)
                    Bot.UnReserveAllByUser(OtherSID);//remove reserved weapons from user
                    ItemsReserved = 0;//reset weapons reserved
                    ItemReserveHandler.Stop();
                }//if (success)
                else
                {
                    Log.Warn("Trade might have failed.");//log
                    Bot.SteamFriends.SendChatMessage(OtherSID, EChatEntryType.ChatMsg, "Did something go wrong with the trade? =C");//send ItemRemovedMsg
                }//else
            }//if (Validate())
            OnTradeClose();//close the trade
        }//OnTradeAccept()

        //GLOBALIZED
        /// <summary>
        /// Triggers when the trade has closed.
        /// </summary>
        public override void OnTradeClose()
        {
            ChooseDonate = false;//reset variable
            base.OnTradeClose();//close the trade
            Bot.SteamFriends.SetPersonaState(EPersonaState.Online);//Set status
            Bot.CheckBackpack();//Check backpack to see if items are there. Do this in case accepting lagged out, but it still went through.
            Bot.informHandler.Start();//Start the information handler.
        }//OnTradeClose()

        //GLOBALIZED
        /// <summary>
        /// Triggered when a message is received from the trade chat.
        /// </summary>
        /// <param name="message">message received</param>
        public override void OnTradeMessage(string message)
        {
            Bot.log.Warn("[TRADE MESSAGE] " + message);//Log user sent a trade message

            message = message.ToLower();//lowercase the message to deal with it easier

            if (message.Contains("backpack"))
            {
                Trade.SendMessage("Please type backpack in the regular chat window. Thank you!");//tell user to type that elsewhere.
                return;//stop code
            }//if (message.Contains("backpack"))

            else if (message.Contains("donate"))
            {
                Trade.SendMessage("Please type donate in the regular chat window. Thank you!");//tell user to type that elsewhere.
                return;//stop code
            }//else if (message.Contains("donate"))

            if (IsAdmin)
            {
                if (message == "donate")
                {
                    ChooseDonate = true;//admin override to donate
                    Trade.SendMessage("You are now donating");//tell admin status has been changed
                }//if (message == "donate")

                else
                {
                    clsFunctions.ProcessTradeMessage(message, this.Trade, this.Bot);//See if the message from the admin was a trade command.
                }//else
            }//if (ItemRemovedMsg.StartsWith("give") && IsAdmin && ItemRemovedMsg.Length > 5)
        }//OnTradeMessage()

        //NON GLOBALIZED
        /// <summary>
        /// TradeCurrencyPoll Method to add different types of metals
        /// </summary>
        public void Test()
        {
            TF2Currency change = Bot.userCurrency.GetChange(Bot.myCurrency);

            if (change.Neutral())
            {
                return;//no change to remove or add
            }
            if (change.Positive())
            {
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
                        Trade.SendMessage("Sorry. I do not have enough refined!");
                    }
                }

                #endregion Refined Adding

                #region Reclaimed Adding

                if (change.Reclaimed > 0)
                {
                    if (InventoryRec - Bot.myCurrency.Reclaimed > change.Reclaimed)
                    {
                        if (Bot.myCurrency.Reclaimed == 2)
                        {
                            if (InventoryRef - Bot.myCurrency.Refined - 1 >= 1)
                            {
                                uint RecRemoved = Trade.RemoveAllItemsByDefindex(5001, 2);
                                Bot.myCurrency.RemoveRec((int)RecRemoved);
                                if (RecRemoved == 2)
                                {
                                    Bot.log.Success(String.Format("Removed {0} reclaimed", RecRemoved));
                                    if (Trade.AddItemByDefindex(5002))
                                    {
                                        Bot.myCurrency.AddRef();
                                        Bot.log.Success("Added 1 refined.");
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
                            Bot.log.Success(String.Format("Added {0} rec", RecAdded));
                            if (RecAdded == change.Reclaimed)
                            {
                                return;
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

                #endregion Reclaimed Adding

                #region Scrap Adding

                if (change.Scrap > 0)
                {
                    if (InventoryScrap - Bot.myCurrency.Scrap >= change.Scrap)
                    {
                        if (Bot.myCurrency.Scrap == 2)
                        {
                            if (Bot.myCurrency.Reclaimed == 2)
                            {
                                if (InventoryRef - Bot.myCurrency.Refined >= 1)
                                {
                                    uint RecRemoved = Trade.RemoveAllItemsByDefindex(5001, 2);
                                    Bot.myCurrency.RemoveRec((int)RecRemoved);
                                    if (RecRemoved == 2)
                                    {
                                        Bot.log.Success(String.Format("Removed {0} reclaimed", RecRemoved));

                                        uint ScrapRemoved = Trade.RemoveAllItemsByDefindex(5000, 2);
                                        Bot.myCurrency.RemoveScrap((int)ScrapRemoved);
                                        if (ScrapRemoved == 2)
                                        {
                                            Bot.log.Success(String.Format("Removed {0} scrap.", ScrapRemoved));
                                            if (Trade.AddItemByDefindex(5002))
                                            {
                                                Bot.myCurrency.AddRef();
                                                Bot.log.Success("Added 1 refined.");
                                            }
                                            else
                                            {
                                                Trade.CancelTrade();
                                                Bot.SteamFriends.SendChatMessage(OtherSID, EChatEntryType.ChatMsg, "Sorry. Something went wrong. Please trade me again.");
                                                Bot.log.Error("Something went wrong removing 2 rec 2 scrap and adding a ref (adding ref part)");
                                            }
                                        }
                                        else
                                        {
                                            Trade.CancelTrade();
                                            Bot.SteamFriends.SendChatMessage(OtherSID, EChatEntryType.ChatMsg, "Sorry. Something went wrong. Please trade me again.");
                                            Bot.log.Error("Something went wrong removing 2 scrap and adding a ref (removing 2 scrap)");
                                        }
                                    }
                                }
                            }
                            else
                            {
                                if (InventoryRec - Bot.myCurrency.Reclaimed >= 1)
                                {
                                    uint ScrapRemoved = Trade.RemoveAllItemsByDefindex(5000, 2);
                                    Bot.myCurrency.RemoveScrap((int)ScrapRemoved);
                                    if (ScrapRemoved == 2)
                                    {
                                        Bot.log.Success(String.Format("Removed {0} scrap.", ScrapRemoved));

                                        if (Trade.AddItemByDefindex(5001))
                                        {
                                            Bot.myCurrency.AddRec();
                                            Bot.log.Success("Added 1 rec.");
                                        }
                                        else
                                        {
                                            Trade.CancelTrade();
                                            Bot.SteamFriends.SendChatMessage(OtherSID, EChatEntryType.ChatMsg, "Sorry. Something went wrong. Please trade me again.");
                                            Bot.log.Error("Something went wrong removing 2 rec 2 scrap and adding a ref (adding ref part)");
                                        }
                                    }
                                    else
                                    {
                                        Trade.CancelTrade();
                                        Bot.SteamFriends.SendChatMessage(OtherSID, EChatEntryType.ChatMsg, "Sorry. Something went wrong. Please trade me again.");
                                        Bot.log.Error("Something went wrong removing 2 scrap and adding a ref (removing 2 scrap)");
                                    }
                                }
                                else
                                {
                                    Trade.SendMessage("I don't have enough reclaimed!");
                                }
                            }
                        }
                        else
                        {
                            uint ScrapAdded = Trade.AddAllItemsByDefindex(5000, (uint)change.Scrap);
                            Bot.myCurrency.AddScrap((int)ScrapAdded);
                            if (ScrapAdded == change.Scrap)
                            {
                                Bot.log.Success(String.Format("Added {0} scrap.", ScrapAdded));
                            }
                            else
                            {
                                Trade.CancelTrade();
                                Bot.SteamFriends.SendChatMessage(OtherSID, EChatEntryType.ChatMsg, "Sorry. Something went wrong. Please trade me again.");
                                Bot.log.Error("Something went wrong adding scrap.");
                            }
                        }
                    }
                    else
                    {
                        Trade.SendMessage("Sorry. I do not have enough scrap!");
                    }
                }

                #endregion Scrap Adding
            }
            else if (change.Negative())
            {
                change.MakePositive();
                if (Bot.myCurrency.Weapon > 0 && Bot.myCurrency.Refined == 0 & Bot.myCurrency.Reclaimed == 0 && Bot.myCurrency.Scrap == 0)
                {
                    //user has items reserved so they haven't added enough metal yet.
                    return;
                }
                else if (Bot.myCurrency.Weapon >= 0 && (Bot.myCurrency.Refined > 0 || Bot.myCurrency.Reclaimed > 0 || Bot.myCurrency.Scrap > 0))
                {
                    if (change.Refined > 0)
                    {
                        if (Bot.myCurrency.Refined >= change.Refined)
                        {
                            uint RefRemoved = Trade.RemoveAllItemsByDefindex(5002, (uint)change.Refined);
                            Bot.myCurrency.RemoveRef((int)RefRemoved);
                            if (RefRemoved == change.Refined)
                            {
                                Bot.log.Success(String.Format("Removed {0} ref.", RefRemoved));
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
                    if (change.Reclaimed > 0)
                    {
                        if (Bot.myCurrency.Refined >= change.Refined)
                        {
                            uint RecRemoved = Trade.RemoveAllItemsByDefindex(5001, (uint)change.Reclaimed);
                            Bot.myCurrency.RemoveRec((int)RecRemoved);
                            if (RecRemoved == change.Reclaimed)
                            {
                                Bot.log.Success(String.Format("Removed {0} rec.", RecRemoved));
                            }
                            else
                            {
                                Trade.CancelTrade();
                                Bot.SteamFriends.SendChatMessage(OtherSID, EChatEntryType.ChatMsg, "Sorry. Something went wrong. Please trade me again.");
                                Bot.log.Error("Something went wrong removing rec");
                            }
                        }
                        else
                        {
                            Trade.CancelTrade();
                            Bot.SteamFriends.SendChatMessage(OtherSID, EChatEntryType.ChatMsg, "Sorry. Something went wrong. Please trade me again.");
                            Bot.log.Error("Something went wrong removing rec");
                        }
                    }
                    if (change.Scrap > 0)
                    {
                        if (Bot.myCurrency.Scrap >= change.Scrap)
                        {
                            uint ScrapRemoved = Trade.RemoveAllItemsByDefindex(5000, (uint)change.Scrap);
                            Bot.myCurrency.RemoveScrap((int)ScrapRemoved);
                            if (ScrapRemoved == change.Scrap)
                            {
                                Bot.log.Success(String.Format("Removed {0} scrap.", ScrapRemoved));
                            }
                            else
                            {
                                Trade.CancelTrade();
                                Bot.SteamFriends.SendChatMessage(OtherSID, EChatEntryType.ChatMsg, "Sorry. Something went wrong. Please trade me again.");
                                Bot.log.Error("Something went wrong removing scrap");
                            }
                        }
                        else
                        {
                            Trade.CancelTrade();
                            Bot.SteamFriends.SendChatMessage(OtherSID, EChatEntryType.ChatMsg, "Sorry. Something went wrong. Please trade me again.");
                            Bot.log.Error("Something went wrong removing scrap");
                        }
                    }                    
                    return;
                }//else if (Bot.myCurrency.Weapon >= 0 && (Bot.myCurrency.Refined > 0 || Bot.myCurrency.Reclaimed > 0 || Bot.myCurrency.Scrap > 0))
            }
        }//TradeCurrencyPoll()

        //NON GLOBALIZED
        /// <summary>
        /// The main logic of the trade. Calculates the number of scrap needed to remove or add in trades.
        /// </summary>
        /// <param name="error">only set if there is an internal error</param>
        public void CheckMetalCountInTrade()
        {
            int ScrapToAdd = 0;//scrap to add
            decimal ScrapAddNum = (decimal)(UserWepsAdded - ItemsBotAdded) / 2;//scrap to add math

            if ((UserWepsAdded - ItemsBotAdded) % 2 == 1)
            {
                ScrapAddNum -= 0.5M;//round down a weapon
            }//if ((UserWepsAdded - ItemsBotAdded) % 2 == 1)
            if (BotMetalAdded < (int)ScrapAddNum || BotMetalAdded > (int)ScrapAddNum)
            {
                ScrapToAdd = (int)ScrapAddNum - BotMetalAdded;//get metal to add/subtract
            }//if (BotMetalAdded < (int)ScrapAddNum || BotMetalAdded > (int)ScrapAddNum)
            if (ScrapToAdd > 0)
            {
                uint ScrapThatWasAdded = Trade.AddAllItemsByDefindex(5000, (uint)ScrapToAdd);//try to add the scrap we need
                BotMetalAdded += (int)ScrapThatWasAdded;//add the scrap added successfully to vars
                BotScrapAdded += (int)ScrapThatWasAdded;//add the scrap added successfully to vars
                if (ScrapThatWasAdded == ScrapToAdd)
                {
                    Bot.log.Success(String.Format("Added {0} scrap.", ScrapThatWasAdded));//log added scrap
                }//if (ScrapThatWasAdded == ScrapToAdd)
                else
                {
                    Bot.log.Warn(String.Format("Adding metal err. Needed to add {0} and only added {1}.", ScrapToAdd, ScrapThatWasAdded));//log error
                    Trade.SendMessage("I do not have enough metal! D; Sorry!");
                    Bot.log.Error("Not enough metal!");
                    return;
                }//else
            }//if (ScrapToAdd > 0)
            else if (ScrapToAdd < 0)
            {
                if (ItemsBotAdded > 0 && BotMetalAdded == 0)
                {
                    Trade.SendMessage(String.Format("Error. You must put up {0} scrap or equivalent in items. You have {1} scrap showing.", (double)ItemsBotAdded / (double)2, (double)UserWepsAdded / (double)2));//show info to user
                    Bot.log.Warn("User has not put up enough items to get weapons chosen.");//log info
                    return;//stop code
                }//if (ItemsBotAdded > 0 && BotMetalAdded == 0)
                uint ScrapToRemove = (uint)Math.Abs(ScrapToAdd);//scrap needed to remove (must be positive to do math right)
                uint ScrapRemoved = Trade.RemoveAllItemsByDefindex(5000, (uint)ScrapToRemove);//get scrap removed
                BotMetalAdded -= (int)ScrapRemoved;//save to vars
                BotScrapAdded -= (int)ScrapRemoved;//save to vars

                if (ScrapRemoved == ScrapToRemove)
                {
                    Bot.log.Success(String.Format("Removed {0} scrap.", ScrapRemoved));//if successful
                }//if (ScrapRemoved == ScrapToRemove)
                else
                {
                    Bot.log.Warn(String.Format("Removing metal err. Needed to remove {0} and only removed {1}", ScrapToRemove, ScrapRemoved));//log error
                    Bot.SteamFriends.SendChatMessage(OtherSID, EChatEntryType.ChatMsg, "Something went wrong. Please trade me again.");//show error
                    Trade.CancelTrade();//cancel trade
                }//else
            }//else if (ScrapToAdd < 0)
        }//CheckMetalInTrade

        //NON GLOBALIZED
        /// <summary>
        /// Used to reinitialize variables used in trades.
        /// </summary>
        public void ReInit()
        {
            Bot.myCurrency.Clear();//reset
            Bot.userCurrency.Clear();//reset
            UnknownItems = 0;//reset
            ItemsBotAdded = 0;//reset
            UserMetalAdded = 0;//reset
            UserWepsAdded = 0;//reset
            InventoryMetal = 0;//reset
            InventoryRec = 0;//reset
            InventoryBlacklistItem = 0;//reset
            InventoryWeps = 0;//reset
            InventoryRef = 0;//reset
            InventoryScrap = 0;//reset
            BotRefAdded = 0;//reset
            BotRecAdded = 0;//reset
            BotScrapAdded = 0;//reset
            BotMetalAdded = 0;//reset
        }//ReInit()

        //NON GLOBALIZED
        /// <summary>
        /// Called to count the bot's inventory levels.
        /// </summary>
        /// <param name="message">To send a message in trade about inventory levels</param>
        public void TradeCountInventory(bool message)
        {
            //Bot.MyInventory = Trade.MyInventory;//Get the inventory from Trade.
            foreach (Inventory.Item item in Bot.MyInventory.Items)
            {
                if (item.IsNotTradeable || item.IsNotCraftable)
                {
                    //Don't count the item
                }//if (item.IsNotTradeable || item.IsNotCraftable)

                else if (item.Defindex == 5000)
                {
                    InventoryMetal++;//add to total metal
                    InventoryScrap++;//add scrap
                }//else if (item.defindex == 5000)

                else if (item.Defindex == 5001)
                {
                    InventoryMetal += 3;//add to total metal
                    InventoryRec++;//add rec
                }//else if (item.defindex == 5001)

                else if (item.Defindex == 5002)
                {
                    InventoryMetal += 9;//add to total metal
                    InventoryRef++;//add ref
                }//else if (item.defindex == 5002)
                else if (clsFunctions.WepBlackList.Contains(item.Defindex) && !clsFunctions.schema.GetItem(item.Defindex).Name.Contains("Tux") ||
                    ((item.Quality != "6" && item.Defindex != 5000 & item.Defindex != 5001 && item.Defindex != 5002)))
                {
                    InventoryBlacklistItem++;//add item to blacklist
                }//else if (clsFunctions.WepBlackList.Contains(item.defindex) && !clsFunctions.schema.GetItem(item.defindex).Name.Contains("Tux") || ((item.Quality != "6" && item.defindex != 5000 & item.defindex != 5001 && item.defindex != 5002)))
                else if (clsFunctions.schema.GetItem(item.Defindex).CraftMaterialType == "weapon")
                {
                    InventoryWeps++;//add weapon
                }//else if (clsFunctions.schema.GetItem(item.defindex).CraftMaterialType == "weapon")
                else
                {
                    UnknownItems++;//item is unknown
                }//else
            }//foreach (Inventory.Item item in Bot.MyInventory.items)
            if (message)
            {
                if (!ChooseDonate)
                {
                    Trade.SendMessage(String.Format("Success. I have {0} scrap. Please put up the items you wish to bank.", InventoryScrap));//show scrap
                }//if (!ChooseDonate)

                else
                {
                    Trade.SendMessage("Inventory loaded! Please put up the items you wish to donate. Thank you ^^");//thank user for donating
                }//else

                Bot.log.Success("I have " + InventoryRef + " ref " + InventoryRec + " rec " + InventoryScrap + " scrap.");//log inventory levels
            }//if (message)
        }//TradeCountInventory()

        //NON GLOBALIZED
        /// <summary>
        /// Called to validate the trade
        /// </summary>
        /// <returns>If trade is okay or not</returns>
        public bool Validate()
        {
            if (ChooseDonate)
            {
                return true;//trade is fine
            }//if (ChooseDonate)

            List<string> errors = new List<string>();//List of errors

            int iCheckedWeps = 0;
            
            List<Inventory.Item> items = new List<Inventory.Item>();//holds items user has offered

            foreach (TradeUserAssets id in Trade.OtherOfferedItems)
            {
                if (id.appid == 440)
                    items.Add(Trade.OtherInventory.GetItem(id.assetid));//Get item
            }//foreach (ulong id in Trade.OtherOfferedItems)

            foreach (Inventory.Item item in items)
            {
                Schema.Item newitem = clsFunctions.schema.GetItem(item.Defindex);//get schema item information

                if (newitem.CraftMaterialType == "weapon" || clsFunctions.WepBlackList.Contains(newitem.Defindex))
                {
                    if (item.IsNotCraftable)
                    {
                        errors.Add(String.Format("{0} is not Craftable!", newitem.Name));//add error
                    }//if (item.IsNotCraftable)
                    else
                    {
                        iCheckedWeps++;//weapon is okay
                    }//else
                }//if (newitem.CraftMaterialType == "weapon" || clsFunctions.WepBlackList.Contains(newitem.defindex))

                else if (newitem.Defindex == 5000 && ItemsReserved > 0)
                {
                    iCheckedWeps += 2;//add to checked weapons
                }//else if (newitem.defindex == 5000 && ItemsReserved > 0)

                else if (newitem.Defindex == 5001 && ItemsReserved > 0)
                {
                    iCheckedWeps += 6;//add to checked weapons
                }//else if (newitem.defindex == 5001 && ItemsReserved > 0)

                else if (newitem.Defindex == 5002 && ItemsReserved > 0)
                {
                    iCheckedWeps += 18;//add to checked weapons
                }//else if (newitem.defindex == 5002 && ItemsReserved > 0)
                else if (newitem.ItemName.Contains("Mann Co. Supply Crate") && !newitem.ItemName.Contains("Key"))
                {
                    errors.Add("" + newitem.ItemName + " will not be accepted!");
                }
            }//foreach (Inventory.Item item in items)

            if (iCheckedWeps != UserWepsAdded)
            {
                ///This happens when something wasn't triggered
                Bot.log.Warn("Weapons in trade didn't match ones in the trade vars. Setting. " + UserWepsAdded + " before " + iCheckedWeps + " after.");//Log warning
                UserWepsAdded = iCheckedWeps;//reset weapons
                CheckMetalCountInTrade();//Recheck metal
            }//if (iCheckedWeps != UserWepsAdded)

            if (UserWepsAdded < ItemsBotAdded && ItemsReserved > 0)
            {
                errors.Add(String.Format("You must put up {0} scrap or equivalent in items.", (double)ItemsBotAdded / 2));//add error
                Bot.log.Warn("User has not put up enough items to get weapons chosen.");//log error
            }//if (UserWepsAdded < ItemsBotAdded && ItemsReserved > 0)

            CheckMetalCountInTrade();//recheck to make sure

            if (errors.Count > 0)
            {
                Trade.SendMessage("There are errors in your trade:");//tell user errors are present
            }//if (errors.Count > 0)

            foreach (string error in errors)
            {
                Trade.SendMessage(error);//send error
                Bot.log.Warn(String.Format("Trade Validation error: {0} with user {1}", error, Bot.SteamFriends.GetFriendPersonaName(Trade.OtherSID)));//log error
            }//foreach (string error in errors)

            return errors.Count == 0;//return if there are errors or not
        }//Validate()

        //GLOBALIZED
        /// <summary>
        /// Triggered when the user has not traded for their items in 6 minutes
        /// </summary>
        public void OnReserveElapsed()
        {
            Bot.SteamFriends.SendChatMessage(OtherSID, EChatEntryType.ChatMsg, "I'm sorry. You haven't traded for your items in 6 minutes. I have removed them.");//send message.
            Bot.UnReserveAllByUser(OtherSID);//remove all reserved items from user            
        }//OnReserveElapsed()

        /// <summary>
        /// Used to check if the hat is on the blacklist
        /// </summary>
        /// <param name="item">Inventory.Item item version</param>
        /// <param name="sReason">Reason the hat is not able to be sold</param>
        /// <returns>True if blacklisted false if not.</returns>
        public bool CheckBlackList(Inventory.Item item, out string sReason)
        {
            return clsFunctions.CheckWeaponBlacklist(item, out sReason);
        }

        /// <summary>
        /// Used to add the default currency to the bots currency var
        /// </summary>
        public void AddDefaultCurrency()
        {
            switch (Item_Type)
            {
                case "weapons":
                    Bot.myCurrency.AddWeapon();//add a weapon
                    break;//case "weapons"
                case "hats":
                    Bot.myCurrency.AddHat();//add a hat
                    break;//case "hats"
            }//switch (Item_Type)
        }//AddDefaultCurrency()


        /// <summary>
        /// Used to calculate the price in trades
        /// </summary>
        /// <returns>Price the user has to pay</returns>
        public double CalculatePrice()
        {
            double dePrice = 0;//price variable
            switch (Item_Type)
            {
                case "weapons":
                    //dePrice = ItemsBotAdded / 2;
                    TF2Currency tempcurn = new TF2Currency(0, 0, 0, 0, ItemsBotAdded);
                    dePrice = tempcurn.ToMetal();
                    break;//case "weapons"
                case "hats":
                    dePrice = clsFunctions.ConvertHatToMetal(ItemsBotAdded);
                    break;//case "hats"
                case "items":
                    TF2Currency tempCurn = new TF2Currency();
                    tempCurn.AddScrap(ItemsBotAdded);
                    dePrice = tempCurn.ToPrice();
                    break;//case "items"
            }//switch (Item_Type)
            return dePrice;
        }//CalculatePrice()
    }//class
}//namespace