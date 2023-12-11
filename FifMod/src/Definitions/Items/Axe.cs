using System;
using System.Collections;
using GameNetcodeStuff;
using Unity.Netcode;
using UnityEngine;

namespace FifMod.Definitions
{
    public class AxeProperties : FifModItemProperties
    {
        public override int Price => ConfigManager.ConfigSledgePrice.Value;
        public override string ItemAssetPath => "Items/Axe/AxeItem.asset";
        public override string InfoAssetPath => "Items/Axe/AxeInfo.asset";

        public override Type CustomBehaviour => typeof(AxeBehaviour);
    }

    public class AxeBehaviour : GrabbableObject
    {
        private AudioSource _axeSource;
        private bool _isHolding;
        private bool _isReelingUp;
        private PlayerControllerB _previousPlayerHeldBy;
        private RoundManager _roundManager;

        private AudioClip _reelUpAudio;
        private AudioClip _swingAudio;
        private AudioClip[] _hitAudio;

        private const int HIT_MASK = 11012424;
        private const int HIT_FORCE = 3;

        public override void Start()
        {
            _axeSource = GetComponent<AudioSource>();
            _reelUpAudio = FifMod.Assets.GetAsset<AudioClip>("Items/Axe/AxeReelUp.wav");
            _swingAudio = FifMod.Assets.GetAsset<AudioClip>("Items/Axe/AxeSwing.wav");
            _hitAudio = new AudioClip[]
            {
                FifMod.Assets.GetAsset<AudioClip>("Items/Axe/AxeHit.wav")
            };

            grabbable = true;
            grabbableToEnemies = false;

            base.Start();
        }

        public override void ItemActivate(bool used, bool buttonDown = true)
        {
            if (!playerHeldBy) return;
            _isHolding = buttonDown;

            if (!_isReelingUp && buttonDown)
            {
                _isReelingUp = true;
                _previousPlayerHeldBy = playerHeldBy;

                StopCoroutine(nameof(CO_ReelUp));
                StartCoroutine(nameof(CO_ReelUp));
            }
        }

        private IEnumerator CO_ReelUp()
        {
            playerHeldBy.activatingItem = true;
            playerHeldBy.twoHanded = true;
            playerHeldBy.playerBodyAnimator.ResetTrigger("shovelHit");
            playerHeldBy.playerBodyAnimator.SetBool("reelingUp", true);

            _axeSource.PlayOneShot(_reelUpAudio);
            PlayReelUpServerRpc();

            yield return new WaitForSeconds(0.5f);
            yield return new WaitUntil(() => !_isHolding || !isHeld);
            SwingAxe(!isHeld);

            yield return new WaitForSeconds(0.13f);
            HitAxe(!isHeld);

            yield return new WaitForSeconds(0.5f);
            _isReelingUp = false;
        }

        [ServerRpc]
        private void PlayReelUpServerRpc()
        {
            PlayReelUpClientRpc();
        }

        [ClientRpc]
        private void PlayReelUpClientRpc()
        {
            _axeSource.PlayOneShot(_reelUpAudio);
        }

        public override void DiscardItem()
        {
            playerHeldBy.activatingItem = false;
            base.DiscardItem();
        }

        private void SwingAxe(bool cancel = false)
        {
            _previousPlayerHeldBy.playerBodyAnimator.SetBool("reelingUp", value: false);
            if (!cancel)
            {
                _axeSource.PlayOneShot(_swingAudio);
                _previousPlayerHeldBy.UpdateSpecialAnimationValue(specialAnimation: true, (short)_previousPlayerHeldBy.transform.localEulerAngles.y, 0.4f);
            }
        }

        public override void GrabItem()
        {
            _roundManager = FindObjectOfType<RoundManager>();
        }

        private void HitAxe(bool cancel = false)
        {
            if (!_previousPlayerHeldBy) throw new("Previous player is null on this client when HitAxe is called.");
            _previousPlayerHeldBy.activatingItem = false;
            var flag = false;
            var surfaceId = -1;
            var cameraTransform = _previousPlayerHeldBy.gameplayCamera.transform;

            if (!cancel)
            {
                _previousPlayerHeldBy.twoHanded = false;
                Debug.DrawRay(cameraTransform.position + cameraTransform.right * -0.35f, cameraTransform.forward * 1.85f, Color.blue, 5f);

                var objectsHit = Physics.SphereCastAll(cameraTransform.position + cameraTransform.right * -0.35f, 0.75f, cameraTransform.forward, 1.85f, HIT_MASK, QueryTriggerInteraction.Collide);
                Array.Sort(objectsHit, (RaycastHit first, RaycastHit second) => first.distance > second.distance ? 1 : -1);

                var start = cameraTransform.position;
                for (int i = 0; i < objectsHit.Length; i++)
                {
                    var isHitSolid = Physics.Linecast(start, objectsHit[i].point, out _, StartOfRound.Instance.collidersAndRoomMaskAndDefault);
                    if (objectsHit[i].transform.gameObject.layer == 8 || objectsHit[i].transform.gameObject.layer == 11)
                    {
                        flag = true;
                        start = objectsHit[i].point + objectsHit[i].normal * 0.01f;

                        string targetTag = objectsHit[i].collider.gameObject.tag;
                        for (int j = 0; j < StartOfRound.Instance.footstepSurfaces.Length; j++)
                        {
                            if (StartOfRound.Instance.footstepSurfaces[j].surfaceTag == targetTag)
                            {
                                _axeSource.PlayOneShot(StartOfRound.Instance.footstepSurfaces[j].hitSurfaceSFX);
                                WalkieTalkie.TransmitOneShotAudio(_axeSource, StartOfRound.Instance.footstepSurfaces[j].hitSurfaceSFX);
                                surfaceId = j;
                                break;
                            }
                        }
                    }
                    else if (objectsHit[i].transform.TryGetComponent(out IHittable target) && !(objectsHit[i].transform == _previousPlayerHeldBy.transform) && (objectsHit[i].point == Vector3.zero || !isHitSolid))
                    {
                        flag = true;
                        target.Hit(HIT_FORCE, cameraTransform.forward, _previousPlayerHeldBy, true);
                    }
                }
            }

            if (flag)
            {
                RoundManager.PlayRandomClip(_axeSource, _hitAudio);
                _roundManager.PlayAudibleNoise(transform.position, 17f, 0.8f);
                playerHeldBy.playerBodyAnimator.SetTrigger("shovelHit");
                HitAxeServerRpc(surfaceId);
            }
        }

        [ServerRpc]
        private void HitAxeServerRpc(int surfaceId)
        {
            HitAxeClientRpc(surfaceId);
        }

        [ClientRpc]
        private void HitAxeClientRpc(int surfaceId)
        {
            RoundManager.PlayRandomClip(_axeSource, _hitAudio);
            if (surfaceId != -1)
            {
                HitSurface(surfaceId);
            }
        }

        private void HitSurface(int surfaceID)
        {
            _axeSource.PlayOneShot(StartOfRound.Instance.footstepSurfaces[surfaceID].hitSurfaceSFX);
            WalkieTalkie.TransmitOneShotAudio(_axeSource, StartOfRound.Instance.footstepSurfaces[surfaceID].hitSurfaceSFX);
        }
    }
}