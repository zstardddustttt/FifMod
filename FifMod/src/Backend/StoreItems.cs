using System.Collections.Generic;
using System.Linq;
using FifMod.Utils;
using HarmonyLib;
using UnityEngine;

namespace FifMod
{
    public partial class FifModBackend
    {
        private static readonly List<StoreItem> _storeItems = new();
        public static StoreItem[] StoreItems => _storeItems.ToArray();

        public static void RegisterStoreItem(Item storeItem, int price, TerminalNode itemInfo = null, TerminalNode buyNode1 = null, TerminalNode buyNode2 = null)
        {
            var item = new StoreItem(storeItem, price, itemInfo, buyNode1, buyNode2);
            _storeItems.Add(item);
        }

        public readonly struct StoreItem
        {
            public readonly Item item;
            public readonly int price;
            public readonly TerminalNode itemInfo;
            public readonly TerminalNode buyNode1;
            public readonly TerminalNode buyNode2;

            public StoreItem(Item item, int price, TerminalNode itemInfo, TerminalNode buyNode1, TerminalNode buyNode2)
            {
                this.item = item;
                this.price = price;
                this.itemInfo = itemInfo;
                this.buyNode1 = buyNode1;
                this.buyNode2 = buyNode2;
            }
        }
    }
}