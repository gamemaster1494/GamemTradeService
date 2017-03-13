using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SteamKit2;
using SteamTrade;
using System.Threading;
using Newtonsoft.Json;
//using SteamBot;

namespace SteamBot
{
    public class InformHandler
    {
        private Bot bot;

        public BotType botType { get; private set; }

        private bool bStarted = false;

        private Inventory inventory;

        private Schema schema;

        private bool bCounted = false;

        private DateTime timeCounted = DateTime.Now;        

        private double ScrapMetal;
        private double ReclaimedMetal;
        private double RefinedMetal;
        private double Hats;
        private double Items;
        private double Unknown;
        private double Weapons;
        private double Blacklist;
        private double BotTotal;
        private double Keys;

        public InformHandler(Bot bot, string type)
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

                case "KeybankHandler":
                    this.botType = BotType.KeybankingBot;
                    break;

                case "OneScrapUserHandler":
                default:
                    this.botType = BotType.OneScrapBot;
                    break;
            }
            this.schema = Schema.FetchSchema(bot.GetAPIKey());
        }

        public void Start()
        {
            if(!bStarted)
            {
                Thread thread;
                if (botType == BotType.ScrapbankingBot)
                    thread = new Thread(new ThreadStart(ScrapbankingMethod));
                else if (botType == BotType.HatbankingBot)
                    thread = new Thread(new ThreadStart(HatbankingMethod));
                else if (botType == BotType.OneScrapBot)
                    thread = new Thread(new ThreadStart(OneScrapBankingMethod));
                else if (botType == BotType.KeybankingBot)
                    thread = new Thread(new ThreadStart(KeybankMethod));
                else
                    return;
                ScrapMetal = 0;
                ReclaimedMetal = 0;
                RefinedMetal = 0;
                Hats = 0;
                Items = 0;
                Unknown = 0;
                Weapons = 0;
                Blacklist = 0;
                BotTotal = 0;
                Keys = 0;
                bCounted = false;
                bStarted = true;    
                thread.Start();                 
            }
        }

        public string AdminStatsMessage()
        {
            try
            {
                while (!bCounted) ;

                string RequestString = "";

                if (botType == BotType.ScrapbankingBot)
                {
                    RequestString = String.Format("I have {0} ref {1} rec {2} scrap {3} weapons {4} blacklist {5} unknown ({6})", RefinedMetal, ReclaimedMetal, ScrapMetal, (Weapons / 0.5), (Blacklist / 0.5), Unknown, BotTotal);
                }
                else if (botType == BotType.HatbankingBot)
                {
                    RequestString = String.Format("I have {0} ref {1} rec {2} scrap {3} hats {4} blacklist {5} unknown ({6})", RefinedMetal, ReclaimedMetal, ScrapMetal, Hats, Blacklist, Unknown, BotTotal);
                }
                else if (botType == BotType.OneScrapBot)
                {
                    RequestString = String.Format("I have {0} ref {1} rec {2} scrap {3} items to sell ({4})", RefinedMetal, ReclaimedMetal, ScrapMetal, Items, BotTotal);
                }
                else if (botType == BotType.KeybankingBot)
                {
                    RequestString = String.Format("I have {0} Keys {1} ref {2} rec {3} scrap ({4})", Keys, RefinedMetal, ReclaimedMetal, ScrapMetal, BotTotal);
                }

                return RequestString;
            }
            catch
            {
                return "ERROR!";
            }
        }

        public string UserStatsMessage()
        {
            try
            {
                while (bStarted && !bCounted) ;

                string RequestString = "";

                if (botType == BotType.ScrapbankingBot)
                {
                    RequestString = String.Format("I have {0} scrap and {1} weapons currently available.", (ScrapMetal + (ReclaimedMetal * 3) + (RefinedMetal * 9)), Weapons);
                }
                else if (botType == BotType.HatbankingBot)
                {
                    RequestString = String.Format("I can buy {0} hats and currently have {1} hats for sale.", GetHatBotSets(), Hats);
                }
                else if (botType == BotType.KeybankingBot)
                {
                    RequestString = String.Format("I can buy {0} keys and currently have {1} keys for sale.", GetKeySets(), Keys);
                }

                return RequestString;
            }
            catch
            {
                return "ERROR!";
            }
        }

        public void ScrapbankingMethod()
        {
            try
            {
                Thread.Sleep(5000);
                inventory = Inventory.FetchInventory(bot.SteamClient.SteamID.ConvertToUInt64(), bot.GetAPIKey());
                if (inventory == null)
                {
                    inventory = Inventory.FetchInventory(bot.SteamClient.SteamID.ConvertToUInt64(), bot.GetAPIKey());
                    if (inventory == null)
                    {
                        bStarted = false;
                        return;
                    }
                }
                foreach (Inventory.Item item in inventory.Items)
                {
                    if (item.IsNotTradeable)
                    {

                    }
                    else if (item.IsNotCraftable)
                    {
                        Blacklist += 0.5;
                    }
                    else if (item.Defindex == 5000)
                    {
                        ScrapMetal += 1;
                    }
                    else if (item.Defindex == 5001)
                    {
                        ReclaimedMetal += 1;
                    }
                    else if (item.Defindex == 5002)
                    {
                        RefinedMetal += 1;
                    }
                    else if (clsFunctions.WepBlackList.Contains(item.Defindex) || item.Quality != "6")
                    {
                        Blacklist += 0.5;
                    }
                    else if (schema.GetItem(item.Defindex).CraftMaterialType == "weapon")
                    {
                        Weapons += 0.5;
                    }
                    else
                    {
                        Unknown++;
                    }
                }
                timeCounted = DateTime.Now;
                bCounted = true;
                BotTotal = ScrapMetal + (ReclaimedMetal * 3) + (RefinedMetal * 9) + Weapons + Blacklist;
                //if (BotTotal < clsFunctions.SCRAPBANK_INVENTORY_LEVEL)
                //{
                //    bot.SteamFriends.SendChatMessage(clsFunctions.BotsOwnerID, EChatEntryType.ChatMsg, "Problem! I have " + BotTotal + " when i should have " + clsFunctions.SCRAPBANK_INVENTORY_LEVEL + "!");
                //}
                if (BotTotal > clsFunctions.SCRAPBANK_INVENTORY_LEVEL + 3)
                {
                    bot.SteamFriends.SendChatMessage(clsFunctions.BotsOwnerID, EChatEntryType.ChatMsg, "Hey, I have extra items.");
                }
                if (Blacklist > 0)
                {
                    bot.SteamFriends.SendChatMessage(clsFunctions.BotsOwnerID, EChatEntryType.ChatMsg, "Hey, I have " + (Blacklist / 0.5) + " blacklisted items.");
                }
                if (Unknown > 0)
                {
                    bot.SteamFriends.SendChatMessage(clsFunctions.BotsOwnerID, EChatEntryType.ChatMsg, "Hey, I have " + Unknown + " unknown items.");
                }

                if (ReclaimedMetal > 0 || RefinedMetal > 0)
                {
                    if (!bot.craftHandler.InGame && bot.CurrentTrade == null)
                    {
                        bot.craftHandler.Start(CraftingType.ScrapbankMetalToScrap);
                    }
                    //bot.SteamFriends.SendChatMessage(clsFunctions.BotsOwnerID, EChatEntryType.ChatMsg, "I have ref or rec!");
                }
                else if (ScrapMetal < 20) //|| (QuarryRef > 0 || QuarryRec > 0))
                {
                    if (!bot.craftHandler.InGame && bot.CurrentTrade == null)
                    {
                        bot.craftHandler.Start(CraftingType.ScrapbankSmeltWeps);
                    }
                }
                bStarted = false;
            }
            catch
            {
                bCounted = false;
                bStarted = false;
            }
        }

        public void HatbankingMethod()
        {
            try
            {
                Thread.Sleep(5000);
                inventory = Inventory.FetchInventory(bot.SteamClient.SteamID.ConvertToUInt64(), bot.GetAPIKey());
                if (inventory == null)
                {
                    inventory = Inventory.FetchInventory(bot.SteamClient.SteamID.ConvertToUInt64(), bot.GetAPIKey());
                    if (inventory == null)
                    {
                        bStarted = false;
                        return;
                    }
                }
                foreach (Inventory.Item item in inventory.Items)
                {
                    if (item.IsNotTradeable)
                    {

                    }
                    else if (item.Defindex == 5000)
                    {
                        ScrapMetal += 1;
                    }
                    else if (item.Defindex == 5001)
                    {
                        ReclaimedMetal += 1;
                    }
                    else if (item.Defindex == 5002)
                    {
                        RefinedMetal += 1;
                    }
                    else if (clsFunctions.CheckHatPrice(item.Defindex, item.Quality))
                    {
                        Blacklist += 1;
                    }
                    else if (schema.GetItem(item.Defindex).CraftMaterialType == "hat")
                    {
                        Hats += 1;
                    }
                    else
                    {
                        Unknown += 1;
                    }
                }
                BotTotal = GetHatBotValue();
                bCounted = true;
                timeCounted = DateTime.Now;
                bool test = false;
                if (BotTotal.ToString().Contains(clsFunctions.HATBANK_INVENTORY_LEVEL.ToString()))
                    test = true;
                if (BotTotal < clsFunctions.HATBANK_INVENTORY_LEVEL && test == false && !clsFunctions.OPERATION_FIRE_STORM)
                {
                    bot.SteamFriends.SendChatMessage(clsFunctions.BotsOwnerID, EChatEntryType.ChatMsg, String.Format("Problem! I have {0} inventory value when i should have {1}!", BotTotal, clsFunctions.HATBANK_INVENTORY_LEVEL));
                }
                if (Blacklist > 0)
                {
                    bot.SteamFriends.SendChatMessage(clsFunctions.BotsOwnerID, EChatEntryType.ChatMsg, "Hey, I have " + Blacklist + " blacklisted items.");
                }
                if (Unknown > 0)
                {
                    bot.SteamFriends.SendChatMessage(clsFunctions.BotsOwnerID, EChatEntryType.ChatMsg, "Hey, I have " + Unknown + "unknown items.");
                }

                if (ReclaimedMetal < 4)
                {
                    if (!bot.craftHandler.InGame && bot.CurrentTrade == null)
                    {
                        bot.craftHandler.Start(CraftingType.HatbankCrafting);
                    }
                }

                bStarted = false;
            }
            catch
            {
                bCounted = false;
                bStarted = false;
            }
        }

        public void OneScrapBankingMethod()
        {
            try
            {
                Thread.Sleep(5000);
                inventory = Inventory.FetchInventory(bot.SteamClient.SteamID.ConvertToUInt64(), bot.GetAPIKey());
                if (inventory == null)
                {
                    inventory = Inventory.FetchInventory(bot.SteamClient.SteamID.ConvertToUInt64(), bot.GetAPIKey());
                    if (inventory == null)
                    {
                        bStarted = false;
                        return;
                    }
                }
                foreach (Inventory.Item item in inventory.Items)
                {
                    if (item.IsNotTradeable)
                    {

                    }
                    else if (item.Defindex == 5000)
                    {
                        ScrapMetal += 1;
                    }
                    else if (item.Defindex == 5001)
                    {
                        ReclaimedMetal += 1;
                    }
                    else if (item.Defindex == 5002)
                    {
                        RefinedMetal += 1;
                    }
                    else
                    {
                        Items += 1;
                    }
                }
                bCounted = true;
                BotTotal = ScrapMetal + (ReclaimedMetal * 3) + (RefinedMetal * 9);

                if (BotTotal < clsFunctions.ONESCRAPBANK_INVENTORY_LEVEL)
                {
                    bot.SteamFriends.SendChatMessage(clsFunctions.BotsOwnerID, EChatEntryType.ChatMsg, "Problem! I have " + BotTotal + " instead of " + clsFunctions.ONESCRAPBANK_INVENTORY_LEVEL);
                }
                if (BotTotal % 10 > clsFunctions.ONESCRAPBANK_INVENTORY_LEVEL)
                {
                    bot.SteamFriends.SendChatMessage(clsFunctions.BotsOwnerID, EChatEntryType.ChatMsg, "I have " + (BotTotal - clsFunctions.ONESCRAPBANK_INVENTORY_LEVEL) + " extra scrap.");
                }

                if (ScrapMetal > 10)
                {
                    if (!bot.craftHandler.InGame && bot.CurrentTrade == null)
                    {
                        bot.craftHandler.Start(CraftingType.CombineMetal);
                    }
                }

                bStarted = false;
            }
            catch
            {
                bCounted = false;
                bStarted = false;
            }
        }

        public void KeybankMethod()
        {
            try
            {
                Thread.Sleep(5000);
                inventory = Inventory.FetchInventory(bot.SteamClient.SteamID.ConvertToUInt64(), bot.GetAPIKey());
                if (inventory == null)
                {
                    inventory = Inventory.FetchInventory(bot.SteamClient.SteamID.ConvertToUInt64(), bot.GetAPIKey());
                    if (inventory == null)
                    {
                        bStarted = false;
                        return;
                    }
                }
                foreach (Inventory.Item item in inventory.Items)
                {
                    if (item.IsNotTradeable)
                    {

                    }
                    else if (item.Defindex == 5000)
                    {
                        ScrapMetal += 1;
                    }
                    else if (item.Defindex == 5001)
                    {
                        ReclaimedMetal += 1;
                    }
                    else if (item.Defindex == 5002)
                    {
                        RefinedMetal += 1;
                    }
                    else if (schema.GetItem(item.Defindex).ItemName == "Decoder Ring" || item.Defindex == 5021)
                    {
                        Keys += 1;
                    }
                    else
                    {
                        //Console.WriteLine(schema.GetItem(item.Defindex).ItemName);
                        //Console.WriteLine(schema.GetItem(item.Defindex).Name);
                        Unknown += 1;
                    }
                }
                BotTotal = GetKeySets();
                bCounted = true;
                timeCounted = DateTime.Now;

                if (Unknown > 0)
                {
                    bot.SteamFriends.SendChatMessage(clsFunctions.BotsOwnerID, EChatEntryType.ChatMsg, "Hey, I have " + Unknown + " unknown items.");
                }

                if (ReclaimedMetal < 4 || ScrapMetal < 5)
                {
                    if (!bot.craftHandler.InGame && bot.CurrentTrade == null)
                    {
                        bot.craftHandler.Start(CraftingType.KeybankCrafting);
                    }
                }
                else if (ReclaimedMetal >= 9 || ScrapMetal >= 9)
                {
                    if (!bot.craftHandler.InGame && bot.CurrentTrade == null)
                    {
                        bot.craftHandler.Start(CraftingType.KeybankCrafting);
                    }
                }

                bStarted = false;
            }
            catch
            {
                bCounted = false;
                bStarted = false;
            }
        }

        public double GetHatBotValue()
        {
            double Metal = RefinedMetal;
            double rec = ReclaimedMetal;
            double scrap = ScrapMetal;
            double hat = Hats;
            double blacklist = Blacklist;
            while (rec > 0)
            {
                rec--;

                if (Metal.ToString().EndsWith(".66"))
                {
                    Metal -= 0.66;
                    Metal++;
                }
                else
                {
                    Metal += .33;
                }
            }
            while (scrap > 0)
            {
                scrap--;
                Metal += 0.11;
                if (Metal.ToString().EndsWith(".99"))
                {
                    Metal -= 0.99;
                    Metal++;
                }
            }
            while (hat > 0)
            {
                hat--;
                if (Metal.ToString().EndsWith(".88"))
                {
                    Metal -= 0.88;
                    Metal += 2.22;
                }
                else if (Metal.ToString().EndsWith(".77"))
                {
                    Metal -= 0.77;
                    Metal += 2.11;
                }
                else if (Metal.ToString().EndsWith(".66"))
                {
                    Metal -= 0.66;
                    Metal += 2;
                }
                else
                {
                    Metal += 1.33;
                }                
            }
            while (blacklist > 0)
            {
                blacklist--;
                if (Metal.ToString().EndsWith(".88"))
                {
                    Metal -= 0.88;
                    Metal += 2.22;
                }
                else if (Metal.ToString().EndsWith(".77"))
                {
                    Metal -= 0.77;
                    Metal += 2.11;
                }
                else if (Metal.ToString().EndsWith(".66"))
                {
                    Metal -= 0.66;
                    Metal += 2;
                }
                else
                {
                    Metal += 1.33;
                }    
            }
            return Metal;            
        }

        public int GetHatBotSets()
        {
            int sets = 0;
            int refined = (int)RefinedMetal;
            int reclaimed = (int)ReclaimedMetal;
            while (refined >= 4)
            {
                sets += 3;
                refined -= 4;
            }
            while (refined > 0 && reclaimed > 0)
            {
                refined--;
                reclaimed--;
                sets++;
            }
            return sets;
           
        }

        public int GetKeySets()
        {
            TF2Currency tempCurn = new TF2Currency(0, (int)RefinedMetal, (int)ReclaimedMetal, (int)ScrapMetal, 0);
            return tempCurn.ToKeys();
        }

    }
}
