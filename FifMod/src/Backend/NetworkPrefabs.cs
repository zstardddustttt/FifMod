using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using HarmonyLib;

namespace FifMod
{
    public partial class FifModBackend
    {
        private static readonly List<GameObject> _networkPrefabs = new();
        public static GameObject[] NetworkPrefabs => _networkPrefabs.ToArray();

        public static void RegisterNetworkPrefab(GameObject prefab)
        {
            if (!prefab.GetComponent<NetworkObject>())
            {
                FifMod.Logger.LogWarning("there is no NetworkObject component attached to the prefab, skipping");
                return;
            }

            _networkPrefabs.Add(prefab);
        }
    }
}