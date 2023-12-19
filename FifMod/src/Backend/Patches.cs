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
                    if (FifModGameInfo.IsMansion && !scrap.spawnFlags.HasFlag(ScrapSpawnFlags.Mansion)) continue;
                    if (!FifModGameInfo.IsMansion && !scrap.spawnFlags.HasFlag(ScrapSpawnFlags.Facility)) continue;

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
                    if (FifModGameInfo.IsMansion && !mapObject.spawnFlags.HasFlag(MapObjectSpawnFlags.Mansion)) continue;
                    if (!FifModGameInfo.IsMansion && !mapObject.spawnFlags.HasFlag(MapObjectSpawnFlags.Facility)) continue;

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
            var itemList = __instance.buyableItemsList.ToList();

            var buyKeyword = __instance.terminalNodes.allKeywords.First(keyword => keyword.word == "buy");
            var cancelPurchaseNode = buyKeyword.compatibleNouns[0].result.terminalOptions[1].result;
            var infoKeyword = __instance.terminalNodes.allKeywords.First(keyword => keyword.word == "info");

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
                        noun = __instance.terminalNodes.allKeywords.First(keyword2 => keyword2.word == "confirm"),
                        result = buyNode2
                    },
                    new()
                    {
                        noun = __instance.terminalNodes.allKeywords.First(keyword2 => keyword2.word == "deny"),
                        result = cancelPurchaseNode
                    }
                };

                var keyword = FifModBackendUtils.CreateTerminalKeyword(itemName.ToLowerInvariant().Replace(" ", "-"), defaultVerb: buyKeyword);

                var allKeywords = __instance.terminalNodes.allKeywords.ToList();
                allKeywords.Add(keyword);
                __instance.terminalNodes.allKeywords = allKeywords.ToArray();

                var nouns = buyKeyword.compatibleNouns.ToList();
                nouns.Add(new CompatibleNoun()
                {
                    noun = keyword,
                    result = buyNode1
                });
                buyKeyword.compatibleNouns = nouns.ToArray();

                var itemInfo = storeItem.itemInfo ?? FifModBackendUtils.GetDefaultItemInfo(itemName);
                __instance.terminalNodes.allKeywords = allKeywords.ToArray();

                var itemInfoNouns = infoKeyword.compatibleNouns.ToList();
                itemInfoNouns.Add(new CompatibleNoun()
                {
                    noun = keyword,
                    result = itemInfo
                });
                infoKeyword.compatibleNouns = itemInfoNouns.ToArray();
                FifMod.Logger.LogInfo($"Backend registered store item in terminal: {storeItem.item.itemName}");
            }

            __instance.buyableItemsList = itemList.ToArray();
        }
    }
}