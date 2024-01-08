using GameNetcodeStuff;
using HarmonyLib;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using static TerminalApi.TerminalApi;

namespace YATM.Teleport
{
    [HarmonyPatch(typeof(Terminal), "RunTerminalEvents")]
    class EventsPatch
    {
        public static int CheckPlayerName(string name, TerminalNode node)
        {
            List<string> Players = new List<string>();
            for (int i = 0; i < StartOfRound.Instance.mapScreen.radarTargets.Count; i++)
            {
                Players.Add(StartOfRound.Instance.mapScreen.radarTargets[i].name);
            }
            // compare playername with our received name, kind of unnecessary considering we already did that in ParseWordPatch
            for (int j = 0; j < Players.Count; j++)
            {
                //Plugin.mls.LogMessage($"Events Patch: {Players[j]}");
                if (Players[j].ToLower() == name.ToLower())
                {
                    node.displayText = "Teleporting " + Players[j] + "...\n\n";
                    return j;
                }
            }
            node.displayText = "Player " + name + " not found\n";
            return -1;
        }

        static IEnumerator PostfixCoroutine(Terminal __instance, TerminalNode node)
        {
            int result = -1;
            string name = null;
            string[] displayStorage;
            ShipTeleporter[] ShipTeleporterList;
            ShipTeleporter ShipTeleporter;
            if (string.IsNullOrWhiteSpace(node.terminalEvent) || node.terminalEvent != "teleport")
            {
                yield break;
            }

            //Plugin.mls.LogMessage($"Events Patch: {node.displayText}");
            displayStorage = node.displayText.Split("\n");
            if (displayStorage.Length == 3)
            {
                // get playername
                name = displayStorage[1];
                result = CheckPlayerName(name, node);
            }

            // Check if teleporters are even available
            ShipTeleporterList = Object.FindObjectsOfType<ShipTeleporter>();
            if (ShipTeleporterList.Length > 0 && !ShipTeleporterList[0].isInverseTeleporter)
            {
                ShipTeleporter = ShipTeleporterList[0];
            }
            else if (ShipTeleporterList.Length > 1)
            {
                ShipTeleporter = ShipTeleporterList[1];
            }
            else
            {
                node.displayText = "Teleporter unavailble.\n";
                if (result > -1 && name != null)
                {
                    // remove playername noun keyword preemptively and update
                    DeleteKeyword(name);
                    UpdateKeyword(GetKeyword("teleport"));
                    UpdateKeyword(GetKeyword("tp"));
                }
                yield break;
            }

            // check if on cooldown
            if (!ShipTeleporter.buttonTrigger.interactable)
            {
                node.displayText = $"Teleporter on cooldown.\n";
                if (result > -1 && name != null)
                {
                    // remove playername noun keyword preemptively and update
                    DeleteKeyword(name);
                    UpdateKeyword(GetKeyword("teleport"));
                    UpdateKeyword(GetKeyword("tp"));
                }
                yield break;
            }

            // Set up monitor if needed, then tp the player
            // RACE CONDITION: Sync of radar targets seems to always happen AFTER the sync for the tp button
            // this means that tp by playername is broken
            if (displayStorage.Length == 3)
            {
                if (result > -1)
                {
                    int i = 0;
                    StartOfRound.Instance.mapScreen.SwitchRadarTargetAndSync(result);
                    //Plugin.mls.LogMessage($"Events Patch: {StartOfRound.Instance.mapScreen.targetedPlayer.name}");
                    // remove playername noun keyword preemptively and update
                    DeleteKeyword(name);
                    UpdateKeyword(GetKeyword("teleport"));
                    UpdateKeyword(GetKeyword("tp"));
                }
                // else should never happen but also should never break in case of disconnect etc.
            } 
            else
            {
                node.displayText = $"Teleporting player...";
            }
            ShipTeleporter.PressTeleportButtonOnLocalClient();
        }

        static void Postfix(Terminal __instance, TerminalNode node)
        {
            // Start the coroutine
            __instance.StartCoroutine(PostfixCoroutine(__instance, node));
        }
    }
}
