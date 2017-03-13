using System;
using System.Collections.Generic;
using System.Threading;
using SteamKit2;
using SteamKit2.Internal;
using SteamTrade;
using SteamBot.TF2GC;

namespace SteamBot
{
    public class CraftingHandler
    {
        private Bot bot;

        private Dictionary<int, List<long>> metal = new Dictionary<int, List<long>>();

        private Object lck = new Object();

        private Object welcome = new Object();

        private bool CraftedItem = false;

        public bool InGame = false;

        private bool bStarted = false;

        public CraftingHandler(Bot bot)
        {
            this.bot = bot;
        }

        public void OnWelcome()
        {
            InGame = true;
        }

        public void Start(CraftingType type)
        {
            if (!bStarted)
            {
                Thread thread;
                if (type == CraftingType.ScrapbankSmeltWeps)
                    thread = new Thread(new ThreadStart(ScrapbankMethod));
                else if (type == CraftingType.HatbankCrafting)
                    thread = new Thread(new ThreadStart(HatbankMethod));
                else if (type == CraftingType.ScrapbankMetalToScrap)
                    thread = new Thread(new ThreadStart(MetalToScrapMethod));
                else if (type == CraftingType.CombineMetal)
                    thread = new Thread(new ThreadStart(CompactMetal));
                else if (type == CraftingType.KeybankCrafting)
                    thread = new Thread(new ThreadStart(KeybankMethod));
                else
                    return;
                thread.Start();
                bStarted = true;
            }
        }

        public void LaunchGame()
        {
            ClientMsgProtobuf<CMsgClientGamesPlayed> clientMsg = new ClientMsgProtobuf<CMsgClientGamesPlayed>(EMsg.ClientGamesPlayed);
            clientMsg.Body.games_played.Add(new CMsgClientGamesPlayed.GamePlayed
            {
                game_id = 440,
            });

            bot.SteamClient.Send(clientMsg);

            InGame = true;
            bot.CurrentGame = 440;
        }

        public void ExitGame()
        {
            var clientMsg = new ClientMsgProtobuf<CMsgClientGamesPlayed.GamePlayed>(EMsg.ClientGamesPlayedNoDataBlob);
            bot.SteamClient.Send(clientMsg);
            bot.CurrentGame = 0;
            InGame = false;
        }

        //private CraftResponceCallback responce;
        private Object craftWait = new Object();

        //public void OnCraft(CraftResponceCallback responce)
        //{
        //    this.responce = responce;
        //    CraftedItem = true;
        //}

        //private CraftResponceCallback Scrap(long item1, long item2)
        //{
        //    return Craft(ECraftingRecipie.SmeltClassWeapon, new long[] { item1, item2 });
        //}

        //private CraftResponceCallback Craft(ECraftingRecipie recipie, long[] items)
        //{
        //    if (!InGame)
        //    {
        //        try
        //        {
        //            LaunchGame();
        //            Thread.Sleep(50);
        //            while (!InGame)
        //            {
        //                Thread.Sleep(100);
        //            }
        //        }
        //        catch (ThreadInterruptedException e)
        //        {
        //            bot.log.Error(e.StackTrace);
        //        }
        //    }
        //    CraftedItem = false;
        //    bot.SteamGameCoordinator.Craft(recipie, items);
        //    Thread.Sleep(50);
        //    while (!CraftedItem)
        //    {
        //        Thread.Sleep(100);
        //    }
        //    return responce;
        //}

