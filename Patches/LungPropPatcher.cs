using HarmonyLib;
using InsanityMod.Managers;

namespace InsanityMod.Patches
{
    [HarmonyPatch(typeof(LungProp))]
    internal static class LungPropPatcher
    {
        [HarmonyPatch("EquipItem")]
        [HarmonyPrefix]
        private static void EquipItemPrefix(LungProp __instance)
        {
            if (__instance.isLungDocked)
                InsanityManager.OnApparatusRemoved();
        }
    }
}
