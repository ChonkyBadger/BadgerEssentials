using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using CitizenFX.Core;
using static CitizenFX.Core.Native.API;
using CitizenFX.Core.UI;
using System.Linq;

namespace BadgerEssentials
{
    public class BadgerEssentials : BaseScript
    {
        string jsonConfig = LoadResourceFile("BadgerEssentials", "config/config.json");
        string jsonPostals = LoadResourceFile("BadgerEssentials", "config/postals.json");

        JArray a;

        // Still to be used
        bool announcement = false;
        string announcementHeader; // Json Config
        int displayTime; // Json Config

        // Revive
        bool deadCheck;
        int revTimer;
        bool revTimerActive = false;
        int revDelay;

        // Announcement
        int annTimer;
        bool annTimerActive;
        int announcementDuration;
        string annMsg;

        // Display Elements
        bool toggleHud = true;

        bool peacetimeStatus = false;
        string peacetimeText = "~r~disabled";
        float peacetimeStatusX;
        float peacetimeStatusY;
        float peacetimeStatusScale;

        string currentAOP;
        float aopX;
        float aopY;
        float aopScale;
        float postalX;
        float postalY;
        float postalScale;

        // Postal Stuff
        float nearestPostalDistance;
        int nearestPostalCode;
        int postalArrayIndex;
        List<int> postalCodeValues = new List<int>();
        List<int> postalXValues = new List<int>();
        List<int> postalYValues = new List<int>();

