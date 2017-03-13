using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SteamTrade.TradeWebAPI;
using SteamTrade;
using SteamKit2;
using ChatterBotAPI;
using System.Text.RegularExpressions;
using System.Threading;

namespace SteamBot
{
    public class TradingCardUserHandler : UserHandler
    {
        //private TF2Currency tfScrapToBuyCards = clsFunctions.tfPriceToBuyCards;
        //private TF2Currency tfScrapToSellCards = clsFunctions.tfPriceToSellCards;
        private GenericInventory mySteamInventory = new GenericInventory();
        private GenericInventory OtherSteamInventory = new GenericInventory();
        public FriendAddedHandler FriendAddedHandler;//Friend Added Handler

        public ItemReserveHandler ItemReserveHandler;//Item Reserve Handler
        private ChatterBot chatterBot;

        private ChatterBotSession chatterBotsession;
        private string Item_Type = "cards";

        
        private int MAX_RESERVE = clsFunctions.CARDBANK_MAX_RESERVE;//Used to globalize code

        private int MAX_RESERVE_DONATOR = clsFunctions.CARDBANK_DONATOR_RESERVE;//Used to globalize code

        private int UserMetalAdded, BotScrapAdded, BotRecAdded, BotRefAdded, BotMetalAdded;
        private int InventoryMetal, InventoryScrap, InventoryRec, InventoryRef;

        private int ItemsBotAdded = 0;

        private int ItemsReserved = 0;

        private bool ChooseDonate = false;

//[AutomatedBot] TestBot: Object AppID: 753 7:08:52 AM
//[AutomatedBot] TestBot: Object ContextId: 6 7:08:52 AM
//[AutomatedBot] TestBot: Steam Inventory Item Added. 7:08:53 AM
//[AutomatedBot] TestBot: Type: Beat Hazard Foil Trading Card 7:08:53 AM
//[AutomatedBot] TestBot: Marketable: Yes 7:08:53 AM

