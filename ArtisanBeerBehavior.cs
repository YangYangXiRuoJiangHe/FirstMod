using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.Settlements.Workshops;


namespace FirstMod
{
    public class ArtisanBeerBehavior : CampaignBehaviorBase
    {
        //注册事件
        public override void RegisterEvents()
        {
            CampaignEvents.WorkshopTypeChangedEvent.AddNonSerializedListener(this, WorkshopTypeChangedEvent);
            CampaignEvents.DailyTickTownEvent.AddNonSerializedListener(this, DailyTick);
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