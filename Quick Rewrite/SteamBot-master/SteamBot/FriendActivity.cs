using System;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace SteamBot
{
    public class FriendActivity
    {
        public static FriendActivity LoadFriendActivity(string filename)
        {
            string json;
            FriendActivity friendactiv = new FriendActivity();
            if (File.Exists(filename))
            {
                TextReader reader = new StreamReader(filename);
                json = reader.ReadToEnd();
                reader.Close();
                friendactiv = JsonConvert.DeserializeObject<FriendActivity>(json);

            }
            else
            {
                friendactiv = new FriendActivity();
                friendactiv.FriendActivityInfo = new Dictionary<string, DateTime>();
                friendactiv.FriendActivityInfo.Add(clsFunctions.BotsOwnerID.ConvertToUInt64().ToString(), DateTime.Now);
            }

            return friendactiv;
        }

        public static void UpdateFriendActivity(string filename, FriendActivity friendactivity)
        {
            string json = JsonConvert.SerializeObject(friendactivity);
            File.WriteAllText(filename, json);
        }

        public Dictionary<string, DateTime> FriendActivityInfo { get; set; }
    }
}
