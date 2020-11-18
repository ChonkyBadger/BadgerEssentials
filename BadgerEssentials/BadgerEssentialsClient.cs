using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft;
using Newtonsoft.Json.Linq;
using CitizenFX.Core;
using static CitizenFX.Core.Native.API;
using CitizenFX.Core.UI;

namespace BadgerEssentials
{
    public class BadgerEssentials : BaseScript
    {
        string jsonConfig = LoadResourceFile("BadgerEssentials", "config/config.json");

        string ann = null;
        bool announcement = false;
        string announcementHeader; // Json Config
        int displayTime; // Json Config
        int timer = 0;
        string[] anns = { };

        // Display Elements
        string currentAOP = "Sandy Shores";
        bool peacetimeStatus = false;
        string peacetimeText = "~r~disabled";

        float peacetimeStatusX;
        float peacetimeStatusY;

        float aopX;
        float aopY;

        public BadgerEssentials()
		{
            Tick += onTick;

            //
            // Parse json config
            //

            JObject o = JObject.Parse(jsonConfig);
            peacetimeStatusX = (float)o.SelectToken("displayElements.peacetime.x");
            peacetimeStatusY = (float)o.SelectToken("displayElements.peacetime.y");
            aopX = (float)o.SelectToken("displayElements.aop.x");
            aopY = (float)o.SelectToken("displayElements.aop.y");

            //
            // Event Listeners
            //

            // Revive Command Event from server
            EventHandlers["BadgerEssentials:RevivePlayer"] += new Action<int>(RevivePlayer);

            //
            // Commands
            //

            // Suicide command
            RegisterCommand("die", new Action<int, List<object>, string>((source, args, raw) =>
            {
                int ped = GetPlayerPed(-1);
                if (!IsEntityDead(ped))
                {
                    Game.PlayerPed.Kill();
                    Screen.ShowNotification("~y~Successfuly Suicided");
                }
            }), false);

            // Set AOP command
            RegisterCommand("setAOP", new Action<int, List<object>, string>((source, args, raw) =>
            {
                if (args.Count > 0)
                    currentAOP = String.Join(" ", args);
            }), false);

            // Toggle Peacetime
            RegisterCommand("peacetime", new Action<int, List<object>, string>((source, args, raw) =>
            {
				if (peacetimeStatus == false)
				{
					peacetimeStatus = true;
					peacetimeText = "~g~enabled";
				}
				else 
				{
                    peacetimeStatus = false;
                    peacetimeText = "~r~disabled";
                }
            }), false);
        }

        // Method runs every tick
        private async Task onTick()
        {
            int ped = GetPlayerPed(-1);
            // AOP Text
            Draw2DText(aopX, aopY, "~y~AOP~s~: " + currentAOP, 0.43f, 1); // 0.165f, 0.96f

            // PeaceTime
            Draw2DText(peacetimeStatusX, peacetimeStatusY, "~y~Peacetime:~s~ " + peacetimeText, 0.43f, 1); // 0.165f, 0.94f

            if (peacetimeStatus)
            {
                DisablePlayerFiring(ped, true);
                SetPlayerCanDoDriveBy(ped, false);
                DisableControlAction(0, 140, true); // Melee key "r"

                if (IsControlPressed(0, 106))
                    Screen.ShowNotification("~r~Peacetime is enabled. ~n~~s~You can not shoot.");
            }

            // Dead / Revive / Respawn text
            if (IsEntityDead(ped))
            {
                Draw2DText(0.5f, 0.3f, "~r~You are knocked out or dead...", 1.0f, 0);
                Draw2DText(0.5f, 0.4f, "~y~You may use ~g~/revive ~y~if you were knocked out", 1.0f, 0);
                Draw2DText(0.5f, 0.5f, "~y~If you are dead, you must use ~g~/respawn", 1.0f, 0);
            }
        }

        //
        // Methods
        //
        public void RevivePlayer(int eventParam1)
		{
            int ped = GetPlayerPed(-1);
            var pedpos = GetEntityCoords(ped, true);
            if (IsEntityDead(ped))
                NetworkResurrectLocalPlayer(pedpos.X, pedpos.Y, pedpos.Z, 0, true, false);
            SetPlayerInvincible(ped, false);
            ClearPedBloodDamage(ped);
		}

        // Make an announcement on screen - NOT FINISHED YET
        public void Announce([FromSource] string msg)
		{
            timer = 0;
            announcement = true;
            ann = msg;

            // Split up message if more than 70
            if (ann.Length > 70)
            {

			}
		}

        // Draw 2D Text on screen
        public void Draw2DText([FromSource] float x, float y, string text, float scale, int allignment)
        {
            SetTextFont(4);
            SetTextScale(scale, scale);
            SetTextColour(255, 255, 255, 255);
            SetTextDropShadow();
            SetTextOutline();
            if (allignment == 0)
                SetTextJustification(0); // Center
            else if (allignment == 1) 
                SetTextJustification(1); // Left
            else if (allignment == 2)
                SetTextJustification(2); // Right
            SetTextEntry("STRING");
            AddTextComponentString(text);
            DrawText(x, y);
        }  
    }
}
