using HarmonyLib;
using TaleWorlds.MountAndBlade;

namespace EliteRecruitsInCastles
{
    public class SubModule : MBSubModuleBase
    {
        protected override void OnSubModuleLoad()
        {
            base.OnSubModuleLoad();
            new Harmony("EliteRecruitsInCastles.EliteRecruitsInCastles").PatchAll();
        }
    }
}
