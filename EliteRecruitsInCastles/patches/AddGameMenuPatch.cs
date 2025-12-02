using HarmonyLib;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CampaignBehaviors;
using TaleWorlds.CampaignSystem.GameMenus;

namespace EliteRecruitsInCastles.patches
{
    [HarmonyPatch(typeof(PlayerTownVisitCampaignBehavior), "AddGameMenus")]
    internal class AddGameMenuPatch
    {
        [HarmonyPostfix]
        public static void Postfix(ref CampaignGameStarter campaignGameSystemStarter)
        {
            campaignGameSystemStarter.AddGameMenuOption("castle", "recruit_volunteers", "{=E31IJyqs}Recruit troops", new GameMenuOption.OnConditionDelegate(game_menu_recruit_castle_volunteers_on_condition), new GameMenuOption.OnConsequenceDelegate(game_menu_recruit_castle_volunteers_on_consequence), false, 4, false, null);
        }

        public static bool game_menu_recruit_castle_volunteers_on_condition(MenuCallbackArgs args)
        {
            args.optionLeaveType = GameMenuOption.LeaveType.Recruit;
            return true;
        }

        public static void game_menu_recruit_castle_volunteers_on_consequence(MenuCallbackArgs args)
        {
            args.MenuContext.OpenRecruitVolunteers();
        }
    }
}
