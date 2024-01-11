using GameNetcodeStuff;
using HarmonyLib;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using static TerminalApi.TerminalApi;

namespace YATM.Inverse
{
    [HarmonyPatch(typeof(Terminal), "RunTerminalEvents")]
    class EventsPatch
    {
        static IEnumerator PostfixCoroutine(Terminal __instance, TerminalNode node)
        {
            ShipTeleporter[] ShipTeleporterList;
            ShipTeleporter TrgetShipTeleporter;

            if (string.IsNullOrWhiteSpace(node.terminalEvent) || node.terminalEvent != "inverse")
            {
                yield break;
            }

            // Check if teleporters are even available

            ShipTeleporterList = Object.FindObjectsOfType<ShipTeleporter>();
            if (ShipTeleporterList.Length > 0 && ShipTeleporterList[0].isInverseTeleporter)
            {
                TrgetShipTeleporter = ShipTeleporterList[0];
            }
            else if (ShipTeleporterList.Length > 1)
            {
                TrgetShipTeleporter = ShipTeleporterList[1];
            }
            else
            {
                node.displayText = "Inverse Teleporter unavailble.\n";
                yield break;
            }
            // Check if on planet to not waste cooldown
            if (StartOfRound.Instance.inShipPhase)
            {
                node.displayText = $"Cannot inverse teleport when not on a planet.\n";
                yield break;
            }
            // Check if on cooldown
            if (!TrgetShipTeleporter.buttonTrigger.interactable)
            {
                node.displayText = $"Inverse Teleporter on cooldown.\n";
                yield break;
            }
            node.displayText = $"Inverse Teleporter activated.\nHave a nice trip and don't get stuck!\n";
            TrgetShipTeleporter.PressTeleportButtonOnLocalClient();
        }

        static void Postfix(Terminal __instance, TerminalNode node)
        {
            // Start the coroutine
            __instance.StartCoroutine(PostfixCoroutine(__instance, node));
        }
    }
}
