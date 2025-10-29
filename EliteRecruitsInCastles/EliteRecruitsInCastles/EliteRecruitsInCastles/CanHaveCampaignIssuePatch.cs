using HarmonyLib;
using TaleWorlds.CampaignSystem;

namespace EliteRecruitsInCastles.EliteRecruitsInCastles
{
    [HarmonyPatch(typeof(Hero), nameof(Hero.CanHaveCampaignIssues))]
    internal class CanHaveCampaignIssuePatch
    {
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
