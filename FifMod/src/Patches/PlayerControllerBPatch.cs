using GameNetcodeStuff;
using HarmonyLib;
using Unity.Netcode;

namespace FifMod.Patches
{
    [HarmonyPatch(typeof(PlayerControllerB))]
    internal class P_PlayerControllerB
    {
        [HarmonyPatch("ConnectClientToPlayerObject")]
        [HarmonyPostfix]
        private static void SendFifmodMessage(ref ulong ___playerClientId)
        {
            if (___playerClientId != NetworkManager.Singleton.LocalClientId) return;
            var body = $"This server is using FifMod v{PluginInfo.PLUGIN_VERSION}. You can submit an idea or bug report here: https://github.com/zSt4rdust/FifMod";
            var fifmodDialogue = new DialogueSegment[]
            {
                new()
                {
                    bodyText = body,
                    waitTime = 5f,
                    speakerText = "Welcome!"
                }
            };

            HUDManager.Instance.ReadDialogue(fifmodDialogue);
        }
    }
}