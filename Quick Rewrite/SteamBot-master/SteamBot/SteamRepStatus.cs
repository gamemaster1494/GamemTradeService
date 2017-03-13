using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using Newtonsoft.Json;

namespace SteamBot
{
    public class SteamRepStatus
    {
        public static SteamRep GetSteamRepStatus(string sID)
        {
            string url = "http://steamrep.com/api/beta2/reputation/" + sID + "?json=1";
            string cachefile = "reputation/" + sID + ".json";
            DateTime lastmodified;
            string result;
            if (!Directory.Exists(AppDomain.CurrentDomain.BaseDirectory + "/reputation"))
            {
                Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory + "/reputation");
            }
            try
            {
                lastmodified = File.GetCreationTime(cachefile);
                if (DateTime.Now.Subtract(lastmodified).Ticks >= TimeSpan.TicksPerDay)
                {
                    WebClient client = new WebClient();
                    result = client.DownloadString(url);
                    File.WriteAllText(cachefile, result);
                    File.SetCreationTime(cachefile, DateTime.Now);
                }
                else
                {
                    TextReader reader = new StreamReader(cachefile);
                    result = reader.ReadToEnd();
                    reader.Close();
                }
                return JsonConvert.DeserializeObject<SteamRep>(result);
            }
            catch (FileNotFoundException)
            {
                WebClient client = new WebClient();
                result = client.DownloadString(url);
                File.WriteAllText(cachefile, result);
                File.SetCreationTime(cachefile, DateTime.Now);
                return JsonConvert.DeserializeObject<SteamRep>(result);
            }
            catch
            {
                if (File.Exists(cachefile))
                {
                    TextReader reader = new StreamReader(cachefile);
                    result = reader.ReadToEnd();
                    reader.Close();
                    return JsonConvert.DeserializeObject<SteamRep>(result);
                }
                else
                {
                    SteamRep rep = new SteamRep();
                    rep.steamrep = new SteamRepResponce();
                    rep.steamrep.Reputation = new SteamReputation();
                    rep.steamrep.Reputation.SummaryRep = "";
                    return rep;
                }
            }
        }
        public static SteamRep ConvertToRep(string sInfo)
        {
            return JsonConvert.DeserializeObject<SteamRep>(sInfo);
        }
    }

    public class SteamRep
    {
        [JsonProperty("steamrep")]
        public SteamRepResponce steamrep { get; set; }
    }

    public class SteamRepResponce
    {
        [JsonProperty("flags")]
        public Dictionary<string, string> flags { get; set; }

        [JsonProperty("steamID32")]
        public string SteamID32 { get; set; }

        [JsonProperty("steamID64")]
        public Int64 SteamID64 { get; set; }

        [JsonProperty("steamrepurl")]
        public string SteamRepURL { get; set; }

        [JsonProperty("reputation")]
        public SteamReputation Reputation { get; set; }
    }

    public class SteamReputation
    {
        [JsonProperty("full")]
        public string FullRep { get; set; }

        [JsonProperty("summary")]
        public string SummaryRep { get; set; }
    }
}
