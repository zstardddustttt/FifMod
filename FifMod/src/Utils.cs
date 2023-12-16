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

            var objects1 = Physics.OverlapSphere(explosionPosition, maxDamageRange, 2621448, QueryTriggerInteraction.Collide);
            for (int i = 0; i < objects1.Length; i++)
            {
                var distanceToObject = Vector3.Distance(explosionPosition, objects1[i].transform.position);
                if (distanceToObject > 4f && Physics.Linecast(explosionPosition, objects1[i].transform.position + Vector3.up * 0.3f, 256, QueryTriggerInteraction.Ignore))
                {
                    continue;
                }

                if (objects1[i].gameObject.layer == 3)
                {
                    if (objects1[i].gameObject.TryGetComponent(out PlayerControllerB player) && player.IsOwner)
                    {
                        var damageMultiplier = 1f - Mathf.Clamp01((distanceToObject - minDamageRange) / (maxDamageRange - minDamageRange));
                        player.DamagePlayer((int)(damage * damageMultiplier), causeOfDeath: causeOfDeath);
                    }
                }
                else if (objects1[i].gameObject.layer == 21)
                {
                    var componentInChildren = objects1[i].gameObject.GetComponentInChildren<Landmine>();
                    if (componentInChildren != null && !componentInChildren.hasExploded && distanceToObject < 6f)
                    {
                        componentInChildren.StartCoroutine(componentInChildren.TriggerOtherMineDelayed(componentInChildren));
                    }
                }
                else if (objects1[i].gameObject.layer == 19)
                {
                    var componentInChildren2 = objects1[i].gameObject.GetComponentInChildren<EnemyAICollisionDetect>();
                    if (componentInChildren2 != null && componentInChildren2.mainScript.IsOwner && distanceToObject < 4.5f)
                    {
                        componentInChildren2.mainScript.HitEnemyOnLocalClient(enemyHitForce, playerWhoHit: attacker);
                    }
                }
            }

            var objects2 = Physics.OverlapSphere(explosionPosition, 10f, ~LayerMask.GetMask("Colliders"));
            for (int i = 0; i < objects2.Length; i++)
            {
                if (objects2[i].TryGetComponent(out Rigidbody rb))
                {
                    rb.AddExplosionForce(100f, explosionPosition, 10f, 3f, ForceMode.Impulse);
                }
            }
        }
    }
}