        public void ScrapbankMethod()
        {
            if (bot.CurrentGame != 440)
            {
                LaunchGame();
                Thread.Sleep(2000);
            }
            int loopcount = 0;
            Monitor.Enter(this.lck);
            while (loopcount < 1)
            {
                Inventory MyInventory = Inventory.FetchInventory(bot.SteamClient.SteamID.ConvertToUInt64(), bot.GetAPIKey());
                if (MyInventory == null)
                {
                    bot.log.Error("Could not fetch own inventory via Steam API");
                    Monitor.PulseAll(this.lck);
                    return;
                }

                List<ulong> scraps = new List<ulong>();
                List<ulong> recs = new List<ulong>();
                List<ulong> refs = new List<ulong>();
                Dictionary<string, List<ulong>> weapons = new Dictionary<string, List<ulong>>();

                //uShort = defindex ulong = id
                Schema schema = Schema.FetchSchema(bot.GetAPIKey());

                #region Counting

                foreach (Inventory.Item item in MyInventory.Items)
                {
                    Schema.Item schemaItem = schema.GetItem(item.Defindex);
                    if (item.Defindex == 5000)
                    {
                        scraps.Add(item.Id);
                    }
                    else if (item.Defindex == 5001)
                    {
                        recs.Add(item.Id);
                    }
                    else if (item.Defindex == 5002)
                    {
                        refs.Add(item.Id);
                    }
                    else
                    {
                        if (schemaItem.CraftMaterialType == "weapon" && !item.IsNotCraftable && !item.IsNotTradeable)
                        {
                            if (!weapons.ContainsKey(schemaItem.UsableByClasses[0]))
                            {
                                weapons.Add(schemaItem.UsableByClasses[0], new List<ulong>());
                            }
                            weapons[schemaItem.UsableByClasses[0]].Add(item.Id);
                        }
                    }
                }

                #endregion Counting

                #region Getting Craft Pairs

                List<string> Defindex = new List<string>();
                List<CraftingPair> craftingpairs = new List<CraftingPair>();
                ulong item1, item2;
                foreach (KeyValuePair<string, List<ulong>> pair in weapons)
                {
                    item1 = 0;
                    item2 = 0;
                    Defindex.Clear();
                    foreach (ulong id in pair.Value)
                    {
                        Inventory.Item item = MyInventory.GetItem(id);
                        Schema.Item schemaItem = schema.GetItem(item.Defindex);

                        if (Defindex.Contains(schemaItem.ItemName) && !bot.dReserved.ContainsKey(item.Id))
                        {
                            if (item1 == 0)
                            {
                                item1 = item.Id;
                            }
                            else if (item2 == 0)
                            {
                                item2 = item.Id;
                                CraftingPair crPair = new CraftingPair(CraftingType.ScrapbankSmeltWeps);
                                crPair.SetItem(item1);
                                crPair.SetItem(item2);
                                craftingpairs.Add(crPair);
                                item1 = 0;
                                item2 = 0;
                            }
                        }
                        else
                        {
                            Defindex.Add(schemaItem.ItemName);
                        }
                    }
                }

                #endregion Getting Craft Pairs

                #region Crafting

                foreach (CraftingPair pair in craftingpairs)
                {
                    Crafting.CraftItems(bot, pair.Item1, pair.Item2);
                    Thread.Sleep(100);
                    //CraftResponceCallback callback = Scrap((long)pair.Item1, (long)pair.Item2);
                    //if (callback != null)
                    //{
                    //    foreach (long itemID in callback.GetItems())
                    //    {
                    //        scraps.Add((ulong)itemID);
                    //    }
                    //    bot.log.Success("Item crafted into scrap!");
                    //}
                }

                while (refs.Count > 0)
                {
                    Crafting.CraftItems(bot, refs[0]);
                    Thread.Sleep(100);
                    refs.RemoveAt(0);

                    //CraftResponceCallback callback = Craft(ECraftingRecipie.Unknown, new long[] { (long)refs[0] });
                    //if (callback != null)
                    //{
                    //    refs.RemoveAt(0);
                    //    foreach (long itemID in callback.GetItems())
                    //    {
                    //        recs.Add((ulong)itemID);
                    //    }
                    //    bot.log.Success("Refined smelted into reclaimed!");
                    //}
                }

                while (recs.Count > 0)
                {
                    Crafting.CraftItems(bot, recs[0]);
                    Thread.Sleep(100);
                    recs.RemoveAt(0);

                    //CraftResponceCallback callback = Craft(ECraftingRecipie.Unknown, new long[] { (long)recs[0] });
                    //if (callback != null)
                    //{
                    //    recs.RemoveAt(0);
                    //    foreach (long itemID in callback.GetItems())
                    //    {
                    //        scraps.Add((ulong)itemID);
                    //    }
                    //    bot.log.Success("Reclaimed smelted into scrap!");
                    //}
                }

                #endregion Crafting
                loopcount++;
            }
            ExitGame();
            Monitor.Exit(this.lck);
            bStarted = false;
        }

