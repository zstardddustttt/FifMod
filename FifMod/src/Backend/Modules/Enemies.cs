using System.Collections.Generic;
using FifMod.Base;

namespace FifMod
{
    public partial class FifModBackend
    {
        private static readonly List<Enemy> _enemies = new();
        public static Enemy[] Enemies => _enemies.ToArray();

        public readonly struct Enemy
        {
            public readonly EnemyType enemy;
            public readonly TerminalNode info;
            public readonly FifModRarity rarity;
            public readonly MoonFlags moons;
            public readonly EnemySpawnFlags spawnFlags;

            public Enemy(EnemyType enemy, TerminalNode info, FifModRarity rarity, MoonFlags moons, EnemySpawnFlags spawnFlags)
            {
                this.enemy = enemy;
                this.info = info;
                this.rarity = rarity;
                this.moons = moons;
                this.spawnFlags = spawnFlags;
            }
        }

        public static void RegisterEnemy(EnemyType enemy, TerminalNode info, FifModRarity rarity, MoonFlags moons, EnemySpawnFlags spawnFlags)
        {
            if (spawnFlags.HasFlag(EnemySpawnFlags.Outside)) enemy.isOutsideEnemy = true;
            else enemy.isOutsideEnemy = false;

            if (spawnFlags.HasFlag(EnemySpawnFlags.Daytime)) enemy.isDaytimeEnemy = true;
            else enemy.isDaytimeEnemy = false;

            _enemies.Add(new(enemy, info, rarity, moons, spawnFlags));
        }
    }
}