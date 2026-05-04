using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;

namespace InsanityMod.Patches
{
    [HarmonyPatch(typeof(StartOfRound))]
    internal static class ItemRegistrationPatcher
    {
        private static bool _itemsRegistered;

        [HarmonyPatch("Awake")]
        [HarmonyPostfix]
        private static void RegisterItems(StartOfRound __instance)
        {
            if (_itemsRegistered) return;
            if (AssetBundleLoader.ValueBreadItem == null ||
                AssetBundleLoader.LucidDoomItem == null) return;

            var list = new List<Item>(__instance.allItemsList.itemsList)
            {
                AssetBundleLoader.ValueBreadItem,
                AssetBundleLoader.LucidDoomItem
            };
            __instance.allItemsList.itemsList = list;
            _itemsRegistered = true;
        }
    }

    [HarmonyPatch(typeof(Terminal))]
    internal static class TerminalShopPatcher
    {
        private static bool _shopRegistered;

        [HarmonyPatch("Awake")]
        [HarmonyPostfix]
        private static void RegisterShop(Terminal __instance)
        {
            if (_shopRegistered) return;
            if (AssetBundleLoader.ValueBreadItem == null ||
                AssetBundleLoader.LucidDoomItem == null) return;

            RegisterShopItem(__instance, AssetBundleLoader.ValueBreadItem, ModConfig.BreadShopPrice.Value);
            RegisterShopItem(__instance, AssetBundleLoader.LucidDoomItem, ModConfig.PotionShopPrice.Value);
            _shopRegistered = true;
        }

        private static void RegisterShopItem(Terminal terminal, Item item, int price)
        {
            var buyNode = ScriptableObject.CreateInstance<TerminalNode>();
            buyNode.displayText = $"구매: {item.itemName}\n가격: {price} 크레딧.\n\n[CONFIRM]";
            buyNode.clearPreviousText = true;
            buyNode.buyItemIndex = item.itemId;

            var keyword = ScriptableObject.CreateInstance<TerminalKeyword>();
            keyword.word = item.itemName.ToLower().Replace(" ", "");
            keyword.specialKeywordResult = buyNode;

            var keywordList = new List<TerminalKeyword>(terminal.terminalNodes.allKeywords) { keyword };
            terminal.terminalNodes.allKeywords = keywordList.ToArray();
        }
    }
}
