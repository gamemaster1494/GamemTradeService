//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using SteamKit2;
//using Newtonsoft.Json;

//namespace SteamBot
//{
//    public class ItemDefinition
//    {
//        [JsonProperty("name")]
//        public string sName { get; set; }

//        [JsonProperty("id")]
//        public ulong id { get; set; }

//        [JsonProperty("sell")]
//        public decimal deSellFor { get; set; } //1 wep = 0.5. 1 scrap = 1.

//        [JsonProperty("buy")]
//        public decimal deBuyFor { get; set; }

//        [JsonProperty("custom_name")]
//        public string sCustomName { get; set; }

//        [JsonProperty("custom_description")]
//        public string sCustomDescription { get; set; }

//        [JsonProperty("craftable")]
//        public bool bCraftable { get; set; }

//        [JsonProperty("quality")]
//        public int iQuality { get; set; }

//        [JsonProperty("class")]
//        public string sClass { get; set; }

//        [JsonProperty("reservedby")]
//        public SteamID sidReservedBy { get; set; }

//        [JsonProperty("tag")]
//        public string sTag { get; set; }

//        [JsonProperty("itemtype")]
//        public string sItemType { get; set; }

//        public ItemDefinition(string sNameIn, ulong uIdIn, decimal deSellForIn, decimal deBuyForIn, int iQualityIn, string sClassIn, string sItemTypeIn, bool bCraftableIn = true, string sCustomNameIn = "", string sCustomDescriptionIn = "")
//        {
//            this.sName = sNameIn;
//            this.id = uIdIn;
//            this.deSellFor = deSellForIn;
//            this.deBuyFor = deBuyForIn;
//            this.sClass = sClassIn;
//            this.sCustomName = sCustomNameIn;
//            this.sCustomDescription = sCustomDescriptionIn;
//            this.bCraftable = bCraftableIn;
//            this.iQuality = iQualityIn;
//            this.sItemType = sItemTypeIn;
//            //this.bReserved = bReservedIn;
//        }//ItemDefinition()

//        public string FormatItem()
//        {
//            return string.Format("|{0,-10}|{1,-45}|{2,6}|{3,6}|", this.sClass, clsFunctions.ConvertQualityToString(this.iQuality) + " " + this.sName, clsFunctions.ConvertToString(this.deSellFor, this.sName), clsFunctions.ConvertToString(this.deBuyFor, this.sName));
//        }
//    }
//}
