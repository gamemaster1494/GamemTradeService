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
    public class HatbankUserHandler : UserHandler
    {
        private ChatterBotFactory factory = new ChatterBotFactory();

        private ChatterBot chatterBot;

        private ChatterBotSession chatterBotsession;

        private int MAX_RESERVE = clsFunctions.SCRAPBANK_MAX_RESERVE;//Used to globalize code

        private int MAX_RESERVE_DONATOR = clsFunctions.SCRAPBANK_DOANTOR_RESERVE;//Used to globalize code

        private TF2Currency HatBuyPrice = new TF2Currency(0, 1, 1, 0, 0);

        private TF2Currency HatSellPrice = new TF2Currency(0, 1, 1, 0, 0);

        private string Item_Type = "hats";//Used to globalize code

        private string Default_Currency = "ref";//used to globalize code

        public FriendAddedHandler FriendAddedHandler;//Friend Added Handler

        public ItemReserveHandler ItemReserveHandler;//Item Reserve Handler

        private List<Inventory.Item> donatedItems = new List<Inventory.Item>();//items Donated

        //private TF2Currency Bot.myCurrency = new TF2Currency();//Bots currency

        //private TF2Currency Bot.userCurrency = new TF2Currency();//Users currency

        private int InventoryRec, InventoryRef, InventoryScrap, InventoryMetal, InventoryHat, UnknownItems, UserRefAdded, UserRecAdded, UserScrapAdded, ItemsBotAdded, UserHatsAdded, InventoryBlacklistItem;

        private int ItemsReserved;

        private bool ChooseDonate = false;

        private Dictionary<uint, int> InventoryCounts = new Dictionary<uint, int>();
        
        /// <summary>
        /// called when bot is made with this handle
        /// </summary>
        /// <param name="bot">bot assigned to</param>
        /// <param name="sid">SteamID of the other user</param>
        public HatbankUserHandler(Bot bot, SteamID sid) 
            : base(bot, sid) 
        {
            chatterBot = factory.Create(ChatterBotType.PANDORABOTS, "RANDOMDANCING");
            chatterBotsession = chatterBot.CreateSession();
            this.FriendAddedHandler = new SteamBot.FriendAddedHandler(bot, bot.BotControlClass.Substring(9), OtherSID);//Initialize
            this.ItemReserveHandler = new SteamBot.ItemReserveHandler(bot, bot.BotControlClass.Substring(9), OtherSID);//Initialize
            this.ItemReserveHandler.OnElapsed += this.OnReserveElapsed;//Set trigger method
            bot.GetInventory();//Get Inventory
        }//HatbankUserHandler()

        //GLOBALIZED
        /// <summary>
        /// Triggered when the bot gets a Group Invite
        /// </summary>
        /// <returns>True to accept invite, False to decline invite.</returns>
        public override bool OnGroupAdd()
        {
            return clsFunctions.DealWithGroupAdd(OtherSID, this.Bot.BotControlClass);
        }//OnClanAdd()


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
                        Bot.SteamFriends.SendChatMessage(OtherSID, type, String.Format("Sorry. You can currently reserve {0} {1} at a time. Donation in the future will increase this.)", MAX_RESERVE, Item_Type));//show max they can reserve
                        return;//stop code
                    }//if (clsFunctions.SCRAPBANK_MAX_RESERVE == ItemsReserved)
                }

                else
                {
                    if (MAX_RESERVE_DONATOR == ItemsReserved)
                    {
                        Bot.SteamFriends.SendChatMessage(OtherSID, type, String.Format("Sorry. As a donator,  you can only reserve {0} {1}.", MAX_RESERVE_DONATOR, Item_Type));
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
                    string MsgToSend = clsFunctions.DealWithGenericMessage(BackupMessage, OtherSID,this.Bot);//See if there is a response to the general ItemRemovedMsg
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
                if (clsFunctions.ScammerList.Contains(OtherSID.ConvertToUInt64()))
                    return false;
            }
            catch { }

            if (Bot.craftHandler.InGame)
            {
                Bot.SteamFriends.SendChatMessage(OtherSID, EChatEntryType.ChatMsg, "Sorry. I'm not available to trade. I'm currently crafting weapons. Sorry!");//show error
                return false;//making metal can't trade
            }//if (Bot.craftHandler.InGame)

            if (ItemsReserved == 0 && clsFunctions.OPERATION_FIRE_STORM)
            {
                Bot.SteamFriends.SendChatMessage(OtherSID, EChatEntryType.ChatMsg, "I'm sorry. Due to Operation Fire Storm, I am currently not buying hats.");
                return false;
            }

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
                Bot.SteamFriends.SendChatMessage(OtherSID, EChatEntryType.ChatMsg, "I'm sorry. I believe SteamAPI is down. Please try to trade again in a few minutes.");
                Bot.log.Warn("Issue getting inventory item. API down? Closing trade.");
                return;
            }
            string ItemAddedMsg = String.Format("User added {0} {1} {2} {3}", inventoryItem.IsNotCraftable.ToString().ToLower() == "true" ? "NonCraftable" : "Craftable", clsFunctions.ConvertQualityToString(schemaItem.ItemQuality), schemaItem.ItemName, schemaItem.CraftMaterialType); //ready ItemRemovedMsg
            if (!ChooseDonate)
            {
                if (schemaItem.CraftMaterialType == "hat" || (clsFunctions.CheckHatPrice(inventoryItem.Defindex, inventoryItem.Quality) && (schemaItem.ItemSlot == "misc" || schemaItem.ItemSlot == "head") && (inventoryItem.Quality != "13")))
                {
                    if (!clsFunctions.isRoboHat(schemaItem))
                    {
                        if (!clsFunctions.IsGifted(inventoryItem.Attributes))
                        {
                            if (!inventoryItem.IsNotCraftable && inventoryItem.Quality != "1")
                            {
                                if (InventoryCounts.ContainsKey(inventoryItem.Defindex))
                                {
                                    if (InventoryCounts[inventoryItem.Defindex] >= clsFunctions.HATBANK_MAX_ALLOWED_HATS)
                                    {
                                        Bot.SteamFriends.SendChatMessage(OtherSID, EChatEntryType.ChatMsg, "I'm sorry. But I have too many " + schemaItem.ItemName + ". I will not accept it.");
                                        Trade.SendMessage("I'm sorry. But I have too many " + schemaItem.ItemName + ". I will not accept it.");
                                    }
                                    else
                                    {
                                        Bot.log.Success(String.Format("User added {0} {1} {2} {3}", inventoryItem.IsNotCraftable.ToString().ToLower() == "true" ? "Uncraftable" : "Craftable", clsFunctions.ConvertQualityToString(schemaItem.ItemQuality), schemaItem.ItemName, schemaItem.CraftMaterialType));
                                        UserHatsAdded++;
                                        Bot.userCurrency.AddHat();
                                    }
                                }
                                else
                                {
                                    Bot.log.Success(String.Format("User added {0} {1} {2} {3}", inventoryItem.IsNotCraftable.ToString().ToLower() == "true" ? "Uncraftable" : "Craftable", clsFunctions.ConvertQualityToString(schemaItem.ItemQuality), schemaItem.ItemName, schemaItem.CraftMaterialType));
                                    UserHatsAdded++;
                                    Bot.userCurrency.AddHat();
                                }
                            }
                            else
                            {
                                if (inventoryItem.Quality == "1")
                                    Trade.SendMessage(String.Format("{0} is a genuine! I will not accept this!", schemaItem.ItemName));
                                else
                                    Trade.SendMessage(String.Format("{0} is not Craftable!", schemaItem.ItemName));
                                Bot.log.Warn(String.Format("User added {0} {1} {2} {3}", inventoryItem.IsNotCraftable.ToString().ToLower() == "true" ? "Uncraftable" : "Craftable", clsFunctions.ConvertQualityToString(schemaItem.ItemQuality), schemaItem.ItemName, schemaItem.CraftMaterialType));
                            }
                        }
                        else
                        {
                            Trade.SendMessage(String.Format("{0} is gifted! I do not accept gifted hats.", schemaItem.ItemName));
                            Bot.log.Warn("User added a gifted hat.");
                        }
                    }
                    else
                    {
                        Trade.SendMessage(String.Format("{0} is a robo hat! I will not accept these!", schemaItem.ItemName));
                    }
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
                TradeCurrencyPoll();
            }
            else
            {
                donatedItems.Add(inventoryItem);
                Bot.log.Success(String.Format("User added the donation of {0} {1} {2} {3}", inventoryItem.IsNotCraftable.ToString().ToLower() == "true" ? "Uncraftable" : "Craftable", clsFunctions.ConvertQualityToString(schemaItem.ItemQuality), schemaItem.ItemName, schemaItem.CraftMaterialType));
            }
        }
        
        //NON GLOBALIZED
        /// <summary>
        /// Triggered when a user removes an item from the trade
        /// </summary>
        /// <param name="schemaItem">Schham version of item</param>
        /// <param name="inventoryItem">Inventory version of item</param>
        public override void OnTradeRemoveItem(Schema.Item schemaItem, Inventory.Item inventoryItem)
        {
            if (!ChooseDonate)
            {
                if (!clsFunctions.isRoboHat(schemaItem) && schemaItem.CraftMaterialType == "hat" ||  (!clsFunctions.isRoboHat(schemaItem) && clsFunctions.CheckHatPrice(inventoryItem.Defindex, inventoryItem.Quality) && (schemaItem.ItemSlot == "misc" || schemaItem.ItemSlot == "head")))
                {                   
                    if (!clsFunctions.IsGifted(inventoryItem.Attributes))
                    {
                        if (!inventoryItem.IsNotCraftable && inventoryItem.Quality != "1")
                        {
                            if (InventoryCounts.ContainsKey(inventoryItem.Defindex))
                            {
                                if (InventoryCounts[inventoryItem.Defindex] >= clsFunctions.HATBANK_MAX_ALLOWED_HATS)
                                {
                                    Bot.log.Warn("User removed a hat that the bot had too much of.");
                                }
                                else
                                {
                                    Bot.log.Warn(String.Format("User removed {0} {1} {2} {3}", inventoryItem.IsNotCraftable.ToString().ToLower() == "true" ? "Uncraftable" : "Craftable", clsFunctions.ConvertQualityToString(schemaItem.ItemQuality), schemaItem.ItemName, schemaItem.CraftMaterialType));
                                    UserHatsAdded--;
                                    Bot.userCurrency.RemoveHat(); 
                                }
                            }
                            else
                            {
                                Bot.log.Warn(String.Format("User removed {0} {1} {2} {3}", inventoryItem.IsNotCraftable.ToString().ToLower() == "true" ? "Uncraftable" : "Craftable", clsFunctions.ConvertQualityToString(schemaItem.ItemQuality), schemaItem.ItemName, schemaItem.CraftMaterialType));
                                UserHatsAdded--;
                                Bot.userCurrency.RemoveHat(); 
                            }                                                   
                        }
                        else
                        {
                            Bot.log.Success(String.Format("User removed {0} {1} {2} {3}", inventoryItem.IsNotCraftable.ToString().ToLower() == "true" ? "Uncraftable" : "Craftable", clsFunctions.ConvertQualityToString(schemaItem.ItemQuality), schemaItem.ItemName, schemaItem.CraftMaterialType));
                        }
                    }
                    else
                    {
                        Bot.log.Warn("User removed gifted hat.");
                    }
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
                    Bot.log.Warn(String.Format("User removed non hat item: {0} {1} {2} {3}", inventoryItem.IsNotCraftable.ToString().ToLower() == "true" ? "Uncraftable" : "Craftable", clsFunctions.ConvertQualityToString(schemaItem.ItemQuality), schemaItem.ItemName, schemaItem.CraftMaterialType));
                }
                TradeCurrencyPoll();
            }
            else
            {
                donatedItems.Remove(inventoryItem);
                Bot.log.Warn(String.Format("User removed the donation of {0} {1} {2} {3}", inventoryItem.IsNotCraftable.ToString().ToLower() == "true" ? "Uncraftable" : "Craftable", clsFunctions.ConvertQualityToString(schemaItem.ItemQuality), schemaItem.ItemName, schemaItem.CraftMaterialType));
            }
        }

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
                        Bot.SteamFriends.SendChatMessage(OtherSID, EChatEntryType.ChatMsg, clsFunctions.HATBANK_TRADE_COMPLETED_MESSAGE);//send trade message
                        clsFunctions.AddToTradeNumber(this.Bot);
                        //Random rnd = new Random();
                        //if (rnd.Next(1, 10) <= 2)
                        //{
                        //    Bot.SteamFriends.SendChatMessage(OtherSID, EChatEntryType.ChatMsg, "If you are part of the Gamem Trading Services group, code: YayHatz may mean something to you... PLEASE DO NOT SHARE THIS!");
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
                Trade.SendMessage("Please type donate to a Scrapbanking Bot. Thank you!");//tell user to type that elsewhere.
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
        /// Called to check the metal and make sure everything is even in the trade.
        /// </summary>
        public void TradeCurrencyPoll()
        {
            //while (inTrade)
            //{                
            TF2Currency change = Bot.userCurrency.GetChange(Bot.myCurrency);

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
                        if (change.Refined == 1 && change.Reclaimed == 1 && InventoryRec - 4 >= 0)
                        {
                            uint RecAdded = Trade.AddAllItemsByDefindex(5001, 4);
                            Bot.myCurrency.AddRec((int)RecAdded);
                            if (RecAdded == 4)
                            {
                                Bot.log.Success("Added 4 rec!");
                                return;
                            }
                        }
                        else
                        {
                            Trade.SendMessage("Sorry. I do not have enough refined!");
                        }
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
                                uint RecRemoved = Trade.RemoveAllItemsByDefindex(5001, 2);
                                Bot.myCurrency.RemoveRec((int)RecRemoved);
                                if (RecRemoved == 2)
                                {
                                    Bot.log.Success(String.Format("Removed {0} reclaimed", RecRemoved));
                                    uint RefAdded = Trade.AddAllItemsByDefindex(5002, 1);

                                    if (RefAdded == 1)
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
                                            change.RemoveScrap(1);
                                            if (change.Scrap > 0)
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
                #endregion
            }
            else if (change.Negative())
            {
                change.MakePositive();
                //- change means bot needs to remove items.
                if (Bot.myCurrency.Hat > 0 && Bot.myCurrency.Refined == 0 && Bot.myCurrency.Reclaimed == 0 && Bot.myCurrency.Scrap == 0)
                {
                    Trade.SendMessage(String.Format("You must put up {0} ref or equivalent in hats.", clsFunctions.ConvertHatToMetal(ItemsBotAdded)));
                    Bot.log.Warn("User has not put up enough items to get hats chosen.");
                    return;
                }
                else if (Bot.myCurrency.Hat > 0 && (Bot.myCurrency.Refined > 0 || Bot.myCurrency.Reclaimed > 0 || Bot.myCurrency.Scrap > 0))
                {
                    if (Bot.myCurrency.Refined > 0)
                    {
                        uint RefRemoved = Trade.RemoveAllItemsByDefindex(5002, (uint)Bot.myCurrency.Refined);
                        if (RefRemoved == Bot.myCurrency.Refined)
                        {
                            Bot.myCurrency.RemoveRef((int)RefRemoved);
                            Bot.log.Success(String.Format("Removed {0} refined", RefRemoved));
                        }
                        else
                        {
                            Trade.CancelTrade();
                            Bot.SteamFriends.SendChatMessage(OtherSID, EChatEntryType.ChatMsg, "Sorry. Something went wrong. Please trade me again.");
                            Bot.log.Error("Something went wrong removing ref");
                        }
                    }
                    if (Bot.myCurrency.Reclaimed > 0)
                    {
                        uint RecRemoved = Trade.RemoveAllItemsByDefindex(5001, (uint)Bot.myCurrency.Reclaimed);
                        if (RecRemoved == Bot.myCurrency.Reclaimed)
                        {
                            Bot.myCurrency.RemoveRec((int)RecRemoved);
                            Bot.log.Success(String.Format("Removed {0} rec", RecRemoved));
                        }
                        else
                        {
                            Trade.CancelTrade();
                            Bot.SteamFriends.SendChatMessage(OtherSID, EChatEntryType.ChatMsg, "Sorry. Something went wrong. Please trade me again.");
                            Bot.log.Error("Something went wrong removing rec");
                        }
                    }
                }
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
                                int rectoadd = change.Reclaimed + ((int)RefRemoved * 3);
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

                if (change.Scrap > 0)
                {
                    if (Bot.myCurrency.Scrap < change.Scrap && Bot.myCurrency.Reclaimed > 0)
                    {
                        int iRecToRemove = (change.Scrap / 3) + 1;
                        uint RecRemoved = Trade.RemoveAllItemsByDefindex(5001, (uint)iRecToRemove);
                        if (RecRemoved == iRecToRemove)
                        {
                            Bot.myCurrency.RemoveRec((int)RecRemoved);
                            Bot.log.Success(String.Format("Removed {0} rec", RecRemoved));
                            change.RemoveScrap((int)RecRemoved * 3);
                            if (change.Scrap < 0)
                            {
                                uint ScrapAdded = Trade.AddAllItemsByDefindex(5000, (uint)Math.Abs(change.Scrap));
                                Bot.myCurrency.AddScrap((int)ScrapAdded);
                                if (ScrapAdded == Math.Abs(change.Scrap))
                                {
                                    Bot.log.Success(String.Format("Added {0} scrap.", ScrapAdded));
                                    change.AddScrap((int)ScrapAdded);
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
                            Trade.CancelTrade();
                            Bot.SteamFriends.SendChatMessage(OtherSID, EChatEntryType.ChatMsg, "Sorry. Something went wrong. Please trade me again.");
                            Bot.log.Error("Something went wrong removing rec");
                        }
                    }
                    else if (Bot.myCurrency.Scrap >= change.Scrap)
                    {
                        uint ScrapRemoved = Trade.RemoveAllItemsByDefindex(5000, (uint)change.Scrap);
                        Bot.myCurrency.RemoveScrap((int)ScrapRemoved);
                        if (ScrapRemoved == change.Scrap)
                        {
                            Bot.log.Success("Removed " + change.Scrap + " scrap!");
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
            }
        }

        //NON GLOBALIZED
        /// <summary>
        /// Used to reset trade vars
        /// </summary>
        public void ReInit()
        {
            Bot.myCurrency.Clear();
            Bot.userCurrency.Clear();
            InventoryRec = 0;
            InventoryRef = 0;
            InventoryScrap = 0;
            UserScrapAdded = 0;
            UserRecAdded = 0;
            UserRefAdded = 0;
            ItemsBotAdded = 0;
            UserHatsAdded = 0;
            InventoryHat = 0;
        }

        //NON GLOBALIZED
        /// <summary>
        /// Triggered to count the bots inventory at the beginning of the trade
        /// </summary>
        /// <param name="message">Send a message in trade about the current bot levels.</param>
        public void TradeCountInventory(bool message)
        {
            //Bot.MyInventory = Trade.MyInventory;//Get the inventory from the Trade
            InventoryCounts.Clear();
            foreach (Inventory.Item item in Bot.MyInventory.Items)
            {
                if (!InventoryCounts.ContainsKey(item.Defindex))
                {
                    InventoryCounts.Add(item.Defindex, 1);
                }
                else
                {
                    InventoryCounts[item.Defindex]++;
                }
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

                else if (clsFunctions.CheckHatPrice(item.Defindex, item.Quality))
                {
                    InventoryBlacklistItem++;//Add item to blacklist
                }//else if (clsFunctions.CheckHatPrice(item.defindex, item.Quality))

                else if (clsFunctions.schema.GetItem(item.Defindex).CraftMaterialType == "hat" || clsFunctions.schema.GetItem(item.Defindex).ItemSlot == "misc")
                {
                    InventoryHat++;//Add to hats
                }//else if (clsFunctions.schema.GetItem(item.defindex).CraftMaterialType == "hat" || clsFunctions.schema.GetItem(item.defindex).ItemSlot == "misc")
                
                else
                {
                    UnknownItems++;//Add to unknown items
                }//else

            }//foreach (Inventory.Item item in Bot.MyInventory.items)

            TF2Currency tempCurn = new TF2Currency(0, InventoryRef, InventoryRec, InventoryScrap);//Temporary currency to count hats the bot can buy

            if (message)
            {
                if (!ChooseDonate)
                {
                    Trade.SendMessage("Success! I can currently buy " + tempCurn.ToHats() + " hats.");//show hats to buy
                }//if (!ChooseDonate)
                else
                {
                    Trade.SendMessage("Success! Please add the items you wish to donate. Thank you ^^.");//tell user to proceed.
                }//else
                Bot.log.Success(String.Format("I have {0} ref {1} rec {2} scrap and {3} hats.", InventoryRef, InventoryRec, InventoryScrap, InventoryHat));//Log inventory
            }//if (message)
        }//TradeCountInventory()

        //NON GLOBALIZED
        /// <summary>
        /// Triggered to check the trade to make sure the trade is valid.
        /// </summary>
        /// <returns>True if trade is valid, false if not</returns>
        public bool Validate()
        {
            List<string> errors = new List<string>();//Contains errors
            Schema schema = Trade.CurrentSchema;//Schema
            int iCheckedHats = 0;//Hats checked
            int iCheckedScrap = 0;//scrap checked
            int iCheckedRec = 0;//rec checked
            int iCheckedRef = 0;//ref checked
            if (ChooseDonate)
            {
                return true;//trade is fine
            }//if (ChooseDonate)

            List<Inventory.Item> items = new List<Inventory.Item>();//List to contain other users items

            foreach (TradeUserAssets id in Trade.OtherOfferedItems)
            {
                if(id.appid == 440)
                items.Add(Trade.OtherInventory.GetItem(id.assetid));//Get item
            }//foreach (ulong id in Trade.OtherOfferedItems)

            foreach (Inventory.Item item in items)
            {
                Schema.Item newitem = schema.GetItem(item.Defindex);//Get item
                if (newitem.CraftMaterialType == "hat" || clsFunctions.CheckHatPrice(item.Defindex, item.Quality))
                {
                    if (item.IsNotCraftable || clsFunctions.IsGifted(item.Attributes) || item.Quality == "1")
                    {
                        errors.Add(String.Format("{0} is not acceptable!", newitem.Name));//Add error
                    }//if (item.IsNotCraftable || clsFunctions.IsGifted(item.Attributes) || item.Quality == "1")
                    else if (clsFunctions.isRoboHat(newitem))
                    {
                        errors.Add(String.Format("{0} is a robo hat! I do not accept these!", newitem.ItemName));//add error
                    }//else if (clsFunctions.isRoboHat(newitem))
                    else
                    {
                        iCheckedHats++;//Add to checked hats
                    }//else
                }//if (newitem.CraftMaterialType == "hat" || clsFunctions.CheckHatPrice(item.defindex, item.Quality))
                else if (newitem.Defindex == 5000 && ItemsReserved > 0)
                {
                    iCheckedScrap++;//Add to checked scrap
                }//else if (newitem.defindex == 5000 && ItemsReserved > 0)
                else if (newitem.Defindex == 5001 && ItemsReserved > 0)
                {
                    iCheckedRec++;//Add to checked rec
                }//else if (newitem.defindex == 5001 && ItemsReserved > 0)
                else if (newitem.Defindex == 5002 && ItemsReserved > 0)
                {
                    iCheckedRef++;//Add to checked ref
                }//else if (newitem.defindex == 5002 && ItemsReserved > 0)
                else if (newitem.ItemName.ToLower().Contains("supply crate") && !newitem.Name.ToLower().Contains("key"))
                {
                    errors.Add("" + newitem.Name + " will not be accepted!");//Add unaccepted item
                }//else if (newitem.Name.ToLower().Contains("supply crate") && !newitem.Name.ToLower().Contains("key"))
            }//foreach (Inventory.Item item in items)

            if (iCheckedScrap != UserScrapAdded)
            {
                Bot.log.Warn("Scrap count in trade didn't match ones in the trade vars. Setting. " + UserScrapAdded + " before " + iCheckedScrap + " after.");//Log error
                Bot.SteamFriends.SendChatMessage(OtherSID, EChatEntryType.ChatMsg, "Sorry. Something went wrong in trade. Please trade me again.");//Show user error.
                Trade.CancelTrade();//Cancel Trade
                return false;//Not valid trade
            }//if (iCheckedScrap != UserScrapAdded)
            if (iCheckedRec != UserRecAdded)
            {
                Bot.log.Warn("Rec count in trade didn't match ones in the trade vars. Setting. " + UserRecAdded + " before " + iCheckedRec + " after.");//Log error
                Bot.SteamFriends.SendChatMessage(OtherSID, EChatEntryType.ChatMsg, "Sorry. Something went wrong in trade. Please trade me again.");//Show user error.
                Trade.CancelTrade();//Cancel Trade
                return false;//Not valid trade
            }//if (iCheckedRec != UserRecAdded)
            if (iCheckedRef != UserRefAdded)
            {
                Bot.log.Warn("Ref count in trade didn't match ones in the trade vars. Setting. " + UserRefAdded + " before " + iCheckedRef + " after.");//Log error
                Bot.SteamFriends.SendChatMessage(OtherSID, EChatEntryType.ChatMsg, "Sorry. Something went wrong in trade. Please trade me again.");//Show user error.
                Trade.CancelTrade();//Cancel Trade
                return false;//Not valid trade
            }//if (iCheckedRef != UserRefAdded)

            TF2Currency change = Bot.userCurrency.GetChange(Bot.myCurrency);//Used to calculate change

            if (!change.Neutral() && ItemsBotAdded > 0)
            {
                errors.Add(String.Format("You must put up {0} ref or equivalent in hats.", clsFunctions.ConvertHatToMetal(ItemsBotAdded)));//Show error
                Bot.log.Warn("User has not put up enough items to get hats chosen.");//Log error
            }//if (!change.Neutral() && ItemsBotAdded > 0)

            TradeCurrencyPoll();//Recheck metal counts
            if (errors.Count > 0)
            {
                Trade.SendMessage("There are errors in your trade:");//Tell user errors
                foreach (string error in errors)
                {
                    Trade.SendMessage(error);//Send error
                    Bot.log.Warn(String.Format("Trade Validation error: {0} with user {1}", error, Bot.SteamFriends.GetFriendPersonaName(Trade.OtherSID)));//Log error
                }//foreach (string error in errors)
            }//if (errors.Count > 0)

            return errors.Count == 0;//If no errors its true, otherwise, false
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



        public bool CheckBlackList(Inventory.Item item, out string sReason)
        {
            sReason = "";
            try
            {
                
                Schema.Item sitem = (Schema.FetchSchema(Bot.GetAPIKey()).GetItem(item.Defindex));
                if (item.IsNotTradeable)
                {
                    sReason = "That item is not tradable.";
                    return false;
                }
                if (sitem.Name.ToLower().Contains("tux"))
                {
                    sReason = "That item is not for sale.";
                    return false;
                }
                if (sitem.CraftMaterialType == "craft_bar")
                {
                    sReason = "You can't buy metal!";
                    return false;
                }
                if (clsFunctions.CheckHatPrice(item.Defindex, item.Quality))
                {
                    sReason = "I'm sorry. This item is worth more than a craft hat. I am programmed to keep it in case the user that banked this realizes it was an accident.";
                    return false;
                }
                return true;
            }
            catch (Exception)
            {
                sReason = "Unknown error occurred. Please try again or contact my owner.";
                return false;
            }
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
                case "items":
                    Bot.myCurrency.AddScrap();//add a scrap
                    break;//case "items"
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
                    dePrice = ItemsBotAdded / 2;
                    break;//case "weapons"
                case "hats":
                    dePrice = clsFunctions.ConvertHatToMetal(ItemsBotAdded);
                    break;//case "hats"                    
            }//switch (Item_Type)
            return dePrice;
        }//CalculatePrice()
    }//class
}//namespace