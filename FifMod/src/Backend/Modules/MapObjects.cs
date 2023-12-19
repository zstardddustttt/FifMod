using System;
using System.Collections.Generic;
using FifMod.Base;
using UnityEngine;

namespace FifMod
{
    public partial class FifModBackend
    {
        private static readonly List<MapObject> _mapObjects = new();
        public static MapObject[] MapObjects => _mapObjects.ToArray();

        public static void RegisterMapObject(GameObject prefab, Func<SelectableLevel, AnimationCurve> spawnRateFunction, bool facingAwayFromWall, MoonFlags moons, MapObjectSpawnFlags spawnFlags)
        {
            _mapObjects.Add(new(prefab, spawnRateFunction, facingAwayFromWall, moons, spawnFlags));
        }

        public readonly struct MapObject
        {
            public readonly GameObject prefab;
            public readonly Func<SelectableLevel, AnimationCurve> spawnRateFunction;
            public readonly bool facingAwayFromWall;
            public readonly MoonFlags moons;
            public readonly MapObjectSpawnFlags spawnFlags;

            public MapObject(GameObject prefab, Func<SelectableLevel, AnimationCurve> spawnRateFunction, bool facingAwayFromWall, MoonFlags moons, MapObjectSpawnFlags spawnFlags)
            {
                this.prefab = prefab;
                this.spawnRateFunction = spawnRateFunction;
                this.facingAwayFromWall = facingAwayFromWall;
                this.moons = moons;
                this.spawnFlags = spawnFlags;
            }
        }
    }
}