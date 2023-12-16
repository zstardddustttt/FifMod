using System.Collections;
using GameNetcodeStuff;
using UnityEngine;

namespace FifMod.Utils
{
    public static class FifModUtils
    {
        public static string FormatKey(this string key)
        {
            return key switch
            {
                "leftButton" => "LMB",
                "rightButton" => "RMB",
                "middleButton" => "MMB",
                _ => key.ToUpper(),
            };
        }

        public static float PoundsToItemWeight(int pounds)
        {
            return (float)pounds / 105 + 1;
        }

        public static void CreateExplosion(Vector3 explosionPosition, bool spawnExplosionEffect = false, int damage = 20, float minDamageRange = 5.7f, float maxDamageRange = 6.4f, int enemyHitForce = 6, CauseOfDeath causeOfDeath = CauseOfDeath.Blast, PlayerControllerB attacker = null)
        {
            Transform holder = null;
            if (RoundManager.Instance && RoundManager.Instance.mapPropsContainer && RoundManager.Instance.mapPropsContainer.transform)
            {
                holder = RoundManager.Instance.mapPropsContainer.transform;
            }

            if (spawnExplosionEffect)
            {
                Object.Instantiate(StartOfRound.Instance.explosionPrefab, explosionPosition, Quaternion.Euler(-90f, 0f, 0f), holder).SetActive(value: true);
            }

            var distanceToPlayer = Vector3.Distance(GameNetworkManager.Instance.localPlayerController.transform.position, explosionPosition);
            if (distanceToPlayer < 14f)
                HUDManager.Instance.ShakeCamera(ScreenShakeType.Big);
            else if (distanceToPlayer < 25f)
                HUDManager.Instance.ShakeCamera(ScreenShakeType.Small);

            var objects = Physics.OverlapSphere(explosionPosition, maxDamageRange, 2621448, QueryTriggerInteraction.Collide);
            for (int i = 0; i < objects.Length; i++)
            {
                var distanceToObject = Vector3.Distance(explosionPosition, objects[i].transform.position);
                if (distanceToObject > 4f && Physics.Linecast(explosionPosition, objects[i].transform.position + Vector3.up * 0.3f, 256, QueryTriggerInteraction.Ignore))
                {
                    continue;
                }

                if (objects[i].gameObject.layer == 3)
                {
                    if (objects[i].gameObject.TryGetComponent(out PlayerControllerB player) && player.IsOwner)
                    {
                        if (distanceToObject <= minDamageRange)
                        {
                            Vector3 bodyVelocity = (player.gameplayCamera.transform.position - explosionPosition) * 200f / Vector3.Distance(player.gameplayCamera.transform.position, explosionPosition);
                            player.KillPlayer(bodyVelocity, spawnBody: true, CauseOfDeath.Blast);
                        }
                        else if (distanceToObject > minDamageRange && distanceToObject < maxDamageRange)
                        {
                            var damageMultiplier = 1f - Mathf.Clamp01((distanceToObject - minDamageRange) / (maxDamageRange - minDamageRange));
                            player.DamagePlayer((int)(damage * damageMultiplier), causeOfDeath: causeOfDeath);
                        }
                    }
                }
                else if (objects[i].gameObject.layer == 21)
                {
                    var componentInChildren = objects[i].gameObject.GetComponentInChildren<Landmine>();
                    if (componentInChildren != null && !componentInChildren.hasExploded && distanceToObject < 6f)
                    {
                        componentInChildren.StartCoroutine(componentInChildren.TriggerOtherMineDelayed(componentInChildren));
                    }
                }
                else if (objects[i].gameObject.layer == 19)
                {
                    var componentInChildren2 = objects[i].gameObject.GetComponentInChildren<EnemyAICollisionDetect>();
                    if (componentInChildren2 != null && componentInChildren2.mainScript.IsOwner && distanceToObject < 4.5f)
                    {
                        componentInChildren2.mainScript.HitEnemyOnLocalClient(enemyHitForce, playerWhoHit: attacker);
                    }
                }
            }
        }
    }
}