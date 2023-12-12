using System;
using LethalLib.Modules;

namespace FifMod.Definitions
{
    public class SilverBarProperties : FifModScrapProperties
    {
        public override string ItemAssetPath => "Scraps/SilverBar/SilverBar.asset";
        public override int Rarity => ConfigManager.ScrapsSilverBarRarity.Value;
        public override Levels.LevelTypes Moons => Levels.LevelTypes.All;

        public override Type CustomBehaviour => null;
    }
}