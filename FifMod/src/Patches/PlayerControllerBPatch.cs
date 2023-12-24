using System.Linq;
using GameNetcodeStuff;
using HarmonyLib;
using Unity.Netcode;
using UnityEngine;

namespace FifMod.Patches
{
    [HarmonyPatch(typeof(PlayerControllerB))]
    internal class P_PlayerControllerB
    {
        private static bool spawned;

        [HarmonyPatch(nameof(PlayerControllerB.ConnectClientToPlayerObject))]
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

        [HarmonyPatch(nameof(PlayerControllerB.Emote2_performed))]
        [HarmonyPrefix]
        private static void SpawnMimic(ref PlayerControllerB __instance)
        {
            if (spawned) return;

            var pos = __instance.serverPlayerPosition;
            var allAINodes = GameObject.FindGameObjectsWithTag("AINode");
            var nodesTempArray = allAINodes.OrderBy((GameObject x) => Vector3.Distance(pos, x.transform.position)).ToArray();
            var result = nodesTempArray[0].transform;

            var enemyIdx = RoundManager.Instance.currentLevel.Enemies.FindIndex(enemy => enemy.enemyType.enemyName == "Container mimic");
            RoundManager.Instance.SpawnEnemyOnServer(result.position, 0, enemyIdx);

            spawned = true;
        }
    }
}