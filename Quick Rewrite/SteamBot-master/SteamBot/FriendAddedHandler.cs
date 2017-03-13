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
    public class FriendAddedHandler
    {
        private Bot bot;

        public BotType botType { get; private set; }

        private bool bStarted = false;

        private SteamID OtherSID;

        public FriendAddedHandler(Bot bot, string type, SteamID otherSID)
        {
            this.bot = bot;
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
                case "DonationBotUserHandler":
                    botType = BotType.DonationBot;
                    break;
                default:
                    this.botType = BotType.OneScrapBot;
                    break;
            }
            this.OtherSID = otherSID;
        }

        public void Start()
        {
            if (!bStarted)
            {
                Thread thread;
                thread = new Thread(new ThreadStart(FriendAddedMessage));
                thread.Start();
                bStarted = true;
            }
        }

        public void FriendAddedMessage()
        {
            Thread.Sleep(clsFunctions.FriendAddedMessageInterval);
            string MessageToSend = clsFunctions.GetFriendAddedMessage(this.botType);
            if (this.botType == BotType.KeybankingBot)
                bot.SteamFriends.SendChatMessage(OtherSID, EChatEntryType.ChatMsg, String.Format(MessageToSend, clsFunctions.GetKeySellPrice(), clsFunctions.GetKeyBuyPrice()));
            else
                bot.SteamFriends.SendChatMessage(OtherSID, EChatEntryType.ChatMsg, MessageToSend);
            bStarted = false;
            bot.log.Success("Friend added message sent!");
        }
    }
}
