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
        public static bool inprogress = false;
        public static DateTime LastCheck = DateTime.UtcNow;
        public static System.Timers.Timer Timer = new System.Timers.Timer();
        private static string tag = TShock.Utils.ColorTag("HAL9000:", Color.Orange);

        internal static void AntiLagTimer()
        {
            Timer.Interval = AntiLag.config.clearIntevalMS;
            Timer.Enabled = AntiLag.config.enabled;
            Timer.Elapsed += new ElapsedEventHandler(TimerElapsed);
        }

        internal static void TimerElapsed(object sender, ElapsedEventArgs args)
        {
            bool isEvent = (Main.invasionType == 0) ? false : true;

            if (isEvent && AntiLag.config.disableHalOnEvents)
                return;
            bool flag = !inprogress;
            IDictionary<int,Item> activeItems = new Dictionary<int,Item>();
            
            if (flag)
            {
                int num = 0;
                int num2;
                for (int i = 0; i < 400; i = num2 + 1)
                {
                    bool active = Main.item[i].active;
                    if (active)
                    {
                        activeItems.Add(i,Main.item[i]);
                        num2 = num;
                        num = num2 + 1;
                    }
                    num2 = i;
                }
                bool flag2 = num > 150;
                if (flag2)
                {
                    int num3 = 5;
                    inprogress = true;
                    bool flag3 = num > 275;
                    if (flag3)
                    {
                        num3 = 2;
                    }
                    else
                    {
                        bool flag4 = num > 225;
                        if (flag4)
                        {
                            num3 = 5;
                        }
                    }
                    
                    if( (AntiLag.config.itemAmountToKeepOnEvents != 0 && isEvent) 
                      || (AntiLag.config.itemAmountToKeep != 0 && !isEvent) )
                        activeItems = activeItems.OrderBy(i =>-i.Value.timeSinceItemSpawned).ToDictionary(i => i.Key, i => i.Value);
                    

                    TShock.Utils.Broadcast(string.Format("{0} Discovered {1} trash items. Removing in {2} seconds", tag,
                                           num - (isEvent ? AntiLag.config.itemAmountToKeepOnEvents : AntiLag.config.itemAmountToKeep), num3), Color.Silver);
                    Thread.Sleep(AntiLag.config.baseTimeUntilClearLag * num3);
                    
                    int i = 0;
                    foreach (KeyValuePair<int, Item> kvp in activeItems)
                    {
                        bool active2 = kvp.Value.active;

                        if (isEvent 
                            && AntiLag.config.itemAmountToKeepOnEvents != 0
                            && i > activeItems.Count - AntiLag.config.itemAmountToKeepOnEvents)
                            break;

                        if (!isEvent
                           && AntiLag.config.itemAmountToKeep != 0
                           && i > activeItems.Count - AntiLag.config.itemAmountToKeep)
                            break;

                        if (active2)
                        {

                            Main.item[kvp.Key].active = false;
                            
                            TSPlayer.All.SendData(PacketTypes.UpdateItemDrop, "", kvp.Key, 0f, 0f, 0f, 0);
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