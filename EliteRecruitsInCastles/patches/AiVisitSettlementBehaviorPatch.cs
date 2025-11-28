using HarmonyLib;
using Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CampaignBehaviors;
using TaleWorlds.CampaignSystem.CampaignBehaviors.AiBehaviors;
using TaleWorlds.CampaignSystem.Naval;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Library;
using TaleWorlds.LinQuick;

namespace EliteRecruitsInCastles.patches
{
    [HarmonyPatch(typeof(AiVisitSettlementBehavior), "AiHourlyTick")]
    internal class AiVisitSettlementBehaviorPatch : AiVisitSettlementBehavior
    {
        [HarmonyPrefix]
        static bool Prefix(ref AiVisitSettlementBehavior __instance, MobileParty mobileParty, PartyThinkParams p)
        {

            var CalculateVisitHideoutScoresForBanditPartyAccess = AccessTools.Method(typeof(AiVisitSettlementBehavior), "CalculateVisitHideoutScoresForBanditParty");
            var CalculatePartyParametersAccess = AccessTools.Method(typeof(AiVisitSettlementBehavior), "CalculatePartyParameters");
            var CalculateSellItemScorePatch = AccessTools.Method(typeof(AiVisitSettlementBehavior), "CalculateSellItemScore");
            var FillSettlementsToVisitWithDistancesAsDaysPatch = AccessTools.Method(typeof(AiVisitSettlementBehavior), "FillSettlementsToVisitWithDistancesAsDays");
            var CalculateMergeScoreForLeaderlessPartyPatch = AccessTools.Method(typeof(AiVisitSettlementBehavior), "CalculateMergeScoreForLeaderlessParty");
            var AddBehaviorTupleWithScorePatch = AccessTools.Method(typeof(AiVisitSettlementBehavior), "AddBehaviorTupleWithScore");
            var GetApproximateVolunteersCanBeRecruitedDataFromSettlementPatch = AccessTools.Method(typeof(AiVisitSettlementBehavior), "GetApproximateVolunteersCanBeRecruitedDataFromSettlement");
            var MaximumMeaningfulDistanceAsDaysPatch = AccessTools.Method(typeof(AiVisitSettlementBehavior), "MaximumMeaningfulDistanceAsDays");
            var CalculateBeingSettlementOwnerScoresPatch = AccessTools.Method(typeof(AiVisitSettlementBehavior), "CalculateBeingSettlementOwnerScores");
            var CalculateMergeScoreForDisbandingPartyPatch = AccessTools.Method(typeof(AiVisitSettlementBehavior), "CalculateMergeScoreForDisbandingParty");
            var _disbandPartyCampaignBehaviorPatch = AccessTools.Field(typeof(AiVisitSettlementBehavior), "_disbandPartyCampaignBehavior");

            Settlement currentSettlement = mobileParty.CurrentSettlement;
            if (((currentSettlement != null) ? currentSettlement.SiegeEvent : null) != null)
            {
                return false;
            }
            Settlement currentSettlementOfMobilePartyForAICalculation = MobilePartyHelper.GetCurrentSettlementOfMobilePartyForAICalculation(mobileParty);
            if (mobileParty.IsBandit)
            {
                CalculateVisitHideoutScoresForBanditPartyAccess.Invoke(__instance, new object[] { mobileParty, currentSettlementOfMobilePartyForAICalculation, p });
                return false;
            }
            IFaction mapFaction = mobileParty.MapFaction;
            if (mobileParty.IsMilitia || mobileParty.IsCaravan || mobileParty.IsPatrolParty || mobileParty.IsVillager || (!mapFaction.IsMinorFaction && !mapFaction.IsKingdomFaction && (mobileParty.LeaderHero == null || !mobileParty.LeaderHero.IsLord)))
            {
                return false;
            }
            if (mobileParty.Army == null || mobileParty.AttachedTo == null || mobileParty.Army.LeaderParty == mobileParty)
            {
                Hero leaderHero = mobileParty.LeaderHero;
                ValueTuple<float, float, int, int> valueTuple = (ValueTuple<float, float, int, int>)CalculatePartyParametersAccess.Invoke(__instance, new object[] { mobileParty });
                float item = valueTuple.Item1;
                float item2 = valueTuple.Item2;
                int item3 = valueTuple.Item3;
                int item4 = valueTuple.Item4;
                float num = item2 / Math.Min(1f, Math.Max(0.1f, item));
                float num2 = (num >= 1f) ? 0.33f : ((MathF.Max(1f, MathF.Min(2f, num)) - 0.5f) / 1.5f);
                float num3 = mobileParty.Food;
                float num4 = -mobileParty.FoodChange;
                int num5 = mobileParty.PartyTradeGold;
                if (mobileParty.Army != null && mobileParty == mobileParty.Army.LeaderParty)
                {
                    foreach (MobileParty mobileParty2 in mobileParty.Army.LeaderParty.AttachedParties)
                    {
                        num3 += mobileParty2.Food;
                        num4 += -mobileParty2.FoodChange;
                        num5 += mobileParty2.PartyTradeGold;
                    }
                }
                float num6 = 1f;
                if (leaderHero != null && mobileParty.IsLordParty)
                {
                    num6 = (float)CalculateSellItemScorePatch.Invoke(__instance, new object[] { mobileParty });
                }
                int num7 = mobileParty.Party.PrisonerSizeLimit;
                if (mobileParty.Army != null)
                {
                    foreach (MobileParty mobileParty3 in mobileParty.Army.LeaderParty.AttachedParties)
                    {
                        num7 += mobileParty3.Party.PrisonerSizeLimit;
                    }
                }
                //
                SortedDictionary<ValueTuple<float, int>, ValueTuple<Settlement, MobileParty.NavigationType, bool, bool>> _settlementsWithDistances = new SortedDictionary<ValueTuple<float, int>, ValueTuple<Settlement, MobileParty.NavigationType, bool, bool>>();
                FillSettlementsToVisitWithDistancesAsDaysPatch.Invoke(__instance, new object[] { mobileParty, _settlementsWithDistances });
                float num8 = PartyBaseHelper.FindPartySizeNormalLimit(mobileParty);
                float num9 = 2000f;
                float num10 = 2000f;
                if (leaderHero != null)
                {
                    num9 = HeroHelper.StartRecruitingMoneyLimitForClanLeader(leaderHero);
                    num10 = HeroHelper.StartRecruitingMoneyLimit(leaderHero);
                }
                float num11 = 0.2f;
                float num12 = 1f;
                foreach (KeyValuePair<ValueTuple<float, int>, ValueTuple<Settlement, MobileParty.NavigationType, bool, bool>> keyValuePair in _settlementsWithDistances)
                {
                    Settlement item5 = keyValuePair.Value.Item1;
                    MobileParty.NavigationType item6 = keyValuePair.Value.Item2;
                    float item7 = keyValuePair.Key.Item1;
                    bool item8 = keyValuePair.Value.Item3;
                    bool item9 = keyValuePair.Value.Item4;
                    float num13 = 1.6f;
                    if (mobileParty.IsDisbanding)
                    {
                        goto IL_2F4;
                    }
                    IDisbandPartyCampaignBehavior disbandPartyCampaignBehavior = _disbandPartyCampaignBehaviorPatch.GetValue(__instance) as IDisbandPartyCampaignBehavior;
                    if (disbandPartyCampaignBehavior != null && disbandPartyCampaignBehavior.IsPartyWaitingForDisband(mobileParty))
                    {
                        goto IL_2F4;
                    }
                    if (leaderHero == null)
                    {
                        bool newFlag = false;
                        object[] mergeArgs = new object[] { mobileParty, item5, item7, newFlag };
                        float visitingNearbySettlementScore = (float)CalculateMergeScoreForLeaderlessPartyPatch.Invoke(__instance, mergeArgs);
                        newFlag = (bool)mergeArgs[3];
                        if (newFlag)
                        {
                            AddBehaviorTupleWithScorePatch.Invoke(__instance, new object[] { p, item5, visitingNearbySettlementScore, item6, item8, item9 });
                        }
                    }
                    else
                    {
                        if (item7 >= (float)MaximumMeaningfulDistanceAsDaysPatch.Invoke(__instance, new object[] { item6 }))
                        {
                            AddBehaviorTupleWithScorePatch.Invoke(__instance, new object[] { p, item5, 0.025f, item6, item8, item9 });
                            continue;
                        }
                        float num14 = MathF.Max(num11, item7);
                        float num15 = 1f;
                        if (item7 > num11)
                        {
                            num15 = num12 / (num12 - num11 + item7);
                        }
                        float num16 = num15;
                        if (item < 0.6f)
                        {
                            num16 = MathF.Pow(num15, MathF.Pow(0.6f / MathF.Max(0.15f, item), 0.3f));
                        }
                        float num17 = 1f;
                        float num18 = (float)item3 / (float)item4;
                        bool flag2 = mobileParty.Army != null && mobileParty.AttachedTo == null && mobileParty.Army.LeaderParty != mobileParty;
                        if (item5.IsFortification && num18 > 0.2f)
                        {
                            num17 = MBMath.Map(num18 - 0.2f, 0f, 0.8f, 1f, 5f);
                            if (flag2 || mobileParty.MapEvent != null || mobileParty.SiegeEvent != null)
                            {
                                num17 *= 0.6f;
                            }
                        }
                        float num19 = 1f;
                        if (mobileParty.DefaultBehavior == AiBehavior.GoToSettlement && ((item5 == currentSettlementOfMobilePartyForAICalculation && currentSettlementOfMobilePartyForAICalculation.IsFortification) || (currentSettlementOfMobilePartyForAICalculation == null && item5 == mobileParty.TargetSettlement)))
                        {
                            num19 = 1.2f;
                        }
                        else if (currentSettlementOfMobilePartyForAICalculation == null && item5 == mobileParty.LastVisitedSettlement)
                        {
                            num19 = 0.8f;
                        }
                        float num20 = (num18 > 0.2f) ? 1f : 0.16f;
                        float num21 = Math.Max(0f, num3) / num4;
                        if (num4 > 0f && (mobileParty.BesiegedSettlement == null || num21 <= 1f) && num5 > 100 && (item5.IsTown || (item5.IsVillage && mobileParty.Army == null)))
                        {
                            float neededFoodsInDaysThresholdForSiege = Campaign.Current.Models.MobilePartyAIModel.NeededFoodsInDaysThresholdForSiege;
                            if (num21 < neededFoodsInDaysThresholdForSiege)
                            {
                                float num22 = (float)((int)(num4 * ((num21 < 1f && item5.IsVillage) ? Campaign.Current.Models.PartyFoodBuyingModel.MinimumDaysFoodToLastWhileBuyingFoodFromVillage : Campaign.Current.Models.PartyFoodBuyingModel.MinimumDaysFoodToLastWhileBuyingFoodFromTown)) + 1);
                                float num23 = neededFoodsInDaysThresholdForSiege * 0.5f;
                                float num24 = num23 - Math.Min(num23, Math.Max(0f, num21 - 1f));
                                float num25 = num22 + 20f * (float)(item5.IsTown ? 2 : 1) * ((num14 > num12) ? 1f : (num14 / num12));
                                int val = (int)((float)(num5 - 100) / Campaign.Current.Models.PartyFoodBuyingModel.LowCostFoodPriceAverage);
                                num20 += num24 * num24 * 0.093f * ((num21 < num23) ? (15f + 0.5f * (num23 - num21)) : 1f) * Math.Min(num25, (float)Math.Min(val, item5.ItemRoster.TotalFood)) / num25;
                            }
                        }
                        float num26 = 0f;
                        float num27 = 1f;
                        if (item < 1f && mobileParty.GetAvailableWageBudget() > 0)
                        {
                            int num28 = item5.NumberOfLordPartiesAt;
                            int num29 = item5.NumberOfLordPartiesTargeting;
                            if (currentSettlementOfMobilePartyForAICalculation == item5)
                            {
                                int num30 = num28;
                                Army army = mobileParty.Army;
                                num28 = num30 - ((army != null) ? army.LeaderPartyAndAttachedPartiesCount : 1);
                                if (num28 < 0)
                                {
                                    num28 = 0;
                                }
                            }
                            if (mobileParty.TargetSettlement == item5 || (mobileParty.Army != null && mobileParty.Army.LeaderParty.TargetSettlement == item5))
                            {
                                int num31 = num29;
                                Army army2 = mobileParty.Army;
                                num29 = num31 - ((army2 != null) ? army2.LeaderPartyAndAttachedPartiesCount : 1);
                                if (num29 < 0)
                                {
                                    num29 = 0;
                                }
                            }
                            if (mobileParty.Army != null)
                            {
                                num29 += mobileParty.Army.LeaderPartyAndAttachedPartiesCount;
                            }
                            if (!mobileParty.Party.IsStarving && (float)mobileParty.PartyTradeGold > num10 && (leaderHero.Clan.Leader == leaderHero || (float)leaderHero.Clan.Gold > num9) && num8 > mobileParty.PartySizeRatio)
                            {                                
                                ValueTuple<int, float> approximateVolunteersCanBeRecruitedDataFromSettlement = (ValueTuple<int, float>)GetApproximateVolunteersCanBeRecruitedDataFromSettlementPatch.Invoke(__instance, new object[] { leaderHero, item5 });
                                num26 = (float)approximateVolunteersCanBeRecruitedDataFromSettlement.Item1;
                                if (num26 > 0f)
                                {
                                    float item10 = approximateVolunteersCanBeRecruitedDataFromSettlement.Item2;
                                    num26 = Math.Min(num26, (float)MathF.Floor((float)mobileParty.GetAvailableWageBudget() / item10));
                                }
                            }
                            float num32 = num26 * num15 / MathF.Sqrt((float)(1 + num28 + num29));
                            float num33 = (num32 < 1f) ? num32 : ((float)Math.Pow((double)num32, (double)num2));
                            num27 = Math.Max(Math.Min(1f, num20), Math.Max((mapFaction == item5.MapFaction) ? 0.25f : 0.16f, num * Math.Max(1f, Math.Min(2f, num)) * num33 * (1f - 0.9f * num18) * (1f - 0.9f * num18)));
                        }
                        num13 *= num27 * num17 * num20 * num16;
                        if (num13 >= 8f)
                        {
                            AddBehaviorTupleWithScorePatch.Invoke(__instance, new object[] { p, item5, num13, item6, item8, item9 });
                            break;
                        }
                        float num34 = 1f;
                        if (num26 > 0f && !flag2)
                        {
                            num34 = 1f + ((mobileParty.DefaultBehavior == AiBehavior.GoToSettlement && item5 != currentSettlementOfMobilePartyForAICalculation && num14 < num11) ? (0.1f * MathF.Min(5f, num26) - 0.1f * MathF.Min(5f, num26) * (num14 / num11) * (num14 / num11)) : 0f);
                        }
                        float num35 = (item5.IsCastle && !flag2 && num20 < 1f) ? 1.4f : 1f;
                        num13 *= (item5.IsTown ? num6 : 1f) * num34 * num35;
                        if (num13 >= 8f)
                        {
                            AddBehaviorTupleWithScorePatch.Invoke(__instance, new object[] { p, item5, num13, item6, item8, item9 });
                            break;
                        }
                        int num36 = mobileParty.PrisonRoster.TotalRegulars;
                        if (mobileParty.PrisonRoster.TotalHeroes > 0)
                        {
                            foreach (TroopRosterElement troopRosterElement in mobileParty.PrisonRoster.GetTroopRoster())
                            {
                                if (troopRosterElement.Character.IsHero && troopRosterElement.Character.HeroObject.Clan.IsAtWarWith(item5.MapFaction))
                                {
                                    num36 += 6;
                                }
                            }
                        }
                        float num37 = 1f;
                        float num38 = 1f;
                        if (mobileParty.Army != null && mobileParty.Army.LeaderParty.AttachedParties.Contains(mobileParty))
                        {
                            if (mobileParty.Army.LeaderParty != mobileParty)
                            {
                                num37 = ((float)mobileParty.Army.CohesionThresholdForDispersion - mobileParty.Army.Cohesion) / (float)mobileParty.Army.CohesionThresholdForDispersion;
                            }
                            num38 = ((MobileParty.MainParty != null && mobileParty.Army == MobileParty.MainParty.Army) ? 0.6f : 0.8f);
                            foreach (MobileParty mobileParty4 in mobileParty.Army.LeaderParty.AttachedParties)
                            {
                                num36 += mobileParty4.PrisonRoster.TotalRegulars;
                                if (mobileParty4.PrisonRoster.TotalHeroes > 0)
                                {
                                    foreach (TroopRosterElement troopRosterElement2 in mobileParty4.PrisonRoster.GetTroopRoster())
                                    {
                                        if (troopRosterElement2.Character.IsHero && troopRosterElement2.Character.HeroObject.Clan.IsAtWarWith(item5.MapFaction))
                                        {
                                            num36 += 6;
                                        }
                                    }
                                }
                            }
                        }
                        float num39 = item5.IsFortification ? (1f + 2f * (float)(num36 / num7)) : 1f;
                        float num40 = (mobileParty.DesiredAiNavigationType == item6) ? 1.5f : 1f;
                        float num41 = 1f;
                        float num42 = 1f;
                        float num43 = 1f;
                        float num44 = 1f;
                        float num45 = 1f;
                        if (num20 <= 0.5f)
                        {
                            ValueTuple<float, float, float, float> valueTuple2 = (ValueTuple<float, float, float, float>)CalculateBeingSettlementOwnerScoresPatch.Invoke(__instance, new object[] { mobileParty, item5, currentSettlementOfMobilePartyForAICalculation, -1f, num15, item });
                            num41 = valueTuple2.Item1;
                            num42 = valueTuple2.Item2;
                            num43 = valueTuple2.Item3;
                            num44 = valueTuple2.Item4;
                        }
                        float num46 = 1f;
                        if (item5.HasPort && mobileParty.Ships.Any<Ship>())
                        {
                            float num47 = mobileParty.Ships.AverageQ((Ship x) => x.HitPoints / x.MaxHitPoints);
                            if (num47 < 0.8f)
                            {
                                if (num47 > 0.6f)
                                {
                                    num46 = 1.5f;
                                }
                                else if (num47 > 0.4f)
                                {
                                    num46 = 1.75f;
                                }
                                else
                                {
                                    num46 = 3f;
                                }
                            }
                        }
                        num13 *= num45 * num19 * num37 * num39 * num38 * num41 * num43 * num42 * num44 * num40 * num46;
                    }
                IL_C14:
                    if (num13 > 0.025f)
                    {
                        AddBehaviorTupleWithScorePatch.Invoke(__instance, new object[] { p, item5, num13, item6, item8, item9 });
                        continue;
                    }
                    continue;
                IL_2F4:
                    float visitingNearbySettlementScore2 = (float)CalculateMergeScoreForDisbandingPartyPatch.Invoke(__instance, new object[] { mobileParty, item5, item7 });
                    AddBehaviorTupleWithScorePatch.Invoke(__instance, new object[] { p, item5, visitingNearbySettlementScore2, item6, item8, item9 });
                    goto IL_C14;
                }
            }
            return false;
        }
    }
}
