using System;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace SteamBot
{
    public class RaffleHandler
    {
        public static RaffleHandler LoadRaffleInformation(string filename)
        {
            try
            {
                TextReader reader = new StreamReader(filename);
                string json = reader.ReadToEnd();
                reader.Close();

                RaffleHandler raffleHandler = JsonConvert.DeserializeObject<RaffleHandler>(json);

                return raffleHandler;
            }
            catch
            {
                return null;
            }
        }

        public static void SaveRaffleInformation(string filename, RaffleHandler handler)
        {
            try
            {
                TextWriter writer = new StreamWriter(filename, false);
                string json = JsonConvert.SerializeObject(handler);
                writer.Write(json);
                writer.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public static bool PickWinners(string filename, RaffleHandler handler, Bot bot)
        {
            try
            {
                List<ulong> entries = new List<ulong>();
                foreach (KeyValuePair<ulong, int> pair in handler.Entries)
                {
                    for (int i = 1; i <= pair.Value; i++)
                    {
                        entries.Add(pair.Key);                        
                    }
                }
                Random rnd = new Random();
                Dictionary<ulong, string> winners = new Dictionary<ulong, string>();
                int prizecount = 0;
                while (prizecount < handler.Prizes.Count)
                {
                    int winnernum = rnd.Next(0, entries.Count - 1);
                    if (!winners.ContainsKey(entries[winnernum]) && clsFunctions.IsMemberOfGroup(new SteamKit2.SteamID(entries[winnernum])) && !clsFunctions.IsUserScammer(entries[winnernum].ToString()))
                    {
                        winners.Add(entries[winnernum], handler.Prizes[prizecount]);
                    }
                    else
                    {
                        prizecount--;
                    }
                    prizecount++;
                }
                TextWriter writer = new StreamWriter(filename, false);
                foreach (KeyValuePair<ulong, string> pair in winners)
                {
                    if (clsFunctions.DonationBotID.ContainsKey(bot.SteamClient.SteamID))
                    {
                        bot.SteamFriends.AddFriend(new SteamKit2.SteamID(pair.Key));
                        Console.WriteLine("Added friend for winning!");
                    }
                    writer.WriteLine(String.Format("({0}) {1} won the item: {2}", pair.Key, bot.SteamFriends.GetFriendPersonaName(new SteamKit2.SteamID(pair.Key)), pair.Value));
                }
                writer.Close();
                
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }

        public Dictionary<ulong, int> Entries { get; set; }

        public Dictionary<string, List<ulong>> SpecialCodes { get; set; }

        //public Dictionary<string, List<ulong>> GlobalCodes { get; set; }

        public List<string> Prizes { get; set; }
    }   
}
