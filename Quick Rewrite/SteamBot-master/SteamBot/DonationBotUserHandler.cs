using SteamKit2;
using System;
using System.Collections.Generic;
using SteamTrade;
using System.IO;
using SteamTrade.TradeWebAPI;

namespace SteamBot
{
    public class DonationBotUserHandler : UserHandler
    {
        public string FileToCreate;
        public int KeysPutUp;

        public int BudsPutUp;

        public FriendAddedHandler FriendAddedHandler;

        public FriendRemoveHandler FriendRemoveHandler;

        public DonationBotUserHandler (Bot bot, SteamID sid) : base(bot, sid) 
        {
            this.FriendAddedHandler = new FriendAddedHandler(bot, bot.BotControlClass.Substring(9), OtherSID);//Initialize
            this.FriendRemoveHandler = new FriendRemoveHandler(this.Bot, this.OtherSID);
        }


        //GLOBALIZED
        /// <summary>
        /// Triggered when the bot gets a Group Invite
        /// </summary>
        /// <returns>To accept or not</returns>
        public override bool OnGroupAdd()
        {
            return clsFunctions.DealWithGroupAdd(OtherSID, this.Bot.BotControlClass);
        }//OnClanAdd()

        public override bool OnFriendAdd () 
        {
            Bot.log.Success(Bot.SteamFriends.GetFriendPersonaName(OtherSID) + " (" + OtherSID.ConvertToUInt64() + ") added me!");//show someone added the bot
            FriendAddedHandler.Start();
            if (!IsAdmin)
            {
                FriendRemoveHandler.Start();
            }
            return true;
        }

        public override void OnFriendRemove () 
        {
            Bot.log.Warn( Bot.SteamFriends.GetFriendPersonaName(OtherSID)  + " removed me from friends list.");//show someone removed me
        }

        public override void OnLoginCompleted()
        {
            
        }

        public override void OnTradeSuccess()
        {

        }

        public override void OnTradeTimeout()
        {

        }

        public override void OnMessage (string message, EChatEntryType type) 
        {
            if (IsAdmin)
            {
                string MessageHandled;
                MessageHandled = clsFunctions.DealWithAdminCommand(this.Bot, OtherSID, message);//Deal with the ItemRemovedMsg, or receive something back if the bot needs specific things to do.

                if (MessageHandled == String.Empty)
                {
                    return;
                }
                if (message == "donate")
                {
                    Bot.SteamTrade.Trade(OtherSID);
                    return;
                }
            }
            Bot.SteamFriends.SendChatMessage(OtherSID, type, "To donate to Gamem Trade Services, just trade me! You have 10 minutes from adding me before I remove you!");
        }

        public override bool OnTradeRequest() 
        {
            Bot.log.Success(Bot.SteamFriends.GetFriendPersonaName(OtherSID) + " (" + OtherSID.ConvertToUInt64() + ") requested to trade with me!");//show someone added the bot
            return true;
        }
        
        public override void OnTradeError (string error) 
        {
            //Bot.SteamFriends.SendChatMessage (OtherSID, 
            //                                  EChatEntryType.ChatMsg,
            //                                  "Oh, there was an error: " + error + "."
            //                                  );
            Bot.log.Warn (error);
        }
        
        
        public override void OnTradeInit() 
        {
            Bot.log.Success("Trade started!");
            Bot.SteamFriends.SetPersonaState(EPersonaState.Busy);
            FileToCreate = "Donation\\" + System.DateTime.Today.ToString("MM-dd-yyyy") + ".log";

            if (!File.Exists(FileToCreate))
            {
                Bot.log.Success("File created!");
                File.Create(FileToCreate);
            }
            if (!IsAdmin)
            {
                EChatEntryType type = EChatEntryType.ChatMsg;
                Bot.SteamFriends.SendChatMessage(OtherSID, type, "Thank you for donating to Gamem's Trading Service!");
            }
            else
            {
                foreach (var item in Trade.MyInventory.Items)
                {
                    if (!item.IsNotTradeable)
                    {
                        Trade.AddItem(item.Id);
                    }
                }
            }
        }
        
        public override void OnTradeAddItem (Schema.Item schemaItem, Inventory.Item inventoryItem) 
        {
            if (schemaItem == null || inventoryItem == null)
            {
                Trade.CancelTrade();
                Bot.SteamFriends.SendChatMessage(OtherSID, EChatEntryType.ChatMsg, "I'm sorry. I believe SteamAPI is down. Please try to trade again in a few minutes.");
                Bot.log.Warn("Issue getting inventory item. API down? Closing trade.");
                return;
            }
            string ItemAddedMsg = String.Format("User added {0} {1} {2} {3}", inventoryItem.IsNotCraftable.ToString().ToLower() == "true" ? "NonCraftable" : "Craftable", clsFunctions.ConvertQualityToString(schemaItem.ItemQuality), schemaItem.ItemName, schemaItem.CraftMaterialType); //ready ItemRemovedMsg
            Bot.log.Success("User donated: " + ItemAddedMsg);
        }
        
