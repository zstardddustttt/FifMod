using HarmonyLib;

namespace FifMod.Patches
{
    [HarmonyPatch(typeof(StartOfRound))]
    internal class P_StartOfRound
    {
        [HarmonyPatch(nameof(StartOfRound.Awake))]
        [HarmonyPostfix]
        private static void SetItemShipCapacity(ref int ___maxShipItemCapacity)
        {
            var targetCapacity = ConfigManager.MiscShipCapacity.Value;

            ___maxShipItemCapacity = targetCapacity;
            FifMod.Logger.LogInfo($"Maximum amount of items that can be saved set to {targetCapacity}");
        }
    }
}