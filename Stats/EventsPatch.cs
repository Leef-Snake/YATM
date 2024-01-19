using HarmonyLib;
using System.Collections;

namespace YATM
{
    [HarmonyPatch(typeof(Terminal), "RunTerminalEvents")]
    class EventsPatch
    {
        public static string BuildStats()
        {
            string newDisplayText;
            EndOfGameStats eogStats = StartOfRound.Instance.gameStats;
            if (eogStats.daysSpent == 0 && eogStats.allStepsTaken == 0)
            {
                newDisplayText = "No stats to display yet\n";
            } 
            else
            {
                newDisplayText =
                    "Days passed: " + eogStats.daysSpent +
                    "\nDeaths died: " + eogStats.deaths +
                    "\nTotal scrap value collected: " + eogStats.scrapValueCollected +
                    "\nSteps Taken: " + eogStats.allStepsTaken + "\n";
            }
            return newDisplayText;
            }

        static IEnumerator PostfixCoroutine(Terminal __instance, TerminalNode node)
        {
            
            if (string.IsNullOrWhiteSpace(node.terminalEvent) || node.terminalEvent != "stats")
            {
                yield break;
            }
            
            // Call output constructor
            node.displayText = BuildStats();
        }

        static void Postfix(Terminal __instance, TerminalNode node)
        {
            // Start the coroutine
            __instance.StartCoroutine(PostfixCoroutine(__instance, node));
        }
    }
}