        public void HatbankMethod()
        {
            if (bot.CurrentGame != 440)
            {
                LaunchGame();
                Thread.Sleep(2000);
            }
            int loopcount = 0;
            Monitor.Enter(this.lck);
            while (loopcount <= 1)
            {
                Inventory MyInventory = Inventory.FetchInventory(bot.SteamClient.SteamID.ConvertToUInt64(), bot.GetAPIKey());
                if (MyInventory == null)
                {
                    bot.log.Error("Could not fetch own inventory via Steam API");
                    Monitor.PulseAll(this.lck);
                    return;
                }

                List<ulong> scraps = new List<ulong>();
                List<ulong> recs = new List<ulong>();
                List<ulong> refs = new List<ulong>();

                foreach (Inventory.Item item in MyInventory.Items)
                {
                    if (item.Defindex == 5000)
                    {
                        scraps.Add(item.Id);
                    }
                    else if (item.Defindex == 5001)
                    {
                        recs.Add(item.Id);
                    }
                    else if (item.Defindex == 5002)
                    {
                        refs.Add(item.Id);
                    }
                }

                while (scraps.Count >= 3)
                {
                    Crafting.CraftItems(bot, scraps[0], scraps[1], scraps[2]);
                    scraps.RemoveRange(0, 3);
                    Thread.Sleep(100);
                    //CraftResponceCallback callback = Craft(ECraftingRecipie.CombineScrap, new long[] { (long)scraps[0], (long)scraps[1], (long)scraps[2] });
                    //if (callback != null)
                    //{
                    //    scraps.RemoveRange(0, 3);
                    //    foreach (var item in callback.GetItems())
                    //    {
                    //        recs.Add((ulong)item);
                    //    }
                    //    bot.log.Success("scrap made into rec!");
                    //}
                }

                if (recs.Count < 12 && refs.Count >= 1)
                {
                    Crafting.CraftItems(bot, refs[0]);
                    refs.RemoveAt(0);
                    Thread.Sleep(100);

                    //CraftResponceCallback callback = Craft(ECraftingRecipie.SmeltRefined, new long[] { (long)refs[0] });
                    //if (callback != null)
                    //{
                    //    refs.RemoveAt(0);
                    //    foreach (var item in callback.GetItems())
                    //    {
                    //        recs.Add((ulong)item);
                    //    }
                    //    bot.log.Success("Ref smelted to recs!");
                    //}
                }

                //while (recs.Count > 14)
                //{
                //    CraftResponceCallback callback = Craft(ECraftingRecipie.CombineReclaimed, new long[] { (long)recs[0], (long)recs[1], (long)recs[2] });
                //    if (callback != null)
                //    {
                //        recs.RemoveRange(0, 3);
                //        foreach (var item in callback.GetItems())
                //        {
                //            refs.Add((ulong)item);
                //        }
                //        bot.log.Success("Rec combined to ref!");
                //    }
                //}

                //while (scraps.Count < 8 && recs.Count > 1)
                //{
                //    CraftResponceCallback callback = Craft(ECraftingRecipie.SmeltReclaimed, new long[] { (long)recs[0] });
                //    if (callback != null)
                //    {
                //        recs.RemoveAt(0);
                //        foreach (var item in callback.GetItems())
                //        {
                //            scraps.Add((ulong)item);
                //        }
                //        bot.log.Success("Rec smelted to scrap!");
                //    }
                //}
                loopcount++;
            }

            ExitGame();
            Monitor.Exit(this.lck);
            bStarted = false;
        }

