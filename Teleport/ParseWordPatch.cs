using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;
using TerminalApi;
using static TerminalApi.TerminalApi;

namespace YATM.Teleport
{
    [HarmonyPatch(typeof(Terminal), "ParseWord")]
    class ParseWordPatch
    {
        static void Postfix(ref TerminalKeyword __result, string playerWord, int specificityRequired = 2)
        {
            Plugin.mls.LogMessage($"ParseWord Postfix: playerWord = {playerWord}");
            TerminalKeyword result;
            string playerName;
            if (__result == null)
            {
                for (int i = 0; i < StartOfRound.Instance.mapScreen.radarTargets.Count; i++)
                {
                    playerName = StartOfRound.Instance.mapScreen.radarTargets[i].name.ToLower();
                    if (playerName.StartsWith(playerWord.ToLower()))
                    {
                        TerminalNode node = GetKeyword("teleport").specialKeywordResult;
                        node.displayText = "[Teleport]\n" + playerName;

                        result = GetKeyword(playerName);
                        if (!result)
                        {
                            result = CreateTerminalKeyword(playerName, false, node);
                            TerminalKeyword vTeleport = GetKeyword("teleport");
                            TerminalKeyword vTp = GetKeyword("tp");
                            vTeleport.AddCompatibleNoun(result, node);
                            vTp.AddCompatibleNoun(result, node);
                            AddTerminalKeyword(result);
                            UpdateKeyword(vTeleport);
                            UpdateKeyword(vTp);
                        }

                        Plugin.mls.LogMessage($"ParseWord Postfix: Modified result");
                        __result = result;
                    }
                }
            }
        }
    }
}
