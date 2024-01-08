using HarmonyLib;
using System.Collections;
using System.Collections.Generic;
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
            for (int j = 0; j < Players.Count; j++)
            {
                if (Players[j].ToLower().StartsWith(name.ToLower()))
                {
                    node.displayText = "Teleporting " + Players[j] + "...\n";
                    return j;
                }
            }
            node.displayText = "Player " + name + " not found\n";
            return -1;
        }

        static IEnumerator PostfixCoroutine(Terminal __instance, TerminalNode node)
        {
            int result;
            string name;
            string[] displayStorage;
            if (string.IsNullOrWhiteSpace(node.terminalEvent) || node.terminalEvent != "teleport")
            {
                yield break;
            }
            // Check if teleporters are even available
            ShipTeleporter[] ShipTeleporterList = Object.FindObjectsOfType<ShipTeleporter>();
            ShipTeleporter ShipTeleporter;
            if (ShipTeleporterList.Length > 0)
            {
                if (!ShipTeleporterList[0].isInverseTeleporter)
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
                    yield break;
                }
            } 
            else
            {
                node.displayText = "Teleporter unavailble.\n";
                yield break;
            }

            if (!ShipTeleporter.buttonTrigger.interactable)
            {
                node.displayText = $"Teleporter on cooldown.\n";
                yield break;
            }
            // Set up monitor if needed, then tp the player
            displayStorage = node.displayText.Split("\n");
            if (displayStorage.Length == 2)
            {
                name = displayStorage[1];
                // get playername
                result = CheckPlayerName(name, node);
                if (result > -1)
                {
                    StartOfRound.Instance.mapScreen.SwitchRadarTargetAndSync(result);
                    ShipTeleporter.PressTeleportButtonOnLocalClient();
                    // remove playername noun keyword preemptively and update
                    DeleteKeyword(name);
                    UpdateKeyword(GetKeyword("teleport"));
                    UpdateKeyword(GetKeyword("tp"));
                }
                // else should never happen but also should never break in case of disconnect etc.
            } 
            else
            {
                ShipTeleporter.PressTeleportButtonOnLocalClient();
            }
        }

        static void Postfix(Terminal __instance, TerminalNode node)
        {
            // Start the coroutine
            __instance.StartCoroutine(PostfixCoroutine(__instance, node));
        }
    }
}
