using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using CitizenFX.Core;
using static CitizenFX.Core.Native.API;

namespace BadgerEssentialsServer
{
    public class BadgerEssentialsServer : BaseScript
    {
        string jsonConfig = LoadResourceFile(GetCurrentResourceName(), "config/config.json");

        string currentAOP = "Sandy Shores test";
        bool peacetime = false;
        string currentPriorityStatus = "none";
        int priorityTime = 0;

        bool priorityTimerActive = false;
        public BadgerEssentialsServer()
        {
            Tick += OnTickPriorityTimer;

            JObject o = JObject.Parse(jsonConfig);

            currentAOP = (string)o.SelectToken("displayElements.aop.defaultAOP");
            //
            // Event Listeners
            //

            EventHandlers["BadgerEssentials:GetAOPFromServer"] += new Action<int>(SendAOP);
            EventHandlers["BadgerEssentials:GetAOPFromBadgerAOPVote"] += new Action<string>(SetAOPFromVote);

            //
            // Commands
            //

            // Revive Command
            RegisterCommand("revive", new Action<int, List<object>, string>((source, args, raw) =>
            {
                PlayerList pl = new PlayerList();

                // Revive Self 
                if (args.Count == 0 || int.Parse(args[0].ToString()) == source)
                {
                    Player player = pl[source];

                    if (IsPlayerAceAllowed(source.ToString(), "BadgerEssentials.Bypass.ReviveTimer"))
                    {
                        TriggerClientEvent(player, "BadgerEssentials:RevivePlayer", true, true);
                    }
                    else
                    {
                        TriggerClientEvent(player, "BadgerEssentials:RevivePlayer", true, false);
                    }
                }
                // Revive other person 
                else if (int.Parse(args[0].ToString()) != source)
                {
                    string playerName = GetPlayerName(args[0].ToString());
                    Player player = pl[int.Parse(args[0].ToString())];

                    if (!string.IsNullOrEmpty(playerName))
                    {
                        TriggerClientEvent(player, "BadgerEssentials:RevivePlayer", false, false); ;
                    }
                }
            }), false);

            // Announcement Command
            RegisterCommand("announce", new Action<int, List<object>, string>((source, args, raw) =>
            {
                if (IsPlayerAceAllowed(source.ToString(), "BadgerEssentials.Command.Announce") && args.Count > 0)
				{
                    string announcementMsg = String.Join(" ", args);
                    TriggerClientEvent("BadgerEssentials:Announce", announcementMsg);
				}
            }), false);

            //
            // PRIORITY COOLDOWN COMMANDS
            //

            // Priority Cooldown
            RegisterCommand("pc", new Action<int, List<object>, string>((source, args, raw) =>
            {
                if (args.Count > 0 && args[0].ToString() != "0")
                {
                    if (IsPlayerAceAllowed(source.ToString(), "BadgerEssentials.Command.PriorityCooldown") ||
                        IsPlayerAceAllowed(source.ToString(), "BadgerEssentials.Command.PCOnHold"))
                    {
                        currentPriorityStatus = "pc";
                        priorityTime = int.Parse(args[0].ToString());
                        TriggerClientEvent("BadgerEssentials:PriorityCooldown", "pc", priorityTime);
                    }
                }
            }), false);

            // Priority Cooldown in progress
            RegisterCommand("pc-inprogress", new Action<int, List<object>, string>((source, args, raw) =>
            {
                if (IsPlayerAceAllowed(source.ToString(), "BadgerEssentials.Command.PriorityCooldown") ||
                    IsPlayerAceAllowed(source.ToString(), "BadgerEssentials.Command.PCInProgress"))
                {
                    currentPriorityStatus = "inprogress";
                    priorityTime = 0;
                    TriggerClientEvent("BadgerEssentials:PriorityCooldown", "inprogress", 0);
                }
            }), false);

            // Priority Cooldown on hold
            RegisterCommand("pc-onhold", new Action<int, List<object>, string>((source, args, raw) =>
            {
                if (IsPlayerAceAllowed(source.ToString(), "BadgerEssentials.Command.PriorityCooldown") ||
                    IsPlayerAceAllowed(source.ToString(), "BadgerEssentials.Command.PCOnHold"))
                {
                    currentPriorityStatus = "onhold";
                    priorityTime = 0;
                    TriggerClientEvent("BadgerEssentials:PriorityCooldown", "onhold", 0);
                }
            }), false);

            // Priority Cooldown reset
            RegisterCommand("pc-reset", new Action<int, List<object>, string>((source, args, raw) =>
            {
                if (IsPlayerAceAllowed(source.ToString(), "BadgerEssentials.Command.PriorityCooldown") ||
                    IsPlayerAceAllowed(source.ToString(), "BadgerEssentials.Command.PCReset"))
                {
                    currentPriorityStatus = "reset";
                    priorityTime = 0;
                    TriggerClientEvent("BadgerEssentials:PriorityCooldown", "reset", 0);
                }
            }), false);

            // Toggle Peacetime
            RegisterCommand("pt", new Action<int, List<object>, string>((source, args, raw) =>
            {
                if (IsPlayerAceAllowed(source.ToString(), "BadgerEssentials.Command.Peacetime"))
                {
                    if (peacetime)
                        peacetime = false;
                    else peacetime = true;

                    TriggerClientEvent("BadgerEssentials:Peacetime", peacetime);
                }
            }), false);

            // Set AOP
            RegisterCommand("setAOP", new Action<int, List<object>, string>((source, args, raw) =>
            {
                if (args.Count > 0 && IsPlayerAceAllowed(source.ToString(), "BadgerEssentials.Command.SetAOP"))
                {
                    string targetAOP = String.Join(" ", args);
                    currentAOP = targetAOP;
                    TriggerClientEvent("BadgerEssentials:SetAOP", targetAOP);
                }
            }), false);
        }

        // Receive AOP / Peacetime / PC status from server
        private void SendAOP(int source)
		{
            TriggerClientEvent("BadgerEssentials:SendAOPToClient", currentAOP, peacetime, currentPriorityStatus, priorityTime);
        }

        // Receive aop from BadgerAOPVote
        private void SetAOPFromVote(string aop)
        {
            Debug.WriteLine("received from aop vote");

            currentAOP = aop;
            TriggerClientEvent("BadgerEssentials:SetAOP", aop);
        }

        private async Task OnTickPriorityTimer()
        {
            if (!priorityTimerActive && priorityTime > 0)
                priorityTimerActive = true;
            else if (priorityTimerActive && priorityTime > 0)
            {
                await Delay(60000);
                priorityTime--; 

                // Update remaining time for client or disable if time is 0
                if (priorityTime > 0)
                    TriggerClientEvent("BadgerEssentials:PriorityCooldown", "pc", priorityTime);
                else TriggerClientEvent("BadgerEssentials:PriorityCooldown", "reset", priorityTime);
            }
            else if (priorityTimerActive && priorityTime <= 0)
            {
                priorityTimerActive = false;
                currentPriorityStatus = "none";
            }
        }
    }
}