        //All appID is 753
        //All ContextID is 6
        //Type is going to be the key. If it says Trading Card its a card, if it says Emoticon, its a emote.
        public TradingCardUserHandler(Bot bot, SteamID sid)
            : base(bot, sid)
        {
            chatterBot = clsFunctions.factory.Create(ChatterBotType.PANDORABOTS, "b0dafd24ee35a477");
            chatterBotsession = chatterBot.CreateSession();
            this.FriendAddedHandler = new SteamBot.FriendAddedHandler(bot, bot.BotControlClass.Substring(9), OtherSID);//Initialize
            this.ItemReserveHandler = new SteamBot.ItemReserveHandler(bot, bot.BotControlClass.Substring(9), OtherSID);//Initialize
            this.ItemReserveHandler.OnElapsed += this.OnReserveElapsed;//Set trigger method
            bot.GetInventory();//Get Inventory
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

        public override void OnLoginCompleted()
        {
        }

        public override void OnChatRoomMessage(SteamID chatID, SteamID sender, string message)
        {
            Log.Info(Bot.SteamFriends.GetFriendPersonaName(sender) + ": " + message);
            base.OnChatRoomMessage(chatID, sender, message);
        }

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

        public override void OnMessage(string message, EChatEntryType type)
        {
            //string tempmsg = message.ToLower();

            //if (IsAdmin && message == "collect")
            //{                
            //    bCollecting = true;
            //    Bot.SteamTrade.Trade(OtherSID);
            //    return;
            //}
            //if (message == "backpack")
            //{
            //    Bot.SteamFriends.SendChatMessage(OtherSID, type, Bot.BackpackURL);
            //    return;
            //}
            //else if (message == "clear")
            //{
            //    Bot.UnReserveAllByUser(OtherSID);
            //    Bot.SteamFriends.SendChatMessage(OtherSID,EChatEntryType.ChatMsg,"Items cleared!");
            //    return;
            //}
            //else if (message.StartsWith(Bot.BackpackURL) && message.Length > Bot.BackpackURL.Length)
            //{
            //    //http://steamcommunity.com/id/gamemaster1494/inventory/#753_6_236385945
            //    string[] sLines = message.Split('_');
            //    if (!Bot.dReserved.ContainsKey(Convert.ToUInt64(sLines[2])))
            //    {
            //        List<long> contextID = new List<long>();



            //        contextID.Add(1);
            //        contextID.Add(6);
            //        mySteamInventory.load(753,contextID,Bot.SteamClient.SteamID);
            //        if (mySteamInventory == null)
            //        {
            //            mySteamInventory.load(753, contextID, Bot.SteamClient.SteamID);

            //            if (mySteamInventory == null)
            //            {
            //                Bot.SteamFriends.SendChatMessage(OtherSID, type, "I'm sorry. SteamAPI may be down. I cannot seem to retrieve my backpack. Please try again later.");
            //                return;
            //            }
            //        }
                                    
            //        GenericInventory.ItemDescription tmpDescrip = mySteamInventory.getDescription(Convert.ToUInt64(sLines[2]));

            //        if (tmpDescrip == null)
            //        {
            //            Bot.GetInventory();
            //            tmpDescrip = mySteamInventory.getDescription(Convert.ToUInt64(sLines[2]));
            //            if (tmpDescrip == null)
            //            {
            //                Bot.SteamFriends.SendChatMessage(OtherSID, type, "Sorry. I do not have that item anymore. Please refresh my inventory.");//Item already traded. Tell user to refresh.
            //                return;//stop code
            //            }
            //        }

            //        Bot.dReserved.Add(Convert.ToUInt64(sLines[2]), OtherSID);
            //        ItemsReserved++;
            //        Bot.SteamFriends.SendChatMessage(OtherSID, type, "Item reserved! Trade when ready.");
            //        Bot.log.Success(String.Format("{0} reserved {1}", Bot.SteamFriends.GetFriendPersonaName(OtherSID), tmpDescrip.name));//log item reserved
            //    }
            //    else
            //    {
            //        if (Bot.dReserved[Convert.ToUInt64(sLines[2])].ConvertToUInt64() == OtherSID.ConvertToUInt64())
            //        {
            //            Bot.SteamFriends.SendChatMessage(OtherSID, type, "You have already reserved this item silly!");//they already reserved this item
            //        }//if (Bot.dReserved[Convert.ToUInt64(sLines[2])].ConvertToUInt64() == OtherSID.ConvertToUInt64())
            //        else
            //        {
            //            Bot.SteamFriends.SendChatMessage(OtherSID, type, "Sorry! Someone else has reserved this item...");//someone already reserved this item
            //        }//else
            //    }//else
            //    return;
            //}
            //Bot.SteamFriends.SendChatMessage(OtherSID, type, Bot.ChatResponse);
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

                    Bot.dReserved.Add(Convert.ToUInt64(sLines[2]), OtherSID);//add item to reserved list
                    ItemsReserved++;//add to weapons reserved
                    Bot.SteamFriends.SendChatMessage(OtherSID, type, "Item Reserved! Trade when ready.");//tell user item was reserved
                    Bot.log.Success(String.Format("{0} reserved {1}", Bot.SteamFriends.GetFriendPersonaName(OtherSID), clsFunctions.schema.GetItem(item.Defindex).Name));//log item reserved
                    this.ItemReserveHandler.Start();
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
        }

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
        /// Triggered when an error occurs in trade.
        /// </summary>
        /// <param name="error">Error description</param>
        public override void OnTradeError(string error)
        {
            Bot.log.Error(error);//log trade error
        }//OnTradeError()

        public override void OnTradeTimeout()
        {
            Bot.SteamFriends.SendChatMessage(OtherSID, EChatEntryType.ChatMsg,
                                              "Sorry, but you were AFK and the trade was canceled.");
            Bot.log.Info("User was kicked because he was AFK.");
        }

        public void CheckMetal()
        {
            int ScrapToAdd = 0;//scrap to add
            decimal ScrapAddNum = (decimal)(Bot.userCurrency.ToScrap() - Bot.myCurrency.ToScrap());//scrap to add math

            if (BotMetalAdded < (int)ScrapAddNum || BotMetalAdded > (int)ScrapAddNum)
            {
                ScrapToAdd = (int)ScrapAddNum - BotMetalAdded;//get metal to add/subtract
            }
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
                    Trade.SendMessage(String.Format("Error. You must put up {0} scrap or equivalent in items. You have {1} scrap showing.", Bot.userCurrency.ToScrap(),Bot.userCurrency.ToScrap()));//show info to user
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
                    Trade.SendMessage(String.Format("You must put up {0} ref or equivalent in cards.", clsFunctions.ConvertHatToMetal(ItemsBotAdded)));
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


        public override void OnTradeInit()
        {
            ReInit();
            Bot.SteamFriends.SetPersonaState(EPersonaState.Busy);//set status to busy
            Bot.log.Success("Trade started!");//log trade started

            Trade.SendMessage("Please wait while I load our inventories.");
            TradeCountInventory();
            List<long> contextID = new List<long>();



            contextID.Add(1);
            contextID.Add(6);

            mySteamInventory.load(753, contextID, Bot.SteamClient.SteamID);
            Thread.Sleep(3000);
            OtherSteamInventory.load(753, contextID, OtherSID);
            Thread.Sleep(5000);
            if (!mySteamInventory.isLoaded)
            {                               
                Bot.SteamFriends.SendChatMessage(OtherSID, EChatEntryType.ChatMsg, "Error opening my inventory.");
                Bot.CloseTrade();
                Bot.log.Warn("Unable to obtain my inventory.. Closed the trade.");
                foreach (string error in mySteamInventory.errors)
                {
                    Console.WriteLine(error);
                }
                return;
            }

            if (!OtherSteamInventory.isLoaded)
            {
                Bot.SteamFriends.SendChatMessage(OtherSID, EChatEntryType.ChatMsg, "Error opening your inventory.");
                Bot.CloseTrade();
                Bot.log.Warn("Unable to obtain their inventory.. Closed the trade.");
                foreach (string error in OtherSteamInventory.errors)
                {
                    Console.WriteLine(error);
                }
                return;
            }


            if (Trade != null && !ChooseDonate && ItemsReserved > 0)
            {
                int iItemSuccess = 0;
                foreach (KeyValuePair<ulong, SteamID> pair in Bot.dReserved)
                {
                    if (pair.Value == Trade.OtherSID)
                    {
                        if (Trade.AddItem(pair.Key, 753,6))
                        {
                            iItemSuccess++;
                            Bot.myCurrency.AddScrap((int)clsFunctions.tfPriceToSellCards.ToScrap());
                        }
                    }
                }
                ItemsBotAdded = iItemSuccess;
                if (ItemsReserved == ItemsBotAdded)
                {
                    Trade.SendMessage(String.Format("Success. I have added {0} items. You must put up {1} scrap.", ItemsReserved, (int)clsFunctions.tfPriceToSellCards.ToScrap() * ItemsReserved));//tell user price
                    Bot.log.Success(ItemsBotAdded + " Items added!");//log items added
                }//if (ItemsReserved == ItemsBotAdded)
                else
                {
                    Trade.SendMessage(String.Format("Semi-Success. I have added {0} items of the {1} you reserved... You can close trade and try to trade again or put up {2} scrap.", ItemsBotAdded, ItemsReserved, (int)clsFunctions.tfPriceToSellCards.ToScrap() * ItemsBotAdded));//tell user there was error but allow them to pay # it added
                    Bot.log.Success(ItemsBotAdded + " Items added out of " + ItemsReserved);//show items added out of # it should
                }//else
            }//if itemsreserverd > 0
            else
            {
                Trade.SendMessage("Success! Please add trading cards for me to buy!");
            }
            if (ChooseDonate && IsAdmin)
            {
                Trade.SendMessage("Trade started. Please enter commands for me to do things.");
                Trade.SendMessage("Type !help for commands.");
            }
            

        }

        public override void OnTradeAddItem(Schema.Item schemaItem, Inventory.Item inventoryItem) 
        {

            switch (inventoryItem.AppId)
            {
                case 440:
                    if (schemaItem.CraftMaterialType == "craft_bar" && ItemsBotAdded > 0)
                    {
                        switch (schemaItem.Defindex)
                        {
                            case 5000:
                                Bot.log.Success("User added a scrap metal.");//log metal added
                                Bot.userCurrency.AddScrap();//add metal
                                break;//5000
                            case 5001:
                                Trade.SendMessage("I only accept scrap!");
                                break;//5001
                            case 5002:
                                Trade.SendMessage("I only accept scrap!");
                                break;//5002
                        }//switch (schemaItem.defindex)
                    }
                    break;

                case 753:
                    GenericInventory.ItemDescription tmpDescrip = OtherSteamInventory.getDescription(inventoryItem.Id);
                    if (tmpDescrip.type.ToLower().Contains("trading card"))
                    {
                        Bot.log.Success("User added a trading card!");
                        Bot.userCurrency.AddScrap((int)clsFunctions.tfPriceToBuyCards.ToScrap());
                        
                    }
                    break;

                default:
                    Trade.SendMessage("Invalid item added! I only accept metal and cards!");
                    break;
            }
            CheckMetal();
        }

        public override void OnTradeRemoveItem(Schema.Item schemaItem, Inventory.Item inventoryItem) 
        {
            switch (inventoryItem.AppId)
            {
                case 440:
                    if (schemaItem.CraftMaterialType == "craft_bar" && ItemsBotAdded > 0)
                    {
                        switch (schemaItem.Defindex)
                        {
                            case 5000:
                                Bot.log.Warn("User removed a scrap metal.");//log metal added
                                Bot.userCurrency.RemoveScrap();//add metal
                                break;//5000
                            case 5001:
                                Trade.SendMessage("I only accept scrap!");
                                break;//5001
                            case 5002:
                                Trade.SendMessage("I only accept scrap!");
                                break;//5002
                        }//switch (schemaItem.defindex)
                    }
                    break;

                case 753:
                    GenericInventory.ItemDescription tmpDescrip = OtherSteamInventory.getDescription(inventoryItem.Id);
                    if (tmpDescrip.type.ToLower().Contains("trading card"))
                    {
                        Bot.log.Warn("User removed a trading card!");
                        Bot.userCurrency.RemoveScrap((int)clsFunctions.tfPriceToBuyCards.ToScrap());
                    }
                    break;

                default:
                    
                    break;
            }
            CheckMetal();
        }

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
        

        public static void ProcessTradeMessage(string message, Trade Trade, Bot Bot)
        {
            if (message.StartsWith(clsFunctions.AddCmd))
                clsFunctions.HandleAddCommand(message, Bot.CurrentTrade, Bot);
            else if (message.StartsWith(clsFunctions.RemoveCmd))
                clsFunctions.HandleRemoveCommand(message, Trade);
        }

        public static void PrintHelpMessage(Trade Trade)
        {
            Trade.SendMessage(String.Format("{0} {1} - adds all crates", clsFunctions.AddCmd, clsFunctions.AddCratesSubCmd));
            Trade.SendMessage(String.Format("{0} {1} - adds all metal", clsFunctions.AddCmd, clsFunctions.AddMetalSubCmd));
            Trade.SendMessage(String.Format("{0} {1} - adds all weapons", clsFunctions.AddCmd, clsFunctions.AddWepsSubCmd));
            Trade.SendMessage(String.Format(@"{0} <craft_material_type> [amount] - adds all or a given amount of items of a given crafing type.", clsFunctions.AddCmd));
            Trade.SendMessage(String.Format(@"{0} <defindex> [amount] - adds all or a given amount of items of a given defindex.", clsFunctions.AddCmd));

            Trade.SendMessage(@"See http://wiki.teamfortress.com/wiki/WebAPI/GetSchema for info about craft_material_type or defindex.");
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

        public override void OnTradeSuccess()
        {
            ChooseDonate = false;//reset variable
            base.OnTradeClose();//close the trade            
            Bot.SteamFriends.SetPersonaState(EPersonaState.Online);//Set status
            Bot.CheckBackpack();//Check backpack to see if items are there. Do this in case accepting lagged out, but it still went through.
            Bot.informHandler.Start();//Start the information handler.
        }

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

        public bool Validate()
        {

            List<string> errors = new List<string>();

            if (!ChooseDonate)
            {
                return true;
            }

            int iCheckedScrap = 0;
            int iUserScrap = 0;
            
            foreach (TradeUserAssets asset in Trade.OtherOfferedItems)
            {
                
               switch(asset.appid)
               {
                   case 440:
                       Inventory.Item item = Trade.OtherInventory.GetItem(asset.assetid);
                       Schema.Item schemaItem = Trade.CurrentSchema.GetItem(item.Defindex);
                       if (schemaItem.CraftMaterialType == "craft_bar")
                       {
                           switch (item.Defindex)
                           {
                               case 5000:
                                   iUserScrap += 1;
                                   break;
                               case 5001:
                                   errors.Add("I do not accept reclaimed! I only accept scrap!");
                                   break;
                               case 5002:
                                   errors.Add("I do not accept refined! I only accept scrap!");
                                   break;
                           }
                       }
                       else
                       {
                           errors.Add("Invalid item! I will not accept " + schemaItem.ItemName + "!");
                       }
                       break;
                   case 753:

                       GenericInventory.ItemDescription tmpDescrip = OtherSteamInventory.getDescription(asset.assetid);
                       if (tmpDescrip.type.ToLower().Contains("trading card"))
                       {
                           iUserScrap += (int)clsFunctions.tfPriceToBuyCards.ToScrap();
                       }
                       break;

                   default:
                       break;
                     
               }
            }

            foreach (TradeUserAssets assets in Trade.MyOfferedItems)
            {
                switch (assets.appid)
                {
                    case 440:
                        Inventory.Item item = Trade.MyInventory.GetItem(assets.assetid);
                        Schema.Item schemaItem = Trade.CurrentSchema.GetItem(item.Defindex);
                        if (schemaItem.CraftMaterialType == "craft_bar")
                        {
                            switch (item.Defindex)
                            {
                                case 5000:
                                    iCheckedScrap += 1;
                                    break;
                                case 5001:
                                    iCheckedScrap += 3;
                                    break;
                                case 5002:
                                    iCheckedScrap += 9;
                                    break;
                            }
                        }
                        break;
                    case 753:

                        GenericInventory.ItemDescription tmpDescrip = mySteamInventory.getDescription(assets.assetid);
                        if (tmpDescrip.type.ToLower().Contains("trading card"))
                        {
                            iCheckedScrap += (int)clsFunctions.tfPriceToSellCards.ToScrap();
                        }
                        break;
                }
            }

            if (Bot.userCurrency.ToScrap() != iUserScrap)
            {
                Bot.log.Warn("Scrap user had in trade didn't mach vars. setting " + Bot.userCurrency.ToScrap() + " before " + iUserScrap + " after.");
                Bot.userCurrency = new TF2Currency(0,0,0,iUserScrap,0);
            }

            if(iUserScrap < iCheckedScrap && ItemsReserved > 0)
            {
                errors.Add(String.Format("You must put up {0} scrap or equivalent in items.", iCheckedScrap));//add error
                Bot.log.Warn("User has not put up enough items to get cards chosen.");//log error
            }

            if (iCheckedScrap > iUserScrap)
            {
                errors.Add("You do not have enough metal added! You must have " + iCheckedScrap + " for these items.");
            }


            // send the errors
            if (errors.Count != 0)
                Trade.SendMessage("There were errors in your trade: ");
            foreach (string error in errors)
            {
                Trade.SendMessage(error);
            }

            return errors.Count == 0;
        }

        public void ReInit()
        {
            Bot.myCurrency.Clear();//reset
            Bot.userCurrency.Clear();//reset
            ItemsBotAdded = 0;//reset
            UserMetalAdded = 0;//reset
            InventoryMetal = 0;//reset
            InventoryRec = 0;//reset
            InventoryRef = 0;//reset
            InventoryScrap = 0;//reset
            BotRefAdded = 0;//reset
            BotRecAdded = 0;//reset
            BotScrapAdded = 0;//reset
            BotMetalAdded = 0;//reset

        }//ReInit()

                //GLOBALIZED
        /// <summary>
        /// Triggered when the user has not traded for their items in 6 minutes
        /// </summary>
        public void OnReserveElapsed()
        {
            Bot.SteamFriends.SendChatMessage(OtherSID, EChatEntryType.ChatMsg, "I'm sorry. You haven't traded for your items in 6 minutes. I have removed them.");//send message.
            Bot.UnReserveAllByUser(OtherSID);
        }


        public void TradeCountInventory()
        {
            try
            {
                foreach (Inventory.Item item in Trade.MyInventory.Items)
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

                }//foreach (Inventory.Item item in Bot.MyInventory.items)
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
