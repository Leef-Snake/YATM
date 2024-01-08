using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using TerminalApi;
using static TerminalApi.TerminalApi;
using static TerminalApi.Events.Events;
using System.Reflection;
using System.Collections.Generic;

namespace YATM
{
    [BepInPlugin(pluginGUID, pluginName, pluginVersion)]
    public class Plugin : BaseUnityPlugin
    {
        private const string pluginGUID = "com.leefsnake.yatm";
        private const string pluginName = "YATM";
        private const string pluginVersion = "1.0.1";

        public static ManualLogSource mls = BepInEx.Logging.Logger.CreateLogSource(pluginGUID);

        private void Awake()
        {
            // Plugin startup logic
            
            //TerminalParsedSentence += TextParsed;

            Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");

            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());

            TerminalNode nodeTeleConfirm = CreateTerminalNode("[Teleport]", true, "teleport");
            TerminalKeyword vTeleport = CreateTerminalKeyword("teleport", true, nodeTeleConfirm);
            TerminalKeyword vTeleportShort = CreateTerminalKeyword("tp", true, nodeTeleConfirm);
            nodeTeleConfirm.isConfirmationNode = true;
            AddTerminalKeyword(vTeleport);
            AddTerminalKeyword(vTeleportShort);
            

            TerminalNode nodeStats = CreateTerminalNode("[ShowStats]", true, "stats");
            TerminalKeyword vShow = CreateTerminalKeyword("show", true);
            TerminalKeyword nStats = CreateTerminalKeyword("stats", false, nodeStats);
            vShow = vShow.AddCompatibleNoun(nStats, nodeStats);
            nStats.defaultVerb = vShow;
            AddTerminalKeyword(nStats);
            AddTerminalKeyword(vShow);
        }
        /*
        private void TextParsed(object sender, TerminalParseSentenceEventArgs e)
        {
            Logger.LogMessage($"Text submitted: {e.SubmittedText} Node Returned: {e.ReturnedNode.name}");
            Logger.LogMessage($"Node displayText: {e.ReturnedNode.displayText}");
        }
        */
    }
}
