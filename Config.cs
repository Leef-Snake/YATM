using BepInEx.Configuration;

namespace YATM
{
    public class Config
    {
        public static ConfigEntry<bool> configEnableTeleport;
        public static ConfigEntry<bool> configEnableTeleportByName;
        public static ConfigEntry<bool> configEnableInverse;
        public static ConfigEntry<bool> configEnableStats;

        public Config(ConfigFile cfg)
        {
            configEnableTeleport = cfg.Bind(
                    "General.Toggles",                  // Config section
                    "EnableTeleport",                   // Key of this config
                    true,                               // Default value
                    "Enable Teleport command"           // Description
            );
            // experimental, disabled by default
            configEnableTeleportByName = cfg.Bind(
                    "General.Toggles",
                    "EnableTeleportByName",
                    false,
                    "EXPERIMENTAL: Enable ability to teleport specific player by their name"
            );

            configEnableInverse = cfg.Bind(
                    "General.Toggles",
                    "EnableInverse",
                    true,
                    "Enable ITP command"
            );

            configEnableStats = cfg.Bind(
                    "General.Toggles",
                    "EnableStats",
                    true,
                    "Enable Stats command"
            );

        }
    }
}
