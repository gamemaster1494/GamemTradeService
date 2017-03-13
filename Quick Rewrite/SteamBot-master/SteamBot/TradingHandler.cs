using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SteamKit2;
using SteamTrade;
using System.Threading;
using Newtonsoft.Json;

namespace SteamBot
{
    public class TradingHandler
    {

        private Bot Bot;//the bot itself.

        public BotType botType;//type of bot it is

        public int InventoryRef;

        public int InventoryRec;

        public int InventoryScrap;

        private bool bStarted = false;//started or not

        private int Interval = 1000;//interval in ms the trade will poll

        private SteamID OtherSID;//SteamID the bot is trading

        public TradingHandler(Bot bot, string type, SteamID otherSID)
        {
            this.Bot = bot;
            switch (type)
            {
                case "ScrapbankUserHandler":
                    this.botType = BotType.ScrapbankingBot;
                    break;

                case "AdminUserHandler":
                    this.botType = BotType.AdminBot;
                    break;

                case "HatbankUserHandler":
                    this.botType = BotType.HatbankingBot;
                    break;

                case "OneScrapUserHandler":
                    botType = BotType.OneScrapBot;
                    break;

                case "OneWepUserHandler":
                    botType = BotType.OneWepBot;
                    break;

                case "KeybankHandler":
                    botType = BotType.KeybankingBot;
                    break;

                default:
                    this.botType = BotType.OneScrapBot;
                    break;
            }
            this.OtherSID = otherSID;
        }//TradingHandler()


        public void Stop()
        {
            bStarted = false;//stop the thread
        }//Stop()

