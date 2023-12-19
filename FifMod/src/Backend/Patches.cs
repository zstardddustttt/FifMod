using System.Collections.Generic;
using System.Linq;
using FifMod.Base;
using FifMod.Utils;
using HarmonyLib;
using Unity.Netcode;
using static FifMod.FifModBackend;

namespace FifMod.Patches
{
    internal class FifModBackendPatches
    {
        [HarmonyPatch(typeof(GameNetworkManager), nameof(GameNetworkManager.Start))]
        [HarmonyPrefix]
        private static void GameNetworkManager_Start()
        {
            foreach (var prefab in FifModBackend.NetworkPrefabs)
            {
                NetworkManager.Singleton.AddNetworkPrefab(prefab);
            }
        }

        [HarmonyPatch(typeof(RoundManager), nameof(RoundManager.SpawnScrapInLevel))]
        [HarmonyPrefix]
        private static void RoundManager_SpawnScrapInLevel()
        {
            var startOfRound = StartOfRound.Instance;
            foreach (SelectableLevel level in startOfRound.levels)
            {
                if (!FifModBackendUtils.TryGetMoonFlagFromName(level.name, out MoonFlags flag)) continue;

                foreach (Scrap scrap in Scraps)
                {
                    var scrapIdx = level.spawnableScrap.FindIndex(current => current.spawnableItem == scrap.item);
                    if (scrapIdx != -1) level.spawnableScrap.RemoveAt(scrapIdx);

                    if (!scrap.moons.HasFlag(flag)) continue;

                    var isMansion = FifModGameInfo.IsMansion;
                    if (isMansion && !scrap.spawnFlags.HasFlag(ScrapSpawnFlags.Mansion)) continue;
                    if (!isMansion && !scrap.spawnFlags.HasFlag(ScrapSpawnFlags.Facility)) continue;

                    var scrapItem = new SpawnableItemWithRarity()
                    {
                        spawnableItem = scrap.item,
                        rarity = scrap.rarity.GetRarityOfFlags(flag)[0]
                    };

                    level.spawnableScrap.Add(scrapItem);
                }
            }
        }

        [HarmonyPatch(typeof(RoundManager), nameof(RoundManager.SpawnMapObjects))]
        [HarmonyPrefix]
        private static void RoundManager_SpawnMapObject()
        {
            var startOfRound = StartOfRound.Instance;
            foreach (var level in startOfRound.levels)
            {
                if (!FifModBackendUtils.TryGetMoonFlagFromName(level.name, out MoonFlags flag)) continue;
                var levelMapObjects = level.spawnableMapObjects.ToList();

                foreach (var mapObject in MapObjects)
                {
                    var objectIdx = levelMapObjects.FindIndex(current => current.prefabToSpawn == mapObject.prefab);
                    if (objectIdx != -1) levelMapObjects.RemoveAt(objectIdx);

                    if (!mapObject.moons.HasFlag(flag)) continue;

                    var isMansion = FifModGameInfo.IsMansion;
                    if (isMansion && !mapObject.spawnFlags.HasFlag(MapObjectSpawnFlags.Mansion)) continue;
                    if (!isMansion && !mapObject.spawnFlags.HasFlag(MapObjectSpawnFlags.Facility)) continue;

                    var spawnableMapObject = new SpawnableMapObject()
                    {
                        numberToSpawn = mapObject.spawnRateFunction(level),
                        prefabToSpawn = mapObject.prefab,
                        spawnFacingAwayFromWall = mapObject.facingAwayFromWall
                    };

                    levelMapObjects.Add(spawnableMapObject);
                }

                level.spawnableMapObjects = levelMapObjects.ToArray();
            }

            var randomMapObjects = UnityEngine.Object.FindObjectsOfType<RandomMapObject>();
            foreach (var randomMapObject in randomMapObjects)
            {
                foreach (MapObject mapObject in MapObjects)
                {
                    if (!randomMapObject.spawnablePrefabs.Any((prefab) => prefab == mapObject.prefab))
                    {
                        randomMapObject.spawnablePrefabs.Add(mapObject.prefab);
                    }
                }
            }
        }

