using System.Collections.Generic;
using System;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.Settlements.Workshops;
using TaleWorlds.CampaignSystem.Settlements.Locations;
using TaleWorlds.CampaignSystem.AgentOrigins;
using TaleWorlds.CampaignSystem.Encounters;
using TaleWorlds.Core;


namespace FirstMod
{
    public class ArtisanBeerBehavior : CampaignBehaviorBase
    {
        //注册事件
        public override void RegisterEvents()
        {
            CampaignEvents.WorkshopTypeChangedEvent.AddNonSerializedListener(this, WorkshopTypeChangedEvent);
            CampaignEvents.DailyTickTownEvent.AddNonSerializedListener(this, DailyTick);
            CampaignEvents.LocationCharactersAreReadyToSpawnEvent.AddNonSerializedListener(this, new Action<Dictionary<string, int>>(this.LocationCharactersAreReadyToSpawn));
            CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener(this, OnSessionLaunched);
        }

        private void OnSessionLaunched(CampaignGameStarter starter)
        {
            this.AddDialogs(starter);
        }

        private void AddDialogs(CampaignGameStarter starter)
        {
            starter.AddPlayerLine("tavernkeeper_talk_ask_artisan_beer", "tavernkeeper_talk", "tavernkeeper_artisan_beer", "你这有手工啤酒卖吗?",null,null);
            starter.AddDialogLine("tavernkeeper_talk_artisan_beer_a", "tavernkeeper_artisan_beer", "tavernkeeper_talk", "当然有，但不出售给外人", () =>
            {
                foreach(var workshop in Settlement.CurrentSettlement.Town.Workshops)
                {
                    if (workshop.WorkshopType.StringId == "brewery") return true;
                }
                return false;
            }, null);
            starter.AddDialogLine("tavernkeeper_talk_artisan_beer_b", "tavernkeeper_artisan_beer", "tavernkeeper_talk", "我这全是工业酒精勾兑的，专卖给那些大老粗，向您这样的勇士，我可不敢卖给您", null, null);
        }

        //生成NPC
        private void LocationCharactersAreReadyToSpawn(Dictionary<string, int> unusedUsablePointCount)
        {
            Location locationWithId = Settlement.CurrentSettlement.LocationComplex.GetLocationWithId("center");
            if (!(CampaignMission.Current.Location == locationWithId && CampaignTime.Now.IsDayTime)) return;
            Settlement settlement = PlayerEncounter.LocationEncounter.Settlement;
            CharacterObject shopWorker = settlement.Culture.ShopWorker;
            Monster monsterWithSuffix = TaleWorlds.Core.FaceGen.GetMonsterWithSuffix(shopWorker.Race, "_settlement");
            int minValue;
            int maxValue;
            Campaign.Current.Models.AgeModel.GetAgeLimitForLocation(shopWorker, out minValue, out maxValue, "");
            //获取所有的工作坊
            foreach (Workshop workshop in settlement.Town.Workshops)
            {
                //只在酿酒厂（brewery）中生成新的酿酒工人NPC
                if (workshop.WorkshopType.StringId == "brewery")
                {
                    int num;
                    unusedUsablePointCount.TryGetValue(workshop.Tag, out num);
                    //查看工坊是否还有生成位置
                    if (num > 0f)
                    {
                        CharacterObject caravanMaster = Settlement.CurrentSettlement.Culture.CaravanMaster;
                        LocationCharacter locationCharacter = new LocationCharacter(new AgentData(new 
                            SimpleAgentOrigin(caravanMaster, -1, null, default(UniqueTroopDescriptor))).Monster(monsterWithSuffix).Age(MBRandom.RandomInt(minValue, maxValue)), 
                            new LocationCharacter.AddBehaviorsDelegate(SandBoxManager.Instance.AgentBehaviorManager.AddWandererBehaviors), 
                            workshop.Tag, true, LocationCharacter.CharacterRelations.Neutral, null, true, false, null, false, false, true);
                        locationWithId.AddCharacter(locationCharacter);
                    }
                }
            }
        }

        private void DailyTick(Town town)
        {
            foreach(var workshop in town.Workshops)
            {
                //城市研讨会
                //InformationManager.DisplayMessage(new InformationMessage(string.Format("{0} has a workshop {1}", town.Name, workshop.Name)));
                if(workshop.WorkshopType.StringId == "brewery")
                {
                    //酿酒师的工资
                    workshop.ChangeGold( -TaleWorlds.Library.MathF.Round(workshop.Expense * 0.15));
                }
            }
        }

        private void WorkshopTypeChangedEvent(Workshop workshop)
        {
        }

        //保存行为
        public override void SyncData(IDataStore dataStore)
        {
           
        }
    }

}