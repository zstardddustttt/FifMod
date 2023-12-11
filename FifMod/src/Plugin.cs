using BepInEx;
using BepInEx.Logging;

namespace FifMod
{
    [BepInPlugin("zstardustttt.lethal.fifmod", "FifMod", "1.0.0")]
    [BepInDependency("evaisa.lethallib")]
    [BepInProcess("Lethal Company.exe")]
    public class FifMod : BaseUnityPlugin
    {
        public static new ManualLogSource Logger { get; private set; }
        public static FifModAssets Assets { get; private set; }

        private void Awake()
        {
            Logger = new("FifMod");
            BepInEx.Logging.Logger.Sources.Add(Logger);

            ConfigManager.BindConfigFile(Config);
            Assets = new("fifmodassets");
            ContentManager.RegisterContent(Assets);

            Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
        }
    }
}
