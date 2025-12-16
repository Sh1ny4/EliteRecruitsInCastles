using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using TaleWorlds.CampaignSystem.CampaignBehaviors.AiBehaviors;
using TaleWorlds.CampaignSystem.Settlements;

namespace EliteRecruitsInCastles.patches
{
    [HarmonyPatch(typeof(AiVisitSettlementBehavior), "AiHourlyTick")]
    public class AIHourlyTickTranspiler
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var GetIsCastle = AccessTools.Method(typeof(Settlement), "get_IsCastle");
            bool found = false;
            var instruction = new List<CodeInstruction>(instructions);
            for (var i = 0; i < instruction.Count; i++)
            {
                if (i == 570 && instruction[i].opcode == OpCodes.Ldloc_S)
                {
                    instruction[i].opcode = OpCodes.Nop;
                }
                if (instruction[i].opcode == OpCodes.Callvirt && (MethodInfo)instruction[i].operand == GetIsCastle && !found)
                {
                    found = true;
                    instruction[i].operand = null;
                    instruction[i].opcode = OpCodes.Ldc_I4_0;
                }
                yield return instruction[i];
            }
            /*
            var instruction = new List<CodeInstruction>(instructions);
            if (instruction[570].opcode == OpCodes.Ldloc_S && instruction[571].opcode == OpCodes.Callvirt && instruction[572].opcode == OpCodes.Brtrue)
            {
                instruction[570].opcode = OpCodes.Nop;
                instruction[571].opcode = OpCodes.Nop;
                instruction[572].opcode = OpCodes.Nop;
            }
            return instruction;
            */
        }
    }
}
