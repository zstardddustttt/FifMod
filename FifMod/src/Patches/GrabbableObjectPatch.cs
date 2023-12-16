using System.Linq;
using FifMod.Utils;
using HarmonyLib;
using UnityEngine;

namespace FifMod.Patches
{
    [HarmonyPatch(typeof(GrabbableObject))]
    internal class P_GrabbableObject
    {
        [HarmonyPatch("EquipItem")]
        [HarmonyPrefix]
        private static void ApplyTooltips(ref Item ___itemProperties)
        {
            if (!ContentManager.TryGetItemProperties(___itemProperties, out FifModItemProperties properties))
                return;

            if (properties.Tooltips == null) return;
            var remapPanel = Object.FindObjectOfType<KepRemapPanel>(true);

            ___itemProperties.toolTips = new string[properties.Tooltips.Count];
            for (int i = 0; i < properties.Tooltips.Count; i++)
            {
                var tooltip = properties.Tooltips.ElementAt(i);
                var key = remapPanel.remappableKeys.Find((rKey) => rKey.ControlName == tooltip.Key).currentInput.action.controls[0].name;
                ___itemProperties.toolTips[i] = $"{tooltip.Value} : [{key.FormatKey()}]";
            }
        }
    }
}