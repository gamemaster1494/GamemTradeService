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
    public class ItemReserveHandler
    {
        private Bot bot;

        public BotType botType { get; private set; }

        public bool bStarted = false;

        private SteamID OtherSID;

        private int MinutesUntilDone = clsFunctions.ItemReserveInternval / 60000;

        public delegate void OnReserveElapsed();

        public event OnReserveElapsed OnElapsed;

        public ItemReserveHandler(Bot bot, string type, SteamID otherSID)
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
                thread = new Thread(new ThreadStart(ReserveTimer));
                thread.Start();
                bStarted = true;
            }
        }

        public void Stop()
        {
            if (bStarted)
                bStarted = false;
        }

        public void ReserveTimer()
        {
            bStarted = true;
            while (bStarted)
            {
                Thread.Sleep(TimeSpan.FromMinutes(1));
                if (bStarted)
                {
                    MinutesUntilDone--;
                    if (MinutesUntilDone == 1)
                    {
                        bot.SteamFriends.SendChatMessage(OtherSID, EChatEntryType.ChatMsg, "Are you AFK? You only have 1 minute until I release your items.");
                    }
                    if (MinutesUntilDone == 0)
                    {
                        bStarted = false;
                        OnElapsed();
                    }
                }
            }
        }
    }
}
