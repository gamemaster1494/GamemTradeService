using System;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace SteamBot
{
    public class ItemRaffleData
    {
        public static ItemRaffleData LoadRaffleData(string fileName)
        {
            TextReader reader = new StreamReader(fileName);
            string json = reader.ReadToEnd();
            reader.Close();

            ItemRaffleData data = JsonConvert.DeserializeObject<ItemRaffleData>(json);

            return data;
        }

        public static void SaveItemRaffleData(ItemRaffleData data, string filename)
        {
            string json = JsonConvert.SerializeObject(data);
            File.WriteAllText(filename, json);
        }

        public static ulong PickWinner(ItemRaffleData data)
        {
            List<ulong> EntriesList = new List<ulong>();
            foreach (KeyValuePair<ulong, int> pair in data.Entries)
            {
                for (int i = 1; i <= pair.Value; i++)
                {
                    EntriesList.Add(pair.Key);
                }
            }

            Random rnd = new Random();
            return EntriesList[rnd.Next(EntriesList.Count - 1)];
        }
        

        public ulong ItemID { get; set; }

        public string ItemDescription { get; set; }

        public TF2Currency price { get; set; }

        public int SlotsLeft { get; set; }

        public int TotalSlots { get; set; }

        public Dictionary<ulong, int> Entries { get; set; }

        public ulong Winner { get; set; }
    }
}
