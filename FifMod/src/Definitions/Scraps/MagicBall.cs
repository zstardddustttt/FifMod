using System;
using System.Collections;
using LethalLib.Modules;
using Unity.Netcode;
using UnityEngine;

namespace FifMod.Definitions
{
    public class MagicBallProperties : FifModScrapProperties
    {
        public override string ItemAssetPath => "Scraps/MagicBall/MagicBallItem.asset";
        public override int Rarity => ConfigManager.ScrapsMagicBallRarity.Value;
        public override Levels.LevelTypes Moons => Levels.LevelTypes.All;

        public override Type CustomBehaviour => typeof(MagicBallBehaviour);
    }

    public class MagicBallBehaviour : GrabbableObject
    {
        private bool _canShake;
        private Transform _answerObject;
        private float _targetRotation;

        private AudioSource _audioSource;
        private AudioClip _shakeSound;
        private AudioClip _yesSound;
        private AudioClip _noSound;

        public override void Start()
        {
            _audioSource = GetComponent<AudioSource>();
            _shakeSound = FifMod.Assets.GetAsset<AudioClip>("Scraps/MagicBall/MagicBallShake.wav");
            _yesSound = FifMod.Assets.GetAsset<AudioClip>("Scraps/MagicBall/MagicBallYes.wav");
            _noSound = FifMod.Assets.GetAsset<AudioClip>("Scraps/MagicBall/MagicBallNo.wav");

            grabbable = true;
            grabbableToEnemies = true;
            _canShake = true;

            _answerObject = transform.GetChild(0).GetChild(0);
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
            PlayShakeServerRpc();

            MoveRotationServerRpc(90);
            yield return new WaitForSeconds(0.4f);

            var randomChoice = UnityEngine.Random.Range(0, 2);
            SyncRandomServerRpc(randomChoice);

            yield return new WaitForSeconds(0.2f);
            _canShake = true;
        }

        [ServerRpc]
        private void PlayShakeServerRpc()
        {
            PlayShakeClientRpc();
        }

        [ClientRpc]
        private void PlayShakeClientRpc()
        {
            _audioSource.PlayOneShot(_shakeSound);
        }

        [ServerRpc]
        private void SyncRandomServerRpc(int choice)
        {
            SyncRandomClientRpc(choice);
        }

        [ClientRpc]
        private void SyncRandomClientRpc(int choice)
        {
            MoveRotation(choice * 180);
            _audioSource.PlayOneShot(choice == 0 ? _yesSound : _noSound);
        }

        [ServerRpc]
        private void MoveRotationServerRpc(float rotation)
        {
            MoveRotationClientRpc(rotation);
        }

        [ClientRpc]
        private void MoveRotationClientRpc(float rotation)
        {
            MoveRotation(rotation);
        }

        private void MoveRotation(float rotation)
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