﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft;
using Newtonsoft.Json.Linq;
using CitizenFX.Core;
using static CitizenFX.Core.Native.API;

namespace BadgerEssentialsServer
{
    public class BadgerEssentialsServer : BaseScript
    {
        int reviveDelay = 5; // Set to config later
        int[] revTimer;
        bool revTimerActive;

        public BadgerEssentialsServer()
        {
            Tick += onTick;

            //
            // Event Listeners
            //

            //
            // Commands
            //

            // Revive Command
            RegisterCommand("revive", new Action<int, List<object>, string>((source, args, raw) =>
            {
                // Revive Self 
                if (args.Count == 0 || int.Parse(args[0].ToString()) == source)
                    TriggerClientEvent("BadgerEssentials:RevivePlayer", source, true);
                // Revive other person 
                else if (int.Parse(args[0].ToString()) != source)
                {
                    string playerName = GetPlayerName(args[0].ToString());
                    if (!string.IsNullOrEmpty(playerName))
                        TriggerClientEvent("BadgerEssentials:RevivePlayer", int.Parse(args[0].ToString()), false);
                }
            }), false);
        }

        // Runs every tick
        private async Task onTick()
        {
        }

        //
        // Methods
        //
    }
}
