using BepInEx.Configuration;

namespace FifMod
{
    public static class ConfigManager
    {
        public static ConfigEntry<int> ConfigSledgePrice { get; private set; }

        public static void BindConfigFile(ConfigFile config)
        {
            ConfigSledgePrice = config.Bind("Items", "SledgePrice", 110);
        }
    }
}