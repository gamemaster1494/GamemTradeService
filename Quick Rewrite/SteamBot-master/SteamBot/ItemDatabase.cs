//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using Newtonsoft.Json;
//using System.IO;

//namespace SteamBot
//{
//    public class ItemDatabase
//    {
//        public Dictionary<string, int> dCurrencyValues;
//        public List<ItemDefinition> lItemDatabase;

//        public static ItemDatabase LoadItemDatabase(string sFilePath)
//        {
//            TextReader reader = new StreamReader(sFilePath);
//            string json = reader.ReadToEnd();
//            reader.Close();

//            ItemDatabase database = JsonConvert.DeserializeObject<ItemDatabase>(json);

//            return database;
//        }

//        public ItemDatabase()
//        {
//            lItemDatabase = new List<ItemDefinition>();
//            dCurrencyValues = new Dictionary<string, int>();
//        }

//        public void ExportDatabase(string sPath, string sFileName)
//        {
//            //if (!File.Exists(sPath + sFileName))
//            //{
//            //    File.Create(sPath + sFileName);
//            //}

//            string sToWrite = JsonConvert.SerializeObject(this);

//            TextWriter writer = new StreamWriter(sPath + sFileName);
//            writer.Write(sToWrite);
//            writer.Close();
//        }

//        public void AddToList(ItemDefinition iItem)
//        {
//            lItemDatabase.Add(iItem);
//        }

//        public void UpdateCurrencyCost()
//        {
//            dCurrencyValues.Add("Scrap", 1);
//            dCurrencyValues.Add("Reclaimed", 3);
//            dCurrencyValues.Add("Refined", 9);
//            ItemDefinition item = SearchForItem((ulong)5021);
//            dCurrencyValues.Add("Key", (int)item.deSellFor);
//            item = SearchForItem((ulong)126);
//            dCurrencyValues.Add("Bill", (int)item.deSellFor);
//            item = SearchForItem((ulong)143);
//            dCurrencyValues.Add("Earbud", (int)item.deSellFor);
//        }

//        public string FormatAsCurrency(decimal deSellFor)
//        {
//            int iEarbuds = 0;
//            int iBills = 0;
//            int iKeys = 0;
//            int iRef = 0;
//            int iRec = 0;
//            int iScrap = 0;
//            int iWeps = 0;
//            while (deSellFor - (decimal)dCurrencyValues["Earbud"] >= 0)
//            {
//                iEarbuds++;
//                deSellFor -= (decimal)dCurrencyValues["Earbud"];
//            }
//            while (deSellFor - (decimal)dCurrencyValues["Bill"] >= 0)
//            {
//                iBills++;
//                deSellFor -= (decimal)dCurrencyValues["Bill"];
//            }
//            while (deSellFor - (decimal)dCurrencyValues["Key"] >= 0)
//            {
//                iKeys++;
//                deSellFor -= (decimal)dCurrencyValues["Key"];
//            }
//            while (deSellFor - (decimal)dCurrencyValues["Refined"] >= 0)
//            {
//                iRef++;
//                deSellFor -= (decimal)dCurrencyValues["Refined"];
//            }
//            while (deSellFor - (decimal)dCurrencyValues["Reclaimed"] >= 0)
//            {
//                iRec++;
//                deSellFor -= (decimal)dCurrencyValues["Reclaimed"];
//            }
//            while (deSellFor - (decimal)dCurrencyValues["Scrap"] >= 0)
//            {
//                iScrap++;
//                deSellFor -= (decimal)dCurrencyValues["Scrap"];
//            }
//            string sOutput = "";
//            if (iEarbuds > 0)
//            {
//                sOutput+= "" + iEarbuds.ToString() + " Earbud(s), ";
//            }
//            if (iBills > 0)
//            {
//                sOutput += "" + iBills.ToString() + " Bill(s), ";
//            }
//            if (iKeys > 0)
//            {
//                sOutput += "" + iKeys.ToString() + " Key(s), ";
//            }
//            if (iRef > 0)
//            {
//                sOutput += "" + iRef.ToString() + " Refined, ";
//            }
//            if (iRec > 0)
//            {
//                sOutput += "" + iRec.ToString() + " Reclaimed, ";
//            }
//            if (iScrap > 0)
//            {
//                sOutput += "" + iScrap.ToString() + " Scrap";
//            }
//            if (deSellFor > 0)
//            {
//                sOutput += " 1 Craftable Weapon ";
//            }
//            return sOutput;            
//        }

//        public ItemDefinition SearchForItem(ulong iID, int iQuality, bool bCraftable = true)
//        {            
//            foreach (ItemDefinition item in lItemDatabase)
//            {
//                if (item.id == iID && item.iQuality == iQuality && item.bCraftable == bCraftable)
//                {
//                    return item;//return item in database
//                }//if item matches
//            }//foreach item in the database
//            return new ItemDefinition("NONE", 0, 0, 0, 0,"NONE","NONE");//no item found return default
//        }

//        public ItemDefinition SearchForItem(ulong iID)
//        {
//            foreach (ItemDefinition item in lItemDatabase)
//            {
//                if (item.id == iID)
//                {
//                    return item;//return item
//                }//if item id's match
//            }//foreach item in database
//            return new ItemDefinition("NONE", 0, 0, 0, 0, "NONE","NONE");//no item found return default
//        }

//        public List<ItemDefinition> SearchForItem(string sItemName, ref List<ItemDefinition> lstItems)
//        {
            
//            foreach (ItemDefinition item in lItemDatabase)
//            {
//                if (item.sName.ToLower().Contains(sItemName))
//                {
//                    lstItems.Add(item);
//                }
//            }
//            return lstItems;
//        }
//    }
//    //public class ItemDefinition
//    //{
//    //    public string sName;
//    //    public ulong id;
//    //    public decimal deSellFor; //1 wep = 0.5. 1 scrap = 1.
//    //    public decimal deBuyFor;
//    //    public string sCustomName;
//    //    public string sCustomDescription;
//    //    public bool bCraftable;
//    //    public int iQuality;
//    //    public SteamID sidReservedBy;
//    //    public string sTag;
//    //    public string sClass;
//    //    //public bool bReserved = false;

//    //    public ItemDefinition(string sNameIn, ulong uIdIn, decimal deSellForIn, decimal deBuyForIn, int iQualityIn, bool bCraftableIn = true, string sCustomNameIn = "", string sCustomDescriptionIn = "")
//    //    {
//    //        this.sName = sNameIn;
//    //        this.id = uIdIn;
//    //        this.deSellFor = deSellForIn;
//    //        this.deBuyFor = deBuyForIn;
//    //        this.sCustomName = sCustomNameIn;
//    //        this.sCustomDescription = sCustomDescriptionIn;
//    //        this.bCraftable = bCraftableIn;
//    //        this.iQuality = iQualityIn;

//    //        //this.bReserved = bReservedIn;
//    //    }//ItemDefinition()
//    //}
//}