        [HarmonyPatch(typeof(RoundManager), nameof(RoundManager.GeneratedFloorPostProcessing))]
        [HarmonyPrefix]
        private static void RoundManager_GeneratedFloorPostProcessing()
        {
            var startOfRound = StartOfRound.Instance;
            foreach (var level in startOfRound.levels)
            {
                if (!FifModBackendUtils.TryGetMoonFlagFromName(level.name, out MoonFlags flag)) continue;

                foreach (Enemy spawnableEnemy in Enemies)
                {
                    var enemyIdx = level.Enemies.FindIndex(current => current.enemyType == spawnableEnemy.enemy);
                    if (enemyIdx != -1) level.Enemies.RemoveAt(enemyIdx);

                    var enemyOutsideIdx = level.OutsideEnemies.FindIndex(current => current.enemyType == spawnableEnemy.enemy);
                    if (enemyOutsideIdx != -1) level.OutsideEnemies.RemoveAt(enemyOutsideIdx);

                    var enemyDaytimeIdx = level.DaytimeEnemies.FindIndex(current => current.enemyType == spawnableEnemy.enemy);
                    if (enemyDaytimeIdx != -1) level.DaytimeEnemies.RemoveAt(enemyDaytimeIdx);

                    if (!spawnableEnemy.moons.HasFlag(flag)) continue;

                    var spawnableEnemyWithRarity = new SpawnableEnemyWithRarity()
                    {
                        enemyType = spawnableEnemy.enemy,
                        rarity = spawnableEnemy.rarity.GetRarityOfFlags(flag)[0]
                    };

                    if (spawnableEnemy.spawnFlags.HasFlag(EnemySpawnFlags.Outside))
                    {
                        level.OutsideEnemies.Add(spawnableEnemyWithRarity);
                        FifMod.Logger.LogInfo($"enemy {spawnableEnemy.enemy.name} now spawns outside");
                    }

                    if (spawnableEnemy.spawnFlags.HasFlag(EnemySpawnFlags.Daytime))
                    {
                        level.DaytimeEnemies.Add(spawnableEnemyWithRarity);
                        FifMod.Logger.LogInfo($"enemy {spawnableEnemy.enemy.name} now spawns daytime");
                    }

                    if (spawnableEnemy.spawnFlags.HasFlag(EnemySpawnFlags.Facility) || spawnableEnemy.spawnFlags.HasFlag(EnemySpawnFlags.Mansion))
                    {
                        var isMansion = FifModGameInfo.IsMansion;

                        if (isMansion && !spawnableEnemy.spawnFlags.HasFlag(EnemySpawnFlags.Mansion)) continue;
                        if (!isMansion && !spawnableEnemy.spawnFlags.HasFlag(EnemySpawnFlags.Facility)) continue;

                        level.Enemies.Add(spawnableEnemyWithRarity);
                        FifMod.Logger.LogInfo($"enemy {spawnableEnemy.enemy.name} now spawns inside");
                    }
                }
            }
        }

        [HarmonyPatch(typeof(StartOfRound), nameof(StartOfRound.Awake))]
        [HarmonyPostfix]
        private static void StartOfRound_Awake(StartOfRound __instance)
        {
            foreach (Scrap scrap in Scraps)
            {
                if (!__instance.allItemsList.itemsList.Contains(scrap.item))
                {
                    FifMod.Logger.LogInfo($"Backend registered scrap: {scrap.item.itemName}");
                    __instance.allItemsList.itemsList.Add(scrap.item);
                }
            }

            foreach (StoreItem storeItem in StoreItems)
            {
                if (!__instance.allItemsList.itemsList.Contains(storeItem.item))
                {
                    FifMod.Logger.LogInfo($"Backend registered store item: {storeItem.item.itemName}");
                    __instance.allItemsList.itemsList.Add(storeItem.item);
                }
            }
        }

        [HarmonyPatch(typeof(Terminal), nameof(Terminal.Awake))]
        [HarmonyPrefix]
        private static void Terminal_Awake(Terminal __instance)
        {
            RegisterItemsInTerminal(__instance);
            RegisterEnemiesInTerminal(__instance);
        }

