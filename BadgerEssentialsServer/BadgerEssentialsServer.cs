using System;
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
        public BadgerEssentialsServer()
		{
            Tick += onTick;

            //
            // Commands
            //

            // Revive Command
            RegisterCommand("revive", new Action<int, List<object>, string>((source, args, raw) =>
            {
                // Revive Self - Add timer later
                if (args.Count == 0)
                {
                    TriggerClientEvent("BadgerEssentials:RevivePlayer", source);
                }
                // Revive other person - Add timer later
                else if (int.Parse(args[0].ToString()) != source)
                {
                    TriggerClientEvent("BadgerEssentials:RevivePlayer", int.Parse(args[0].ToString()));
				}
			}), false);
        }

        // Runs every tick
        private async Task onTick()
        {

        }
    }
}
