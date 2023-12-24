using System.Collections;
using UnityEngine;

namespace FifMod.Base
{
    public abstract class FifModEnemy : EnemyAI
    {
        public override void Start()
        {
            base.Start();
            if (IsServer) StartCoroutine(nameof(CO_EnemyBehaviour));
        }

        protected abstract IEnumerator CO_EnemyBehaviour();

        public bool CheckForPlayers(float distance)
        {
            foreach (var player in StartOfRound.Instance.allPlayerScripts)
            {
                if (!PlayerIsTargetable(player)) continue;

                var distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);
                if (distanceToPlayer < distance)
                {
                    return true;
                }
            }

            return false;
        }
    }
}