        public void MetalToScrapMethod()
        {
            if (bot.CurrentGame != 440)
            {
                LaunchGame();
                Thread.Sleep(2000);
            }
            int loopcount = 0;
            Monitor.Enter(this.lck);
            while (loopcount <= 1)
            {
                List<ulong> refs = new List<ulong>();
                List<ulong> recs = new List<ulong>();
                Inventory MyInventory = Inventory.FetchInventory(bot.SteamClient.SteamID.ConvertToUInt64(), bot.GetAPIKey());
                if (MyInventory == null)
                {
                    bot.log.Error("Could not fetch own inventory via Steam API");
                    Monitor.PulseAll(this.lck);
                    return;
                }

                foreach (Inventory.Item item in MyInventory.Items)
                {
                    if (item.Defindex == 5001)
                    {
                        recs.Add(item.Id);
                    }
                    else if (item.Defindex == 5002)
                    {
                        refs.Add(item.Id);
                    }
                }

                while (refs.Count > 0)
                {
                    Crafting.CraftItems(bot, refs[0]);
                    refs.RemoveAt(0);
                    Thread.Sleep(100);
                    //CraftResponceCallback callback = Craft(ECraftingRecipie.SmeltRefined, new long[] { (long)refs[0] });
                    //if (callback != null)
                    //{
                    //    refs.RemoveAt(0);
                    //    foreach (long itemID in callback.GetItems())
                    //    {
                    //        recs.Add((ulong)itemID);
                    //    }
                    //    bot.log.Success("Refined smelted into reclaimed!");
                    //}
                }

                while (recs.Count > 0)
                {
                    Crafting.CraftItems(bot, recs[0]);
                    recs.RemoveAt(0);
                    Thread.Sleep(100);
                    //CraftResponceCallback callback = Craft(ECraftingRecipie.SmeltReclaimed, new long[] { (long)recs[0] });
                    //if (callback != null)
                    //{
                    //    recs.RemoveAt(0);
                    //    bot.log.Success("Reclaimed smelted into scrap!");
                    //}
                }
                loopcount++;
            }
            ExitGame();
            Monitor.Exit(this.lck);
            bStarted = false;
        }

        public void CompactMetal()
        {
            if (bot.CurrentGame != 440)
            {
                LaunchGame();
                Thread.Sleep(2000);
            }
            int loopcount = 0;
            int ScrapToKeep = 10;
            Monitor.Enter(this);

            while (loopcount <= 1)
            {

                List<ulong> Recs = new List<ulong>();
                List<ulong> Scraps = new List<ulong>();
                List<ulong> Refs = new List<ulong>();

                Inventory MyInventory = Inventory.FetchInventory(bot.SteamClient.SteamID.ConvertToUInt64(), bot.GetAPIKey());
                if (MyInventory == null)
                {
                    bot.log.Error("Could not fetch own inventory via Steam API");
                    Monitor.PulseAll(this.lck);
                    return;
                }

                foreach (Inventory.Item item in MyInventory.Items)
                {
                    if (item.Defindex == 5000)
                    {
                        Scraps.Add(item.Id);
                    }
                    else if (item.Defindex == 5001)
                    {
                        Recs.Add(item.Id);
                    }
                    else if (item.Defindex == 5002)
                    {
                        Refs.Add(item.Id);
                    }
                }

                while (Scraps.Count - ScrapToKeep >= 3)
                {
                    Crafting.CraftItems(bot, Scraps[0], Scraps[1], Scraps[2]);
                    Scraps.RemoveRange(0, 3);
                    Thread.Sleep(100);
                    //CraftResponceCallback callback = Craft(ECraftingRecipie.CombineScrap, new long[] { (long)Scraps[0], (long)Scraps[1], (long)Scraps[2] });
                    //if (callback != null)
                    //{
                    //    Scraps.RemoveRange(0, 3);
                    //    foreach (var item in callback.GetItems())
                    //    {
                    //        Recs.Add((ulong)item);
                    //    }
                    //    bot.log.Success("rec created from scrap!");
                    //}
                }

                while (Recs.Count >= 3)
                {
                    Crafting.CraftItems(bot, Recs[0], Recs[1], Recs[2]);
                    Recs.RemoveRange(0, 3);
                    Thread.Sleep(100);
                    //CraftResponceCallback callback = Craft(ECraftingRecipie.CombineReclaimed, new long[] { (long)Recs[0], (long)Recs[1], (long)Recs[2] });
                    //if (callback != null)
                    //{
                    //    Recs.RemoveRange(0, 3);
                    //    foreach (var item in callback.GetItems())
                    //    {
                    //        Refs.Add((ulong)item);
                    //    }
                    //    bot.log.Success("ref created from rec!");
                    //}
                }
                loopcount++;
            }
            ExitGame();
            Monitor.Exit(this);
            bStarted = false;
        }

