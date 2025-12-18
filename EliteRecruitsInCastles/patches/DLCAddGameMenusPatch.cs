using HarmonyLib;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameMenus;

namespace EliteRecruitsInCastles.patches
{
    [HarmonyPatch(typeof(NavalTransitionCampaignBehavior), "AddGameMenus")]
    internal class DLCAddGameMenusPatch
    {
        //will implement later, file is excluded in project
        [HarmonyPostfix]
        public static void Postfix(CampaignGameStarter campaignGameStarter)
        {
            campaignGameStarter.AddGameMenuOption("port_menu", "recruit_volunteers", "{=E31IJyqs}Recruit troops", new GameMenuOption.OnConditionDelegate(game_menu_recruit_port_volunteers_on_condition), new GameMenuOption.OnConsequenceDelegate(game_menu_recruit_port_volunteers_on_consequence), false, 3, false, null);
        }

        public static bool game_menu_recruit_port_volunteers_on_condition(MenuCallbackArgs args)
        {
            args.optionLeaveType = GameMenuOption.LeaveType.Recruit;
            return true;
        }

        public static void game_menu_recruit_port_volunteers_on_consequence(MenuCallbackArgs args)
        {
            args.MenuContext.OpenRecruitVolunteers();
        }
    }
}
