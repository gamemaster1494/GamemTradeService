///SteamBot.BackpackTF
///Created by Jacob Douglas (Gamem)
///Created on 7/3/2014
///(c) Copyright 2014, Nebula

///Used to receive the price list from backpack.tf
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Web.Script.Serialization;

namespace SteamBot
{
    public static class BackpackTF
    {
        public static dynamic FetchSchema()
        {
            //TODO: Store API in config file maybe
            var apiKey = "<APIKEY>";//API key

            var url = "http://backpack.tf/api/IGetPrices/v4/?key=" + apiKey + "&compress=1";//ULR to get backpack.tfs API

            string cachefile = "tf_pricelist.cache";//name of cache file
            string sConversionResult = "";//Holds converted result

            TimeSpan difference = DateTime.Now - System.IO.File.GetCreationTime(cachefile);//calculate differenc in time gotten last

            if (System.IO.File.Exists(cachefile) && difference.TotalMinutes < 10)
            {
                TextReader reader = new StreamReader(cachefile);//create new text reader
                sConversionResult = reader.ReadToEnd();//Read from file since it hasn't been long since we got it last time.
                reader.Close();//close reader
            }//if (System.IO.File.Exists(cachefile) && difference.TotalMinutes < 10)
            else
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);//create a request for the info

                HttpWebResponse response = null;//response. holds the response from the web

                try
                {
                    response = (HttpWebResponse)request.GetResponse();//try to get the response
                }//try
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);//write error to console
                    response = null;//set to null to process later on
                }//catch

                try
                {
                    DateTime SchemaLastRequested = response.LastModified;//Get info
                    if (response != null)
                    {
                        using (StreamReader sr = new StreamReader(response.GetResponseStream()))
                        {
                            sConversionResult = sr.ReadToEnd();//read entire response                            
                            sr.Close();//close stream reader
                        }//using (StreamReader sr = new StreamReader(response.GetResponseStream()))
                        File.WriteAllText(cachefile, sConversionResult);//write entire response to file
                        System.IO.File.SetCreationTime(cachefile, SchemaLastRequested);//set creation time
                    }//if (response != null)
                    else
                    {
                        TextReader reader = new StreamReader(cachefile);//create new reader
                        sConversionResult = reader.ReadToEnd();//read file to end
                        reader.Close();//close reader
                    }//else
                    response.Close();//close response
                }//try
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);//write error to screen
                    TextReader reader = new StreamReader(cachefile);//create a new reader
                    sConversionResult = reader.ReadToEnd();//Read file to the end
                    reader.Close();//close reader

                    if (response != null)
                    {
                        response.Close();//close response
                    }//if (response != null)
                }//catch (Exception ex)
            }//else

            JavaScriptSerializer serializer = new JavaScriptSerializer();//create new serializer            
            var results = serializer.Deserialize<dynamic>(sConversionResult);//convert it
            return results;//return the result
        }//FetchSchema()



        [Serializable]
        public class Responce
        {
            public int success { get; set; }

            public string message { get; set; }

            public long current_time { get; set; }

            public decimal raw_usd_value { get; set; }

            public string usd_currency { get; set; }

            public string usd_currency_index { get; set; }

            public Dictionary<string, BackpackTFItem> items { get; set; }

        }

        [Serializable]
        public class BackpackTFItem
        {
            public int[] defindex { get; set; }

            public Dictionary<int, Dictionary<string, Dictionary<string, BackpackTFItemPrices>>> prices { get; set; }

        }

        [Serializable]
        public class BackpackTFItemPrices
        {
            public string currency { get; set; }

            public decimal value { get; set; }

            public decimal value_high { get; set; }

            public decimal value_raw { get; set; }

            public long last_update { get; set; }

            public decimal difference { get; set; }
        }
    }
}
