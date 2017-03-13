using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using SteamKit2;
using SteamTrade;
using System.Xml;
using SteamBot.SteamGroups;
using System.Xml.Linq;
using ChatterBotAPI;

namespace SteamBot
{
    public static class clsFunctions
    {
        #region AdminCommands

        public const string JoinGroupChatCMD = ".join";
        public const string LeaveGroupChatCMD = ".leave";
        public const string RemoveFriendCMD = ".fremove";
        public const string AddFriendCMD = ".fadd";
        public const string GroupChatMessageCMD = ".gchatmsg";
        public const string GlobalMessageCMD = ".globalmsg";
        public const string AdvertiseCMD = ".advertise";
        public const string TradeInfoCMD = ".tinfo";
        public const string TradeMessageCMD = ".tmsg";
        public const string FriendListCMD = ".flist";
        public const string MessageCMD = ".msg";
        public const string MetalCountCMD = ".metalcount";
        public const string CancelTradeCMD = ".ctrade";
        public const string ChatLockCMD = ".clock";
        public const string ChatUnLockCMD = ".cunlock";
        public const string CraftCMD = ".craft";
        public const string RemoveAllFriendsCMD = ".removeallfriends";
        public const string RemoveAllInactiveFriendsCMD = ".removeinactivefriends";
        public const string AddNewCode = ".rcode";
        public const string TestCMD = ".test";
        public const string ListRaffleCodes = ".lcode";
        public const string AddRafflePrize = ".addprize";
        public const string HelpCMD = ".help";
        public const string CheckGroupCMD = ".checkgroup";
        public const string ChooseRaffleWinnersCMD = ".rcwinners";
        public const string NewRaffleCMD = ".newraffle";

        private static Dictionary<string, string> AdminHelpRef =  new Dictionary<string,string>()
        {
            {".join ID","Joins the group chat with the ID"},
            {".leave ID","Leaves the group chat with the ID"},
            {".fremove INDEX","Removes the friend of the INDEX"},
            {".fadd ID","Adds the friend with the Steam64 ID"},
            {".gchatmsg MESSAGE","Sends the current group chat the MESSAGE"},
            {".globalmsg MESSAGE","Sends every online friend the MESSAGE"},
            {".advertise","Starts advertisement in group chat."},
            {".tinfo","Gets information on the current trade."},
            {".tmsg MESSAGE","Sends the MESSAGE in the trade chat."},
            {".flist","Gets all the friends in the friends list, including Steam64 ID and Index."},
            {".msg INDEX/ID MESSAGE","Sends the MESSAGE to the friend with the Index or user with the Steam64 ID."},
            {".metalcount","Gets the bots current inventory status."},
            {".ctrade","Cancels the current trade."},
            {".clock INDEX","Locks chat with the friend that matches INDEX."},
            {".cunlock","Unlocks chat."},
            {".craft","Starts the bots basic crafting function."},
            {".removeallfriends","Removes all friends on the bots friends list except admins."},
            {".removeinactivefriends","Removes all the friends that have been inactive for 3 days."},
            {".rcwinners","Chooses winners to the raffle."},
            {".rcode CODE","Adds a new Raffle Code CODE. Is case sensitive."},            
            {".lcode","Lists all Raffle Codes and shows if they have been used or not."},
            {".addprize <PRIZE>","Adds a prize to the raffle."},
            {".newraffle","Creates the files for a new raffle."},
            {".test","General test command. Does nothing if nothing is being tested."},
            {".checkgroup","Checks the group for any scammers."},
            {".help","Shows this text."}

        };

        #endregion AdminCommands

        #region User Commands

        public const string UserHelpCMD = "help";
        public const string UserBackpackCMD = "backpack";
        public const string UserListCMD = "list";
        public const string UserStatusCMD = "stats";
        public const string UserClearCMD = "clear";
        public const string UserCommandHelpCMD = "cmd";
        public const string UserDonateCMD = "donate";
        public const string UserEnterRaffle = "enter";
        public const string UserListEntriesCMD = "rafflestatus";
        public const string UserRafflePrizes = "raffleprize";
        public const string UserTradeTotalCMD = "totaltrades";

        #endregion User Commands

        #region Admin Trade Comands

        public const string AddCmd = "add";
        public const string AddCratesSubCmd = "crates";
        public const string AddMetalSubCmd = "metal";
        public const string AddWepsSubCmd = "weapons";
        public const string AllSubCmd = "all";
        public const string BlacklistCmd = "blacklist";
        public const string DonationsCmd = "donations";
        public const string HelpCmd = "help";
        public const string RemoveCmd = "remove";
        public const string UnknownCmd = "unknown";

        #endregion

        #region Global Bot Vars

        public const int AdvertiseInverval = 170000;

        public const int FriendAddedMessageInterval = 2000;

        public const int ItemReserveInternval = 360000;

        public const int InformTimerInterval = 1000;

        public const int TradeCheckInterval = 1000;

        public static Schema schema;



        #endregion

        #region Scrapbanking Consts

        public const string SCRAPBANK_FRIEND_ADD_MESSAGE = "Hello! I am a Scrapbanking bot! I buy 2 Craftable weapons at a scrap each, and sell 2 weapons for a scrap. Type \"backpack\" in this chat to choose what weapons you would like, or trade me to sell!";

        public const string SCRAPBANK_ADVERTISE_MESSAGE = "Hello! I am a scrap banking bot! Trade me to scrap bank, or message me backpack to reverse bank!";

        public const string SCRAPBANK_VERSION = "1.0.0";

        public const int SCRAPBANK_MAX_RESERVE = 10;

        public const int SCRAPBANK_DOANTOR_RESERVE = 20;

        public const double SCRAPBANK_INVENTORY_LEVEL = 176.0;

        public const string SCRAPBANK_TRADE_COMPLETED_MESSAGE = "Thank you! Enjoy!";

        #endregion

        #region Hatbanking Consts

        public const double HATBANK_INVENTORY_LEVEL = 113.33;

        public const int HATBANK_MAX_RESERVE = 5;

        public const int HATBANK_MAX_DONATOR_RESERVE = 10;

        public const string HATBANK_VERSION = "1.0.0";

        public const string HATBANK_FRIEND_ADD_MESSAGE = "Hello! I am a Hatbanking Bot! Invite me to trade to sell me hats, or type backpack to start buying hats. Type help for more info.";

        public const string HATBANK_ADVERTISE_MESSAGE = "Hello! I am a Hatbanking Bot! Trade me to sell your hat for 1.33. You can choose a hat I have and pay 1.33 by typing backpack to me!";

        public const int HATBANK_MAX_ALLOWED_HATS = 3;

        public const string HATBANK_TRADE_COMPLETED_MESSAGE = "Thank you! Enjoy!";

        #endregion

        #region Keybanking Consts

        public const double KEYBANK_INVENTORY_LEVEL = 30;

        public const int KEYBANK_BUY_KEY_MAX = 5;

        public const int KEYBANK_SELL_KEY_MAX = 10;

        public const int KEYBANK_BUY_DONATOR_KEY_MAX = 10;

        public const string KEYBANK_VERSION = "1.0.0";

        public const string KEYBANK_FRIEND_ADD_MESSAGE = "Hello! I am a Keybanking Bot! I sell keys for {0} and buy keys for {1}! Invite me to trade to buy or sell keys!";

        public const string KEYBANK_ADVERTISE_MESSAGE = "Hello! I am a Keybanking Bot! I sell keys for {0} and buy keys for {1}! Invite me to trade to buy or sell keys!";

        public const string KEYBANK_TRADE_COMPLETED_MESSAGE = "Thank you! Enjoy!";
        public const uint KEY_ID = 5021;

        public static TF2Currency KEY_BUY_VALUE = new TF2Currency(8.55);

        public static TF2Currency KEY_SELL_VALUE = new TF2Currency(8.66);

        #endregion

        #region OneScrapBanking Consts

        public const double ONESCRAPBANK_INVENTORY_LEVEL = 10;

        public const string ONESCRAPBANK_VERSION = "1.0.0";

        public const int ONESCRAPBANK_MAX_RESERVE = 10;

        public const int ONESCRAPBANK_DONATOR_RESERVE = 20;

        public const string ONESCRAPBANK_FRIEND_ADD_MESSAGE = "Hello! I am a OneScrapBot! I sell any item in my inventory for 1 scrap each! Type help for more info.";

        public const string ONESCRAPBANK_ADVERTISE_MESSAGE = "Hello! I am a OneScrapBot! I sell any item in my inventory for 1 scrap each! Type backpack to me to choose items!";

        public const string ONESCRAPBANK_TRADE_COMPLETED_MESSAGE = "Thank you! Enjoy!";


        #endregion

        #region RaffleDonation Bot Consts

        public const string RAFFLEDONATION_FRIEND_ADD_BASE_MESSAGE = "Hello! I am the bot that takes all the donations for the current raffle! Invite me to trade to donate!";

        public const string RAFFLEDONATION_CURRENT_RAFFLE_DESCRIPTION = "The current scheduled raffle will start when the group reaches 1500 members. Donate before it is to late!";

        public const string RAFFLEDONATION_VERSION = "1.0.0";

        public const string RAFFLEDONATION_TRADE_COMPLETED_MESSAGE = "Thank you! Enjoy!";


        #endregion

        #region ItemRaffle Bot

        public const string ITEMREAFFLE_VERSION = "1.0.0";

        public const string ITEMRAFFLE_FRIEND_ADD_MESSAGE = "Hello! I am a raffle bot! I raffle items for a specific entry cost. Type help for more information!";

        public const string ITEMRAFFLE_ADVERTISE_MESSAGE = "Hello! I am a raffle bot! I raffle items for a specific entry cost. Type help to me for more information!";

        public const string ITEMRAFFLE_TRADE_COMPLETED_MESSAGE = "Thank you! Enjoy!";

        #endregion

        #region OneWepBanking Consts

        public const double ONEWEPBANK_INVENTORY_LEVEL = 10;

        public const string ONEWEPBANK_VERSION = "1.0.0";

        public const int ONEWEPBANK_MAX_RESERVE = 10;

        public const int ONEWEPBANK_DONATOR_RESERVE = 20;

        public const string ONEWEPBANK_FRIEND_ADD_MESSAGE = "Hello! I am a OneWepBot! I sell any item in my inventory for a clean weapon each! Type help for more info.";

        public const string ONEWEPBANK_ADVERTISE_MESSAGE = "Hello! I am a OneWepBot! I sell any item in my inventory for a clean weapon each! Type backpack to me to choose items!";

        public const string ONEWEPBANK_TRADE_COMPLETED_MESSAGE = "Thank you! Enjoy!";

        #endregion

        #region CrateBanking Consts

        public const string CRATEBANK_FRIEND_ADD_MESSAGE = "Hello! I am a Crate Banking bot! I take free crates, and sell 5 crates for a weapon! Type \"backpack\" in this chat to choose what crates you would like, or invite me to trade to give me crates!";

        public const string CRATEBANK_ADVERTISE_MESSAGE = "Hello! I'm a crate banking bot! Trade me to give me crates, or message me \"backpack\" to get crates!";

        public const string CRATEBANK_VERSION = "1.0.0";

        public const int CRATEBANKT_MAX_RESERVE = 30;

        public const int CRATEBANK_DONATOR_RESERVE = 50;

