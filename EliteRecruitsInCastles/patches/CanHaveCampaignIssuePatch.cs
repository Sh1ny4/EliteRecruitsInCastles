using HarmonyLib;
using TaleWorlds.CampaignSystem;

namespace EliteRecruitsInCastles.patches
{
    [HarmonyPatch(typeof(Hero), nameof(Hero.CanHaveCampaignIssues))]
    internal class CanHaveCampaignIssuePatch
    {
        //Patch needed to prevent castle notables from getting a quest, which causes a crash
        [HarmonyPrefix]
        static bool Prefix(ref Hero __instance, ref bool __result)
        {
            if (__instance.Issue != null || (__instance.IsNotable && __instance.CurrentSettlement.IsCastle))
            {
                __result = false;
                return false;
            }
            __result = true;
            bool result = true;
            CampaignEventDispatcher.Instance.CanHaveCampaignIssues(__instance, ref result);
            return result;
        }
    }
}
