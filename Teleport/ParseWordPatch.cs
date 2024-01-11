using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;
using TerminalApi;
using TerminalApi.Classes;
using static TerminalApi.TerminalApi;

namespace YATM.Teleport
{
    [HarmonyPatch(typeof(Terminal), "ParseWord")]
    class ParseWordPatch
    {
        static void Postfix(ref TerminalKeyword __result, string playerWord, int specificityRequired = 2)
        {
            // take the result of ParseWord and modify it if needed
            TerminalKeyword result;
            string playerName;
            if (__result == null)
            {
                // only modify if null and string was a playername
                for (int i = 0; i < StartOfRound.Instance.mapScreen.radarTargets.Count; i++)
                {
                    playerName = StartOfRound.Instance.mapScreen.radarTargets[i].name.ToLower();
                    if (playerName.StartsWith(playerWord.ToLower()))
                    {
                        string sPlayerName = playerName;
                        TerminalNode node = GetKeyword("teleport").specialKeywordResult;
                        // add found playername as compatible noun keyword only to teleport and tp commands
                        node.displayText = "[Teleport]\n" + sPlayerName + "\n";
                        result = GetKeyword(playerName);
                        if (!result)
                        {
#if DEBUG
                            Plugin.mls.LogMessage($"Parse Word Patch name: {playerName}");
                            Plugin.mls.LogMessage($"Parse Word Patch node: {node.displayText}");
                            Plugin.mls.LogMessage($"Parse Word Patch nodeCB: {"[Teleport]\n" + sPlayerName + "\n"}");
#endif
                            result = CreateTerminalKeyword(playerName, false, node);
                            TerminalKeyword vTeleport = GetKeyword("teleport");
                            TerminalKeyword vTp = GetKeyword("tp");
                            vTeleport.AddCompatibleNoun(result, node);
                            vTp.AddCompatibleNoun(result, node);
                            AddTerminalKeyword(result);
                            UpdateKeyword(vTeleport);
                            UpdateKeyword(vTp);
                        }

                        __result = result;
                    }
                }
            }
        }
    }
}
