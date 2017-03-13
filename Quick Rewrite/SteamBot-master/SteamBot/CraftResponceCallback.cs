//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using SteamKit2;

//namespace SteamKit2
//{
//    public class CraftResponceCallback : CallbackMsg    
//    {
//        private List<long> items = new List<long>();

//        public List<long> GetItems()
//        {
//            return this.items;
//        }

//        public CraftResponceCallback(GCMsgCraftItemResponce responce)
//        {
//            foreach (long item in responce.items)
//            {
//                items.Add(item);
//            }
//        }           
//    }
//}