        public BadgerEssentials()
        {
            Tick += OnTickDraw2DText;
            Tick += OnTickRevTimer;
            Tick += OnTickAnnTimer;
            Tick += OnTickPostals;

            //
            // Parse json config
            //

            // Json Config Objects
            JObject o = JObject.Parse(jsonConfig);

            peacetimeStatusX = (float)o.SelectToken("displayElements.peacetime.x");
            peacetimeStatusY = (float)o.SelectToken("displayElements.peacetime.y");
            peacetimeStatusScale = (float)o.SelectToken("displayElements.peacetime.scale");
            currentAOP = (string)o.SelectToken("displayElements.aop.defaultAOP");
            aopX = (float)o.SelectToken("displayElements.aop.x");
            aopY = (float)o.SelectToken("displayElements.aop.y");
            aopScale = (float)o.SelectToken("displayElements.aop.scale");
            postalX = (float)o.SelectToken("displayElements.postal.x");
            postalY = (float)o.SelectToken("displayElements.postal.y");
            postalScale = (float)o.SelectToken("displayElements.postal.scale");
            revDelay = (int)o.SelectToken("commands.revive.cooldown");
            announcementDuration = (int)o.SelectToken("commands.announce.duration");

            // Json Postals Array
            a = Newtonsoft.Json.JsonConvert.DeserializeObject<JArray>(jsonPostals);

            // Put postal code numbers into a list
            foreach (JObject item in a)
			{
                postalCodeValues.Add((int)item.GetValue("code"));
                postalXValues.Add((int)item.GetValue("x"));
                postalYValues.Add((int)item.GetValue("y"));
            }

            //
            // Event Listeners
            //

            EventHandlers["onClientMapStart"] += new Action(OnClientMapStart);
            EventHandlers["BadgerEssentials:RevivePlayer"] += new Action<int, bool>(RevivePlayer);
            EventHandlers["BadgerEssentials:Announce"] += new Action<int, string>(Announce);

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

            // Respawn Command
            RegisterCommand("respawn", new Action<int, List<object>, string>((source, args, raw) =>
            {
                int ped = GetPlayerPed(-1);
                Vector3 pedpos = GetEntityCoords(ped, true);

                if (IsEntityDead(ped))
				{
                    NetworkResurrectLocalPlayer(pedpos.X, pedpos.Y, pedpos.Z, 0, true, false);
                    ClearPedBloodDamage(ped);
                    SetEntityCoords(ped, 1828.43f, 3693.01f, 34.22f, false, false, false, false);
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

            // Nearest Postal Command
            RegisterCommand("postal", new Action<int, List<object>, string>((source, args, raw) =>
            {
                int arrayIndex = Array.IndexOf(postalCodeValues.ToArray(), int.Parse(args[0].ToString()));
                float postalXCoord = postalXValues.ElementAt(arrayIndex);
                float postalYCoord = postalYValues.ElementAt(arrayIndex);
                SetNewWaypoint(postalXCoord, postalYCoord);

                Screen.ShowNotification("~y~Waypoint set to postal~s~ " + args[0]);
            }), false);

            // Toggle-Hud
            RegisterCommand("toggle-hud", new Action<int, List<object>, string>((source, args, raw) =>
            {
                if (toggleHud)
                    toggleHud = false;
                else toggleHud = true;
            }), false);
        }

        //
        // onTick methods;
        //

        private async Task OnTickDraw2DText()
        {
            int ped = GetPlayerPed(-1);

            // Announcement Message
            if (annTimerActive)
            {
                Draw2DText(0.5f, 0.2f, "~r~Announcement!", 1.0f, 0);
                Draw2DText(0.5f, 0.25f, annMsg, 1.0f, 0);
            }

            // Draw 2D Text
            if (toggleHud)
			{
                Draw2DText(aopX, aopY, "~y~AOP~s~: " + currentAOP, aopScale, 1);
                Draw2DText(peacetimeStatusX, peacetimeStatusY, "~y~Peacetime:~s~ " + peacetimeText, peacetimeStatusScale, 1);
                Draw2DText(postalX, postalY, "~y~Nearest Postal:~s~ " + nearestPostalCode + " (" + (int)nearestPostalDistance + "m)", postalScale, 1);
            }

            // Peacetime
            if (peacetimeStatus)
            {
                DisablePlayerFiring(ped, true);
                SetPlayerCanDoDriveBy(ped, false);
                DisableControlAction(0, 140, true); // Melee key "r"

                if (IsControlPressed(0, 106))
                    Screen.ShowNotification("~r~Peacetime is enabled. ~n~~s~You can not shoot.");
            }

            if (IsEntityDead(ped) && toggleHud)
            {
                // Dead / Revive / Respawn text 
                Draw2DText(0.5f, 0.3f, "~r~You are knocked out or dead...", 1.0f, 0);
                Draw2DText(0.5f, 0.4f, "~y~You may use ~g~/revive ~y~if you were knocked out", 1.0f, 0);
                Draw2DText(0.5f, 0.5f, "~y~If you are dead, you must use ~g~/respawn", 1.0f, 0);

            }
        }

        // OnTick Revive Timer
        private async Task OnTickRevTimer()
        {
            int ped = GetPlayerPed(-1);

            if (IsEntityDead(ped))
            {
                // Dead Check
                deadCheck = true;
                if (deadCheck && !revTimerActive)
                {
                    revTimer = revDelay;
                    revTimerActive = true;
                }
                if (revTimerActive && revTimer > 0)
                {
                    await Delay(1000);
                    if (revTimer > 0)
                        revTimer--;
                }

            }
            else if (!IsEntityDead(ped) && deadCheck)
            {
                deadCheck = false;
                revTimerActive = false;
            }
        }

        // OnTick Announcement Timer
        private async Task OnTickAnnTimer()
        {
            if (!annTimerActive && annTimer > 0)
                annTimerActive = true;
            else if (annTimerActive && annTimer > 0)
			{
                await Delay(1000);
                annTimer--;
            }
            else
                annTimerActive = false;
        }

        // OnTick Postals
        private async Task OnTickPostals()
        {
            await Delay(250);

            int ped = GetPlayerPed(-1);
            Vector3 pos = GetEntityCoords(ped, true);

            List<float> distanceList = new List<float>();
            foreach (JObject item in a)
                distanceList.Add(GetDistanceBetweenCoords(pos.X, pos.Y, pos.Z, (float)item.GetValue("x"), (float)item.GetValue("y"), 0, false));
            nearestPostalDistance = distanceList.Min();
            postalArrayIndex = Array.IndexOf(distanceList.ToArray(), distanceList.Min());
            nearestPostalCode = postalCodeValues[postalArrayIndex];
        }


        //
        // Methods
        //

        // Map Start
        public void OnClientMapStart()
		{
            Exports["spawnmanager"].spawnPlayer();
            Debug.WriteLine("Spawn player");
            Wait(2500);
            Exports["spawnmanager"].setAutoSpawn(false);
            Debug.WriteLine("Disabled Auto Spawn");
        }

        // Revive Player
        public void RevivePlayer(int eventParam1, bool selfRevive)
        {
            int ped = GetPlayerPed(-1);
            Vector3 pedpos = GetEntityCoords(ped, true);
            if (IsEntityDead(ped))
            {
                if (!selfRevive)
                {
                    NetworkResurrectLocalPlayer(pedpos.X, pedpos.Y, pedpos.Z, 0, true, false);
                    ClearPedBloodDamage(ped);
                }
                else if (selfRevive && revTimer <= 0)
                {
                    NetworkResurrectLocalPlayer(pedpos.X, pedpos.Y, pedpos.Z, 0, true, false);
                    ClearPedBloodDamage(ped);
                }
                else
                    Screen.ShowNotification("~y~[BadgerEssentials] " + "~r~You cannot revive for " + "~y~" + revTimer + " ~r~more seconds");
            }
        }

        // Make an announcement on screen
        public void Announce(int source, string announcementMsg)
        {
            annMsg = String.Empty;
            // Split up message if more than 70 characters long
            if (announcementMsg.Length > 70)
            {
                string[] words = announcementMsg.Split();
                int index = -1;
                int lineLength = 0;
                foreach (string i in words)
                {
                    index++;
                    string word = words[index];
                    lineLength += word.Length;

                    if (lineLength > 70)
                    {
                        annMsg += "\n";
                        lineLength = 0;
                    }
                    annMsg += word + " ";
                }
            }
            else annMsg = announcementMsg;
            annTimer = announcementDuration;
        }

        // Draw 2D Text on screen
        public void Draw2DText(float x, float y, string text, float scale, int allignment)
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
