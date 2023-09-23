using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Timers;
using Terraria;
using TShockAPI;

namespace AntiLag
{
    internal class ItemClear
    {
        public static bool inprogress = false; // checks if the items are being cleared at the moment
        public static DateTime LastCheck = DateTime.UtcNow;
        public static System.Timers.Timer Timer = new System.Timers.Timer();
        private static string tag = TShock.Utils.ColorTag(AntiLag.config.tag, Color.Orange);

        internal static void AntiLagTimer()
        {
            Timer.Interval = AntiLag.config.clearCheckIntevalMS;
            Timer.Enabled = AntiLag.config.enabled;
            Timer.Elapsed += new ElapsedEventHandler(TimerElapsed);
        }

        internal static void TimerElapsed(object sender, ElapsedEventArgs args)
        {
            bool isEvent = (Main.invasionType == 0) ? false : true;

            if (isEvent && AntiLag.config.disableHalOnEvents)
                return;  
            IDictionary<int,Item> activeItems = new Dictionary<int,Item>();
            
            if (!inprogress)
            {

                // goes through all items in the world and checks what items are active
                for (int i = 0; i < 400; i++) 
                {
                    if (Main.item[i].active)
                    {
                        activeItems.Add(i,Main.item[i]);
                    }
                }

                int numberOfActiveItems = activeItems.Count();
                if (numberOfActiveItems > 150)
                {
                    inprogress = true;
                    int sleepMultiplier = 5;
                    
                    if (numberOfActiveItems > 275)
                    {
                        sleepMultiplier = 2;
                    }
                    
                    // sorts all active items by time since spawned
                    if( (AntiLag.config.itemAmountToKeepOnEvents != 0 && isEvent) 
                      || (AntiLag.config.itemAmountToKeep != 0 && !isEvent) )
                        activeItems = activeItems.OrderBy(i =>-i.Value.timeSinceItemSpawned).ToDictionary(i => i.Key, i => i.Value);

                    int trashItems = numberOfActiveItems - (isEvent ? AntiLag.config.itemAmountToKeepOnEvents : AntiLag.config.itemAmountToKeep);
                    TShock.Utils.Broadcast(string.Format("{0} Discovered {1} trash items. Removing in {2} seconds", tag,
                        trashItems, sleepMultiplier * AntiLag.config.baseTimeUntilClearLagMS / 1000), Color.Silver);


                    Thread.Sleep(AntiLag.config.baseTimeUntilClearLagMS * sleepMultiplier);
                    
                    int i = 0;
                    foreach (KeyValuePair<int, Item> pair in activeItems)
                    {
                        if (isEvent 
                            && AntiLag.config.itemAmountToKeepOnEvents != 0
                            && i > activeItems.Count - AntiLag.config.itemAmountToKeepOnEvents)
                            break;

                        if (!isEvent
                           && AntiLag.config.itemAmountToKeep != 0
                           && i > activeItems.Count - AntiLag.config.itemAmountToKeep)
                            break;


                        if (pair.Value.active)
                        {
                            Main.item[pair.Key].active = false;
                            TSPlayer.All.SendData(PacketTypes.UpdateItemDrop, "", pair.Key, 0f, 0f, 0f, 0);
                        }
                        
                        i++;
                    }
                    
                    if(AntiLag.config.syncTilesOnIntervalToo)
                        Commands.HandleCommand(TSPlayer.Server, "/sync");

                    TShock.Utils.Broadcast(string.Format("{0} All trash items have been cleared And sync'd players to the server", tag), Color.Silver);
                    inprogress = false;
                }
            }
        }
    }
}