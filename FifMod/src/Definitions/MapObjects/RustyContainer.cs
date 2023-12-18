using System;
using GameNetcodeStuff;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

namespace FifMod.Definitions
{
    public class RustyContainerProperties : FifModMapObjectProperties
    {
        public override string PrefabAssetPath => "MapObjects/RustyContainer/RustyContainer.prefab";

        public override Func<SelectableLevel, AnimationCurve> SpawnRateFunction => (level) =>
        {
            return new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 100));
        };

        public override bool SpawnFacingAwayFromWall => true;
        public override MoonFlags Moons => MoonFlags.All;
        public override Type CustomBehaviour => typeof(RustyContainerBehaviour);
    }

    public class RustyContainerBehaviour : NetworkBehaviour
    {
        private InteractTrigger _interactTrigger;
        private Animator _containerAnimator;

        private AudioSource _audioSource;
        private AudioClip _openAudio;

        private void Start()
        {
            _audioSource = GetComponent<AudioSource>();
            _openAudio = FifMod.Assets.GetAsset<AudioClip>("MapObjects/RustyContainer/ContainerOpen.wav");

            _containerAnimator = GetComponentInChildren<Animator>();
            _interactTrigger = GetComponentInChildren<InteractTrigger>();
            _interactTrigger.interactable = true;

            var interactAction = new UnityAction<PlayerControllerB>(OnInteract);
            _interactTrigger.onInteract.AddListener(interactAction);
        }

        private void OnInteract(PlayerControllerB playerInteracted)
        {
            _interactTrigger.interactable = false;
            FifMod.Logger.LogInfo($"{playerInteracted.playerUsername} opened container");

            if (IsServer)
            {
                OnInteractClientRpc();
            }
        }

        [ClientRpc]
        private void OnInteractClientRpc()
        {
            _containerAnimator.SetTrigger("Open");
            _audioSource.PlayOneShot(_openAudio);
        }
    }
}