        private static void RegisterEnemiesInTerminal(Terminal terminal)
        {
            var infoKeyword = terminal.terminalNodes.allKeywords.First(keyword => keyword.word == "info");
            var addedEnemies = new List<string>();

            foreach (Enemy spawnableEnemy in Enemies)
            {
                if (addedEnemies.Contains(spawnableEnemy.enemy.enemyName))
                {
                    FifMod.Logger.LogWarning($"there is already an enemy named {spawnableEnemy.enemy.enemyName}, skipping");
                    continue;
                }

                if (terminal.enemyFiles.Any(x => x.creatureName == spawnableEnemy.info.creatureName))
                {
                    FifMod.Logger.LogWarning($"there is already an enemy named {spawnableEnemy.enemy.enemyName}, skipping");
                    continue;
                }

                var keyword = FifModBackendUtils.CreateEnemyTerminalKeyword(spawnableEnemy.info.creatureName, infoKeyword);
                var allKeywords = terminal.terminalNodes.allKeywords.ToList();
                if (!allKeywords.Any(x => x.word == keyword.word))
                {
                    allKeywords.Add(keyword);
                    terminal.terminalNodes.allKeywords = allKeywords.ToArray();
                }

                var itemInfoNouns = infoKeyword.compatibleNouns.ToList();
                if (!itemInfoNouns.Any(x => x.noun.word == keyword.word))
                {
                    itemInfoNouns.Add(new CompatibleNoun()
                    {
                        noun = keyword,
                        result = spawnableEnemy.info
                    });
                }
                infoKeyword.compatibleNouns = itemInfoNouns.ToArray();

                spawnableEnemy.info.creatureFileID = terminal.enemyFiles.Count;
                terminal.enemyFiles.Add(spawnableEnemy.info);

                spawnableEnemy.enemy.enemyPrefab.GetComponentInChildren<ScanNodeProperties>().creatureScanID = spawnableEnemy.info.creatureFileID;
            }
        }

        private static void RegisterItemsInTerminal(Terminal terminal)
        {
            var itemList = terminal.buyableItemsList.ToList();

            var buyKeyword = terminal.terminalNodes.allKeywords.First(keyword => keyword.word == "buy");
            var cancelPurchaseNode = buyKeyword.compatibleNouns[0].result.terminalOptions[1].result;
            var infoKeyword = terminal.terminalNodes.allKeywords.First(keyword => keyword.word == "info");

            foreach (var storeItem in StoreItems)
            {
                if (itemList.Exists((Item current) => current.itemName == storeItem.item.itemName))
                {
                    FifMod.Logger.LogWarning($"there is already an item named {storeItem.item.itemName}, skipping");
                    continue;
                }

                storeItem.item.creditsWorth = storeItem.price;
                itemList.Add(storeItem.item);

                var itemName = storeItem.item.itemName;

                var buyNode2 = storeItem.buyNode2 ?? FifModBackendUtils.GetDefaultBuyNode2(itemName);
                buyNode2.buyItemIndex = itemList.Count - 1;
                buyNode2.isConfirmationNode = false;
                buyNode2.itemCost = storeItem.price;
                buyNode2.playSyncedClip = 0;

                var buyNode1 = storeItem.buyNode1 ?? FifModBackendUtils.GetDefaultBuyNode1(itemName);
                buyNode1.buyItemIndex = itemList.Count - 1;
                buyNode1.isConfirmationNode = true;
                buyNode1.overrideOptions = true;
                buyNode1.itemCost = storeItem.price;
                buyNode1.terminalOptions = new CompatibleNoun[2]
                {
                    new()
                    {
                        noun = terminal.terminalNodes.allKeywords.First(keyword2 => keyword2.word == "confirm"),
                        result = buyNode2
                    },
                    new()
                    {
                        noun = terminal.terminalNodes.allKeywords.First(keyword2 => keyword2.word == "deny"),
                        result = cancelPurchaseNode
                    }
                };

                var keyword = FifModBackendUtils.CreateTerminalKeyword(itemName.ToLowerInvariant().Replace(" ", "-"), defaultVerb: buyKeyword);

                var allKeywords = terminal.terminalNodes.allKeywords.ToList();
                allKeywords.Add(keyword);
                terminal.terminalNodes.allKeywords = allKeywords.ToArray();

                var nouns = buyKeyword.compatibleNouns.ToList();
                nouns.Add(new CompatibleNoun()
                {
                    noun = keyword,
                    result = buyNode1
                });
                buyKeyword.compatibleNouns = nouns.ToArray();

                var itemInfo = storeItem.itemInfo ?? FifModBackendUtils.GetDefaultItemInfo(itemName);
                terminal.terminalNodes.allKeywords = allKeywords.ToArray();

                var itemInfoNouns = infoKeyword.compatibleNouns.ToList();
                itemInfoNouns.Add(new CompatibleNoun()
                {
                    noun = keyword,
                    result = itemInfo
                });
                infoKeyword.compatibleNouns = itemInfoNouns.ToArray();
                FifMod.Logger.LogInfo($"Backend registered store item in terminal: {storeItem.item.itemName}");
            }

            terminal.buyableItemsList = itemList.ToArray();
        }
    }
}