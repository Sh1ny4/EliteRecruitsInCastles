using HarmonyLib;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CampaignBehaviors;
using TaleWorlds.CampaignSystem.Settlements;

namespace EliteRecruitsInCastles.patches
{
    [HarmonyPatch(typeof(NotablesCampaignBehavior), "SpawnNotablesAtGameStart")]
    public class SpawnNotablesAtGameStartPatch
    {
        [HarmonyPostfix]
        static void Postfix()
        {
            foreach (Settlement settlement in Settlement.All)
            {
                if (settlement.IsCastle)
                {
                    int targetNotableCountForSettlement6 = Campaign.Current.Models.NotableSpawnModel.GetTargetNotableCountForSettlement(settlement, Occupation.RuralNotable);
                    for (int l = 0; l < targetNotableCountForSettlement6; l++)
                    {
                        HeroCreator.CreateNotable(Occupation.RuralNotable, settlement);
                    }
                    int targetNotableCountForSettlement7 = Campaign.Current.Models.NotableSpawnModel.GetTargetNotableCountForSettlement(settlement, Occupation.Headman);
                    for (int m = 0; m < targetNotableCountForSettlement7; m++)
                    {
                        HeroCreator.CreateNotable(Occupation.Headman, settlement);
                    }
                }
            }
        }
    }
}
