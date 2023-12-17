using System.Collections.Generic;

namespace FifMod
{
    public partial class FifModBackend
    {
        private static readonly List<Scrap> _scraps = new();
        public static Scrap[] Scraps => _scraps.ToArray();

        public static void RegisterScrap(Item scrapItem, int rarity, MoonFlags moons, ScrapSpawnFlags spawnFlags)
        {
            var scrap = new Scrap(scrapItem, rarity, moons, spawnFlags);
            _scraps.Add(scrap);
        }

        public readonly struct Scrap
        {
            public readonly Item item;
            public readonly int rarity;
            public readonly MoonFlags moons;
            public readonly ScrapSpawnFlags spawnFlags;

            public Scrap(Item item, int rarity, MoonFlags moons, ScrapSpawnFlags spawnFlags)
            {
                this.item = item;
                this.rarity = rarity;
                this.moons = moons;
                this.spawnFlags = spawnFlags;
            }
        }
    }
}