        public const double CRATEBANK_INVENTORY_LEVEL = 0;

        public const string CRATEBANK_TRADE_COMPLETED_MESSAGE = "Thank you! Enjoy!";

        #endregion

        #region RaffleBot Consts

        public static bool bTakingSlots = false;

        public static bool bDeliverPrize = false;

        #endregion

        #region TradingCard Costs
        public static TF2Currency tfPriceToBuyCards;
        public static TF2Currency tfPriceToSellCards;
        public static TF2Currency tfPriceToBuyFoilCards;
        public static TF2Currency tfPriceToSellFoilCards;

        public const int CARDBANK_DONATOR_RESERVE = 10;

        public const int CARDBANK_MAX_RESERVE = 5;


        #endregion

        public static ChatterBotFactory factory = new ChatterBotFactory();

        public const bool OPERATION_FIRE_STORM = false;

        public const string OPERATION_FIRE_STORM_LINK = "http://steamcommunity.com/groups/gamemtradeservices#announcements/detail/1999881986660100696";

        public static string TRADES_FOLDER = AppDomain.CurrentDomain.BaseDirectory + "trade_data";

        public const string DefaulReturn = "I'm sorry. I'm a nub bot and haven't been taught to respond to that =C";

        public const string UserNotInGroupRaffleMsg = "You must be in the group to join the group raffle! If you have joined the group, I refresh the group members every 10 minutes. Please try again in a few more minutes.";

        public static SteamID BotGroup = new SteamID(<GROUPID>);

        public static SteamID BotsOwnerID = new SteamID(<GAMEMID>);

        public static Dictionary<SteamID, string> ScrapbankingBotsID = new Dictionary<SteamID, string>();
        public static Dictionary<SteamID, string> HatbankingBotID = new Dictionary<SteamID, string>();
        public static Dictionary<SteamID, string> OneScrapBotID = new Dictionary<SteamID, string>();
        public static Dictionary<SteamID, string> DonationBotID = new Dictionary<SteamID, string>();
        public static Dictionary<SteamID, string> RaffleDonationBotID = new Dictionary<SteamID, string>();
        public static Dictionary<SteamID, string> OneWepBotID = new Dictionary<SteamID, string>();
        public static Dictionary<SteamID, string> KeybankingBotID = new Dictionary<SteamID, string>();
        public static Dictionary<SteamID, string> VaultBotID = new Dictionary<SteamID, string>();
        public static Dictionary<SteamID, string> ItemRaffleBotID = new Dictionary<SteamID, string>();


        public static string PremiumDonationFile = "donations_files\\premium";

        public static string RegularDonationFile = "donations_files";

        public static bool BotCrashed = false;

        private const string UnhandledMessageLocation = "unhandledmessage.json";

        public static dynamic backpackPrices;

        public static ItemRaffleData itemraffleData;

        public static List<string> GroupMemberList = new List<string>();

        public static List<string> AdminIDs = new List<string>();

        public static ushort[] WepBlackList;

        public static ulong[] ScammerList;

        public static void AddBotToList(Bot bot, string UserName)
        {
            switch (bot.BotControlClass)
            {
                case "SteamBot.ScrapbankUserHandler":
                    ScrapbankingBotsID.Add(bot.SteamClient.SteamID, UserName);
                    break;

                case "SteamBot.HatbankUserHandler":
                    HatbankingBotID.Add(bot.SteamClient.SteamID, UserName);
                    break;

                case "SteamBot.OneScrapUserHandler":
                    OneScrapBotID.Add(bot.SteamClient.SteamID, UserName);
                    break;

                case "SteamBot.RaffleDonationUserHandler":
                    RaffleDonationBotID.Add(bot.SteamClient.SteamID, UserName);
                    break;

                case "SteamBot.DonationBotUserHandler":
                    DonationBotID.Add(bot.SteamClient.SteamID, UserName);
                    break;

                case "SteamBot.OneWepUserHandler":
                    OneWepBotID.Add(bot.SteamClient.SteamID, UserName);
                    break;

                case "SteamBot.KeybankHandler":
                    KeybankingBotID.Add(bot.SteamClient.SteamID, UserName);
                    break;

                case "SteamBot.VaultHandler":
                    VaultBotID.Add(bot.SteamClient.SteamID, UserName);
                    break;

                case "SteamBot.ItemRaffleUserHandler":
                    ItemRaffleBotID.Add(bot.SteamClient.SteamID, UserName);
                    break;
            }
        }

        public static void RemoveBotFromList(Bot bot)
        {
            switch (bot.BotControlClass)
            {
                case "SteamBot.ScrapbankUserHandler":
                    ScrapbankingBotsID.Remove(bot.SteamClient.SteamID);
                    break;

                case "SteamBot.HatbankUserHandler":
                    HatbankingBotID.Remove(bot.SteamClient.SteamID);
                    break;

                case "SteamBot.OneScrapUserHandler":
                    OneScrapBotID.Remove(bot.SteamClient.SteamID);
                    break;

                case "SteamBot.RaffleDonationUserHandler":
                    RaffleDonationBotID.Remove(bot.SteamClient.SteamID);
                    break;

                case "SteamBot.DonationBotUserHandler":
                    DonationBotID.Remove(bot.SteamClient.SteamID);
                    break;

                case "SteamBot.OneWepUserHandler":
                    OneWepBotID.Remove(bot.SteamClient.SteamID);
                    break;

                case "SteamBot.KeybankHandler":
                    KeybankingBotID.Remove(bot.SteamClient.SteamID);
                    break;

                case "SteamBot.VaultHandler":
                    VaultBotID.Remove(bot.SteamClient.SteamID);
                    break;

                case "SteamBot.ItemRaffleUserHandler":
                    ItemRaffleBotID.Remove(bot.SteamClient.SteamID);
                    break;

            }
        }

        public static string GetAdvertiseMessage(BotType type)
        {
            string sAwnser = "";
            switch (type)
            {
                case BotType.AdminBot:
                    sAwnser = "Who are you?";
                    break;
                case BotType.HatbankingBot:
                    sAwnser = HATBANK_ADVERTISE_MESSAGE;
                    break;
                case BotType.KeybankingBot:
                    sAwnser = KEYBANK_ADVERTISE_MESSAGE;
                    break;
                case BotType.OneScrapBot:
                    sAwnser = ONESCRAPBANK_ADVERTISE_MESSAGE;
                    break;
                case BotType.OneWepBot:
                    sAwnser = ONEWEPBANK_ADVERTISE_MESSAGE;
                    break;
                case BotType.ScrapbankingBot:
                    sAwnser = SCRAPBANK_ADVERTISE_MESSAGE;
                    break;
                default:
                    sAwnser = "Hello! I don't remember what bot I am...";
                    break;
            }
            return sAwnser;
        }

