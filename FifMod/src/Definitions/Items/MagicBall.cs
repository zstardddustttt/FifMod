using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

namespace FifMod.Definitions
{
    public class MagicBallProperties : FifModItemProperties
    {
        public override int Price => 30;
        public override string ItemAssetPath => "Items/MagicBall/MagicBallItem.asset";
        public override string InfoAssetPath => "Items/MagicBall/MagicBallInfo.asset";

        public override Type CustomBehaviour => typeof(MagicBallBehaviour);
    }

    public class MagicBallBehaviour : GrabbableObject
    {
        private bool _canShake;
        private Transform _answerObject;
        private float _targetRotation;

        public override void Start()
        {
            grabbable = true;
            grabbableToEnemies = true;
            _canShake = true;

            _answerObject = transform.GetChild(0);
            _answerObject.transform.localRotation = Quaternion.Euler(0, 90, 0);
            _targetRotation = 90;

            base.Start();
        }

        public override void ItemActivate(bool used, bool buttonDown = true)
        {
            base.ItemActivate(used, buttonDown);

            if (!playerHeldBy) return;
            if (_canShake) StartCoroutine(nameof(CO_ShakeBall));
        }

        private IEnumerator CO_ShakeBall()
        {
            _canShake = false;
            playerHeldBy.playerBodyAnimator.SetTrigger("shakeItem");

            MoveRotationServerRpc(90);
            yield return new WaitForSeconds(0.4f);

            var randomChoice = UnityEngine.Random.Range(0, 2);
            MoveRotationServerRpc(randomChoice * 180);

            yield return new WaitForSeconds(0.2f);
            _canShake = true;
        }

        [ServerRpc]
        private void MoveRotationServerRpc(float rotation)
        {
            MoveRotationClientRpc(rotation);
        }

        [ClientRpc]
        private void MoveRotationClientRpc(float rotation)
        {
            _targetRotation = rotation;
        }

        public override void Update()
        {
            base.Update();
            _answerObject.localRotation = Quaternion.Slerp(_answerObject.localRotation, Quaternion.Euler(0, _targetRotation, 0), Time.deltaTime * 5);
        }
    }
}