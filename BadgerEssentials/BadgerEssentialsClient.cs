using CitizenFX.Core;
using CitizenFX.Core.UI;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static CitizenFX.Core.Native.API;

namespace BadgerEssentials
{
	public class BadgerEssentials : BaseScript
	{
		string jsonConfig = LoadResourceFile(GetCurrentResourceName(), "config/config.json");
		string jsonPostals = LoadResourceFile(GetCurrentResourceName(), "config/postals.json");
		string jsonDisplays = LoadResourceFile(GetCurrentResourceName(), "config/displays.json");

		JArray postals;
		JArray displaysArray;

		public class Display
		{
			public string text;
			public float x;
			public float y;
			public float scale;
			public int allignment;
			public bool enabled;
		}

		List<Display> displays = new List<Display>();

		// Ragdoll Script
		bool isRagdolled = false;
		int ragdollKey;
		bool ragdollEnabled;

		string colour1; // Yellow stuff by default
		string colour2; // White stuff by default

		// Revive
		public class Revive
		{
			public int timer;
			public bool timerActive;
			public int revDelay;
		}
		Revive rev = new Revive();
		bool deadCheck;

		// Announcement
		public class Announcement
		{
			public int timer;
			public bool timerActive;
			public int duration;
			public string msg;
		}
		Announcement ann = new Announcement();

		//
		// Display Elements
		//
		bool toggleHud = true;

		// Street Label
		public class PLD
		{
			public string heading;
			public string street;
			public string crossStreet;
			public string zone;
			public uint streetNameHash;
			public uint crossRoadHash;
		}
		PLD pld = new PLD();

		// Postal
		public class Postal
		{
			public Vector2 pos;
			public float scale;
			public int allignment;
			public bool enabled;

			public float nearestDistance;
			public int nearestCode;
			public int arrayIndex;

			public List<int> codeValues = new List<int>();
			public List<int> xValues = new List<int>();
			public List<int> yValues = new List<int>();
		}
		Postal postal = new Postal();

		// Peacetime
		public class Peacetime
		{
			public bool status;
			public string text;
		}
		Peacetime pt = new Peacetime();

		// Priority Cooldown
		public class PriorityCooldown
		{
			public string status;
		}
		PriorityCooldown pc = new PriorityCooldown();

		// Aop
		public class AOP
		{
			public string currentAOP;
		}
		AOP aop = new AOP();

