using HarmonyLib;
using System;
using System.Reflection;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CampaignBehaviors;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.Settlements.Workshops;

namespace FirstMod
{
    [HarmonyPatch(typeof(WorkshopsCampaignBehavior), "RunTownWorkshop")]
    public class WorkshopPatch
    {
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
                num += (Campaign.Current.Models.WorkshopModel.GetEffectiveConversionSpeedOfProduction(workshop, workshopType.Productions[i].ConversionSpeed, false).ResultNumber) * 0.1f;
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
        }
        //自有方法反射调用
        //static bool Prefix(Town townComponent, Workshop workshop)
        //{
        //    WorkshopType workshopType = workshop.WorkshopType;
        //    bool flag = false;
        //    for (int i = 0; i < workshopType.Productions.Count; i++)
        //    {
        //        float num = workshop.GetProductionProgress(i);
        //        if (num > 1f)
        //        {
        //            num = 1f;
        //        }
        //        num += Campaign.Current.Models.WorkshopModel.GetEffectiveConversionSpeedOfProduction(workshop, workshopType.Productions[i].ConversionSpeed, false).ResultNumber;
        //        if (num >= 1f)
        //        {
        //            bool flag2 = true;
        //            while (flag2 && num >= 1f)
        //            {
        //                // 假设SomeClass是包含所需方法的类
        //                WorkshopsCampaignBehavior behaviorInstance = Campaign.Current.GetCampaignBehavior<WorkshopsCampaignBehavior>();
        //                Type someClassType = typeof(WorkshopsCampaignBehavior);
        //                string methodName = workshop.Owner == Hero.MainHero ? "TickOneProductionCycleForPlayerWorkshop" : "TickOneProductionCycleForNotableWorkshop";
        //                MethodInfo methodInfo = someClassType.GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance, null, new[] { typeof(Production), typeof(Workshop) }, null);

        //                if (methodInfo != null)
        //                {
        //                    // 准备参数
        //                    var parameters = new object[] { workshopType.Productions[i], workshop };

        //                    // 调用方法并传递参数
        //                    flag2 = (bool)methodInfo.Invoke(behaviorInstance, parameters);
        //                }

        //                if (flag2)
        //                {
        //                    flag = true;
        //                }
        //                num -= 1f;
        //            }
        //        }
        //        workshop.SetProgress(i, num);
        //    }
        //    if (flag)
        //    {
        //        workshop.UpdateLastRunTime();
        //    }
        //    return false;
        //}
    }
}
