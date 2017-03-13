using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SteamKit2;
using SteamTrade;
using System.Threading;
using Newtonsoft.Json;

namespace SteamBot
{
    public class FriendRemoveHandler
    {
        private Bot bot;

        private bool bStarted = false;

        private SteamID OtherSID;

        public FriendRemoveHandler(Bot bot, SteamID otherSID)
        {
            this.bot = bot;
            this.OtherSID = otherSID;
        }

        public void Start()
        {
            if (!bStarted)
            {
                Thread thread;
                thread = new Thread(new ThreadStart(FriendRemoveTimer));
                thread.Start();
                bStarted = true;                
            }
        }

        public void FriendRemoveTimer()
        {
            Thread.Sleep(TimeSpan.FromMinutes(10));
            bot.SteamFriends.RemoveFriend(OtherSID);
            bStarted = false;           
        }
    }
}