		public BadgerEssentials()
		{
			Tick += OnTick;
			Tick += OnTickRevTimer; // Revive Timer
			Tick += OnTickAnnTimer; // Announcement Timer
			Tick += OnTick250Ms; // Postal + Street Label
								 //
								 // Parse json config
								 //
			JObject cfg = JObject.Parse(jsonConfig);

			// Ragdoll Script
			ragdollKey = (int)cfg.SelectToken("ragdoll.key");
			ragdollEnabled = (bool)cfg.SelectToken("ragdoll.enabled");

			//
			// Display Elements
			// 

			// Colours
			colour1 = (string)cfg.SelectToken("displayOptions.colour1");
			colour2 = (string)cfg.SelectToken("displayOptions.colour2");		

			// Timers
			rev.revDelay = (int)cfg.SelectToken("commands.revive.cooldown");
			ann.duration = (int)cfg.SelectToken("commands.announce.duration");

			// Json Postals Array
			postals = Newtonsoft.Json.JsonConvert.DeserializeObject<JArray>(jsonPostals);
			displaysArray = Newtonsoft.Json.JsonConvert.DeserializeObject<JArray>(jsonDisplays);

			// Lists
			foreach (JObject item in postals)
			{
				postal.codeValues.Add((int)item.GetValue("code"));
				postal.xValues.Add((int)item.GetValue("x"));
				postal.yValues.Add((int)item.GetValue("y"));
			}

			foreach (JObject display in displaysArray)
			{
				string jText = (string)display.SelectToken("..text");
				float jX = (float)display.SelectToken("..x");
				float jY = (float)display.SelectToken("..y");
				float jScale = (float)display.SelectToken("..scale");
				int jAllignment = (int)display.SelectToken("..textAllignment");
				bool jEnabled = (bool)display.SelectToken("..enabled");

				displays.Add(new Display { text = jText, x = jX, y = jY, scale = jScale, allignment = jAllignment, enabled = jEnabled });
			}

			// Sync up AOP + PT + PC
			TriggerServerEvent("BadgerEssentials:GetAOPFromServer", GetPlayerPed(-1));

			//
			// Event Listeners
			//

			EventHandlers["onClientMapStart"] += new Action(OnClientMapStart); // To disable autospawn
			EventHandlers["BadgerEssentials:RevivePlayer"] += new Action<bool, bool>(RevivePlayer);
			EventHandlers["BadgerEssentials:Announce"] += new Action<string>(Announce);
			EventHandlers["BadgerEssentials:PriorityCooldown"] += new Action<string, int>(SetPriorityCooldown);
			EventHandlers["BadgerEssentials:Peacetime"] += new Action<bool>(TogglePeacetime);
			EventHandlers["BadgerEssentials:SetAOP"] += new Action<string>(SetAOP);
			EventHandlers["BadgerEssentials:SendAOPToClient"] += new Action<string, bool, string, int>(GetAOPFromServer);

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
				Screen.ShowNotification($"{colour1}[BadgerEssentials]\n{colour2}Successfuly Suicided");
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
				int arrayIndex = Array.IndexOf(postal.codeValues.ToArray(), int.Parse(args[0].ToString()));
				float postalXCoord = postal.xValues.ElementAt(arrayIndex);
				float postalYCoord = postal.yValues.ElementAt(arrayIndex);
				SetNewWaypoint(postalXCoord, postalYCoord);

				Screen.ShowNotification($"{colour1}[BadgerEssentials]\n{colour2}Waypoint set to postal {colour1}{args[0]}");
			}), false);

			// Toggle-Hud
			RegisterCommand("toggle-hud", new Action<int, List<object>, string>((source, args, raw) =>
			{
				if (toggleHud)
					toggleHud = false;
				else toggleHud = true;
			}), false);

			//
			// Chat Suggestions
			//

			// toggle-hud command suggestions
			TriggerEvent("chat:addSuggestion", "/toggle-hud", "Toggles the hud");

			// die command suggestions
			TriggerEvent("chat:addSuggestion", "/die", "Kills yourself");

			// revive command suggestions
			TriggerEvent("chat:addSuggestion", "/revive", "Revive yourself or other players", new[]
			{
				new { name = "PlayerID", help = "(Optional) ID of the player you want to revive" }
			});

			// pt command suggestions
			TriggerEvent("chat:addSuggestion", "/pt", "Toggles peacetime");

			// pc command suggestions
			TriggerEvent("chat:addSuggestion", "/pc", "Sets priority cooldown for specified number of minutes", new[]
			{
				new { name = "Duration", help = "Duration of priority cooldown in minutes" }
			});

			// pc-inprogress command suggestions
			TriggerEvent("chat:addSuggestion", "/pc-inprogress", "Sets priority cooldown status to \"Priority in progress\"");

			// pc-onhold command suggestions
			TriggerEvent("chat:addSuggestion", "/pc-onhold", "Sets priority cooldown status to \"Priority on hold\"");

			// pc-reset command suggestions
			TriggerEvent("chat:addSuggestion", "/pc-reset", "Resets priority cooldown status");

			// aop command suggestions
			TriggerEvent("chat:addSuggestion", "/setaop", "Sets the aop", new[]
			{
				new { name = "AOP", help = "What the new AOP should be" }
			});

			// announce command suggestions
			TriggerEvent("chat:addSuggestion", "/announce", "Makes an announcement on screen, visible to all players", new[]
			{
				new { name = "message", help = "Announcement message" }
			});
		}

		//
		// onTick methods;
		//

		private async Task OnTick()
		{
			int ped = GetPlayerPed(-1);
			int id = GetPlayerServerId(NetworkGetEntityOwner(ped));

			if (toggleHud)
			{
				// Display Elements
				foreach (Display disp in displays)
				{
					string crossStreetSlash;
					if (!IsStringNullOrEmpty(pld.crossStreet))
						crossStreetSlash = "/";
					else crossStreetSlash = String.Empty;
					string text = disp.text
						.Replace("{colour1}", colour1).Replace("{colour2}", colour2).Replace("{aop}", aop.currentAOP)
						.Replace("{pcStatus}", pc.status).Replace("{ptStatus}", pt.text).Replace("{nearestPostal}", postal.nearestCode.ToString())
						.Replace("{nearestPostalDistance}", postal.nearestDistance.ToString()).Replace("{playerID}", id.ToString())
						.Replace("{zone}", pld.zone).Replace("{heading}", pld.heading).Replace("{street}", pld.street)
						.Replace("{crossStreetSlash}", crossStreetSlash).Replace("{crossStreet}", pld.crossStreet);

					if (disp.enabled)
						Draw2DText(disp.x, disp.y, text, disp.scale, disp.allignment);
				}

				if (deadCheck)
				{
					// Dead / Revive / Respawn text 
					Draw2DText(0.5f, 0.3f, "~r~You are knocked out or dead...", 1.0f, 0);
					Draw2DText(0.5f, 0.4f, colour1 + "You may use ~g~/revive " + colour1 + "if you were knocked out", 1.0f, 0);
					Draw2DText(0.5f, 0.5f, colour1 + "If you are dead, you must use ~g~/respawn", 1.0f, 0);
				}
			}
			// Announcement Message
			if (ann.timerActive)
				Draw2DText(0.5f, 0.2f, "~r~Announcement! \n ~s~" + ann.msg, 1.0f, 0);

			// Peacetime
			if (pt.status)
			{
				DisablePlayerFiring(ped, true);
				SetPlayerCanDoDriveBy(ped, false);
				DisableControlAction(0, 140, true); // Melee key "r"

				if (IsControlPressed(0, 106))
					Screen.ShowNotification(colour1 + "[BadgerEssentials] \n" + "~r~Peacetime is enabled! " + colour2 + "You can not shoot.");
			}

			// Ragdoll Script
			// Check if "U" (303) is pressed
			if (IsControlJustPressed(1, ragdollKey) && ragdollEnabled)
			{
				if (!isRagdolled)
				{
					isRagdolled = true;
				}
				else
				{
					isRagdolled = false;
				}
			}

			// Ragdoll the player
			if (isRagdolled)
			{
				SetPedToRagdoll(GetPlayerPed(-1), 750, 750, 0, true, true, false);
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
				if (deadCheck && !rev.timerActive)
				{
					rev.timer = rev.revDelay;
					rev.timerActive = true;
				}
				if (rev.timerActive && rev.timer > 0)
				{
					if (rev.timer > 0)
						rev.timer--;
				}

			}
			else
			{
				deadCheck = false;
				rev.timerActive = false;
			}
		}

		// OnTick Announcement Timer
		private async Task OnTickAnnTimer()
		{
			await Delay(1000);
			if (!ann.timerActive && ann.timer > 0)
				ann.timerActive = true;
			else if (ann.timerActive && ann.timer > 0)
			{
				ann.timer--;
			}
			else
				ann.timerActive = false;
		}

		// OnTick PostalsStreetLabel
		private async Task OnTick250Ms()
		{
			await Delay(250);
			int ped = GetPlayerPed(-1);
			Vector3 pos = GetEntityCoords(ped, true);

			// Postal
			List<float> distanceList = new List<float>();
			foreach (JObject item in postals)
				distanceList.Add(GetDistanceBetweenCoords(pos.X, pos.Y, pos.Z, (float)item.GetValue("x"), (float)item.GetValue("y"), 0, false));
			postal.nearestDistance = distanceList.Min();
			postal.arrayIndex = distanceList.IndexOf(distanceList.Min());
			postal.arrayIndex = Array.IndexOf(distanceList.ToArray(), distanceList.Min());
			postal.nearestCode = postal.codeValues[postal.arrayIndex];

			// Street Label
			if (true)
			{
				float rawHeading = GetEntityHeading(ped);

				if (rawHeading <= 337.5 && rawHeading > 292.5)
					pld.heading = "NE";
				else if (rawHeading <= 292.5 && rawHeading > 247.5)
					pld.heading = "E";
				else if (rawHeading <= 247.5 && rawHeading > 202.5)
					pld.heading = "SE";
				else if (rawHeading <= 202.5 && rawHeading > 157.5)
					pld.heading = "S";
				else if (rawHeading <= 157.5 && rawHeading > 112.5)
					pld.heading = "SW";
				else if (rawHeading <= 112.5 && rawHeading > 67.5)
					pld.heading = "W";
				else if (rawHeading <= 67.5 && rawHeading > 22.5)
					pld.heading = "NW";
				else
					pld.heading = "N";

				GetStreetNameAtCoord(pos.X, pos.Y, pos.Z, ref pld.streetNameHash, ref pld.crossRoadHash);
				pld.street = GetStreetNameFromHashKey(pld.streetNameHash);
				pld.crossStreet = GetStreetNameFromHashKey(pld.crossRoadHash);

				pld.zone = GetLabelText(GetNameOfZone(pos.X, pos.Y, pos.Z));
			}
		}


		//
		// Methods
		//

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


		// Map Start
		public void OnClientMapStart()
		{
			Exports["spawnmanager"].spawnPlayer();
			Wait(2500);
			Exports["spawnmanager"].setAutoSpawn(false);
		}

		// Revive Player Command
		public void RevivePlayer(bool selfRevive, bool timerBypass)
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
					if (rev.timer <= 0 || timerBypass)
					{
						NetworkResurrectLocalPlayer(pedpos.X, pedpos.Y, pedpos.Z, GetEntityHeading(ped), true, false);
						ClearPedBloodDamage(ped);
					}
					else
						Screen.ShowNotification(colour1 + "[BadgerEssentials] \n" + colour2 + $"You cannot revive for " + colour1 + rev.timer + colour2 + " more seconds");
				}
			}
		}

		// Make an announcement on screen
		public void Announce(string announcementMsg)
		{
			ann.msg = String.Empty;
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
						ann.msg += "\n";
						lineLength = 0;
					}
					ann.msg += word + " ";
				}
			}
			else ann.msg = announcementMsg;
			ann.timer = ann.duration;
		}

		// Priority Cooldown
		public void SetPriorityCooldown(string priorityCooldown, int minutes)
		{
			if (priorityCooldown == "pc")
				pc.status = minutes + " ~r~minutes";
			else if (priorityCooldown == "inprogress")
				pc.status = "~g~Priority in progress";
			else if (priorityCooldown == "onhold")
				pc.status = "~b~Priorities on hold";
			else if (priorityCooldown == "reset" || priorityCooldown == "none")
				pc.status = "none";
		}

		// For pt command
		public void TogglePeacetime(bool peacetime)
		{
			if (peacetime)
			{
				pt.status = true;
				pt.text = "~g~enabled";
			}
			else
			{
				pt.status = false;
				pt.text = "~r~disabled";
			}
		}

		// For SetAOP command
		public void SetAOP(string newAOP)
		{
			aop.currentAOP = newAOP;
		}

		// Activates once when client joins to sync aop
		public void GetAOPFromServer(string newAOP, bool peacetime, string priority, int priorityTime)
		{
			aop.currentAOP = newAOP;
			SetPriorityCooldown(priority, priorityTime);
			TogglePeacetime(peacetime);
		}
	}
}
