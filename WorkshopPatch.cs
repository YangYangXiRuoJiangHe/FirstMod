using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CampaignBehaviors;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.Settlements.Workshops;

namespace FirstMod
{
    [HarmonyPatch(typeof(WorkshopsCampaignBehavior), "RunTownWorkshop")]
    public class WorkshopPatch
    {
        //转译法，替换原函数的某一部分
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var getResultNumber = AccessTools.Method(typeof(ExplainedNumber), "get_ResultNumber");
            var found = false;
            foreach (var instruction in instructions)
            {
                yield return instruction;
                if (instruction.opcode == OpCodes.Call && instruction.operand == (object)getResultNumber)
                {
                    if(found)
                        throw new ArgumentException("Find multiple ExplainedNumber::get_ResultNumber in WorkshopsCampaignBehavior.RunTownWorkshop");
                    yield return new CodeInstruction(OpCodes.Ldarg_2, null);
                    yield return new CodeInstruction(OpCodes.Call, 
                        AccessTools.Method(typeof(ArtisanBeerBehavior), nameof(ArtisanBeerBehavior.WorkshopProductionEfficiency)));
                    yield return new CodeInstruction(OpCodes.Mul, null);
                    //yield return new CodeInstruction(OpCodes.Call, m_MyExtraMethod);
                    found = true;
                }
            }
            if (found is false)
                throw new ArgumentException("Cannot find ExplainedNumber::get_ResultNumber in WorkshopsCampaignBehavior.RunTownWorkshop");
        }

        //前缀法，返回false不执行原函数，返回true会在执行后执行原函数
        /*public static bool Prepare()
        {
            var method = typeof(WorkshopsCampaignBehavior).GetMethod("RunTownWorkshop");
            if (method == null)
            {
                Console.WriteLine("目标方法 RunTownWorkshop 未找到。");
                return false;
            }
            return true;
        }

        public static bool Prefix(Town townComponent, Workshop workshop, WorkshopsCampaignBehavior __instance)
        {
            WorkshopType workshopType = workshop.WorkshopType;
            bool flag = false;
            for (int i = 0; i < workshopType.Productions.Count; i++)
            {
                float num = workshop.GetProductionProgress(i);
                if (num > 1f)
                {
                    num = 1f;
                }
                num += (Campaign.Current.Models.WorkshopModel.GetEffectiveConversionSpeedOfProduction(workshop, workshopType.Productions[i].ConversionSpeed, false).ResultNumber) * ArtisanBeerBehavior.WorkshopProductionEfficiency(workshop);
                if (num >= 1f)
                {
                    bool flag2 = true;
                    while (flag2 && num >= 1f)
                    {
                        //Harmony自带工具的反射调用
                        var tickOneProductionCycleForPlayerWorkshop = AccessTools.Method(typeof(WorkshopsCampaignBehavior), "TickOneProductionCycleForPlayerWorkshop");
                        var tickOneProductionCycleForNotableWorkshop = AccessTools.Method(typeof(WorkshopsCampaignBehavior), "TickOneProductionCycleForNotableWorkshop");
                        flag2 = (bool)((workshop.Owner == Hero.MainHero) ? tickOneProductionCycleForPlayerWorkshop.Invoke(__instance, new object[] { workshopType.Productions[i], workshop }) : tickOneProductionCycleForNotableWorkshop.Invoke(__instance, new object[] { workshopType.Productions[i], workshop }));
                        if (flag2)
                        {
                            flag = true;
                        }
                        num -= 1f;
                    }
                }
                workshop.SetProgress(i, num);
            }
            if (flag)
            {
                workshop.UpdateLastRunTime();
            }
            return false;
        }*/
    }
}
