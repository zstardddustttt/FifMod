using System;
using System.Collections.Generic;
using FifMod.Utils;
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
            return new AnimationCurve(new Keyframe(0, 2), new Keyframe(1, 4));
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
        private SpawnableItemWithRarity[] _cheapItems;

        private Vector3 LootPosition => transform.position;

        private void Awake()
        {
            var layermaskRails = LayerMask.GetMask("Railing");
            var rails = Physics.OverlapBox(transform.position, new(2.5f, 1, 2.5f), Quaternion.identity, layermaskRails);
            if (rails.Length > 0)
            {
                FifMod.Logger.LogInfo($"Container {gameObject.name} touches railing, destroying");
                Destroy(gameObject);
                return;
            }

            var layermaskMap = LayerMask.GetMask("Room");
            var leftRaycast = Physics.Raycast(transform.position, -transform.right, 2.25f, layermaskMap);
            if (leftRaycast)
            {
                FifMod.Logger.LogInfo($"Container {gameObject.name} spawned too close to the left, moving");
                transform.Translate(0.5f, 0, 0);
            }

            var rightRaycast = Physics.Raycast(transform.position, transform.right, 2.25f, layermaskMap);
            if (rightRaycast)
            {
                FifMod.Logger.LogInfo($"Container {gameObject.name} spawned too close to the right, moving");
                transform.Translate(-0.5f, 0, 0);
            }

            var backRaycast = Physics.Raycast(transform.position, -transform.forward, 2.25f, layermaskMap);
            if (backRaycast)
            {
                FifMod.Logger.LogInfo($"Container {gameObject.name} spawned too close to the back, moving");
                transform.Translate(0, 0, 0.5f);
            }

            var forwardRaycast = Physics.Raycast(transform.position, transform.forward, 2.25f, layermaskMap);
            if (forwardRaycast)
            {
                FifMod.Logger.LogInfo($"Container {gameObject.name} spawned too close to the forward, moving & rotating");
                transform.Rotate(0, 180, 0);
                transform.Translate(0, 0, -0.5f);
            }
        }

        private void Start()
        {
            _audioSource = GetComponent<AudioSource>();
            _openAudio = FifMod.Assets.GetAsset<AudioClip>("MapObjects/RustyContainer/ContainerOpen.wav");

            _containerAnimator = GetComponentInChildren<Animator>();
            _interactTrigger = GetComponentInChildren<InteractTrigger>();
            _interactTrigger.interactable = true;

            var interactAction = new UnityAction<PlayerControllerB>(OnInteract);
            _interactTrigger.onInteract.AddListener(interactAction);

            _cheapItems = RoundManager.Instance.currentLevel.spawnableScrap.FindAll(scrap => scrap.spawnableItem.maxValue <= 55 / 0.4f && scrap.spawnableItem.itemName != "Gift").ToArray();
        }

        private void OnInteract(PlayerControllerB playerInteracted)
        {
            _interactTrigger.interactable = false;
            _interactTrigger.GetComponent<BoxCollider>().enabled = false;
            FifMod.Logger.LogInfo($"{playerInteracted.playerUsername} opened container");

            if (IsServer)
            {
                var dropLoot = new Item[UnityEngine.Random.Range(1, 4)];
                for (int i = 0; i < dropLoot.Length; i++)
                {
                    dropLoot[i] = _cheapItems[UnityEngine.Random.Range(0, _cheapItems.Length)].spawnableItem;
                }

                var lootLog = "container loot:";
                foreach (var loot in dropLoot)
                {
                    lootLog += $" {loot.itemName},";
                }
                lootLog.Remove(lootLog.Length - 1);
                FifMod.Logger.LogInfo(lootLog);

                var spawnedScrapValues = new List<int>();
                var spawnedScrapNetworkObjects = new List<NetworkObjectReference>();
                foreach (var loot in dropLoot)
                {
                    var (value, networkObject) = FifModUtils.SpawnScrap(loot, LootPosition + Vector3.up, RoundManager.Instance.spawnedScrapContainer);
                    spawnedScrapValues.Add(value);
                    spawnedScrapNetworkObjects.Add(networkObject);
                }

                OnInteractClientRpc(spawnedScrapValues.ToArray(), spawnedScrapNetworkObjects.ToArray());
            }
        }

        [ClientRpc]
        private void OnInteractClientRpc(int[] scrapValues, NetworkObjectReference[] scrapNetworkObjects)
        {
            _containerAnimator.SetTrigger("Open");
            _audioSource.PlayOneShot(_openAudio);

            Debug.Log($"spawning loot! amount: {scrapValues.Length}");
            int lootValue = 0;
            for (int i = 0; i < scrapNetworkObjects.Length; i++)
            {
                if (scrapNetworkObjects[i].TryGet(out var networkObject))
                {
                    var grabbableObject = networkObject.GetComponent<GrabbableObject>();
                    if (grabbableObject != null)
                    {
                        if (i >= scrapValues.Length)
                        {
                            Debug.LogError($"spawnedScrap amount exceeded allScrapValue!: {scrapNetworkObjects.Length}");
                            break;
                        }
                        grabbableObject.SetScrapValue(scrapValues[i]);
                        lootValue += scrapValues[i];
                    }
                    else
                    {
                        Debug.LogError("Scrap networkObject object did not contain grabbable object!: " + networkObject.gameObject.name);
                    }
                }
                else
                {
                    Debug.LogError($"Failed to get networkObject reference for scrap. id: {scrapNetworkObjects[i].NetworkObjectId}");
                }
            }
            RoundManager.Instance.totalScrapValueInLevel += lootValue;
        }
    }
}