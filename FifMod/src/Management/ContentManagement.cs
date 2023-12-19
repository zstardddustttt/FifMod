using System;
using System.Collections.Generic;
using System.Reflection;
using FifMod.Utils;
using UnityEngine;
using FifMod.Base;

namespace FifMod
{
    public static class ContentManager
    {
        private static readonly Assembly _assembly = Assembly.GetExecutingAssembly();
        private static readonly Dictionary<Item, FifModItemProperties> _itemProperties = new();

        public static bool TryGetItemProperties(Item item, out FifModItemProperties properties)
        {
            return _itemProperties.TryGetValue(item, out properties);
        }

        private static void RegisterItem(Item item, FifModItemProperties properties)
        {
            _itemProperties.Add(item, properties);
            item.weight = FifModUtils.PoundsToItemWeight(properties.Weight);

            if (properties.CustomBehaviour != null)
            {
                var behaviour = (GrabbableObject)item.spawnPrefab.AddComponent(properties.CustomBehaviour);
                behaviour.itemProperties = item;
            }
            FifModBackend.RegisterNetworkPrefab(item.spawnPrefab);
        }

        public static void RegisterContent(FifModAssets assets)
        {
            var storeItemProperties = new List<FifModStoreItemProperties>();
            var scrapProperties = new List<FifModScrapProperties>();
            var mapObjectProperties = new List<FifModMapObjectProperties>();
            foreach (var type in _assembly.GetTypes())
            {
                if (type.IsAbstract) continue;

                if (type.IsSubclassOf(typeof(FifModStoreItemProperties)))
                {
                    FifMod.Logger.LogInfo($"Found store item properties: {type.Name}");
                    storeItemProperties.Add((FifModStoreItemProperties)Activator.CreateInstance(type));
                }
                else if (type.IsSubclassOf(typeof(FifModScrapProperties)))
                {
                    FifMod.Logger.LogInfo($"Found scrap properties: {type.Name}");
                    scrapProperties.Add((FifModScrapProperties)Activator.CreateInstance(type));
                }
                else if (type.IsSubclassOf(typeof(FifModMapObjectProperties)))
                {
                    FifMod.Logger.LogInfo($"Found map object properties: {type.Name}");
                    mapObjectProperties.Add((FifModMapObjectProperties)Activator.CreateInstance(type));
                }
            }
            FifMod.Logger.LogInfo($"Loaded {storeItemProperties.Count} store items, {scrapProperties.Count} scraps, {mapObjectProperties.Count} map objects");

            var registeredStoreItems = 0;
            foreach (var properties in storeItemProperties)
            {
                if (!assets.TryGetAsset(properties.ItemAssetPath, out Item item))
                {
                    FifMod.Logger.LogWarning($"Item at path {properties.ItemAssetPath} was not found");
                    continue;
                }

                if (!assets.TryGetAsset(properties.InfoAssetPath, out TerminalNode info))
                {
                    FifMod.Logger.LogWarning($"Terminal Node at path {properties.InfoAssetPath} was not found");
                    continue;
                }

                FifMod.Logger.LogInfo($"Registering store item | Name: {item.itemName} | Price: {properties.Price}");
                RegisterItem(item, properties);
                FifModBackend.RegisterStoreItem(item, properties.Price, info);
                registeredStoreItems++;
            }
            FifMod.Logger.LogInfo($"Registered {registeredStoreItems}/{storeItemProperties.Count} store items");

            var registeredScraps = 0;
            foreach (var properties in scrapProperties)
            {
                if (!assets.TryGetAsset(properties.ItemAssetPath, out Item item))
                {
                    FifMod.Logger.LogWarning($"Item at path {properties.ItemAssetPath} was not found");
                    continue;
                }

                item.minValue = (int)(properties.MinValue / 0.4f);
                item.maxValue = (int)(properties.MaxValue / 0.4f);

                var avgCost = (properties.MinValue + properties.MaxValue) / 2;
                FifMod.Logger.LogInfo($"Registering scrap | Name: {item.itemName} | Avg Cost: {avgCost}");
                RegisterItem(item, properties);
                FifModBackend.RegisterScrap(item, properties.Rarity, properties.Moons, properties.SpawnFlags);
                registeredScraps++;
            }
            FifMod.Logger.LogInfo($"Registered {registeredScraps}/{scrapProperties.Count} scraps");

            var registeredMapObjects = 0;
            foreach (var properties in mapObjectProperties)
            {
                if (!assets.TryGetAsset(properties.PrefabAssetPath, out GameObject prefab))
                {
                    FifMod.Logger.LogWarning($"GameObject at path {properties.PrefabAssetPath} was not found");
                    continue;
                }

                if (properties.CustomBehaviour != null)
                {
                    prefab.AddComponent(properties.CustomBehaviour);
                }

                FifMod.Logger.LogInfo($"Registering map object | Name: {prefab.name}");
                FifModBackend.RegisterNetworkPrefab(prefab);
                FifModBackend.RegisterMapObject(prefab, properties.SpawnRateFunction, properties.SpawnFacingAwayFromWall, properties.Moons, properties.SpawnFlags);
                registeredMapObjects++;
            }
            FifMod.Logger.LogInfo($"Registered {registeredMapObjects}/{mapObjectProperties.Count} map objects");

            var types = Assembly.GetExecutingAssembly().GetTypes();
            foreach (var type in types)
            {
                var methods = type.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
                foreach (var method in methods)
                {
                    var attributes = method.GetCustomAttributes(typeof(RuntimeInitializeOnLoadMethodAttribute), false);
                    if (attributes.Length > 0)
                    {
                        method.Invoke(null, null);
                    }
                }
            }
        }
    }
}