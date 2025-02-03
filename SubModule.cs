using System;
using TaleWorlds.CampaignSystem.Party;
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
        public override void OnMissionBehaviorInitialize(Mission mission)
        {
            base.OnMissionBehaviorInitialize(mission);
            //传递实例，而不是方法
            mission.AddMissionBehavior(new ArtisanBeerMissionView());
        }
    }
    //继承于MissionView之后使用OnMissionScreenTick（）不管用，这里使用MissionBehavior的OnMissionTick有用。
    public class ArtisanBeerMissionView : MissionBehavior
    {
        public override MissionBehaviorType BehaviorType => MissionBehaviorType.Other;
        public override void OnMissionTick(float dt)
        {
            base.OnMissionTick(dt);
            if (Input.IsKeyPressed(TaleWorlds.InputSystem.InputKey.Q))
            {
                DrinkBeer();
            }
        }
        //通过drinkbeer回复血量
        private void DrinkBeer()
        {
            if (!(Mission.Mode is MissionMode.Battle or MissionMode.Stealth)) return;
            var itemRoster = MobileParty.MainParty.ItemRoster;
            var artisanBeerObject = MBObjectManager.Instance.GetObject<ItemObject>("artisan_beer");
            if (itemRoster.GetItemNumber(artisanBeerObject) <= 0) return;

            itemRoster.AddToCounts(artisanBeerObject, -1);
            var ma = Mission.MainAgent;
            var oldHealth = ma.Health;
            ma.Health += 20;
            if (ma.Health > ma.HealthLimit) ma.Health = ma.HealthLimit;
            InformationManager.DisplayMessage(new InformationMessage(String.Format("We healed! {0} hp",Mission.MainAgent.Health - oldHealth)));

        }
    }
}