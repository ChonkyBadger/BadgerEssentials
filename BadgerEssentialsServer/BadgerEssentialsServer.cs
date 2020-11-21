using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CitizenFX.Core;
using static CitizenFX.Core.Native.API;

namespace BadgerEssentialsServer
{
    public class BadgerEssentialsServer : BaseScript
    {
        public BadgerEssentialsServer()
        {
            //
            // Event Listeners
            //

            //
            // Commands
            //\n

            // Revive Command
            RegisterCommand("revive", new Action<int, List<object>, string>((source, args, raw) =>
            {

                // Revive Self 
                if (args.Count == 0 || int.Parse(args[0].ToString()) == source)
                    if (IsPlayerAceAllowed(source.ToString(), "BadgerEssentials.Bypass.ReviveTimer"))
                        TriggerClientEvent("BadgerEssentials:RevivePlayer", source, true, true);
                    else
                        TriggerClientEvent("BadgerEssentials:RevivePlayer", source, true, false);
                // Revive other person 
                else if (int.Parse(args[0].ToString()) != source)
                {
                    string playerName = GetPlayerName(args[0].ToString());
                    if (!string.IsNullOrEmpty(playerName))
                        TriggerClientEvent("BadgerEssentials:RevivePlayer", int.Parse(args[0].ToString()), false, false); ;
                }
            }), false);

            // Announcement Command
            RegisterCommand("announce", new Action<int, List<object>, string>((source, args, raw) =>
            {
                if (IsPlayerAceAllowed(source.ToString(), "BadgerEssentials.Command.Announce") && args.Count > 0)
				{
                    string announcementMsg = String.Join(" ", args);
                    TriggerClientEvent("BadgerEssentials:Announce", -1, announcementMsg);
				}
            }), false);

            // Priority Cooldown
            RegisterCommand("pc", new Action<int, List<object>, string>((source, args, raw) =>
            {
                if (IsPlayerAceAllowed(source.ToString(), "BadgerEssentials.Command.PriorityCooldown") && args.Count > 0 && args[0].ToString() != "0")
                    TriggerClientEvent("BadgerEssentials:PriorityCooldown", -1, "pc", int.Parse(args[0].ToString()));
            }), false);
            // Priority Cooldown in progress
            RegisterCommand("pc-inprogress", new Action<int, List<object>, string>((source, args, raw) =>
            {
                if (IsPlayerAceAllowed(source.ToString(), "BadgerEssentials.Command.PriorityCooldown"))
                    TriggerClientEvent("BadgerEssentials:PriorityCooldown", -1, "inprogress", 0);
            }), false);
            // Priority Cooldown on hold
            RegisterCommand("pc-onhold", new Action<int, List<object>, string>((source, args, raw) =>
            {
                if (IsPlayerAceAllowed(source.ToString(), "BadgerEssentials.Command.PriorityCooldown"))
                    TriggerClientEvent("BadgerEssentials:PriorityCooldown", -1, "onhold", 0);
            }), false);
            // Priority Cooldown reset
            RegisterCommand("pc-reset", new Action<int, List<object>, string>((source, args, raw) =>
            {
                if (IsPlayerAceAllowed(source.ToString(), "BadgerEssentials.Command.PriorityCooldown"))
                    TriggerClientEvent("BadgerEssentials:PriorityCooldown", -1, "reset", 0);
            }), false);

            // Toggle Peacetime
            RegisterCommand("pt", new Action<int, List<object>, string>((source, args, raw) =>
            {
                if (IsPlayerAceAllowed(source.ToString(), "BadgerEssentials.Command.Peacetime"))
                    TriggerClientEvent("BadgerEssentials:Peacetime", -1);
            }), false);

            // Set AOP
            RegisterCommand("setAOP", new Action<int, List<object>, string>((source, args, raw) =>
            {
                if (args.Count > 0 && IsPlayerAceAllowed(source.ToString(), "BadgerEssentials.Command.SetAOP"))
                {
                    string targetAOP = String.Join(" ", args);
                    TriggerClientEvent("BadgerEssentials:SetAOP", -1, targetAOP);
                }
            }), false);
        }
    }
}