        public void KeybankMethod()
        {
            if (bot.CurrentGame != 440)
            {
                LaunchGame();
                Thread.Sleep(2000);
            }
            int loopcount = 0;
            Monitor.Enter(this);

            try
            {
                while (loopcount <= 2)
                {
                    List<ulong> Recs = new List<ulong>();
                    List<ulong> Scraps = new List<ulong>();
                    List<ulong> Refs = new List<ulong>();

                    Inventory MyInventory = Inventory.FetchInventory(bot.SteamClient.SteamID.ConvertToUInt64(), bot.GetAPIKey());
                    if (MyInventory == null)
                    {
                        bot.log.Error("Could not fetch own inventory via Steam API");
                        Monitor.PulseAll(this.lck);
                        return;
                    }

                    foreach (Inventory.Item item in MyInventory.Items)
                    {
                        if (item.Defindex == 5000)
                        {
                            Scraps.Add(item.Id);
                        }
                        else if (item.Defindex == 5001)
                        {
                            Recs.Add(item.Id);
                        }
                        else if (item.Defindex == 5002)
                        {
                            Refs.Add(item.Id);
                        }
                    }
                    List<CraftingPair> pairs = new List<CraftingPair>();

                    while (Scraps.Count >= 6)
                    {
                        CraftingPair crPair = new CraftingPair(CraftingType.CombineMetal);
                        crPair.SetItem(Scraps[0]);
                        crPair.SetItem(Scraps[1]);
                        crPair.SetItem(Scraps[2]);
                        Scraps.RemoveRange(0, 3);
                        pairs.Add(crPair);
                    }
                    foreach (CraftingPair pair in pairs)
                    {
                        Crafting.CraftItems(bot, pair.GetItems());
                        Thread.Sleep(100);
                        // bot.log.Success("I THINK I CRAFTED REC FROM SCRAP");
                        //CraftResponceCallback callback = Craft(ECraftingRecipie.CombineScrap, pair.GetItems());
                        //if (callback != null)
                        //{
                        //    foreach (long itemID in callback.GetItems())
                        //    {
                        //        Recs.Add((ulong)itemID);
                        //    }
                        //    bot.log.Success("Scrap combined into rec!");
                        //}
                    }
                    pairs.Clear();

                    if (Recs.Count < 8 && Refs.Count >= 1)
                    {
                        Crafting.CraftItems(bot, Refs[0]);
                        Refs.RemoveAt(0);
                        Thread.Sleep(100);
                        //CraftResponceCallback callback = Craft(ECraftingRecipie.SmeltRefined, new long[] { (long)Refs[0] });
                        //if (callback != null)
                        //{
                        //    Refs.RemoveAt(0);
                        //    foreach (var item in callback.GetItems())
                        //    {
                        //        Recs.Add((ulong)item);
                        //    }
                        //    bot.log.Success("Ref smelted to recs!");
                        //}
                    }

                    while(Recs.Count >= 8)
                    {
                        //Console.WriteLine("Trying to craft REF");
                        Crafting.CraftItems(bot, new ulong[] { Recs[0], Recs[1], Recs[2] });
                        Recs.RemoveRange(0, 3);
                        Thread.Sleep(100);
                        //CraftResponceCallback callback = Craft(ECraftingRecipie.CombineReclaimed, new long[] { (long)Recs[0], (long)Recs[1], (long)Recs[2] });
                        //if (callback != null)
                        //{
                        //    Recs.RemoveRange(0, 3);
                        //    foreach (var item in callback.GetItems())
                        //    {
                        //        Refs.Add((ulong)item);
                        //    }
                        //    bot.log.Success("Rec combined to ref!");
                        //}
                    }

                    if (Scraps.Count < 4)
                    {
                        Crafting.CraftItems(bot, Recs[0]);
                        Recs.RemoveAt(0);
                        Thread.Sleep(100);
                        //CraftResponceCallback callback = Craft(ECraftingRecipie.SmeltReclaimed, new long[] { (long)Recs[0] });
                        //if (callback != null)
                        //{
                        //    Recs.RemoveAt(0);
                        //    foreach (var item in callback.GetItems())
                        //    {
                        //        Scraps.Add((ulong)item);
                        //    }
                        //    bot.log.Success("Rec smelted to scraps!");
                        //}
                    }
                    loopcount=6;
                }
            }
            catch
            {

            }
            finally
            {
                ExitGame();
                Monitor.Exit(this);
                bStarted = false;
            }
            
        }
    }