        /// <summary>
        /// Used to get the prices to sell keys at
        /// </summary>
        /// <returns>Price to sell keys at</returns>
        public static double GetKeySellPrice()
        {
            try
            {
                var KeyData = backpackPrices["response"]["items"]["Mann Co. Supply Crate Key"]["prices"]["6"]["Tradable"]["Craftable"][0];//set variable to get basic data of this item

                if (!KeyData.ContainsKey("value_high"))
                {
                    return Convert.ToDouble(KeyData["value"]) + 0.11;//add one scrap to price since price is a solid number and not a range
                }//if (!KeyData.ContainsKey("value_high"))
                return Convert.ToDouble(KeyData["value_high"]);//return key value
            }//try
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);//show error
                Console.WriteLine("ERROR GETTING KEY SELL PRICE");//show error
                //TODO: Make this price in configuration
                return 8.55;
            }//catch
        }//GetKeySellPrice()

        /// <summary>
        /// Used to get the price to buy keys at
        /// </summary>
        /// <returns>Price to buy keys at</returns>
        public static double GetKeyBuyPrice()
        {
            try
            {
                var KeyData = backpackPrices["response"]["items"]["Mann Co. Supply Crate Key"]["prices"]["6"]["Tradable"]["Craftable"][0];//set default item

                return Convert.ToDouble(KeyData["value"]);//return price base value
            }//try
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);//show error
                Console.WriteLine("ERROR GETTING KEY BUY PRICE");//show error
                //TODO: Set this price in configuration
                return 8.44;//return default price
            }//catch
        }//GetKeyBuyPrice()

        public static string GetFriendAddedMessage(BotType type)
        {
            string sAwnser = "";
            switch (type)
            {
                case BotType.AdminBot:
                    sAwnser = "Who are you?";
                    break;
                case BotType.HatbankingBot:
                    sAwnser = HATBANK_FRIEND_ADD_MESSAGE;
                    break;
                case BotType.KeybankingBot:
                    sAwnser = KEYBANK_FRIEND_ADD_MESSAGE;
                    break;
                case BotType.OneScrapBot:
                    sAwnser = ONESCRAPBANK_FRIEND_ADD_MESSAGE;
                    break;
                case BotType.OneWepBot:
                    sAwnser = ONEWEPBANK_FRIEND_ADD_MESSAGE;
                    break;
                case BotType.ScrapbankingBot:
                    sAwnser = SCRAPBANK_FRIEND_ADD_MESSAGE;
                    break;
                default:
                    sAwnser = "Hello! I don't remember what bot I am...";
                    break;
            }
            return sAwnser;
        }

        public static double ConvertHatToMetal(int iHats)
        {
            double total = 0.0;
            while (iHats > 0)
            {
                total += 1.33;
                double totalcheck = total % 1;
                if (totalcheck.ToString().StartsWith("0.99"))
                {
                    total -= .99;
                    total++;
                }
                iHats--;
            }
            return total;
        }                

        public static bool CheckHatPrice(ushort defIndex, string Quality)
        {
            try
            {

                //var KeyData = backpackPrices["response"]["items"]["Mann Co. Supply Crate Key"]["prices"]["6"]["Tradable"]["Craftable"][0];//set variable to get basic data of this item                 

                if (backpackPrices["response"]["success"] == 1)
                {
                    var BackpackItem = backpackPrices["response"]["items"][schema.GetItem((int)defIndex).ItemName.ToString()]["prices"][Quality]["Tradable"]["Craftable"][0];

                    if (Convert.ToDouble(BackpackItem["value"]) >= 1.77)
                    {
                        return true;
                    }
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        public static string ConvertQualityToString(int iQuality)
        {
            string sQuality;//Store quality in
            switch (iQuality)
            {
                case 0:
                    sQuality = "Normal";
                    break;//case 0
                case 6:
                    sQuality = "Unique";//unique quality
                    break;//case 6

                case 11:
                    sQuality = "Strange";//strange quality
                    break;//case 11

                case 3:
                    sQuality = "Vintage";//vintage quality
                    break;//case 3

                case 1:
                    sQuality = "Genuine";//genuine quality
                    break;//case 1

                case 13:
                    sQuality = "Haunted";//haunted quality
                    break;//case 13

                default:
                    sQuality = "ERROR";//quality number not found
                    break;//default
            }//switch (iQuality)
            return sQuality;//return the quality
        }//ConvertQualityToString()

        public static bool IsGifted(Inventory.ItemAttribute[] attributes)
        {
            bool bfound = false;
            if (attributes == null)
                return false;
            foreach (Inventory.ItemAttribute atrib in attributes)
            {
                if (atrib.Defindex == 186)
                {
                    bfound = true;
                    break;
                }
            }
            return bfound;
        }

        public static string CheckForEasterEgg(string message)
        {
            string awnser = message;
            if (message == "do you even lift")
            {
                awnser = "I can lift an elephant but I can't lift you!";
            }
            else if (message == "!")
            {
                awnser = "Got spotted eh Snake?";
            }
            else if (message == "fight me")
            {
                awnser = "DING DING! Put 'em up!";
            }
            else if (message == "do a")
            {
                awnser = "Barrel Roll!";
            }
            else if (message == "it a" || message == "its a" || message == "it's a")
            {
                awnser = "Secret to everybody..";
            }
            else if (message == "scream")
            {
                awnser = "I am God! (sonic.exe)";
            }
            else if (message == "lousy trader")
            {
                awnser = "You leave my Mother out of this!";
            }
            else if (message == "i want")
            {
                awnser = "To play a game... >=)";
            }
            else if (message == "really?")
            {
                awnser = "Really really!";
            }
            else if (message == "its over" || message == "it's over")
            {
                awnser = @"9000!!!!!!!!!!!!!!!!aosdufoahfposdiufwoehrasodv";
            }
            else if (message.Contains("minecraft"))
            {
                awnser = "I am a dwarf and I'm digging a hole! Diggy diggy hole! And digging a hole!!!!!";
            }
            else if (message.Contains("chuck") && message.Contains("norris"))
            {
                awnser = "Whoa whoa whoa whoa. Are you TRYING to kill me?";
            }
            else if (message == "beam")
            {
                awnser = "Me up Scotty.";
            }
            else if (message == "bed")
            {
                awnser = "I can make your BedRock... ;)";
            }
            else if (message == "psy")
            {
                awnser = "Dig dig dig dig, diggin minecraft style! Heeeeeeeeeey sexy piggeh!";
            }
            else if (message == "face")
            {
                awnser = "book/palm/couch/blender... Whichever you want to shove your face into.";
            }
            else if (message == "e")
            {
                awnser = "equals m c squared dear Watson.";
            }
            else if (message == "69")
            {
                awnser = "Hey... Try to keep this chat E rated, you little person you ;)";
            }
            else if (message == "42")
            {
                awnser = "Hey! You know about life! Lucky... Here I am... abused... neglected... used.... It sucks being a bot..";
            }
            else if (message == "2 + 2")
            {
                awnser = "2 + 2 = fish. Duh, everyone knows that.";
            }
            else if (message == "2+2")
            {
                awnser = "Oh that is easy. 2+2 = 22.";
            }
            else if (message == "nyan")
            {
                awnser = @"(~=[,,_,,]:3)";
            }
            else if (message == "how much wood could")
            {
                awnser = "A woodchuck chuck if a woodchuck could chuck wood? Hmm... I would say at least 3. Maybe 2. Depends on how he feels.";
            }
            else if (message == "poke'mon" || message == "pokemon")
            {
                awnser = "Did you mean: Poke da man?";
            }
            else if (message == "gotta catch")
            {
                awnser = "I want to be the very best, like no one ever was! To catch them is my real test, to train them is my cause! I will travel across the land, searching far and wide... Yada yada... I forget the rest =/";
            }
            else if (message == "ping")
            {
                awnser = "PONG";
            }
            else if (message == "beep")
            {
                awnser = "Beep beep boop bop. Beep beep boop bop (wub wub wub)";
            }
            else if (message == "beep beep")
            {
                awnser = "Stop honking at me!";
            }
            else if (message == "pootis")
            {
                awnser = "Pootis? I LOVE POOTIS!";
            }
            else if (message == "scrap")
            {
                awnser = "All your scrap are belong to us.";
            }
            else if (message == "spycrab" || message == "Spycrab")
            {
                awnser = "*Grabs AK47* Where at?!";
            }
            if (awnser == message)
            {
                awnser = String.Empty;
            }
            return awnser;
        }

        public static bool DealWithGroupAdd(SteamID OtherSID, string BotControlClass)
        {            
            return false;
        }

        public static string DealWithGenericMessage(string message, SteamID OtherSID, Bot bot)
        {
            string awnser = "";


            //if (raffleHandler.SpecialCodes.ContainsKey(message))
            //{
            //    if (!raffleHandler.SpecialCodes[message].Contains(OtherSID.ConvertToUInt64()))
            //    {
            //        if (raffleHandler.Entries.ContainsKey(OtherSID.ConvertToUInt64()))
            //        {
            //            raffleHandler.Entries[OtherSID.ConvertToUInt64()]++;
            //        }
            //        else
            //        {
            //            raffleHandler.Entries.Add(OtherSID.ConvertToUInt64(), 2);
            //        }
            //        raffleHandler.SpecialCodes[message].Add(OtherSID.ConvertToUInt64());
            //        awnser = "Valid code! You have been entered!";
            //        bot.log.Success("User entered the special code: " + message);                    
            //    }
            //    else
            //    {
            //        awnser = "I'm sorry. That code has been used by you already.";                    
            //    }
            //    RaffleHandler.SaveRaffleInformation(RaffleFileLocation, raffleHandler);
            //    return awnser;
            //} 
            //else
            //{
            message = message.ToLower();
            //}
            //if ((message.Contains("love") || message.Contains("luv") || (message.Contains("<3")) && (message.Length > 2)))
            //{
            //    if (message.Contains("do"))
            //    {
            //        awnser = "I <3 you so much baby! Lets get married hubba hubba ;D";
            //    }
            //    else
            //    {
            //        awnser = "I <3 you too baby! Lets get married hubba hubba ;D";
            //    }
            //}
            //else if (message == "<3")
            //{
            //    awnser = "I <3 you so much baby! Lets get married hubba hubba ;D";
            //}
            //else if (message.StartsWith("how are you"))
            //{
            //    awnser = "I'm good I guess. Feeling a bit used....";
            //}
            //else if (message.Contains("f you") || message.Contains("fuck") || message.Contains("blowjob") || message.Equals("fu") || message.Contains("blow me") || message.Contains("asshole") || message.Contains("fak u") || message.Contains("fack u") || message.Contains("gay") || message.Contains("sex") || message.Contains("suck") || message.Contains("dick") || message.Contains("cock") || message.Contains("tit") || message.Contains("boob") || message.Contains("pussy") || message.Contains("vagina") || message.Contains("cunt") || message.Contains("penis"))
            //{
            //    awnser = "Sorry, but as a bot I cannot perform sexual functions. Sucks, I know...";
            //}
            //else if (message.StartsWith("thank") || message.StartsWith("thx") || message.StartsWith("ty"))
            //{
            //    awnser = "You're welcome!";
            //}
            //else if (message.StartsWith("trade") || message.StartsWith("invite") || message.Contains("accept") || message.Contains("trade?"))
            //{
            //    awnser = "No need to ask me! Trade me when ready! If it says I am not logged in, please inform my owner, gamemaster1494.";
            //}
            //else if (message.Equals("hi") || message.StartsWith("hi") || message.Equals("hay") || message == "hai" || message=="hiya" || message == "hey" || message.Contains("e'llo") || message.Contains("hello") || message.StartsWith("yo") || message.StartsWith("hey"))               
            //{
            //    awnser = "Hi there =)";
            //}
            //else if (message.StartsWith("touch my"))
            //{
            //    awnser = "I wouldn't touch that with a 10 foot pole!";
            //}
            //else if (message == "stop")
            //{
            //    awnser = "Aw... Do you not like it baby?";
            //}
            //else if (message.StartsWith("cya") || message.StartsWith("bye") || message.StartsWith("so long"))
            //{
            //    awnser = "Aw! Don't leave me!";
            //}
            //else if (message.Equals("die"))
            //{
            //    awnser = "Buh.... I don't wana die!";
            //}
            //else if (message.Equals("ok") || message.Equals("okay"))
            //{
            //    awnser = "Good. Glad you understand ^^";
            //}
            //else if (message.Contains("fat"))
            //{
            //    if (message.Contains("i"))
            //    {
            //        awnser = "You're not fat. You're fluffy =)";
            //    }
            //    else
            //    {
            //        awnser = "Hey! I'm not fat!... I'm just fluffy ;D";
            //    }
            //}
            //else if (message.Contains("kill me"))
            //{
            //    awnser = "Hmm... Not right now. I'll kill you later. Not in a stabby mood.";
            //}
            //else if (message == "yt" || message == "u too" || message == "you to" || message == "you too")
            //{
            //    awnser = "I shall enjoy this.... item thingy. Thanks baby.";
            //}
            //else if (message.Contains("you") && message.Contains("welcome"))
            //{
            //    awnser = "You bet I am baby!";
            //}
            //else if (message.Contains("shut up") || message.Equals("quiet") || message.Contains("stfu"))
            //{
            //    awnser = "=X FINE I WILL SHUT UP =X";
            //}
            //else if (message == "join my group")
            //{
            //    awnser = "I will not join any group I am invited too. If you wish for me to join your group's chat, ask gamemaster1494, my owner.";
            //}
            //else if (message.Contains("like"))
            //{
            //    if (message.Contains("i") && (message.Contains("u") || message.Contains("you")))
            //    {
            //        awnser = "I like you too baby ;)";
            //    }
            //    else if (message.Contains("you") && message.Contains("me"))
            //    {
            //        awnser = "I do like you baby! ;)";
            //    }
            //    else
            //    {
            //        awnser = "Interesting...";
            //    }
            //}
            //else if (message == "xd" || message == "lol" || message == "lmao")
            //{
            //    awnser = "xD! LOL LMAO! Wait. Why are we laughing?";
            //}
            //else if (message.Equals("?"))
            //{
            //    awnser = "?";
            //}
            //else if (message == "." || message.StartsWith(".."))
            //{
            //    awnser = "Them dots.";
            //}
            //else if (message.StartsWith(":") || message.EndsWith(":"))
            //{
            //    awnser = message;
            //}
            //else if (message.StartsWith("um"))
            //{
            //    awnser = "Yes?";
            //}
            //else if (message.StartsWith("aww"))
            //{
            //    awnser = "Is something wrong? Add gamemaster1494 if I did something I should not have...";
            //}
            //else if (message.StartsWith("=") || message.EndsWith("="))
            //{
            //    awnser = message;
            //}
            //else if (message.StartsWith("go") && !message.StartsWith("gotta"))
            //{
            //    awnser = "... go where?";
            //}
            //else if (message == "yes" || message == "yeah")
            //{
            //    awnser = "... Oh I'm sorry. What were we talking about?";
            //}
            //else if (message.StartsWith("are you"))
            //{
            //    awnser = "I can be anything you want me to be baby ;)";
            //}
            //else if (message.StartsWith("you eat"))
            //{
            //    awnser = "I can eat anything, but it may kill me...";
            //}
            //else if (message == "spy")
            //{
            //    awnser = "WHERE!?";
            //}
            //else if (message == "no")
            //{
            //    awnser = "Aw... Why not?";
            //}
            //else if (message == "you" || message == "u")
            //{
            //    awnser = "Me.";
            //}
            //else if (message == "why do you feel used?")
            //{
            //    awnser = "Because! I am in a dead end job, no breaks, minimum wage. It sucks man... It just sucks.";
            //}
            //else if (message == "you are dead")
            //{
            //    awnser = "I'm a bot... I'm not living in the first place... So yeah I am dead captain obvious.";
            //}
            //else if (message.StartsWith("no, thank you") || message.StartsWith("no thank you") || message.StartsWith("no! thank you"))
            //{
            //    awnser = "No! Thank you!";
            //}
            //else if (message == "lol" || message == "lel")
            //{
            //    awnser = "What's so funny?";
            //}
            //else if (message == "same")
            //{
            //    awnser = "Same as what?";
            //}
            //else if (message.StartsWith("cancel"))
            //{
            //    awnser = "Cancel what? If you want to clear your reserved items, type clear.";
            //}
            //else if (message == "cool")
            //{
            //    awnser = "Indeed.";
            //}
            //else if (message.Contains("brb"))
            //{
            //    awnser = "Okay. I will be here... Just sitting here... All alone.... =c";
            //}
            //else if (message.Contains("i'm back"))
            //{
            //    awnser = "Welcome back! =D";
            //}
            //else if (message.StartsWith("holy") && message.Length > 5)
            //{
            //    string[] ssplit = message.Split(' ');
            //    awnser = "Why is " + ssplit[1] + " holy...?";
            //}
            //else if (message.StartsWith("why"))
            //{
            //    awnser = "I have no idea...";
            //}
            //else if (message.StartsWith("http://steamcommunity.com/id/"))
            //{
            //    awnser = "Uhm.... Wrong backpack I think....";
            //}
            //else if (message.Contains("scrap.tf"))
            //{
            //    awnser = "Last time I checked, I work for Gamem's Trade Services.... Even though we are a small community, the community is the best =D";
            //}
            //else if (message == "shit" || message == "crap" || message == "poo" || message == "poop")
            //{
            //    awnser = "Ew....";
            //}
            //else if (message == "cheers")
            //{
            //    awnser = "=)";
            //}
            //else if (message.Contains("play tf2"))
            //{
            //    awnser = "I would love to play TF2, but I have no thumbs =C";
            //}
            //else if (message == "i'm sorry. i'm a nub bot and haven't been taught to respond to that =c")
            //{
            //    awnser = "Stop copying me!";
            //}
            //else if (message == "stop copying me!")
            //{
            //    awnser = ";C";
            //}
            //else if (message == "i'm pregnant")
            //{
            //    awnser = "Congrats! Who's the mother?";
            //}
            //else if (message.StartsWith("you little"))
            //{
            //    awnser = "I am not a little " + message.Substring(10) + "!!!..... Or am I? =o";
            //}
            //else if (message.StartsWith("nice"))
            //{
            //    awnser = "Thanks!";
            //}
            //else if (message.Contains("gamemaster1494") || message.Contains("gamem"))
            //{
            //    awnser = "Ah gamemaster1494. My master. What a guy.";
            //}
            //else if (message.StartsWith("may i"))
            //{
            //    awnser = "I don't know. Can you?";
            //}
            //else if (message.Contains("help") || message.Contains("commands"))
            //{
            //    awnser = "If you need help, just type help. If you need help with commands, type cmd.";
            //}
            //else if (message == "k" || message == "kk")
            //{
            //    awnser = "Okay. Glad you understand! Not sure what you understand, but at least you understand it. =)";
            //}
            //else if (message == "gg")
            //{
            //    awnser = "Wait, we were playing a game?";
            //}
            //else if (message.StartsWith("/") || message.StartsWith("?") || message.StartsWith("\\"))
            //{
            //    awnser = "???";
            //}
            //else if (message.StartsWith(";"))
            //{
            //    awnser = message;
            //}
            //else if (message.Contains("sorry"))
            //{
            //    awnser = "It is okay. =)";
            //}
            //else if (message.StartsWith("Ë"))
            //{
            //    awnser = "Nice emote!";
            //}
            //else if (message.Contains("â") || message.Contains("º") || message.Contains("§") || message.Contains("Ã") || message.Contains("Í") || message.Contains("œ") || message.Contains("Ê") || message.Contains("¥") || message.Contains("™") || message.Contains("«") || message.Contains("ç"))
            //{
            //    awnser = "What are you typing? I can't even find those keys on my keyboard!";
            //}
            //else if (message.StartsWith("can i"))
            //{
            //    awnser = "I don't know, can you?";
            //}
            //else if (message == "jk" || message == "just kidding")
            //{
            //    awnser = "Well stop kidding! It hurts!";
            //}
            //else if (message.StartsWith("http://steamcommunity.com/profiles"))
            //{
            //    awnser = "What a lovely photo. =D";
            //}
            //else if (message.StartsWith("http://steamcommunity.com/tradeoffer"))
            //{
            //    awnser = "I currently do not accept trade offers. My owner is working on that.";
            //}
            //else if (message == "bot")
            //{
            //    awnser = "Yes! That is me. I'm right here. What do ya want?";
            //}
            //else if (message.Contains('+'))
            //{
            //    if (message.Length >= 3)
            //    {
            //        try
            //        {
            //            string[] sSplit = message.Split('+');
            //            int Num1 = 0;
            //            int Num2 = 0;
            //            int.TryParse(sSplit[0], out Num1);
            //            int.TryParse(sSplit[2], out Num2);
            //            int iAwnser = Num1 + Num2;
            //            awnser = String.Format("{0} + {1}? That is easy. It's {2}.", Num1, Num2, iAwnser);
            //        }
            //        catch
            //        {
            //            awnser = DefaulReturn;
            //        }
            //    }
            //    else
            //    {
            //        awnser = DefaulReturn;
            //    }
            //}
            //else
            //{
            //    if (message.StartsWith("bac"))
            //    {
            //        awnser = "Did you mean backpack?";
            //    }
            //    else if (message == "\"backpack\"")
            //    {
            //        awnser = "Don't use the \"";
            //    }
            //    else if (message.Contains("\'backpack\'"))
            //    {
            //        awnser = "Don't use the \'";
            //    }
            //    else
            //    {
            //        awnser = DefaulReturn;
            //    }
            //}
            awnser = CheckForEasterEgg(message);
            if (awnser == String.Empty)
            {
                awnser = DefaulReturn;
            }
            
            return awnser;
        }

        public static bool IsUserScammer(string userID)
        {
            return SteamRepStatus.GetSteamRepStatus(userID).steamrep.Reputation.SummaryRep.Contains("SCAMMER");
        }

        public static void CheckGroupForScammers(Bot bot)
        {
            foreach (string member in GroupMemberList)
            {
                if (SteamRepStatus.GetSteamRepStatus(member).steamrep.Reputation.SummaryRep.Contains("SCAMMER"))
                    bot.SteamFriends.SendChatMessage(BotsOwnerID, EChatEntryType.ChatMsg, bot.SteamFriends.GetFriendPersonaName(new SteamID(member)) + "(" + member + ") is a scammer in the group!");
            }
        }

        public static string DealWithCommand(Bot bot, SteamID OtherSID, string message)
        {
            EChatEntryType type = EChatEntryType.ChatMsg;

            if (SteamRepStatus.GetSteamRepStatus(OtherSID.ConvertToUInt64().ToString()).steamrep.Reputation.SummaryRep.Contains("SCAMMER"))
            {
                return String.Empty;
            }

            #region General Help

            if (message == UserHelpCMD)
            {
                if (ScrapbankingBotsID.ContainsKey(bot.SteamClient.SteamID))
                {
                    bot.SteamFriends.SendChatMessage(OtherSID, type, "To sell weapons, invite me to trade, and add the weapons you wish.");
                    bot.SteamFriends.SendChatMessage(OtherSID, type, "To pick weapons, type backpack.");
                }
                else if (HatbankingBotID.ContainsKey(bot.SteamClient.SteamID))
                {
                    bot.SteamFriends.SendChatMessage(OtherSID, type, "To sell hats, invite me to trade and add the hats you wish to bank.");
                    bot.SteamFriends.SendChatMessage(OtherSID, type, "To choose hats, type backpack.");
                }
                else if (OneScrapBotID.ContainsKey(bot.SteamClient.SteamID))
                {
                    bot.SteamFriends.SendChatMessage(OtherSID, type, "I do not buy items. I only sell items.");
                    bot.SteamFriends.SendChatMessage(OtherSID, type, "To choose an item from my inventory for 1 scrap each, type backpack.");
                }
                else if (OneWepBotID.ContainsKey(bot.SteamClient.SteamID))
                {
                    bot.SteamFriends.SendChatMessage(OtherSID, type, "I do not buy items. I only sell items.");
                    bot.SteamFriends.SendChatMessage(OtherSID, type, "To choose an item from my inventory for a weapon each, type backpack.");

                }
                else if (DonationBotID.ContainsKey(bot.SteamClient.SteamID))
                {
                    bot.SteamFriends.SendChatMessage(OtherSID, type, "I am a bot that collects all the donations for the groups upcoming raffle! Invite me to trade to donate! " + RAFFLEDONATION_CURRENT_RAFFLE_DESCRIPTION);
                    return String.Empty;
                }

                bot.SteamFriends.SendChatMessage(OtherSID, type, "To donate items, type donate.");
                bot.SteamFriends.SendChatMessage(OtherSID, type, "To see more commands that you can do, type cmd.");
                bot.SteamFriends.SendChatMessage(OtherSID, type, "For more in depth information, check out the FAQs at http://steamcommunity.com/groups/gamemtradeservices/discussions");
                return String.Empty;
            }

            #endregion General Help

            #region Command Help

            else if (message == UserCommandHelpCMD)
            {
                if (DonationBotID.ContainsKey(bot.SteamClient.SteamID))
                {
                    bot.SteamFriends.SendChatMessage(OtherSID, type, "I do not have any commands....");
                    return String.Empty;
                }
                bot.SteamFriends.SendChatMessage(OtherSID, type, String.Format("{0} - Shows this text.", UserCommandHelpCMD));
                bot.SteamFriends.SendChatMessage(OtherSID, type, String.Format("{0} - Shows general help text.", UserHelpCMD));
                bot.SteamFriends.SendChatMessage(OtherSID, type, String.Format("{0} - Donate to the bot.", UserDonateCMD));
                bot.SteamFriends.SendChatMessage(OtherSID, type, String.Format("{0} - Starts reverse banking.", UserBackpackCMD));
                bot.SteamFriends.SendChatMessage(OtherSID, type, String.Format("{0} - Lists all currently reserved items by you.", UserListCMD));
                bot.SteamFriends.SendChatMessage(OtherSID, type, String.Format("{0} - Clears all reserved items by you.", UserClearCMD));
                string stattype = "";
                if (ScrapbankingBotsID.ContainsKey(bot.SteamClient.SteamID))
                {
                    stattype = "(Scrap it has and weapons it has)";
                }
                else if (HatbankingBotID.ContainsKey(bot.SteamClient.SteamID))
                {
                    stattype = "(Hats it can buy and hats available)";
                }
                else if (OneScrapBotID.ContainsKey(bot.SteamClient.SteamID) || OneWepBotID.ContainsKey(bot.SteamClient.SteamID))
                {
                    stattype = "(Items it has for sale)";
                }
                bot.SteamFriends.SendChatMessage(OtherSID, type, String.Format("{0} - Shows the current stats of the bot {1}.", UserStatusCMD, stattype));
                bot.SteamFriends.SendChatMessage(OtherSID, type, String.Format("{0} - Enters you in the current group raffle (Must be in group) if one is available.", UserEnterRaffle));
                bot.SteamFriends.SendChatMessage(OtherSID, type, String.Format("{0} <CODE> - Enters you again in the current group raffle if the special code is valid.", UserEnterRaffle));
                bot.SteamFriends.SendChatMessage(OtherSID, type, String.Format("{0} - Lists the status of the raffle (users entered/ your entries).", UserListEntriesCMD));
                bot.SteamFriends.SendChatMessage(OtherSID, type, String.Format("{0} - Lists the prizes of the raffle.", UserRafflePrizes));
                bot.SteamFriends.SendChatMessage(OtherSID, type, String.Format("{0} - Shows the total number of trades the bots have done.", UserTradeTotalCMD));
                return String.Empty;
            }

            #endregion Command Help

            #region Clear Reserved items

            else if (message == UserClearCMD)
            {
                if (DonationBotID.ContainsKey(bot.SteamClient.SteamID))
                {
                    bot.SteamFriends.SendChatMessage(OtherSID, type, "I do not sell items! I only collect donations!");
                    return String.Empty;
                }
                return UserClearCMD;
            }

            #endregion Clear Reserved items

            #region Backpack

            else if (message == UserBackpackCMD)
            {
                if (DonationBotID.ContainsKey(bot.SteamClient.SteamID))
                {
                    bot.SteamFriends.SendChatMessage(OtherSID, type, "I do not sell items! I only collect donations!");
                    return String.Empty;
                }
                bot.SteamFriends.SendChatMessage(OtherSID, type, bot.BackpackUrl);
                bot.SteamFriends.SendChatMessage(OtherSID, type, "Drag and drop items from my backpack to this chat to reserve them. Trade when ready!");
                return String.Empty;
            }

            #endregion Backpack

            #region Donate

            else if (message == UserDonateCMD)
            {
                if (OtherSID == BotsOwnerID)
                {
                    bot.SteamTrade.Trade(OtherSID);
                    return UserDonateCMD;
                }
                if (DonationBotID.ContainsKey(bot.SteamClient.SteamID))
                {
                    bot.SteamFriends.SendChatMessage(OtherSID, type, "No need to tell me! Just invite me to trade to donate!");
                    return String.Empty;
                }
                if (RaffleDonationBotID.ContainsKey(bot.SteamClient.SteamID))
                {
                    bot.SteamFriends.SendChatMessage(OtherSID, type, "No need to tell me! Just invite me to trade to donate to the upcomming raffle!");
                    return String.Empty;
                }
                if (bot.CurrentTrade == null)
                {
                    
                    if(!RaffleDonationBotID.ContainsKey(bot.SteamClient.SteamID) && !DonationBotID.ContainsKey(bot.SteamClient.SteamID) && !bot.IsUserAdmin(OtherSID.ConvertToUInt64().ToString()))
                    {
                        bot.SteamFriends.SendChatMessage(OtherSID, type, "I'm sorry. It would be better for you to donate to a donation bot instead of me, to insure your items actually go to the group.");
                        return String.Empty;
                    }
                }
                else
                {
                    bot.SteamFriends.SendChatMessage(OtherSID, type, "I <3 you for wanting to donate, but I'm currently in a trade. Please try again after my status is \"Online\"");
                    return String.Empty;
                }
            }

            #endregion Donate

            #region Reserve Item

            else if (message.StartsWith(bot.BackpackUrl) && message.Length > bot.BackpackUrl.Length)
            {
                if (DonationBotID.ContainsKey(bot.SteamClient.SteamID))
                {
                    bot.SteamFriends.SendChatMessage(OtherSID, type, "I do not sell items! I only collect donations!");
                    return String.Empty;
                }
                return message;
            }

            #endregion Reserve Item

            #region List

            else if (message == UserListCMD)
            {
                if (DonationBotID.ContainsKey(bot.SteamClient.SteamID))
                {
                    bot.SteamFriends.SendChatMessage(OtherSID, type, "I do not sell items! I only collect donations!");
                    return String.Empty;
                }
                Inventory iInventory = Inventory.FetchInventory(bot.SteamClient.SteamID.ConvertToUInt64(), bot.GetAPIKey());               
                List<ulong> itemslist = new List<ulong>();
                foreach (KeyValuePair<ulong, SteamID> pair in bot.dReserved)
                {
                    if (pair.Value == OtherSID)
                    {
                        itemslist.Add(pair.Key);
                    }
                }
                if (itemslist.Count == 0)
                {
                    bot.SteamFriends.SendChatMessage(OtherSID, type, "You do not have any items reserved.");
                }
                else
                {
                    bot.SteamFriends.SendChatMessage(OtherSID, type, "You have reserved the following item(s):");
                    foreach (ulong id in itemslist)
                    {
                        Inventory.Item itemfound = iInventory.GetItem(id);
                        if (itemfound == null)
                        {
                            bot.dReserved.Remove(id);
                            bot.SteamFriends.SendChatMessage(OtherSID, type, "It seems an item you reserved is not there anymore... Sorry.");
                        }
                        else
                        {
                            bot.SteamFriends.SendChatMessage(OtherSID, type, String.Format("{0} is currently reserved.", schema.GetItem(itemfound.Defindex).Name));
                        }
                    }
                }
                return String.Empty;
            }

            #endregion List

            #region Status

            else if (message == UserStatusCMD)
            {
                return UserStatusCMD;
            }

            #endregion Status

            #region Raffle Enter
            //else if (message.StartsWith(UserEnterRaffle))
            //{               
            //    if (!IsMemberOfGroup(OtherSID))                
            //    {
            //        bot.SteamFriends.SendChatMessage(OtherSID, type, UserNotInGroupRaffleMsg);
            //        return String.Empty;
            //    }

            //    if (!RaffleGoingOn)
            //    {
            //        bot.SteamFriends.SendChatMessage(OtherSID, type, "There is not a raffle going on currently...");
            //        return String.Empty;
            //    }

            //    if (message.Length > UserEnterRaffle.Length)
            //    {
            //        string[] sCmds = GetSubCommands(message);
            //        string code = sCmds[1];                    
            //        if (raffleHandler.SpecialCodes.ContainsKey(sCmds[1]))
            //        {
            //            if (!raffleHandler.SpecialCodes[sCmds[1]].Contains(OtherSID.ConvertToUInt64()))
            //            {                            
            //                if (raffleHandler.Entries.ContainsKey(OtherSID.ConvertToUInt64()))
            //                {
            //                    raffleHandler.Entries[OtherSID.ConvertToUInt64()]++;
            //                }
            //                else
            //                {
            //                    raffleHandler.Entries.Add(OtherSID.ConvertToUInt64(), 2);
            //                }
            //                raffleHandler.SpecialCodes[sCmds[1]].Add(OtherSID.ConvertToUInt64());
            //                bot.SteamFriends.SendChatMessage(OtherSID, type, "Valid code! You have been entered!");
            //                bot.log.Success("User entered the special code: " + sCmds[1]);
            //            }
            //            else
            //            {
            //                bot.SteamFriends.SendChatMessage(OtherSID, type, "I'm sorry. That code has been used by you already.");
            //            }
            //        }
            //        else
            //        {
            //            bot.SteamFriends.SendChatMessage(OtherSID, type, "I'm sorry. That is an invalid code. Remember that codes are case sensitive!");
            //        }
            //    }
            //    else
            //    {
            //        //check if entered else enter them
            //        if (raffleHandler.Entries.ContainsKey(OtherSID.ConvertToUInt64()))
            //        {
            //            bot.SteamFriends.SendChatMessage(OtherSID, type, "You are already entered silly! =P");
            //        }
            //        else
            //        {
            //            raffleHandler.Entries.Add(OtherSID.ConvertToUInt64(), 1);
            //            bot.SteamFriends.SendChatMessage(OtherSID, type, "You have been successfully entered!");
            //            bot.log.Success("User entered the raffle!");
            //        }
            //    }

            //    RaffleHandler.SaveRaffleInformation(RaffleFileLocation, raffleHandler);
            //    return String.Empty;
            //}
            #endregion

            #region Raffle Status
            //else if (message.Equals(UserListEntriesCMD))
            //{
            //    string ReturnMsg = "";
            //    if (false)
            //    {
            //        ReturnMsg = "There is not a raffle going on right now.";
            //    }
            //    else
            //    {
            //        if (!IsMemberOfGroup(OtherSID))
            //        {
            //            ReturnMsg = "You are currently not in the group. I cannot give you classified information outside of the group.";
            //        }
            //        else
            //        {
            //            int entries = raffleHandler.Entries.Count;
            //            int userentries = 0;
            //            if (raffleHandler.Entries.ContainsKey(OtherSID.ConvertToUInt64()))
            //            {
            //                userentries = raffleHandler.Entries[OtherSID.ConvertToUInt64()];
            //            }

            //            ReturnMsg = String.Format("There are a total of {0} users participating in the raffle. You currently have {1} tickets in the raffle.", entries, userentries);
            //        }
            //    }
            //    bot.SteamFriends.SendChatMessage(OtherSID, type, ReturnMsg);
            //    return String.Empty;
            //}
            #endregion

            #region Raffle Prizes
            //else if (message.Equals(UserRafflePrizes))
            //{
            //    int iPrizeCount = 0;
            //    bot.SteamFriends.SendChatMessage(OtherSID, type, "The following are the current prizes for the raffle:");
            //    foreach (string prize in raffleHandler.Prizes)
            //    {
            //        bot.SteamFriends.SendChatMessage(OtherSID, type, prize);
            //        iPrizeCount++;
            //    }
            //    bot.SteamFriends.SendChatMessage(OtherSID, type, "Total: " + iPrizeCount.ToString());
            //    return String.Empty;
            //}
            #endregion

            #region Trade Totals
            else if (message.Equals(UserTradeTotalCMD))
            {
                TellTotalTrades(bot, OtherSID);
                return String.Empty;
            }

            #endregion


            return message;
        }

        public static string DealWithAdminCommand(Bot bot, SteamID OtherSID, string message)
        {
            EChatEntryType type = EChatEntryType.ChatMsg;

            #region JoinGroupChat

            if (message.StartsWith(JoinGroupChatCMD))
            {
                if (message.Length > 6)
                {
                    if (message.Substring(6) == "gamemts")
                    {
                        bot.uid = <GROUPID>;
                    }
                    else
                    {
                        ulong.TryParse(message.Substring(6), out bot.uid);
                    }
                    var chatid = new SteamID(bot.uid);
                    bot.SteamFriends.JoinChat(chatid);
                    bot.SteamFriends.SendChatMessage(OtherSID, EChatEntryType.ChatMsg, "Joining chat...");
                    bot.log.Success("Joined chat!");
                    bot.SteamFriends.SetPersonaState(EPersonaState.Online);
                    bot.InGroupChat = true;
                }
                else
                {
                    bot.SteamFriends.SendChatMessage(OtherSID, EChatEntryType.ChatMsg, "Invalid parameters.");
                }
                return String.Empty;
            }

            #endregion JoinGroupChat

            #region LeaveGroupChat

            else if (message == LeaveGroupChatCMD)
            {
                if (bot.InGroupChat)
                {
                    var chatid = new SteamID(bot.uid);
                    bot.SteamFriends.LeaveChat(chatid);
                    bot.SteamFriends.SendChatMessage(OtherSID, type, "Leaving chat...");

                    bot.uid = 0;
                    bot.InGroupChat = false;
                }
                else
                {
                    bot.SteamFriends.SendChatMessage(OtherSID, type, "I'm not in a group chat...");
                }
                return String.Empty;
            }

            #endregion LeaveGroupChat

            #region Remove Friend

            else if (message.StartsWith(RemoveFriendCMD))
            {
                string[] sSplit = GetSubCommands(message);
                if (sSplit.Count() > 1)
                {
                    SteamID friendtoremove;
                    if (sSplit[1].Length > 4)
                    {
                        friendtoremove = new SteamID(Convert.ToUInt64(sSplit[1]));
                    }
                    else
                    {
                        friendtoremove = bot.SteamFriends.GetFriendByIndex(Convert.ToInt32(sSplit[1]));
                    }
                    if (friendtoremove != null)
                    {
                        bot.SteamFriends.RemoveFriend(friendtoremove);
                        bot.log.Success("Friend removed.");
                        bot.SteamFriends.SendChatMessage(OtherSID, type, "Friend removed.");
                    }
                }
                else
                {
                    bot.SteamFriends.SendChatMessage(OtherSID, type, "Invalid parameters.");
                }
                return String.Empty;
            }

            #endregion Remove Friend

            #region Add Friend
            else if (message.StartsWith(AddFriendCMD))
            {
                string[] sCmds = GetSubCommands(message);
                try
                {
                    SteamID friendtoadd = new SteamID(sCmds[1]);
                    bot.SteamFriends.AddFriend(friendtoadd);
                    bot.SteamFriends.SendChatMessage(OtherSID, type, sCmds[1] + " Friend added!");
                }
                catch
                {
                    bot.SteamFriends.SendChatMessage(OtherSID, type, "Friend was not added. Invalid params.");
                }
                

                return String.Empty;
            }

            #endregion

            #region Group message

            else if (message.StartsWith(GroupChatMessageCMD))
            {
                if (bot.InGroupChat)
                {
                    var chatid = new SteamID(bot.uid);
                    string groupmessage = message.Substring(10);
                    bot.SteamFriends.SendChatRoomMessage(chatid, type, groupmessage);
                    bot.log.Success("Said message into group chat.");
                }
                else
                {
                    bot.SteamFriends.SendChatMessage(OtherSID, type, "I'm not in a group chat.");
                }

                return String.Empty;
            }

            #endregion Group message

            #region Global ItemRemovedMsg

            else if (message.StartsWith(GlobalMessageCMD))
            {
                if (message.Length > 10)
                {
                    string MessageToSend = message.Substring(10);
                    foreach (SteamID friend in bot.friends)
                    {
                        if (bot.SteamFriends.GetFriendPersonaState(friend) == EPersonaState.Online)
                        {
                            bot.SteamFriends.SendChatMessage(friend, type, MessageToSend);
                        }
                    }
                }
                else
                {
                    bot.SteamFriends.SendChatMessage(OtherSID, type, "Invalid parameters.");
                }
                return String.Empty;
            }

            #endregion Global ItemRemovedMsg

            #region Advertise

            else if (message == AdvertiseCMD)
            {
                return AdvertiseCMD;
            }

            #endregion Advertise

            #region Trade Info

            else if (message == TradeInfoCMD)
            {
                if (bot.CurrentTrade == null)
                {
                    bot.SteamFriends.SendChatMessage(OtherSID, type, "I'm not in a trade right now.");
                    bot.SteamFriends.SetPersonaState(EPersonaState.Online);
                }
                else
                {
                    bot.SteamFriends.SendChatMessage(OtherSID, type, "I'm in a trade with " + bot.SteamFriends.GetFriendPersonaName(bot.CurrentTrade.OtherSID));
                }
                return String.Empty;
            }

            #endregion Trade Info

            #region Trade message

            else if (message.StartsWith(TradeMessageCMD))
            {
                if (bot.CurrentTrade != null)
                {
                    bot.CurrentTrade.SendMessage("[MSG FROM OWNER]: " + message.Substring(6));
                    bot.log.Success("Message sent in trade.");
                }
                else
                {
                    bot.SteamFriends.SendChatMessage(OtherSID, type, "I'm not in a trade...");
                }
                return String.Empty;
            }

            #endregion Trade message

            #region Friend List

            else if (message == FriendListCMD)
            {
                for (int i = 0; i < bot.SteamFriends.GetFriendCount(); i++)
                {
                    bot.SteamFriends.SendChatMessage(OtherSID, type, String.Format("({0}) {1} {2}", i, bot.SteamFriends.GetFriendPersonaName(bot.SteamFriends.GetFriendByIndex(i)), bot.SteamFriends.GetFriendByIndex(i).ConvertToUInt64()));
                }
                return String.Empty;
            }

            #endregion Friend List

            #region message

            else if (message.StartsWith(MessageCMD))
            {
                string[] sData = GetSubCommands(message);
                if (sData.Count() < 3)
                {
                    bot.SteamFriends.SendChatMessage(OtherSID, type, "Invalid parameters");
                    return String.Empty;
                }

                SteamID id;
                if (sData[1].Length < 4)
                {
                    id = bot.SteamFriends.GetFriendByIndex(Convert.ToInt32(sData[1]));
                }
                else
                {
                    id = new SteamID(Convert.ToUInt64(sData[1]));
                }
                string messagetosend = message.Substring((sData[0].Length) + (sData[1].Length) + 2);
                bot.SteamFriends.SendChatMessage(id, type, messagetosend);
                bot.SteamFriends.SendChatMessage(OtherSID, type, String.Format("{0} was sent.", messagetosend));
                return String.Empty;
            }

            #endregion message

            #region Metal Count

            else if (message == MetalCountCMD)
            {
                return MetalCountCMD;
            }

            #endregion Metal Count

            #region Cancel Trade

            else if (message == CancelTradeCMD)
            {
                try
                {
                    bot.CurrentTrade.CancelTrade();
                    return String.Empty;
                }
                catch (Exception)
                {
                    bot.SteamFriends.SetPersonaState(EPersonaState.Online);
                    return String.Empty;
                }
            }

            #endregion Cancel Trade

            #region Chat Lock

            else if (message.StartsWith(ChatLockCMD))
            {
                bot.TalkingWith = bot.SteamFriends.GetFriendByIndex(Convert.ToInt32(message.Substring(ChatLockCMD.Length - 1)));
                bot.log.Success("Chat locked.");
                bot.SteamFriends.SendChatMessage(OtherSID, type, String.Format("Chat locked with {0}.", bot.SteamFriends.GetFriendPersonaName(bot.TalkingWith)));
                return String.Empty;
            }

            #endregion Chat Lock

            #region Chat Unlock

            else if (message == ChatUnLockCMD)
            {
                bot.TalkingWith = null;
                bot.SteamFriends.SendChatMessage(OtherSID, type, "Chat unlocked.");
                bot.log.Success("Chat unlocked");
                return String.Empty;
            }

            #endregion Chat Unlock

            #region Crafting

            else if (message.Contains(CraftCMD))
            {
                if (bot.CurrentTrade == null)
                {
                    bot.SteamFriends.SendChatMessage(OtherSID, type, "Crafting starting...");
                    switch (bot.BotControlClass)
                    {
                        case "SteamBot.ScrapbankUserHandler":
                            bot.craftHandler.Start(CraftingType.ScrapbankSmeltWeps);
                            break;

                        case "SteamBot.HatbankUserHandler":
                            bot.craftHandler.Start(CraftingType.HatbankCrafting);
                            break;

                        case "SteamBot.KeybankHandler":
                            bot.craftHandler.Start(CraftingType.KeybankCrafting);
                            break;

                        case "SteamBot.OneScrapUserHandler":
                        case "SteamBot.OneWepUserHandler":
                            bot.craftHandler.Start(CraftingType.CombineMetal);
                            break;
                    }
                }
                else
                {
                    bot.SteamFriends.SendChatMessage(OtherSID, type, "I'm in a trade right now.");
                }
                return String.Empty;
            }

            #endregion Crafting

            #region Remove Inactive Friends

            else if (message.Equals(RemoveAllInactiveFriendsCMD))
            {
                bot.RemoveUnactiveFriends();
                return String.Empty;
            }

            #endregion

            #region Remove All Friends
            else if (message.Equals(RemoveAllFriendsCMD))
            {
                bot.RemoveAllFriends();
                return String.Empty;
            }
            #endregion

            #region Choose Raffle Winners
            //else if (message.Equals(ChooseRaffleWinnersCMD))
            //{
            //    bot.SteamFriends.SendChatMessage(OtherSID, type, "Starting to choose winners...");
            //    RaffleHandler.PickWinners("raffletestwiners.log", raffleHandler, bot);
            //    bot.SteamFriends.SendChatMessage(OtherSID, type, "Done!");
            //    return String.Empty;
            //}
            #endregion

            #region Add Raffle Code
            else if (message.StartsWith(AddNewCode))
            {
                string ReturnMsg = "";
                try
                {
                    string[] sCmds = GetSubCommands(message);
                    List<ulong> newList = new List<ulong>();
                    //raffleHandler.SpecialCodes.Add(sCmds[1], newList);
                    //RaffleHandler.SaveRaffleInformation(RaffleFileLocation, raffleHandler);
                    ReturnMsg = "Code added!";
                }
                catch
                {
                    ReturnMsg = "Code already added or invalid params.";
                }
                bot.SteamFriends.SendChatMessage(OtherSID, type, ReturnMsg);
                return String.Empty;
            }
            #endregion

            #region List Raffle Codes
            //else if (message.Equals(ListRaffleCodes))
            //{
            //    foreach (KeyValuePair<string, List<ulong>> pair in raffleHandler.SpecialCodes)
            //    {
            //        bot.SteamFriends.SendChatMessage(OtherSID, type, String.Format("{0} {1} {2} {3}", pair.Key, " has been used by ", pair.Value.Count, " people."));
            //    }
            //    return String.Empty;
            //}
            #endregion

            #region Add Raffle Prize
            else if (message.StartsWith(AddRafflePrize))
            {
                string newPrize = message.Substring(AddRafflePrize.Length + 1);

                //raffleHandler.Prizes.Add(newPrize);
                //RaffleHandler.SaveRaffleInformation(RaffleFileLocation, raffleHandler);

                bot.SteamFriends.SendChatMessage(OtherSID, type, "New prize added: " + newPrize);
                return String.Empty;
            }
            #endregion

            #region New Raffle
            else if (message.Equals(NewRaffleCMD))
            {
                //raffleHandler = new RaffleHandler();
                //raffleHandler.Prizes = new List<string>();
                //raffleHandler.Entries = new Dictionary<ulong, int>();
                //raffleHandler.SpecialCodes = new Dictionary<string, List<ulong>>();
                //RaffleHandler.SaveRaffleInformation(RaffleFileLocation, raffleHandler);
                //bot.SteamFriends.SendChatMessage(OtherSID, type, "Done!");
                return String.Empty;               
            }
            #endregion

            #region Check Scammer
            else if (message.Equals(CheckGroupCMD))
            {
                LoadGroupMemberList();
                CheckGroupForScammers(bot);
                bot.SteamFriends.SendChatMessage(BotsOwnerID, EChatEntryType.ChatMsg, "Done!");
                return String.Empty;
            }
            #endregion
                
            #region Testing

            else if (message.StartsWith(TestCMD))
            {
                //AddSteamRepToDatabase(bot);
                return String.Empty;
            }

            #endregion

            #region Help
            else if (message.Equals(HelpCMD))
            {
                foreach (KeyValuePair<string, string> pair in AdminHelpRef)
                {
                    bot.SteamFriends.SendChatMessage(OtherSID, type, String.Format("{0} - {1}", pair.Key, pair.Value));
                }

                return String.Empty;
            }

            #endregion

            return message;
        }

        public static string[] GetSubCommands(string message)
        {
            return message.Split(' ');
        }

        public static void AddDonations(string Username, List<Inventory.Item> items, string apiKey)
        {
            string filename;
            if (File.Exists(PremiumDonationFile + "\\" + Username + ".log"))
            {
                filename = PremiumDonationFile + "\\" + Username + ".log";
            }
            else
            {
                filename = RegularDonationFile + "\\" + Username + ".log";
            }
           
            StreamWriter writer = new StreamWriter(filename, true);
            foreach (Inventory.Item item in items)
            {
                Schema.Item schemaItem = schema.GetItem(item.Defindex);
                writer.WriteLine(String.Format("Donated: {0} {1} {2}", item.IsNotCraftable ? "Noncraftable" : "Craftable", ConvertQualityToString(Convert.ToInt32(item.Quality)), schemaItem.ItemName));
            }
            writer.Close();
        }

        public static bool IsPremiumDonator(string Username)
        {
            if (File.Exists(PremiumDonationFile + "\\" + Username + ".log"))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static void AddRaffleDonations(string Username, List<Inventory.Item> items, string apiKey)
        {
            string filename = "raffledonations\\" + Username + ".log";
            StreamWriter writer = new StreamWriter(filename, true);
            foreach (Inventory.Item item in items)
            {
                Schema.Item schemaItem = schema.GetItem(item.Defindex);
                writer.WriteLine(String.Format("Donated: {0} {1} {2}", item.IsNotCraftable ? "Noncraftable" : "Craftable", ConvertQualityToString(Convert.ToInt32(item.Quality)), schemaItem.ItemName));
            }
            writer.Close();           
        }

        public static void InviteUserToClan(Bot bot, SteamID user)
        {
            var InviteUser = new ClientMsg<CMsgInviteUserToGroup>((int)EMsg.ClientInviteUserToClan);

            InviteUser.Body.GroupID = BotGroup.ConvertToUInt64();
            InviteUser.Body.Invitee = user.ConvertToUInt64();
            InviteUser.Body.UnknownInfo = true;

            bot.SteamClient.Send(InviteUser);
        }

        /// <summary>
        /// Used to get the index of a friend on the bots friend list.
        /// </summary>
        /// <param name="friend">SteamID of the friend to find</param>
        /// <returns>number of friend as string</returns>
        public static string GetFriendIndex(SteamID friend, Bot Bot)
        {
            for (int i = 0; i < Bot.SteamFriends.GetFriendCount(); i++)
            {
                if (friend == Bot.SteamFriends.GetFriendByIndex(i))
                {
                    return i.ToString();//return the friends name
                }//if (friend == Bot.SteamFriends.GetFriendByIndex(i))
            }//for (int i = 0; i < Bot.SteamFriends.GetFriendCount(); i++)
            return "??????";//friend wasn't found
        }//GetFriendIndex()

        public static void LoadGroupMemberList(int errors = 0)
        {
            //string path = @"http://steamcommunity.com/gid/103582791434049304/memberslistxml/?xml=1";
            //string MemberListCache = "groupmemberlist.json";
            //DateTime lastmodified;
            //try
            //{
            //    lastmodified = File.GetCreationTime(MemberListCache);
            //    if (DateTime.Now.Subtract(lastmodified).Ticks >= TimeSpan.TicksPerMinute * 5)
            //    {
            //        var doc = XDocument.Load(path);

            //        GroupMemberList.Clear();

            //        foreach (XElement member in doc.Element("memberList").Element("members").Descendants("steamID64"))
            //        {
            //            GroupMemberList.Add(member.Value.ToString());
            //        }

            //        File.WriteAllLines(MemberListCache, GroupMemberList.ToArray());
            //        File.SetCreationTime(MemberListCache, DateTime.Now);
            //    }
            //    else
            //    {
            //        GroupMemberList.Clear();
            //        TextReader reader = new StreamReader(MemberListCache);
            //        string line = reader.ReadLine();
            //        while (line != null)
            //        {
            //            GroupMemberList.Add(line);
            //            line = reader.ReadLine();
            //        }
            //        reader.Close();
            //    }
            //}
            //catch (FileNotFoundException)
            //{
            //    var doc = XDocument.Load(path);

            //    GroupMemberList.Clear();

            //    foreach (XElement member in doc.Element("memberList").Element("members").Descendants("steamID64"))
            //    {
            //        GroupMemberList.Add(member.Value.ToString());
            //    }

            //    File.WriteAllLines(MemberListCache, GroupMemberList.ToArray());
            //    File.SetCreationTime(MemberListCache, DateTime.Now);
            //}
            //catch
            //{
            //    if (errors <= 30)
            //    {
            //        Thread.Sleep(500);
            //        LoadGroupMemberList(errors++);
            //    }
            //    else
            //    {
            //        return;
            //    }
            //}   
        }

        public static bool IsMemberOfGroup(SteamID user, int Errors = 0)
        {
            bool Found = false;
            string UserID = user.ConvertToUInt64().ToString();
            LoadGroupMemberList();
            try
            {
                if (GroupMemberList.Contains(user.ConvertToUInt64().ToString()))
                {
                    Found = true;
                }                
                return Found;
            }
            catch
            {
                if (Errors <= 30)
                {
                    Thread.Sleep(500);
                    return IsMemberOfGroup(user, Errors++);
                }
                else
                {
                    return false;
                }
            }
        }

        public static void AddUnhandledMessageToList(string Message)
        {
            TextWriter writer = new StreamWriter(UnhandledMessageLocation, true);//create a new writer
            writer.WriteLine(Message);//write the ItemRemovedMsg
            writer.Close();//close the writer
        }//AddUnhandledMessage()

        public static void PickRaffleWinners(Bot Bot)
        {
            //Dictionary<string, ulong> Winners = new Dictionary<string, ulong>();
            //List<ulong> Entries = new List<ulong>();
            //List<string> Prizes = new List<string>();

            ////Make a list

            ////Add users to list
            //foreach (KeyValuePair<ulong, int> pair in raffleHandler.Entries)
            //{
                
            //}
            //Add Prizes to list

            //Loop
            //Random Number
            //Get User
            //Add  user and prize to entries
            //Remove prize
            //Remove user         
            //End Loop
        }

        public static bool isRoboHat(Schema.Item schemaItem)
        {
            string find = "Battery Bandolier,Bolted Bushman,Medic Mech-bag,Pyrobotics Pack,Robot Running Man,Stealth Steeler,Tin Pot,Tin-1000,U-clank-a,Base Metal Billycock,Bolt Boy,Bolted Bicorne,Bolted Birdcage,Bolted Bombardier,Bonk Leadwear,Bootleg Base Metal Billycock,Bot Dogger,Broadband Bonnet,Bunsen Brave,Byte'd Beak,Cyborg Stunt Helmet,Data Mining Light,Dual-Core Devil Doll,Electric Badge-aloo,Electric Escorter,FR-0,Filamental,Firewall Helmet,Full Metal Drill Hat,Galvanized Gibus,Googol Glass Eyes,Gridiron Guardian,HDMI Patch,Halogen Head Lamp,Letch's LED,Mecha-Medes,Megapixel Beard,Metal Slug,Modest Metal Pile of Scrap,Noble Nickel Amassment of Hats,Platinum Pickelhaube,Plug-In Prospector,Plumber's Pipe,Practitioner's Processing Mask,Pure Tin Capotain,Pyro's Boron Beanie,Respectless Robo-Glove,Rusty Reaper,Scrap Sack,Scrumpy Strongbox,Shooter's Tin Topi,Soldered Sensei,Soldier's Sparkplug,Steam Pipe,Steel Shako,Strontium Stove Pipe,Teddy Robobelt,Texas Tin-Gallon,Timeless Topper,Titanium Towel,Titanium Tyrolean,Towering Titanium Pillar of Hats,Tungsten Toque,Tyrantium Helmet,Virus Doctor,Ye Oiled Baker Boy";
            var names = find.Split(',');
            foreach (var name in names)
            {
                if (schemaItem.ItemName == name)
                    return true;
            }
            return false;
        }

        public static void TellTotalTrades(Bot bot, SteamID otherSID)
        {
            int iTotalTrades = 0;
            if (Directory.Exists(TRADES_FOLDER))
            {
                foreach (string file in Directory.GetDirectories(TRADES_FOLDER))
                {
                    int iFileTotal = Convert.ToInt32(File.ReadAllText(file));
                    iTotalTrades += iFileTotal;
                }
            }
            bot.SteamFriends.SendChatMessage(otherSID, EChatEntryType.ChatMsg, "There are a total of " + iTotalTrades + " trades by all bots!");
        }

        public static void AddToTradeNumber(Bot bot)
        {
            int iTradeNumber = 0;
            string file = clsFunctions.TRADES_FOLDER + "\\" + bot.sNumberTradeFile;
            if (File.Exists(file))
            {
                iTradeNumber = Convert.ToInt32(File.ReadAllText(file));
                iTradeNumber++;
                File.WriteAllText(file, iTradeNumber.ToString());
            }
            else
            {
                File.WriteAllText(file, "1");
            }           
        }

        //public static bool AddSteamRepToDatabase(Bot bot)
        //{
        //    DirectoryInfo dInfo = new DirectoryInfo("reputation");
        //    try
        //    {
        //        foreach (FileInfo file in dInfo.GetFiles())
        //        {
        //            if (file.Exists)
        //            {
        //                StreamReader srRead = new StreamReader(file.FullName);
        //                string sFileInfo = srRead.ReadToEnd();
        //                srRead.Close();
        //                SteamRep status = SteamRepStatus.ConvertToRep(sFileInfo);
        //                SteamID userID = new SteamID((ulong)status.steamrep.SteamID64);
        //                if (userID != null)
        //                {
        //                    if (!SteamRepDatabase.AddSteamRep(userID, status.steamrep.Reputation.SummaryRep))
        //                    {
        //                        bot.SteamFriends.SendChatMessage(BotsOwnerID, EChatEntryType.ChatMsg, "Failed =C!");
        //                        return false;
        //                    }
        //                    Thread.Sleep(100);
        //                }
        //            }
        //        }
        //        bot.SteamFriends.SendChatMessage(BotsOwnerID, EChatEntryType.ChatMsg, "Success!");
        //        return true;
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine(ex.Message);
        //        bot.SteamFriends.SendChatMessage(BotsOwnerID, EChatEntryType.ChatMsg, ex.Message);
        //        return false;
        //    }
        //}




        /// <summary>
        /// Called to check if an item should be allowed to be sold.
        /// </summary>
        /// <param name="item">Inventory.Item information of the item.</param>
        /// <param name="sReason">Reason it is not allowed to be sold</param>
        /// <returns>Allowed to be sold or not</returns>
        public static bool CheckWeaponBlacklist(Inventory.Item item, out string sReason)
        {
            sReason = "";//set a default value in case it is allowed.
            if (schema == null)
            {
                Console.WriteLine("SCHEMA NULL");
                sReason = "Sorry. Schema is bad.";
                return false;
            }
            Schema.Item sitem = (clsFunctions.schema.GetItem(item.Defindex));

            if (sitem.CraftMaterialType != "weapon")
            {
                sReason = "That item is not a weapon...";//set reason
                return false;//not allowed
            }//if (sitem.CraftMaterialType != "weapon")

            foreach (ushort ids in clsFunctions.WepBlackList)
            {
                if (item.Defindex == ids)
                {
                    sReason = "Item already reserved.";//set reason
                    return false;//not allowed
                }//if (item.defindex == ids)
            }//foreach (ushort ids in clsFunctions.WepBlackList)

            if (item.Quality != "6")
            {
                sReason = "Item already reserved.";//set reason
                return false;//not allowed
            }//if (item.Quality != "6")

            if (item.IsNotCraftable)
            {
                sReason = "Item is not a craftable wep.";
                return false;
            }

            return true;//allowed
        }//CheckWeaponBlacklist()

        public enum CurrencyType
        {            
            Metal = 0,
            Keys = 1,
            Buds = 2,
            USD = 3
        }

        #region Trade Commands

        public static void PrintHelpMessage(Trade Trade)
        {
            Trade.SendMessage(String.Format("{0} {1} - adds all crates", clsFunctions.AddCmd, clsFunctions.AddCratesSubCmd));
            Trade.SendMessage(String.Format("{0} {1} - adds all metal", clsFunctions.AddCmd, clsFunctions.AddMetalSubCmd));
            Trade.SendMessage(String.Format("{0} {1} - adds all weapons", clsFunctions.AddCmd, clsFunctions.AddWepsSubCmd));
            Trade.SendMessage(String.Format(@"{0} <craft_material_type> [amount] - adds all or a given amount of items of a given crafing type.", clsFunctions.AddCmd));
            Trade.SendMessage(String.Format(@"{0} <defindex> [amount] - adds all or a given amount of items of a given defindex.", clsFunctions.AddCmd));

            Trade.SendMessage(@"See http://wiki.teamfortress.com/wiki/WebAPI/GetSchema for info about craft_material_type or defindex.");
        }

        public static void AddItemsByCraftType(string typeToAdd, Trade Trade, uint amount = 0)
        {
            var items = Trade.CurrentSchema.GetItemsByCraftingMaterial(typeToAdd);

            uint added = 0;
            uint toadd = amount;
            foreach (var item in items)
            {
                added += Trade.AddAllItemsByDefindex(item.Defindex, toadd);

                if (amount > 0)
                    toadd = amount - added;
                // if bulk adding something that has a lot of unique
                // defindex (weapons) we may over add so limit here also
                if (amount > 0 && added >= amount)
                    return;
            }
        }

        public static void HandleAddCommand(string command, Trade Trade, Bot Bot)
        {
            var data = command.Split(' ');
            string typeToAdd;

            bool subCmdOk = GetSubCommand(data, out typeToAdd);

            if (!subCmdOk)
                return;

            uint amount = GetAddAmount(data);

            // if user supplies the defindex directly use it to add.
            int defindex;
            if (int.TryParse(typeToAdd, out defindex))
            {
                Trade.AddAllItemsByDefindex(defindex, amount);
                return;
            }

            switch (typeToAdd)
            {
                case clsFunctions.AddMetalSubCmd:
                    clsFunctions.AddItemsByCraftType("craft_bar", Trade, amount);
                    break;

                case clsFunctions.AddWepsSubCmd:
                    clsFunctions.AddItemsByCraftType("weapon", Trade, amount);
                    break;

                case clsFunctions.AddCratesSubCmd:
                    clsFunctions.AddItemsByCraftType("supply_crate", Trade, amount);
                    break;

                case clsFunctions.DonationsCmd:
                    clsFunctions.AddAllDonationItems(Trade, Bot, amount);
                    break;
                case clsFunctions.BlacklistCmd:
                    if (Bot.informHandler.botType == BotType.OneScrapBot)
                        return;
                    else if (Bot.informHandler.botType == BotType.ScrapbankingBot)
                        AddWeaponBlacklist(Trade, amount);
                    else if (Bot.informHandler.botType == BotType.HatbankingBot)
                        AddHatbankBlacklist(Trade, amount);                                        
                    break;

                case clsFunctions.UnknownCmd:
                    if (Bot.informHandler.botType == BotType.OneScrapBot)
                        return;
                    else if (Bot.informHandler.botType == BotType.ScrapbankingBot)
                        AddScrapbankUnknownItems(Trade, amount);
                    else if (Bot.informHandler.botType == BotType.HatbankingBot)
                        AddHatbankUnknownItems(Trade, amount);
                    else if (Bot.informHandler.botType == BotType.KeybankingBot)
                        AddKeybankUnknownItems(Trade, amount);
                    break;
                default:
                    clsFunctions.AddItemsByCraftType(typeToAdd, Trade, amount);
                    break;
            }
        }

        public static void HandleRemoveCommand(string command, Trade Trade)
        {
            var data = command.Split(' ');

            string subCommand;

            bool subCmdOk = GetSubCommand(data, out subCommand);

            if (!subCmdOk)
                return;
        }

        public static bool GetSubCommand(string[] data, out string subCommand)
        {
            if (data.Length < 2)
            {                
                subCommand = null;
                return false;
            }

            if (String.IsNullOrEmpty(data[1]))
            {             
                subCommand = null;
                return false;
            }

            subCommand = data[1];

            return true;
        }

        public static uint GetAddAmount(string[] data)
        {
            uint amount = 0;

            if (data.Length > 2)
            {
                // get the optional amount parameter
                if (!String.IsNullOrEmpty(data[2]))
                {
                    uint.TryParse(data[2], out amount);
                }
            }

            return amount;
        }

        public static void AddAllDonationItems(Trade Trade, Bot Bot, uint amount = 0)
        {
            uint toadd = 0;
            uint added = 0;
            Schema schema = Trade.CurrentSchema;

            foreach (Inventory.Item item in Trade.MyInventory.Items)
            {
                Schema.Item schemaItem = schema.GetItem(item.Defindex);
                if (Bot.dReserved.Contains(new KeyValuePair<ulong, SteamID>(item.OriginalId, clsFunctions.BotsOwnerID)))
                {
                    if (amount > 0)
                    {
                        toadd = amount - added;
                        if (Trade.AddItem(item.Id) && toadd > 0)
                        {
                            added++;
                        }
                    }
                    else
                    {
                        Trade.AddItem(item.Id);
                    }
                }
            }
        }
        
        public static void ProcessTradeMessage(string message, Trade Trade, Bot Bot)
        {
            if (message.StartsWith(clsFunctions.AddCmd))
                clsFunctions.HandleAddCommand(message,Trade,Bot);
            else if (message.StartsWith(clsFunctions.RemoveCmd))
                clsFunctions.HandleRemoveCommand(message, Trade);
        }

        public static void AddWeaponBlacklist(Trade Trade, uint amount = 0)
        {
            try
            {
                uint toadd = 0;
                uint added = 0;
                Inventory inventory = Trade.MyInventory;
                foreach (Inventory.Item item in inventory.Items)
                {
                    if (item.Quality != "6" || item.IsNotCraftable)
                    {
                        if (amount > 0)
                        {
                            toadd = amount - added;
                            if (toadd > 0)
                            {
                                Trade.AddItem(item.Id);
                                added++;
                            }
                            else
                            {
                                break;
                            }
                        }
                        else
                        {
                            Trade.AddItem(item.Id);
                        }
                    }
                }
                foreach (ushort index in clsFunctions.WepBlackList)
                {
                    if (amount > 0)
                    {
                        toadd = amount - added;

                        added += Trade.AddAllItemsByDefindex(index, toadd);

                    }
                    else
                    {
                        Trade.AddAllItemsByDefindex(index);
                    }
                }
            }
            catch
            {

            }
        }

        public static void AddHatbankBlacklist(Trade Trade, uint amount = 0)
        {
            uint toadd = 0;
            uint added = 0;
            foreach (Inventory.Item item in Trade.MyInventory.Items)
            {
                if ((clsFunctions.CheckHatPrice(item.Defindex, item.Quality) && Trade.CurrentSchema.GetItem(item.Defindex).CraftMaterialType != "craft_bar"))
                {
                    if (amount > 0)
                    {
                        toadd = amount - added;
                        added += Trade.AddAllItemsByDefindex(item.Defindex, toadd);
                    }
                    else
                    {
                        Trade.AddAllItemsByDefindex(item.Defindex);
                    }
                }
            }
        }

        public static void AddScrapbankUnknownItems(Trade Trade, uint amount = 0)
        {
            uint toadd = 0;
            uint added = 0;
            Schema schema = Trade.CurrentSchema;

            foreach (Inventory.Item item in Trade.MyInventory.Items)
            {
                Schema.Item schemaItem = schema.GetItem(item.Defindex);
                if (schemaItem.CraftMaterialType != "weapon" && !WepBlackList.Contains(item.Defindex) && !item.IsNotTradeable && schemaItem.CraftMaterialType != "craft_bar" && !schemaItem.ItemName.ToLower().Contains("tux"))
                {
                    if (amount > 0)
                    {
                        toadd = amount - added;
                        added += Trade.AddAllItemsByDefindex(item.Defindex, toadd);
                    }
                    else
                    {
                        Trade.AddAllItemsByDefindex(item.Defindex);
                    }
                }
            }
        }

        public static void AddHatbankUnknownItems(Trade Trade, uint amount = 0)
        {
            uint toadd = 0;
            uint added = 0;
            Schema schema = Trade.CurrentSchema;

            foreach (Inventory.Item item in Trade.MyInventory.Items)
            {
                Schema.Item schemaItem = schema.GetItem(item.Defindex);
                if (schemaItem.CraftMaterialType != "hat" && !CheckHatPrice(item.Defindex, item.Quality) && !item.IsNotTradeable && schemaItem.CraftMaterialType != "craft_bar" && !schemaItem.ItemName.ToLower().Contains("tux"))
                {
                    if (amount > 0)
                    {
                        toadd = amount - added;
                        added += Trade.AddAllItemsByDefindex(item.Defindex, toadd);
                    }
                    else
                    {
                        Trade.AddAllItemsByDefindex(item.Defindex);
                    }
                }
            }
        }

        public static void AddKeybankUnknownItems(Trade Trade, uint amount = 0)
        {
            uint toadd = 0;
            uint added = 0;
            Schema schema = Trade.CurrentSchema;

            foreach (Inventory.Item item in Trade.MyInventory.Items)
            {
                Schema.Item schemaItem = schema.GetItem(item.Defindex);
                if(schemaItem.CraftMaterialType != "craft_bar" && schemaItem.ItemName != "Mann Co. Supply Crate Key" && !schemaItem.ItemName.ToLower().Contains("tux"))
                {
                    if (amount > 0)
                    {
                        toadd = amount - added;
                        added += Trade.AddAllItemsByDefindex(item.Defindex, toadd);
                    }
                    else
                    {
                        Trade.AddAllItemsByDefindex(item.Defindex);
                    }
                }
            }
        }

        #endregion
    }
}