        public void ScrapbankMethod()
        {
            while (bStarted)
            {
                Thread.Sleep(1000);

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
                            uint RefAdded = Bot.CurrentTrade.AddAllItemsByDefindex(5002, (uint)change.Refined);
                            Bot.myCurrency.AddRef((int)RefAdded);
                            if (RefAdded == change.Refined)
                            {
                                Bot.log.Success(String.Format("Added {0} refined.", RefAdded));
                            }
                        }
                        else
                        {
                            Bot.CurrentTrade.SendMessage("Sorry. I do not have enough refined!");
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
                                    uint RecRemoved = Bot.CurrentTrade.RemoveAllItemsByDefindex(5001, 2);
                                    Bot.myCurrency.RemoveRec((int)RecRemoved);
                                    if (RecRemoved == 2)
                                    {
                                        Bot.log.Success(String.Format("Removed {0} reclaimed", RecRemoved));
                                        if (Bot.CurrentTrade.AddItemByDefindex(5002))
                                        {
                                            Bot.myCurrency.AddRef();
                                            Bot.log.Success("Added 1 refined.");
                                        }
                                        else
                                        {
                                            Bot.CurrentTrade.CancelTrade();
                                            Bot.SteamFriends.SendChatMessage(OtherSID, EChatEntryType.ChatMsg, "Sorry. Something went wrong. Please trade me again.");
                                            Bot.log.Error("Something went wrong removing 2 rec and adding a ref(adding ref part)");
                                        }
                                    }
                                }
                                else
                                {
                                    uint RecAdded = Bot.CurrentTrade.AddAllItemsByDefindex(5001, (uint)change.Reclaimed);
                                    Bot.myCurrency.AddRec((int)RecAdded);
                                    if (RecAdded == change.Reclaimed)
                                    {
                                        Bot.log.Success(String.Format("Added {0} rec", RecAdded));
                                    }
                                    else
                                    {
                                        Bot.CurrentTrade.CancelTrade();
                                        Bot.SteamFriends.SendChatMessage(OtherSID, EChatEntryType.ChatMsg, "Sorry. Something went wrong. Please trade me again.");
                                        Bot.log.Error("Couldn't add reclaimed");
                                    }
                                }
                            }
                            else
                            {
                                uint RecAdded = Bot.CurrentTrade.AddAllItemsByDefindex(5001, (uint)change.Reclaimed);
                                Bot.myCurrency.AddRec((int)RecAdded);
                                Bot.log.Success(String.Format("Added {0} rec", RecAdded));
                                if (RecAdded == change.Reclaimed)
                                {
                                    return;
                                }
                                else
                                {
                                    Bot.CurrentTrade.CancelTrade();
                                    Bot.SteamFriends.SendChatMessage(OtherSID, EChatEntryType.ChatMsg, "Sorry. Something went wrong. Please trade me again.");
                                    Bot.log.Error("Couldn't add reclaimed");
                                }
                            }
                        }
                        else
                        {
                            Bot.CurrentTrade.SendMessage("Sorry. I do not have enough reclaimed!");
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
                                        uint RecRemoved = Bot.CurrentTrade.RemoveAllItemsByDefindex(5001, 2);
                                        Bot.myCurrency.RemoveRec((int)RecRemoved);
                                        if (RecRemoved == 2)
                                        {
                                            Bot.log.Success(String.Format("Removed {0} reclaimed", RecRemoved));

                                            uint ScrapRemoved = Bot.CurrentTrade.RemoveAllItemsByDefindex(5000, 2);
                                            Bot.myCurrency.RemoveScrap((int)ScrapRemoved);
                                            if (ScrapRemoved == 2)
                                            {
                                                Bot.log.Success(String.Format("Removed {0} scrap.", ScrapRemoved));
                                                if (Bot.CurrentTrade.AddItemByDefindex(5002))
                                                {
                                                    Bot.myCurrency.AddRef();
                                                    Bot.log.Success("Added 1 refined.");
                                                }
                                                else
                                                {
                                                    Bot.CurrentTrade.CancelTrade();
                                                    Bot.SteamFriends.SendChatMessage(OtherSID, EChatEntryType.ChatMsg, "Sorry. Something went wrong. Please trade me again.");
                                                    Bot.log.Error("Something went wrong removing 2 rec 2 scrap and adding a ref (adding ref part)");
                                                }
                                            }
                                            else
                                            {
                                                Bot.CurrentTrade.CancelTrade();
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
                                        uint ScrapRemoved = Bot.CurrentTrade.RemoveAllItemsByDefindex(5000, 2);
                                        Bot.myCurrency.RemoveScrap((int)ScrapRemoved);
                                        if (ScrapRemoved == 2)
                                        {
                                            Bot.log.Success(String.Format("Removed {0} scrap.", ScrapRemoved));

                                            if (Bot.CurrentTrade.AddItemByDefindex(5001))
                                            {
                                                Bot.myCurrency.AddRec();
                                                Bot.log.Success("Added 1 rec.");
                                            }
                                            else
                                            {
                                                Bot.CurrentTrade.CancelTrade();
                                                Bot.SteamFriends.SendChatMessage(OtherSID, EChatEntryType.ChatMsg, "Sorry. Something went wrong. Please trade me again.");
                                                Bot.log.Error("Something went wrong removing 2 rec 2 scrap and adding a ref (adding ref part)");
                                            }
                                        }
                                        else
                                        {
                                            Bot.CurrentTrade.CancelTrade();
                                            Bot.SteamFriends.SendChatMessage(OtherSID, EChatEntryType.ChatMsg, "Sorry. Something went wrong. Please trade me again.");
                                            Bot.log.Error("Something went wrong removing 2 scrap and adding a ref (removing 2 scrap)");
                                        }
                                    }
                                    else
                                    {
                                        Bot.CurrentTrade.SendMessage("I don't have enough reclaimed!");
                                    }
                                }
                            }
                            else
                            {
                                uint ScrapAdded = Bot.CurrentTrade.AddAllItemsByDefindex(5000, (uint)change.Scrap);
                                Bot.myCurrency.AddScrap((int)ScrapAdded);
                                if (ScrapAdded == change.Scrap)
                                {
                                    Bot.log.Success(String.Format("Added {0} scrap.", ScrapAdded));
                                }
                                else
                                {
                                    Bot.CurrentTrade.CancelTrade();
                                    Bot.SteamFriends.SendChatMessage(OtherSID, EChatEntryType.ChatMsg, "Sorry. Something went wrong. Please trade me again.");
                                    Bot.log.Error("Something went wrong adding scrap.");
                                }
                            }
                        }
                        else
                        {
                            Bot.CurrentTrade.SendMessage("Sorry. I do not have enough scrap!");
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
                                uint RefRemoved = Bot.CurrentTrade.RemoveAllItemsByDefindex(5002, (uint)change.Refined);
                                Bot.myCurrency.RemoveRef((int)RefRemoved);
                                if (RefRemoved == change.Refined)
                                {
                                    Bot.log.Success(String.Format("Removed {0} ref.", RefRemoved));
                                }
                                else
                                {
                                    Bot.CurrentTrade.CancelTrade();
                                    Bot.SteamFriends.SendChatMessage(OtherSID, EChatEntryType.ChatMsg, "Sorry. Something went wrong. Please trade me again.");
                                    Bot.log.Error("Something went wrong removing ref");
                                }
                            }
                            else
                            {
                                Bot.CurrentTrade.CancelTrade();
                                Bot.SteamFriends.SendChatMessage(OtherSID, EChatEntryType.ChatMsg, "Sorry. Something went wrong. Please trade me again.");
                                Bot.log.Error("Something went wrong removing ref");
                            }
                        }
                        if (change.Reclaimed > 0)
                        {
                            if (Bot.myCurrency.Refined >= change.Refined)
                            {
                                uint RecRemoved = Bot.CurrentTrade.RemoveAllItemsByDefindex(5001, (uint)change.Reclaimed);
                                Bot.myCurrency.RemoveRec((int)RecRemoved);
                                if (RecRemoved == change.Reclaimed)
                                {
                                    Bot.log.Success(String.Format("Removed {0} rec.", RecRemoved));
                                }
                                else
                                {
                                    Bot.CurrentTrade.CancelTrade();
                                    Bot.SteamFriends.SendChatMessage(OtherSID, EChatEntryType.ChatMsg, "Sorry. Something went wrong. Please trade me again.");
                                    Bot.log.Error("Something went wrong removing rec");
                                }
                            }
                            else
                            {
                                Bot.CurrentTrade.CancelTrade();
                                Bot.SteamFriends.SendChatMessage(OtherSID, EChatEntryType.ChatMsg, "Sorry. Something went wrong. Please trade me again.");
                                Bot.log.Error("Something went wrong removing rec");
                            }
                        }
                        if (change.Scrap > 0)
                        {
                            if (Bot.myCurrency.Scrap >= change.Scrap)
                            {
                                uint ScrapRemoved = Bot.CurrentTrade.RemoveAllItemsByDefindex(5000, (uint)change.Scrap);
                                Bot.myCurrency.RemoveScrap((int)ScrapRemoved);
                                if (ScrapRemoved == change.Scrap)
                                {
                                    Bot.log.Success(String.Format("Removed {0} scrap.", ScrapRemoved));
                                }
                                else
                                {
                                    Bot.CurrentTrade.CancelTrade();
                                    Bot.SteamFriends.SendChatMessage(OtherSID, EChatEntryType.ChatMsg, "Sorry. Something went wrong. Please trade me again.");
                                    Bot.log.Error("Something went wrong removing scrap");
                                }
                            }
                            else
                            {
                                Bot.CurrentTrade.CancelTrade();
                                Bot.SteamFriends.SendChatMessage(OtherSID, EChatEntryType.ChatMsg, "Sorry. Something went wrong. Please trade me again.");
                                Bot.log.Error("Something went wrong removing scrap");
                            }
                        }
                        return;
                    }//else if (Bot.myCurrency.Weapon >= 0 && (Bot.myCurrency.Refined > 0 || Bot.myCurrency.Reclaimed > 0 || Bot.myCurrency.Scrap > 0))
                }
            }

        }

        public void KeybankMethod()
        {

        }

        public void OneScrapMethod()
        {

        }

        public void OneWepMethod()
        {

        }

    }//class
}//namespace