    public enum CraftingType
    {
        ScrapbankSmeltWeps = 0,
        ScrapbankMetalToScrap = 1,
        HatbankCrafting = 2,
        CombineMetal = 3,
        SmeltMetal = 4,
        KeybankCrafting = 5
    }

    public enum BotType
    {
        ScrapbankingBot = 0,
        HatbankingBot = 1,
        KeybankingBot = 2,
        OneScrapBot = 3,
        OneWepBot = 4,
        AdminBot = 5,
        VaultBot = 6,
        DonationBot = 7
    }

    public class CraftingPair
    {
        public ulong Item1 = 0;
        public ulong Item2 = 0;
        public ulong Item3 = 0;

        public CraftingType Type;

        public CraftingPair(CraftingType Type)
        {
            this.Type = Type;
        }

        public bool SetItem(ulong Item)
        {
            if (Type == CraftingType.ScrapbankSmeltWeps)
            {
                if (Item1 == 0)
                {
                    Item1 = Item;
                }
                else if (Item2 == 0)
                {
                    Item2 = Item;
                }
                else
                {
                    return false;
                }
            }
            else if (Type == CraftingType.CombineMetal)
            {
                if (Item1 == 0)
                {
                    Item1 = Item;
                }
                else if (Item2 == 0)
                {
                    Item2 = Item;
                }
                else if (Item3 == 0)
                {
                    Item3 = Item;
                }
                else
                {
                    return false;
                }
            }
            else if (Type == CraftingType.SmeltMetal)
            {
                if (Item1 == 0)
                {
                    Item1 = Item;
                }
                else
                {
                    return false;
                }
            }
            return true;
        }

        public ulong[] GetItems()
        {
            if (Type == CraftingType.CombineMetal)
            {
                return new ulong[] { (ulong)Item1, (ulong)Item2, (ulong)Item3 };
            }
            else if (Type == CraftingType.SmeltMetal)
            {
                return new ulong[] { (ulong)Item1 };
            }
            return new ulong[] { (ulong)Item1, (ulong)Item2 };
        }
    }
}