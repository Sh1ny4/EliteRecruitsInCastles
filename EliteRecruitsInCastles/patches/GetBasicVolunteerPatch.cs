using HarmonyLib;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;

namespace EliteRecruitsInCastles.patches
{
    [HarmonyPatch(typeof(DefaultVolunteerModel), nameof(DefaultVolunteerModel.GetBasicVolunteer))]
    public class GetBasicVolunteerPatch
    {
        [HarmonyPrefix]
        static bool Prefix(ref CharacterObject __result, Hero sellerHero)
        {
            Settlement settlement = sellerHero.CurrentSettlement;
            // Castles recruit are elite troops
            if (settlement.IsCastle)
            {
                __result = sellerHero.Culture.EliteBasicTroop;
                return false;
            }
            // Towns can have a custom troop tree, basic troop name has to be <culture ID>_town_recruit, default to regular basic troop if no corresponding NPC can be found
            else if (settlement.IsTown)
            {
                string text = string.Concat(new object[] { sellerHero.Culture.StringId, "_town_recruit" });
                __result = (Game.Current.ObjectManager.GetObject<CharacterObject>(text) ?? sellerHero.Culture.BasicTroop);
                return false;
            }
            // Fishing Villages and ports can have a custom troop tree for marine troops, basic troop name has to be <culture ID>_marine_recruit, default to regular basic troop if no corresponding NPC can be found
            else if ((settlement.Village.VillageType == DefaultVillageTypes.Fisherman) || (settlement.IsTown && settlement.HasPort))
            {
                string text = string.Concat(new object[] { sellerHero.Culture.StringId, "_marine_recruit" });
                __result = (Game.Current.ObjectManager.GetObject<CharacterObject>(text) ?? sellerHero.Culture.BasicTroop);
                return false;
            }
            __result = sellerHero.Culture.BasicTroop;
            return false;
        }
    }
}
