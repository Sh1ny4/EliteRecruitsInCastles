using HarmonyLib;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;
using TaleWorlds.CampaignSystem;
using EliteRecruitsInCastles.EliteRecruitsInCastles;

namespace EliteRecruitsInCastles
{
    public class Submodule : MBSubModuleBase
    {
        protected override void OnSubModuleLoad()
        {
            base.OnSubModuleLoad();
            new Harmony("EliteRecruitsInCastles.EliteRecruitsInCastles").PatchAll();
        }

        protected override void InitializeGameStarter(Game game, IGameStarter starterObject)
        {
            if (starterObject is CampaignGameStarter)
            {
                CampaignGameStarter campaignGameStarter = starterObject as CampaignGameStarter;
                campaignGameStarter.AddBehavior(new CastleRecruitMenu());
                campaignGameStarter.AddBehavior(new AiVisitSettlementBehaviorPatch());
            }
        }
    }
}
