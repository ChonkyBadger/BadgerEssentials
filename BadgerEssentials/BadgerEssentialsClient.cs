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
		string jsonCustomDisplays = LoadResourceFile(GetCurrentResourceName(), "config/customDisplayElements.json");

		JArray a;
		JArray customDisplays;
		bool enableCustomDisplays;

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
			public string line1;
			public string line2;
			public string line1Raw;
			public string line2Raw;
			public string streetSlashColour;
			public string crossStreetColour;

			public Vector2 pos;
			public Vector2 pos2;
			public float scale;
			public float scale2;
			public int allignment = 1;
			public bool enabled;

			public string heading;
			public string street;
			public string zone;
			public uint streetNameHash;
			public uint crossRoadHash;
		}
		PLD pld = new PLD();

		// Player ID
		public class PlayerID
		{
			public Vector2 pos;
			public float scale;
			public int allignment;
			public bool enabled;
		}
		PlayerID playerID = new PlayerID();

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
			public Vector2 pos;
			public float scale;
			public int allignment;
			public bool enabled;

			public bool status;
			public string text;
		}
		Peacetime pt = new Peacetime();

		// Priority Cooldown
		public class PriorityCooldown
		{
			public Vector2 pos;
			public float scale;
			public int allignment;
			public bool enabled;

			public string status;
		}
		PriorityCooldown pc = new PriorityCooldown();

		// Aop
		public class AOP
		{
			public Vector2 pos;
			public float scale;
			public int allignment;
			public bool enabled;

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
			JObject o = JObject.Parse(jsonConfig);

			// Ragdoll Script
			ragdollKey = (int)o.SelectToken("ragdoll.key");
			ragdollEnabled = (bool)o.SelectToken("ragdoll.enabled");

			//
			// Display Elements
			// 

			enableCustomDisplays = (bool)o.SelectToken("displays.enableCustomDisplayElements");

			// Colours
			colour1 = (string)o.SelectToken("displays.colours.colour1");
			colour2 = (string)o.SelectToken("displays.colours.colour2");

			// Street Label
			pld.line1Raw = (string)o.SelectToken("displays.streetLabel.line1Text");
			pld.line2Raw = (string)o.SelectToken("displays.streetLabel.line2Text");
			string pld1 = (string)o.SelectToken("displays.streetLabel.crossStreetColour");
			pld.crossStreetColour = pld1.Replace("{colour1}", colour1).Replace("{colour2}", colour2);
			string pld2 = (string)o.SelectToken("displays.streetLabel.streetSlashColour");
			pld.streetSlashColour = pld2.Replace("{colour1}", colour1).Replace("{colour2}", colour2);
			pld.pos.X = (float)o.SelectToken("displays.streetLabel.x");
			pld.pos.Y = (float)o.SelectToken("displays.streetLabel.y");
			pld.scale = (float)o.SelectToken("displays.streetLabel.scale");
			pld.pos2.X = (float)o.SelectToken("displays.streetLabel.x2");
			pld.pos2.Y = (float)o.SelectToken("displays.streetLabel.y2");
			pld.scale2 = (float)o.SelectToken("displays.streetLabel.scale2");
			pld.enabled = (bool)o.SelectToken("displays.streetLabel.enabled");

			// Pacetime
			pt.pos.X = (float)o.SelectToken("displays.peacetime.x");
			pt.pos.Y = (float)o.SelectToken("displays.peacetime.y");
			pt.scale = (float)o.SelectToken("displays.peacetime.scale");
			pt.allignment = (int)o.SelectToken("displays.peacetime.textAllignment");
			pt.enabled = (bool)o.SelectToken("displays.peacetime.enabled");

			// Priority Cooldown
			pc.pos.X = (float)o.SelectToken("displays.priorityCooldown.x");
			pc.pos.Y = (float)o.SelectToken("displays.priorityCooldown.y");
			pc.scale = (float)o.SelectToken("displays.priorityCooldown.scale");
			pc.allignment = (int)o.SelectToken("displays.priorityCooldown.textAllignment");
			pc.enabled = (bool)o.SelectToken("displays.priorityCooldown.enabled");

			// Aop
			aop.pos.X = (float)o.SelectToken("displays.aop.x");
			aop.pos.Y = (float)o.SelectToken("displays.aop.y");
			aop.scale = (float)o.SelectToken("displays.aop.scale");
			aop.allignment = (int)o.SelectToken("displays.aop.textAllignment");
			aop.enabled = (bool)o.SelectToken("displays.aop.enabled");

			// Player ID
			playerID.pos.X = (float)o.SelectToken("displays.playerID.x");
			playerID.pos.Y = (float)o.SelectToken("displays.playerID.y");
			playerID.scale = (float)o.SelectToken("displays.playerID.scale");
			playerID.allignment = (int)o.SelectToken("displays.playerID.textAllignment");
			playerID.enabled = (bool)o.SelectToken("displays.playerID.enabled");

			// Postal
			postal.pos.X = (float)o.SelectToken("displays.postal.x");
			postal.pos.Y = (float)o.SelectToken("displays.postal.y");
			postal.scale = (float)o.SelectToken("displays.postal.scale");
			postal.allignment = (int)o.SelectToken("displays.postal.textAllignment");
			postal.enabled = (bool)o.SelectToken("displays.postal.enabled");

			// Timers
			rev.revDelay = (int)o.SelectToken("commands.revive.cooldown");
			ann.duration = (int)o.SelectToken("commands.announce.duration");

			// Json Postals Array
			a = Newtonsoft.Json.JsonConvert.DeserializeObject<JArray>(jsonPostals);
			customDisplays = Newtonsoft.Json.JsonConvert.DeserializeObject<JArray>(jsonCustomDisplays);

			// Put postal code numbers and coordinates into three diff lists.
			foreach (JObject item in a)
			{
				postal.codeValues.Add((int)item.GetValue("code"));
				postal.xValues.Add((int)item.GetValue("x"));
				postal.yValues.Add((int)item.GetValue("y"));
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
			Draw2DText(0.3f, 0.3f, "TEST", 1, 1);
			if (toggleHud)
			{
				//
				// Draw HUD Components
				//

				// Standard elements
				if (pld.enabled)
				{
					Draw2DText(pld.pos.X, pld.pos.Y, pld.line1, pld.scale, pld.allignment);
					Draw2DText(pld.pos2.X, pld.pos2.Y, pld.line2, pld.scale2, pld.allignment);
				}
				if (playerID.enabled)
					Draw2DText(playerID.pos.X, playerID.pos.Y, colour1 + "ID: " + colour2 + GetPlayerServerId(NetworkGetEntityOwner(ped)), playerID.scale, playerID.allignment);
				if (postal.enabled)
					Draw2DText(postal.pos.X, postal.pos.Y, colour1 + "Nearest Postal: " + colour2 + postal.nearestCode + " (" + (int)postal.nearestDistance + "m)", postal.scale, postal.allignment);
				if (pt.enabled)
					Draw2DText(pt.pos.X, pt.pos.Y, colour1 + "Peacetime:~s~ " + pt.text, pt.scale, pt.allignment);
				if (pc.enabled)
					Draw2DText(pc.pos.X, pc.pos.Y, colour1 + "Priority Cooldown: " + colour2 + pc.allignment, pc.scale, pc.allignment);
				if (aop.enabled)
					Draw2DText(aop.pos.X, aop.pos.Y, colour1 + "AOP: " + colour2 + aop.currentAOP, aop.scale, aop.allignment);

				// Custom elements 
				if (enableCustomDisplays)
				{
					foreach (JObject item in customDisplays)
					{
						string text = (string)item.GetValue("text");
						float x = (float)item.GetValue("x");
						float y = (float)item.GetValue("y");
						float scale = (float)item.GetValue("scale");
						int allignment = (int)item.GetValue("textAllignment");
						bool enabled = (bool)item.GetValue("enabled");

						if (enabled)
							Draw2DText(x, y, text, scale, allignment);
					}
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
			foreach (JObject item in a)
				distanceList.Add(GetDistanceBetweenCoords(pos.X, pos.Y, pos.Z, (float)item.GetValue("x"), (float)item.GetValue("y"), 0, false));
			postal.nearestDistance = distanceList.Min();
			postal.arrayIndex = distanceList.IndexOf(distanceList.Min());
			postal.arrayIndex = Array.IndexOf(distanceList.ToArray(), distanceList.Min());
			postal.nearestCode = postal.codeValues[postal.arrayIndex];

			// Street Label
			if (pld.enabled)
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
				string crossStreet = GetStreetNameFromHashKey(pld.crossRoadHash);
				if (!IsStringNullOrEmpty(crossStreet))
					pld.street += $" {pld.streetSlashColour}/ {pld.crossStreetColour}{crossStreet}";

				pld.zone = GetLabelText(GetNameOfZone(pos.X, pos.Y, pos.Z));

				pld.line1 = pld.line1Raw.Replace("{colour1}", colour1).Replace("{colour2}", colour2).Replace("{heading}", pld.heading)
					.Replace("{street}", pld.street).Replace("{zone}", pld.zone);
				pld.line2 = pld.line2Raw.Replace("{colour1}", colour1).Replace("{colour2}", colour2).Replace("{heading}", pld.heading)
					.Replace("{street}", pld.street).Replace("{zone}", pld.zone);
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
