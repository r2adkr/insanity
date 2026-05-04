using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;

namespace InsanityMod.Patches
{
    [HarmonyPatch(typeof(StartOfRound))]
    internal static class ItemRegistrationPatcher
    {
        [HarmonyPatch("Awake")]
        [HarmonyPostfix]
        private static void RegisterItems(StartOfRound __instance)
        {
            if (AssetBundleLoader.ValueBreadItem == null ||
                AssetBundleLoader.LucidDoomItem == null) return;

            var list = new List<Item>(__instance.allItemsList.itemsList)
            {
                AssetBundleLoader.ValueBreadItem,
                AssetBundleLoader.LucidDoomItem
            };
            __instance.allItemsList.itemsList = list.ToArray();

            RegisterShopItem(AssetBundleLoader.ValueBreadItem, ModConfig.BreadShopPrice.Value);
            RegisterShopItem(AssetBundleLoader.LucidDoomItem, ModConfig.PotionShopPrice.Value);
        }

        private static void RegisterShopItem(Item item, int price)
        {
            var terminal = Object.FindObjectOfType<Terminal>();
            if (terminal == null) return;

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
