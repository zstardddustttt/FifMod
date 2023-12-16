using BepInEx;
using BepInEx.Logging;
using FifMod.Patches;
using HarmonyLib;

namespace FifMod
{
    [BepInPlugin("zstardustttt.lethal.fifmod", "FifMod", "1.0.0")]
    [BepInDependency("evaisa.lethallib")]
    [BepInProcess("Lethal Company.exe")]
    [BepInIncompatibility("MoreItems")]
    public class FifMod : BaseUnityPlugin
    {
        public static new ManualLogSource Logger { get; private set; }
        public static FifModAssets Assets { get; private set; }
        public readonly static Harmony harmony = new(PluginInfo.PLUGIN_GUID);

        private void Awake()
        {
            Logger = new("FifMod");
            BepInEx.Logging.Logger.Sources.Add(Logger);

            ConfigManager.BindConfigFile(Config);
            Assets = new("fifmodassets");
            ContentManager.RegisterContent(Assets);

            harmony.PatchAll(typeof(FifModBackendPatches));
            harmony.PatchAll();
            Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
        }
    }
}
