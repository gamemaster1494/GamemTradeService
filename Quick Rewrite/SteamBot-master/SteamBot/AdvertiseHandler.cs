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
    public class AdvertiseHandler
    {
        private Bot bot;

        public BotType botType { get; private set; }

        public bool bStarted = false;

        private SteamID OtherSID;

        public AdvertiseHandler(Bot bot, string type, SteamID otherSID)
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
                thread = new Thread(new ThreadStart(Advertise));
            }
        }

        public void Stop()
        {
            if (bStarted)
                bStarted = false;
        }

        public void Advertise()
        {
            bStarted = true;
            while (bStarted)
            {
                Thread.Sleep(clsFunctions.AdvertiseInverval);
                string MessageToSend = clsFunctions.GetAdvertiseMessage(this.botType);
                if (this.botType == BotType.KeybankingBot)
                    bot.SteamFriends.SendChatMessage(OtherSID, EChatEntryType.ChatMsg, String.Format(MessageToSend, clsFunctions.GetKeySellPrice(), clsFunctions.GetKeyBuyPrice()));
                else
                    bot.SteamFriends.SendChatMessage(OtherSID, EChatEntryType.ChatMsg, MessageToSend);
            }
        }

    }
}
