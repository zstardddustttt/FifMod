using System;
using System.Collections;
using FifMod.Base;
using FifMod.Utils;
using GameNetcodeStuff;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

namespace FifMod.Definitions
{
    public class RustyMimic : FifModEnemyProperties
    {
        public override string EnemyAssetPath => "Enemies/RustyMimic/RustyMimicEnemy.asset";
        public override string InfoAssetPath => "Enemies/RustyMimic/RustyMimicInfo.asset";

        public override FifModRarity Rarity => FifModRarity.All(100);
        public override EnemySpawnFlags SpawnFlags => EnemySpawnFlags.Facility;
        public override MoonFlags Moons => MoonFlags.All;

        public override Type CustomBehaviour => typeof(RustyMimicBehaviour);
    }

    public class RustyMimicBehaviour : EnemyAI
    {
        private const float HUNT_UPDATE_DELTA = 1f;
        private const float MIN_HUNT_TIME = 60f;
        private const float HUNT_END_DELAY = 10f;
        private const float HUNT_ATTACK_TIME = 5f;
        private const float SEARCH_UPDATE_DELTA = 0.5f;
        private const float MIN_SEARCH_TIME = 3f;

        private const float SEARCH_SPEED = 3.5f;
        private const float HUNT_SPEED = 0f;
        private const float ENRAGED_SPEED = 5.25f;

        private readonly LayerMask _railingMask = LayerMask.GetMask("Railing");

        private MimicState _state;
        private AISearchRoutine _searchForSpot;
        private InteractTrigger _interactTrigger;

        public override void Start()
        {
            creatureSFX = gameObject.GetChild("MainSFX").GetComponent<AudioSource>();
            creatureVoice = gameObject.GetChild("VoiceSFX").GetComponent<AudioSource>();
            creatureAnimator = GetComponentInChildren<Animator>();
            agent = GetComponent<NavMeshAgent>();

            var enemyCollision = GetComponentInChildren<EnemyAICollisionDetect>();
            enemyCollision.mainScript = this;
            eye = transform;
            base.Start();

            _interactTrigger = GetComponentInChildren<InteractTrigger>();
            _interactTrigger.interactable = true;

            var interactAction = new UnityAction<PlayerControllerB>(OnInteract);
            _interactTrigger.onInteract.AddListener(interactAction);

            agent.speed = 0f;
            if (IsServer) StartCoroutine(nameof(CO_EnemyBehaviour));
        }

        private void OnInteract(PlayerControllerB playerInteracted)
        {
            PlayerInteractedServerRpc(Array.IndexOf(StartOfRound.Instance.allPlayerScripts, playerInteracted));
        }

        [ServerRpc(RequireOwnership = false)]
        private void PlayerInteractedServerRpc(int player)
        {
            SetTriggerClientRpc("Attack");
            AttackClientRpc(player);

            StopCoroutine(nameof(CO_DisableInteractionForTime));
            StartCoroutine(nameof(CO_DisableInteractionForTime));
        }

        private IEnumerator CO_DisableInteractionForTime()
        {
            SetInteractableClientRpc(false);
            yield return new WaitForSeconds(5f);
            SetInteractableClientRpc(true);
        }

        [ClientRpc]
        private void AttackClientRpc(int playerIdx)
        {
            var player = StartOfRound.Instance.allPlayerScripts[playerIdx];
            FifMod.Logger.LogInfo($"attacking {player.playerUsername}");

            if (GameNetworkManager.Instance.localPlayerController == player)
            {
                player.KillPlayer(Vector3.zero, spawnBody: true, CauseOfDeath.Mauling);
            }
        }

        [ClientRpc]
        private void SetTriggerClientRpc(string name)
        {
            creatureAnimator.SetTrigger(name);
            FifMod.Logger.LogInfo($"setting animator trigger on client: {name}");
        }

        [ClientRpc]
        private void SetBoolClientRpc(string name, bool value)
        {
            creatureAnimator.SetBool(name, value);
            FifMod.Logger.LogInfo($"setting animator boolean on client: {name}, to: {value}");
        }

        [ClientRpc]
        private void SetInteractableClientRpc(bool value)
        {
            _interactTrigger.interactable = value;
            FifMod.Logger.LogInfo($"setting interactable on client to: {value}");
        }

        private IEnumerator CO_EnemyBehaviour()
        {
            StartSearch(transform.position, _searchForSpot);
            while (!isEnemyDead && !StartOfRound.Instance.allPlayersDead)
            {
                FifMod.Logger.LogInfo($"starting search phase");
                _state = MimicState.Searching;
                agent.speed = SEARCH_SPEED;
                SetBoolClientRpc("Moving", true);

                var searchingTime = 0f;
                while (true)
                {
                    yield return new WaitForSeconds(SEARCH_UPDATE_DELTA);
                    searchingTime += SEARCH_UPDATE_DELTA;
                    if (searchingTime < MIN_SEARCH_TIME) continue;

                    var nearRailing = Physics.OverlapBoxNonAlloc(transform.position, new(4f, 2f, 4f), Array.Empty<Collider>(), Quaternion.identity, _railingMask) > 0;
                    var nearPlayers = CheckForPlayers(20f);

                    FifMod.Logger.LogInfo($"rusty mimic search update | near railing: {nearRailing}, near players: {nearPlayers}, current position: {transform.position}");
                    if (!nearRailing || nearPlayers)
                    {
                        FifMod.Logger.LogInfo($"stopping searching, starting next phase");
                        break;
                    }
                }

                FifMod.Logger.LogInfo($"starting hunt phase");
                _state = MimicState.Hunting;
                agent.speed = HUNT_SPEED;
                SetBoolClientRpc("Moving", false);

                var huntTime = 0f;
                var endHuntTime = MIN_HUNT_TIME;
                var attackTime = 0f;
                var didKillAnyone = false;
                while (true)
                {
                    yield return new WaitForSeconds(HUNT_UPDATE_DELTA);
                    huntTime += HUNT_UPDATE_DELTA;

                    var playersInFront = Physics.OverlapBox(transform.position + transform.forward, Vector3.one * 1f, Quaternion.identity, 8, QueryTriggerInteraction.Collide);
                    if (playersInFront.Length > 0)
                    {
                        FifMod.Logger.LogInfo($"players in front detected, current attack time: {attackTime}");
                        attackTime += HUNT_UPDATE_DELTA;
                    }
                    else attackTime = 0f;

                    if (attackTime >= HUNT_ATTACK_TIME)
                    {
                        SetTriggerClientRpc("Attack");
                        foreach (var player in playersInFront)
                        {
                            AttackClientRpc(Array.IndexOf(StartOfRound.Instance.allPlayerScripts, player));
                        }
                        didKillAnyone = true;
                        attackTime = 0f;
                    }

                    if (huntTime >= endHuntTime)
                    {
                        var nearPlayers = CheckForPlayers(20f);
                        if (nearPlayers)
                        {
                            FifMod.Logger.LogInfo($"players is near, delaying next phase");
                            endHuntTime += HUNT_END_DELAY;
                        }
                        else
                        {
                            FifMod.Logger.LogInfo($"hunt phase ended");
                            break;
                        }
                    }
                }

                if (didKillAnyone) continue;

                FifMod.Logger.LogInfo($"mimic did not kill anyone, he is enraged");
                _state = MimicState.Enraged;
                agent.speed = ENRAGED_SPEED;
            }
        }

        private bool CheckForPlayers(float distance)
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

    public enum MimicState
    {
        Searching,
        Hunting,
        Enraged,
    }
}