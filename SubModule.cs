using System;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Encyclopedia;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.Settlements.Workshops;
using TaleWorlds.Core;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View.MissionViews;
using TaleWorlds.ObjectSystem;


namespace FirstMod
{
    public class SubModule : MBSubModuleBase
    {
        //模组被加载时会被调用。可以在这里初始化模组资源、添加事件监听器等。
        protected override void OnSubModuleLoad()
        {
            base.OnSubModuleLoad();

        }
        //任务行为初始化
        public override void OnMissionBehaviorInitialize(Mission mission)
        {
            base.OnMissionBehaviorInitialize(mission);
            //传递实例，而不是方法
            mission.AddMissionBehavior(new ArtisanBeerMissionView());
        }
        //游戏启动初始化
        protected override void InitializeGameStarter(Game game, IGameStarter starterObject)
        {
            if(starterObject is CampaignGameStarter starter)
            {
                starter.AddBehavior(new ArtisanBeerBehavior());
            }
        }
    }

}