        public override void OnTradeRemoveItem (Schema.Item schemaItem, Inventory.Item inventoryItem) 
        {
            string ItemRemovedMsg = String.Format("User added {0} {1} {2} {3}", inventoryItem.IsNotCraftable.ToString().ToLower() == "true" ? "NonCraftable" : "Craftable", clsFunctions.ConvertQualityToString(schemaItem.ItemQuality), schemaItem.ItemName, schemaItem.CraftMaterialType); //ready ItemRemovedMsg
            Bot.log.Warn("User removed: " + ItemRemovedMsg);
        }
        
        public override void OnTradeMessage (string message) 
        {
            Bot.log.Warn("User sent trade message: " + message);
            
        }

        //GLOBALIZED
        /// <summary>
        /// Triggers when the trade has closed.
        /// </summary>
        public override void OnTradeClose()
        {
            base.OnTradeClose();//close the trade
            Bot.SteamFriends.SetPersonaState(EPersonaState.Online);//Set status
        }//OnTradeClose()

        public override void OnTradeReady (bool ready) 
        {
            //Because SetReady must use its own version, it's important
            //we poll the trade to make sure everything is up-to-date.
            Trade.Poll();
            if (!ready)
            {
                Trade.SetReady (false);
            }
            else
            {
                Bot.log.Success("User is ready to trade!");
                if (Validate())
                {
                    Trade.SetReady(true);
                    Bot.log.Success("Readying trade.");
                }
                else
                {
                    Bot.log.Warn("Invalid trade!");
                    Trade.SendMessage("Invalid trade!");
                }
            }
        }
        
        public override void OnTradeAccept() 
        {
            if (Validate() || IsAdmin)
            {
                Bot.log.Success("Accepting trade...");
                try
                {               
                    Trade.AcceptTrade();
                    if (!IsAdmin)
                    {
                        string fileDirectory = "Donation";
                        string fileName = OtherSID.ConvertToUInt64().ToString();
                        string fullPath = Path.Combine(fileDirectory, fileName + ".log");
                        fullPath = FileToCreate;
                        string sPastDonation = string.Empty;
                        string sSpacer = @"\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\";
                        if (File.Exists(fullPath))
                        {
                            TextReader reader = new StreamReader(fullPath);
                            sPastDonation = reader.ReadToEnd();
                            reader.Close();
                        }
                        else
                        {
                            File.Create(fullPath);
                        }

                        StreamWriter writer = new StreamWriter(fullPath, false);
                        writer.WriteLine(sSpacer);
                        List<Inventory.Item>items = new List<Inventory.Item>();
                        foreach (TradeUserAssets id in Trade.OtherOfferedItems)
                        {
                            if (id.appid == 440)
                                items.Add(Trade.OtherInventory.GetItem(id.assetid));//Get item
                        }//foreach (ulong id in Trade.OtherOfferedItems)
                        foreach (Inventory.Item id in items)
                        {
                            Schema schema = Trade.CurrentSchema;
                            Inventory inventory = Trade.OtherInventory;
                            Schema.Item schemaItem = schema.GetItem(id.Defindex);
                            string ItemAddedMsg = String.Format("User added {0} {1} {2} {3}", id.IsNotCraftable.ToString().ToLower() == "true" ? "NonCraftable" : "Craftable", clsFunctions.ConvertQualityToString(schemaItem.ItemQuality), schemaItem.ItemName, schemaItem.CraftMaterialType); //ready ItemRemovedMsg
                            writer.WriteLine("Donated: " + ItemAddedMsg);
                        }
                        writer.WriteLine(Bot.SteamFriends.GetFriendPersonaName(OtherSID));
                        writer.WriteLine(OtherSID.ConvertToUInt64().ToString());
                        writer.WriteLine(System.DateTime.Now.ToShortTimeString());
                        if (sPastDonation != null)
                        {
                            writer.Write(sPastDonation);
                        }
                        writer.Close();

                     
                        Bot.SteamFriends.SendChatMessage(OtherSID, EChatEntryType.ChatMsg, "Thank you for donating!");
                        Bot.SteamFriends.SendChatMessage(clsFunctions.BotsOwnerID, EChatEntryType.ChatMsg, "A user just donated!");

                    }

                    Log.Success("Trade Complete!");                  
                }
                catch(System.Exception ex)
                {

                    Log.Warn ("The trade might have failed, but we can't be sure.");
                    Log.Warn(ex.Message);
                }

            }
            Bot.SteamFriends.SetPersonaState(EPersonaState.Online);

            OnTradeClose ();
        }



        public bool Validate ()
        {            
            KeysPutUp = 0;
            
            List<string> errors = new List<string> ();                        
            
            // send the errors
            if (errors.Count != 0)
                Trade.SendMessage("There were errors in your trade: ");
            foreach (string error in errors)
            {
                Trade.SendMessage(error);
            }
            
            return errors.Count == 0 || IsAdmin;
        }
    }
}
