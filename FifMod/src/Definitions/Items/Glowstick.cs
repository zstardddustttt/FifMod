using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace FifMod.Definitions
{
    public class GlowstickProperties : FifModItemProperties
    {
        public override int Price => ConfigManager.ItemsGlowstickPrice.Value;
        public override string ItemAssetPath => "Items/Glowstick/GlowstickItem.asset";
        public override string InfoAssetPath => "Items/Glowstick/GlowstickInfo.asset";

        public override Type CustomBehaviour => typeof(GlowstickBehaviour);
        public override Dictionary<string, string> Tooltips => new()
        {
            {"Item Secondary use", "Power button"},
            {"Item primary use", "Switch color"}
        };

        public override int Weight => 2;
    }

    public class GlowstickBehaviour : GrabbableObject
    {
        private Light _lightSource;
        private AudioSource _audioSource;

        private Material _lightMaterial;
        private bool _lightEnabled;
        private const float LIGHT_FADE_IN_DURATION = 0.5f;
        private const float LIGHT_FADE_OUT_DURATION = 0.25f;
        private float _defaultLightIntensity;
        private float _defaultEmissionIntensity;

        private float _materialEmission;
        private Color _materialColor;

        private AudioClip _enableAudio;
        private AudioClip _disableAudio;
        private AudioClip _switchAudio;

        private float MaterialEmission
        {
            get => _materialEmission;
            set
            {
                _materialEmission = value;
                _lightMaterial.SetColor("_EmissiveColor", _materialColor * _materialEmission);
            }
        }

        private Color MaterialColor
        {
            get => _materialColor;
            set
            {
                _materialColor = value;
                _lightMaterial.SetColor("_EmissiveColor", _materialColor * _materialEmission);
            }
        }

        private readonly Color[] colors = new Color[]
        {
            new(1, 0.1f, 0.05f),
            new(1, 1, 0.1f),
            new(0.05f, 1, 0.1f),
            new(0.1f, 1, 1),
            new(0.05f, 0.1f, 1),
            new(1, 0.1f, 1),
            new(1, 1, 1)
        };

        private int _selectedColor;

        public override void Start()
        {
            grabbable = true;
            grabbableToEnemies = true;
            _enableAudio = FifMod.Assets.GetAsset<AudioClip>("Items/Glowstick/GlowstickOn.wav");
            _disableAudio = FifMod.Assets.GetAsset<AudioClip>("Items/Glowstick/GlowstickOff.wav");
            _switchAudio = FifMod.Assets.GetAsset<AudioClip>("Items/Glowstick/GlowstickSwitch.wav");

            mainObjectRenderer = GetComponentInChildren<MeshRenderer>();
            _lightSource = GetComponentInChildren<Light>();
            _audioSource = GetComponent<AudioSource>();

            _lightMaterial = mainObjectRenderer.materials[1];
            _defaultLightIntensity = _lightSource.intensity;
            _defaultEmissionIntensity = 5f;
            MaterialEmission = 5f;

            ToggleVisual(false, false);
            SelectColor(1);
            base.Start();
        }

        public override void ItemInteractLeftRight(bool right)
        {
            base.ItemInteractLeftRight(right);
            if (!right) ToggleLightServerRpc(!_lightEnabled);
        }

        public override void ItemActivate(bool used, bool buttonDown = true)
        {
            base.ItemActivate(used, buttonDown);

            if (!_lightEnabled) return;
            int nextIdx = (_selectedColor + 1) % colors.Length;
            SelectColorServerRpc(nextIdx);
        }

        public override void PocketItem()
        {
            playerHeldBy.equippedUsableItemQE = false;
            if (playerHeldBy) isBeingUsed = false;

            ToggleVisualServerRpc(false, false);
            base.PocketItem();
        }

        public override void EquipItem()
        {
            playerHeldBy.equippedUsableItemQE = true;

            if (_lightEnabled) ToggleVisualServerRpc(true, false);
            base.EquipItem();
        }

        public override void DiscardItem()
        {
            if (playerHeldBy) playerHeldBy.equippedUsableItemQE = false;
            isBeingUsed = false;
            base.DiscardItem();
        }

        [ServerRpc]
        private void ToggleLightServerRpc(bool enable)
        {
            ToggleLightClientRpc(enable);
        }

        [ClientRpc]
        private void ToggleLightClientRpc(bool enable)
        {
            ToggleLight(enable);
        }

        private void ToggleLight(bool enable)
        {
            _lightEnabled = enable;
            ToggleVisual(enable, true);

            _audioSource.PlayOneShot(enable ? _enableAudio : _disableAudio);
        }

        [ServerRpc]
        private void ToggleVisualServerRpc(bool enable, bool fade)
        {
            ToggleVisualClientRpc(enable, fade);
        }

        [ClientRpc]
        private void ToggleVisualClientRpc(bool enable, bool fade)
        {
            ToggleVisual(enable, fade);
        }

        private void ToggleVisual(bool enable, bool fade)
        {
            if (fade)
            {
                StopCoroutine(nameof(CO_ToggleLightVisual));
                StartCoroutine(nameof(CO_ToggleLightVisual), enable);
            }
            else
            {
                _lightSource.enabled = enable;
                MaterialEmission = enable ? _defaultEmissionIntensity : 0;
            }
        }

        private IEnumerator CO_ToggleLightVisual(bool enable)
        {
            var current = 0f;
            if (enable)
            {
                _lightSource.enabled = true;
                while (current < LIGHT_FADE_IN_DURATION)
                {
                    var inverseLerpValue = Mathf.InverseLerp(0, LIGHT_FADE_IN_DURATION, current);
                    _lightSource.intensity = inverseLerpValue * _defaultLightIntensity;
                    MaterialEmission = inverseLerpValue * _defaultEmissionIntensity;

                    current += Time.deltaTime;
                    yield return null;
                }
                _lightSource.intensity = _defaultLightIntensity;
            }
            else
            {
                current = LIGHT_FADE_OUT_DURATION;
                while (current > 0)
                {
                    var inverseLerpValue = Mathf.InverseLerp(0, LIGHT_FADE_IN_DURATION, current);
                    _lightSource.intensity = inverseLerpValue * _defaultLightIntensity;
                    MaterialEmission = inverseLerpValue * _defaultEmissionIntensity;

                    current -= Time.deltaTime;
                    yield return null;
                }
                _lightSource.intensity = 0f;
                MaterialEmission = 0f;
                _lightSource.enabled = false;
            }
        }

        [ServerRpc]
        private void SelectColorServerRpc(int idx)
        {
            SelectColorClientRpc(idx);
        }

        [ClientRpc]
        private void SelectColorClientRpc(int idx)
        {
            SelectColor(idx);
        }

        private void SelectColor(int idx)
        {
            _selectedColor = idx;
            _audioSource.PlayOneShot(_switchAudio);
        }

        public override void Update()
        {
            base.Update();
            if (_lightSource.enabled)
            {
                _lightSource.color = Color.Lerp(_lightSource.color, colors[_selectedColor], Time.deltaTime * 5);
                MaterialColor = _lightSource.color;
            }
        }
    }
}