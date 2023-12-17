using System;
using System.Collections.Generic;
using UnityEngine;

namespace FifMod
{
    public partial class FifModBackend
    {
        private static readonly List<MapObject> _mapObjects = new();
        public static MapObject[] MapObjects => _mapObjects.ToArray();

        public static void RegisterMapObject(GameObject prefab, Func<SelectableLevel, AnimationCurve> spawnRateFunction, bool facingAwayFromWall, MoonFlags moons)
        {
            _mapObjects.Add(new(prefab, spawnRateFunction, facingAwayFromWall, moons));
        }

        public readonly struct MapObject
        {
            public readonly GameObject prefab;
            public readonly Func<SelectableLevel, AnimationCurve> spawnRateFunction;
            public readonly bool facingAwayFromWall;
            public readonly MoonFlags moons;

            public MapObject(GameObject prefab, Func<SelectableLevel, AnimationCurve> spawnRateFunction, bool facingAwayFromWall, MoonFlags moons)
            {
                this.prefab = prefab;
                this.spawnRateFunction = spawnRateFunction;
                this.facingAwayFromWall = facingAwayFromWall;
                this.moons = moons;
            }
        }
    }
}