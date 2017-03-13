//using System;
//using System.Collections.Generic;
//using System.Threading;
//using SteamKit2;
//using SteamKit2.Internal;
//using SteamTrade;

//namespace SteamBot
//{
//    public class NetworkTradingHandler
//    {
//        private Bot bot;

//        private BotType botType;

//        public bool Active = false;

//        public bool InTrade = false;

//        private Object lck = new Object();

//        public int WepsToGive = 0;

//        private Inventory inventory;

//        public NetworkTradingHandler(Bot bot, string type)
//        {
//            this.bot = bot;
//            switch (type)
//            {
//                case "ScrapbankUserHandler":
//                    this.botType = BotType.ScrapbankingBot;
//                    break;

//                case "AdminUserHandler":
//                    this.botType = BotType.AdminBot;
//                    break;

//                case "HatbankUserHandler":
//                    this.botType = BotType.HatbankingBot;
//                    break;

//                case "KeybankHandler":
//                    this.botType = BotType.KeybankingBot;
//                    break;

//                case "OneScrapUserHandler":
//                default:
//                    this.botType = BotType.OneScrapBot;
//                    break;
//            }
//        }

//        public void Start()
//        {
//            if (!Active)
//            {
//                Thread thread;

//                Active = true;
//            }
//        }

//        public void ScrapbankTradingHandler()
//        {
//            Monitor.Enter(this.lck);

//            inventory = Inventory.FetchInventory(bot.SteamUser.SteamID, bot.GetAPIKey());
            

//            Monitor.Exit(this.lck);
//        }
//    }
//}
