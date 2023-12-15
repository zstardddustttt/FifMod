using System;
using System.Collections.Generic;
using LethalLib.Modules;

namespace FifMod.Definitions
{
    public class SilverBarProperties : FifModScrapProperties
    {
        public override string ItemAssetPath => "Scraps/SilverBar/SilverBar.asset";
        public override int Rarity => ConfigManager.ScrapsSilverBarRarity.Value;
        public override Levels.LevelTypes Moons => Levels.LevelTypes.All;

        public override Type CustomBehaviour => null;
        public override Dictionary<string, string> Tooltips => null;

        public override int Weight => 32;

        public override int MinValue => 75;
        public override int MaxValue => 140;
    }
}