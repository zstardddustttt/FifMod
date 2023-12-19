using System;
using FifMod.Base;
using FifMod.Utils;
using UnityEngine;

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
        public override void Start()
        {
            creatureSFX = gameObject.GetChild("MainSFX").GetComponent<AudioSource>();
            creatureVoice = gameObject.GetChild("VoiceSFX").GetComponent<AudioSource>();
            base.Start();
        }
    }
}