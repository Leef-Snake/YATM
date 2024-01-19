using BepInEx;
using BepInEx.Logging;
using BepInEx.Configuration;
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

        public static Config YatmCfg { get; internal set; }

        private void Awake()
        {
            // Plugin startup logic
            Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");

            YatmCfg = new(base.Config);

            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());

            if (YATM.Config.configEnableTeleport.Value)
            {
                TerminalNode nodeTeleport = CreateTerminalNode("[Teleport]", true, "teleport");
                TerminalKeyword vTeleport = CreateTerminalKeyword("teleport", true, nodeTeleport);
                TerminalKeyword vTeleportShort = CreateTerminalKeyword("tp", true, nodeTeleport);
                AddTerminalKeyword(vTeleport);
                AddTerminalKeyword(vTeleportShort);
            }

            if (YATM.Config.configEnableInverse.Value)
            {
                TerminalNode nodeITP = CreateTerminalNode("[ITP]", true, "inverse");
                TerminalKeyword vInverse = CreateTerminalKeyword("itp", true, nodeITP);
                AddTerminalKeyword(vInverse);
            }

            if (YATM.Config.configEnableStats.Value)
            {
                TerminalNode nodeStats = CreateTerminalNode("[ShowStats]", true, "stats");
                TerminalKeyword vShow = CreateTerminalKeyword("show", true);
                TerminalKeyword nStats = CreateTerminalKeyword("stats", false, nodeStats);
                vShow = vShow.AddCompatibleNoun(nStats, nodeStats);
                nStats.defaultVerb = vShow;
                AddTerminalKeyword(nStats);
                AddTerminalKeyword(vShow);
            }
        }
#if DEBUG
        private void TextParsed(object sender, TerminalParseSentenceEventArgs e)
        {
            Logger.LogMessage($"Text submitted: {e.SubmittedText} Node Returned: {e.ReturnedNode.name}");
            Logger.LogMessage($"Node displayText: {e.ReturnedNode.displayText}");
        }
#endif
    }
}
