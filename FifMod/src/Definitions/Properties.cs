using System;
using System.Collections.Generic;
using UnityEngine;

namespace FifMod
{
    public abstract class FifModProperties
    {
        public abstract Type CustomBehaviour { get; }
        public abstract Type[] CustomBehaviours { get; }
    }

    public abstract class FifModItemProperties : FifModProperties
    {
        public abstract string ItemAssetPath { get; }
        public abstract Dictionary<string, string> Tooltips { get; }
        public abstract int Weight { get; }
    }

    public abstract class FifModScrapProperties : FifModItemProperties
    {
        public abstract int Rarity { get; }
        public abstract MoonFlags Moons { get; }
        public abstract int MinValue { get; }
        public abstract int MaxValue { get; }
    }

    public abstract class FifModStoreItemProperties : FifModItemProperties
    {
        public abstract string InfoAssetPath { get; }
        public abstract int Price { get; }
    }

    public abstract class FifModEntityProperties : FifModProperties
    {
        public abstract MoonFlags Moons { get; }
    }

    public abstract class FifModMapObjectProperties : FifModEntityProperties
    {
        public abstract string ObjectAssetPath { get; }
        public abstract Func<SelectableLevel, AnimationCurve> SpawnRateFunction { get; }
        public abstract MapObjectSpawnFlags SpawnFlags { get; }
    }

    public abstract class FifModEnemyProperties : FifModEntityProperties
    {
        public abstract string EnemyAssetPath { get; }
        public abstract string InfoAssetPath { get; }
        public abstract int Rarity { get; }
        public abstract EnemySpawnFlags SpawnFlags { get; }
    }
}