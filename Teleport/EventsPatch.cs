using GameNetcodeStuff;
using HarmonyLib;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using static TerminalApi.TerminalApi;

namespace YATM.Teleport
{

    [HarmonyPatch]
    class CameraPatch
    {
        static bool first = true;
        static int index = 0;
        [HarmonyPatch(typeof(ManualCameraRenderer), "updateMapTarget")]
        [HarmonyPostfix]
        static void PostfixMap(ManualCameraRenderer __instance, int setRadarTargetIndex, bool calledFromRPC)
        {
            if (EventsPatch.sDoTp && calledFromRPC == true && setRadarTargetIndex == EventsPatch.result)
            {
                if (!__instance.NetworkManager.IsHost && !__instance.NetworkManager.IsServer)
                {
#if DEBUG
                    Plugin.mls.LogMessage($"Teleporting");
#endif
                    EventsPatch.TrgetShipTeleporter.PressTeleportButtonOnLocalClient();
                    EventsPatch.sDoTp = false;
                }
            }
            index = setRadarTargetIndex;
#if DEBUG
            Plugin.mls.LogMessage($"Left UpdateMapTarget, name: {StartOfRound.Instance.mapScreen.targetedPlayer.name}");
            Plugin.mls.LogMessage($"Left UpdateMapTarget, index: {setRadarTargetIndex}");
            Plugin.mls.LogMessage($"Left UpdateMapTarget, tti: {__instance.targetTransformIndex}");
#endif
        }


#if DEBUG
        [HarmonyPatch(typeof(ManualCameraRenderer), "SwitchRadarTargetClientRpc")]
        [HarmonyPostfix]
        static void PostfixSwitchCRPC(ManualCameraRenderer __instance, int switchToIndex)
        {
            Plugin.mls.LogMessage($"Left client RPC, name: {StartOfRound.Instance.mapScreen.targetedPlayer.name}");
            Plugin.mls.LogMessage($"Left client RPC, index: {switchToIndex}");
            Plugin.mls.LogMessage($"Left client RPC, tti: {__instance.targetTransformIndex}");
        }
#endif

        [HarmonyPatch(typeof(ManualCameraRenderer), "SwitchRadarTargetServerRpc")]
        [HarmonyPostfix]
        static void PostfixSwitchSRPC(ManualCameraRenderer __instance, int targetIndex)
        {
            if (EventsPatch.sDoTp && targetIndex == EventsPatch.result && !first)
            {
                if (__instance.NetworkManager.IsHost || __instance.NetworkManager.IsServer)
                {
#if DEBUG
                    Plugin.mls.LogMessage($"Teleporting");
#endif
                    EventsPatch.TrgetShipTeleporter.PressTeleportButtonOnLocalClient();
                    EventsPatch.sDoTp = false;
                }
            }
            first = !first;
#if DEBUG
            Plugin.mls.LogMessage($"Left Server RPC");
#endif
        }

#if DEBUG
        [HarmonyPatch(typeof(ShipTeleporter), "beamUpPlayer")]
        [HarmonyPrefix]
        static void PrefixBeamUp()
        {
            Plugin.mls.LogMessage($"Entered beamUpPlayer");
            PlayerControllerB mapTarget = StartOfRound.Instance.mapScreen.targetedPlayer;
            PlayerControllerB pttp = StartOfRound.Instance.mapScreen.radarTargets[index].transform.gameObject.GetComponent<PlayerControllerB>();
            Plugin.mls.LogMessage($"beamUpPlayer name: {mapTarget.name}");
            Plugin.mls.LogMessage($"beamUpPlayer pttp: {pttp.name}");
            Plugin.mls.LogMessage($"beamUpPlayer index: {index}");
            Plugin.mls.LogMessage($"beamUpPlayer tti: {StartOfRound.Instance.mapScreen.targetTransformIndex}");
        }
#endif

    }

    [HarmonyPatch(typeof(Terminal), "RunTerminalEvents")]
    class EventsPatch
    {
        static public bool sDoSwitch = false;
        static public bool sDoTp = false;
        static public ShipTeleporter TrgetShipTeleporter;
        static public int result;
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
            result = -1;
            string name = null;
            string[] displayStorage;
            ShipTeleporter[] ShipTeleporterList;
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
                TrgetShipTeleporter = ShipTeleporterList[0];
            }
            else if (ShipTeleporterList.Length > 1)
            {
                TrgetShipTeleporter = ShipTeleporterList[1];
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
            if (!TrgetShipTeleporter.buttonTrigger.interactable)
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
            if (displayStorage.Length == 3)
            {
                if (result > -1)
                {
                    sDoSwitch = true;
#if DEBUG
                    Plugin.mls.LogMessage($"Events Patch: {StartOfRound.Instance.mapScreen.targetedPlayer.name}");
#endif
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
            sDoTp = true;
        }

        static void Postfix(Terminal __instance, TerminalNode node)
        {
            // Start the coroutine
            __instance.StartCoroutine(PostfixCoroutine(__instance, node));
            if (sDoSwitch && sDoTp)
            {
                sDoSwitch = false;
                // when we switch first, we teleport in a postfix patch for updateMapTarget to ensure sync
                StartOfRound.Instance.mapScreen.SwitchRadarTargetAndSync(result); 
            } 
            else if (sDoTp)
            {
                sDoTp = false;
                TrgetShipTeleporter.PressTeleportButtonOnLocalClient();
            }
        }
    }
}
