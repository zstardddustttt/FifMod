using BepInEx.Configuration;

namespace FifMod
{
    public static class ConfigManager
    {
        public static ConfigEntry<int> ItemsAxePrice { get; private set; }
        public static ConfigEntry<int> ItemsGlowstickPrice { get; private set; }

        public static ConfigEntry<int> ScrapsMagicBallRarity { get; private set; }
        public static ConfigEntry<int> ScrapsSilverBarRarity { get; private set; }

        public static ConfigEntry<int> MiscShipCapacity { get; private set; }

        public static void BindConfigFile(ConfigFile config)
        {
            ItemsAxePrice = config.Bind("Items", "Axe-Price", 110);
            ItemsGlowstickPrice = config.Bind("Items", "Glowstick-Price", 85);

            ScrapsMagicBallRarity = config.Bind("Scraps", "Magic-Ball-Rarity", 80);
            ScrapsSilverBarRarity = config.Bind("Scraps", "Silver-Bar-Rarity", 50);

            MiscShipCapacity = config.Bind("Misc", "Ship-Capacity", 999, "Increases maximum amount of items that game can save");
        }
    }
}