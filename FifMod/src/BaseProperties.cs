using System;
using LethalLib.Modules;

namespace FifMod
{
    public abstract class FifModProperties { }
    public abstract class FifModObjectProperties : FifModProperties
    {
        public abstract Type CustomBehaviour { get; }
        public abstract string ItemAssetPath { get; }
    }

    public abstract class FifModScrapProperties : FifModObjectProperties
    {
        public abstract int Rarity { get; }
        public abstract Levels.LevelTypes Moons { get; }
    }

    public abstract class FifModItemProperties : FifModObjectProperties
    {
        public abstract string InfoAssetPath { get; }
        public abstract int Price { get; }
    }
}