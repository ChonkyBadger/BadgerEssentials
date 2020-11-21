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

        uint streetNameHash;
        uint crossingRoadHash;
        Vector2 streetLabelPos;
        float streetLabelScale = 0.6f;
        int streetLabelAllignment = 1;
        string streetLabelHeading;
        string streetLabelStreetName;
        string streetLabelZone;

        Vector2 postalPos;
        float postalScale;
        int postalAllignment;

        bool peacetimeStatus = false;
        string peacetimeText = "~r~disabled";
        Vector2 peacetimePos;
        float peacetimeStatusScale;
        int peacetimeAllignment;

        string priorityCooldownStatus = "none";
        bool priorityCooldownTimerActive;
        int priorityCooldownTimer;
        Vector2 priorityCooldownPos;
        float priorityCooldownScale;
        int priorityCooldownAllignment;

        string currentAOP;
        Vector2 aopPos;
        float aopScale;
        int aopAllignment;

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
            Tick += OnTickPriorityTimer;
            Tick += OnTick250Ms; // Postal + Street Label

            //
            // Parse json config
            //

            // Json Config Objects
            JObject o = JObject.Parse(jsonConfig);

            // Street Label
            streetLabelPos.X = (float)o.SelectToken("displayElements.streetLabel.x");
            streetLabelPos.Y = (float)o.SelectToken("displayElements.streetLabel.y");

            // Pacetime
            peacetimePos.X = (float)o.SelectToken("displayElements.peacetime.x");
            peacetimePos.Y = (float)o.SelectToken("displayElements.peacetime.y");
            peacetimeStatusScale = (float)o.SelectToken("displayElements.peacetime.scale");
            peacetimeAllignment = (int)o.SelectToken("displayElements.peacetime.textAllignment");

            // Priority Cooldown
            priorityCooldownPos.X = (float)o.SelectToken("displayElements.priorityCooldown.x");
            priorityCooldownPos.Y = (float)o.SelectToken("displayElements.priorityCooldown.y");
            priorityCooldownScale = (float)o.SelectToken("displayElements.priorityCooldown.scale");
            priorityCooldownAllignment = (int)o.SelectToken("displayElements.priorityCooldown.textAllignment");

            // Aop
            currentAOP = (string)o.SelectToken("displayElements.aop.defaultAOP");
            aopPos.X = (float)o.SelectToken("displayElements.aop.x");
            aopPos.Y = (float)o.SelectToken("displayElements.aop.y");
            aopScale = (float)o.SelectToken("displayElements.aop.scale");
            aopAllignment = (int)o.SelectToken("displayElements.aop.textAllignment");

            // Postal
            postalPos.X = (float)o.SelectToken("displayElements.postal.x");
            postalPos.Y = (float)o.SelectToken("displayElements.postal.y");
            postalScale = (float)o.SelectToken("displayElements.postal.scale");
            postalAllignment = (int)o.SelectToken("displayElements.postal.textAllignment");

            // Timers
            revDelay = (int)o.SelectToken("commands.revive.cooldown");
            announcementDuration = (int)o.SelectToken("commands.announce.duration");

            // Json Postals Array
            a = Newtonsoft.Json.JsonConvert.DeserializeObject<JArray>(jsonPostals);

            // Put postal code numbers and coordinates into three diff lists.
            foreach (JObject item in a)
			{
                postalCodeValues.Add((int)item.GetValue("code"));
                postalXValues.Add((int)item.GetValue("x"));
                postalYValues.Add((int)item.GetValue("y"));
            }

            //
            // Event Listeners
            //

            EventHandlers["onClientMapStart"] += new Action(OnClientMapStart); // To disable autospawn
            EventHandlers["BadgerEssentials:RevivePlayer"] += new Action<int, bool, bool>(RevivePlayer);
            EventHandlers["BadgerEssentials:Announce"] += new Action<int, string>(Announce);
            EventHandlers["BadgerEssentials:PriorityCooldown"] += new Action<int, string, int>(SetPriorityCooldown);
            EventHandlers["BadgerEssentials:Peacetime"] += new Action(TogglePeacetime);
            EventHandlers["BadgerEssentials:SetAOP"] += new Action<string>(SetAOP);

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
            if (toggleHud)
			{
                // Draw 2D Text
                Draw2DText(streetLabelPos.X, streetLabelPos.Y, "~y~" + streetLabelHeading + " ~s~|~y~ " + streetLabelStreetName, streetLabelScale, streetLabelAllignment);
                Draw2DText(streetLabelPos.X, streetLabelPos.Y + 0.030f, streetLabelZone, streetLabelScale / 1.1f, streetLabelAllignment);

                Draw2DText(postalPos.X, postalPos.Y, "~y~Nearest Postal:~s~ " + nearestPostalCode + " (" + (int)nearestPostalDistance + "m)", postalScale, postalAllignment);
                Draw2DText(peacetimePos.X, peacetimePos.Y, "~y~Peacetime:~s~ " + peacetimeText, peacetimeStatusScale, peacetimeAllignment);
                Draw2DText(priorityCooldownPos.X, priorityCooldownPos.Y, "~y~Priority Cooldown:~s~ " + priorityCooldownStatus, priorityCooldownScale, priorityCooldownAllignment);
                Draw2DText(aopPos.X, aopPos.Y, "~y~AOP:~s~ " + currentAOP, aopScale, aopAllignment);

                if (deadCheck)
                {
                    // Dead / Revive / Respawn text 
                    Draw2DText(0.5f, 0.3f, "~r~You are knocked out or dead...", 1.0f, 0);
                    Draw2DText(0.5f, 0.4f, "~y~You may use ~g~/revive ~y~if you were knocked out", 1.0f, 0);
                    Draw2DText(0.5f, 0.5f, "~y~If you are dead, you must use ~g~/respawn", 1.0f, 0);
                }
            }                
            // Announcement Message
            if (annTimerActive)
                Draw2DText(0.5f, 0.2f, "~r~Announcement! \n ~s~" + annMsg, 1.0f, 0);

            // Peacetime
            if (peacetimeStatus)
            {
                DisablePlayerFiring(ped, true);
                SetPlayerCanDoDriveBy(ped, false);
                DisableControlAction(0, 140, true); // Melee key "r"

                if (IsControlPressed(0, 106))
                    Screen.ShowNotification("~r~Peacetime is enabled. ~n~~s~You can not shoot.");
            }
        }

        // OnTick Revive Timer
        private async Task OnTickRevTimer()
        {
            await Delay(1000);
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
                    if (revTimer > 0)
                        revTimer--; 
                }

            }
            else 
            {
                deadCheck = false;
                revTimerActive = false;
            }
        }

        // OnTick Announcement Timer
        private async Task OnTickAnnTimer()
        {
            await Delay(1000);
            if (!annTimerActive && annTimer > 0)
                annTimerActive = true;
            else if (annTimerActive && annTimer > 0)
			{
                annTimer--;
            }
            else
                annTimerActive = false;
        }

        // OnTick Priority Cooldown Timer
        private async Task OnTickPriorityTimer()
		{
            if (!priorityCooldownTimerActive && priorityCooldownTimer > 0) 
                priorityCooldownTimerActive = true;
            else if (priorityCooldownTimerActive && priorityCooldownTimer > 0)
            {
                await Delay(60000);
                priorityCooldownTimer--;
                priorityCooldownStatus = priorityCooldownTimer + " ~r~minutes";
            }
            else if (priorityCooldownTimerActive && priorityCooldownTimer <= 0)
			{
                priorityCooldownTimerActive = false;
                priorityCooldownStatus = "none";
            }                   
		}

        // OnTick PostalsStreetLabel
        private async Task OnTick250Ms()
        {
            await Delay(250);
            int ped = GetPlayerPed(-1);
            Vector3 pos = GetEntityCoords(ped, true);

            // Postal
            List<float> distanceList = new List<float>();
            foreach (JObject item in a)
                distanceList.Add(GetDistanceBetweenCoords(pos.X, pos.Y, pos.Z, (float)item.GetValue("x"), (float)item.GetValue("y"), 0, false));
            nearestPostalDistance = distanceList.Min();
            postalArrayIndex = distanceList.IndexOf(distanceList.Min());
            postalArrayIndex = Array.IndexOf(distanceList.ToArray(), distanceList.Min());
            nearestPostalCode = postalCodeValues[postalArrayIndex];

            // Street Label
            float rawHeading = GetEntityHeading(ped);

            if (rawHeading <= 337.5 && rawHeading > 292.5)
                streetLabelHeading = "NE";
            else if (rawHeading <= 292.5 && rawHeading > 247.5)
                streetLabelHeading = "E";
            else if (rawHeading <= 247.5 && rawHeading > 202.5)
                streetLabelHeading = "SE";
            else if (rawHeading <= 202.5 && rawHeading > 157.5)
                streetLabelHeading = "S";
            else if (rawHeading <= 157.5 && rawHeading > 112.5)
                streetLabelHeading = "SW";
            else if (rawHeading <= 112.5 && rawHeading > 67.5)
                streetLabelHeading = "W";
            else if (rawHeading <= 67.5 && rawHeading > 22.5)
                streetLabelHeading = "NW";
            else
                streetLabelHeading = "N";

            uint streetNameCrossRoad(int streetNameCrossingRoad)
            {
                GetStreetNameAtCoord(pos.X, pos.Y, pos.Z, ref streetNameHash, ref crossingRoadHash);
                if (streetNameCrossingRoad == 0)
                    return streetNameHash;
                else return crossingRoadHash;
            }
            streetLabelStreetName = GetStreetNameFromHashKey(streetNameCrossRoad(0));
            if (GetStreetNameFromHashKey(streetNameCrossRoad(1)) != string.Empty)
                streetLabelStreetName += " ~s~/~y~ " + GetStreetNameFromHashKey(streetNameCrossRoad(1));
            streetLabelZone = GetLabelText(GetNameOfZone(pos.X, pos.Y, pos.Z));
        }


        //
        // Methods
        //

        // Map Start
        public void OnClientMapStart()
		{
            Exports["spawnmanager"].spawnPlayer();;
            Wait(2500);
            Exports["spawnmanager"].setAutoSpawn(false);
        }

        // Revive Player
        public void RevivePlayer(int eventParam1, bool selfRevive, bool timerBypass)
        {
            int ped = GetPlayerPed(-1);
            Vector3 pedpos = GetEntityCoords(ped, true);
            if (IsEntityDead(ped))
            {
                if (!selfRevive)
                {
                    NetworkResurrectLocalPlayer(pedpos.X, pedpos.Y, pedpos.Z, GetEntityHeading(ped), true, false);
                    ClearPedBloodDamage(ped);
                }
                else if (selfRevive)
                {
                    if (revTimer <= 0 || timerBypass)
					{
                        NetworkResurrectLocalPlayer(pedpos.X, pedpos.Y, pedpos.Z, GetEntityHeading(ped), true, false);
                        ClearPedBloodDamage(ped);
                    }
                    else
                        Screen.ShowNotification("~y~[BadgerEssentials] " + "~r~You cannot revive for " + "~y~" + revTimer + " ~r~more seconds");
                }
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

        // Priority Cooldown
        public void SetPriorityCooldown(int source, string priorityCooldown, int minutes)
		{
            if (priorityCooldown == "pc")
            {
                priorityCooldownTimer = minutes;
                priorityCooldownStatus = minutes + " ~r~minutes";
            }
            else if (priorityCooldown == "inprogress")
                priorityCooldownStatus = "~g~Priority in progress";
            else if (priorityCooldown == "onhold")
                priorityCooldownStatus = "~b~Priorities on hold";
            else if (priorityCooldown == "reset")
                priorityCooldownStatus = "none";
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

        // For pt command
		public void TogglePeacetime()
		{
            Debug.WriteLine("test client");
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
        }

        // For SetAOP command
        public void SetAOP(string targetAOP)
		{
            currentAOP = targetAOP;
        }

    }
}
