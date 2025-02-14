using System;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.ObjectSystem;


namespace FirstMod
{
    public partial class ArtisanBeerMissionBehavior : MissionBehavior
    {
        int _soundIndex;
        public override void AfterStart()
        {
            base.AfterStart();
            _soundIndex = SoundEvent.GetEventIdFromString("artisanbeer/drink");

        }
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
            InformationManager.DisplayMessage(new InformationMessage(String.Format("We healed! {0} hp", Mission.MainAgent.Health - oldHealth)));

            SoundEvent eventRef = SoundEvent.CreateEvent(_soundIndex, Mission.Scene);
            eventRef.SetPosition(Mission.MainAgent.Position);
            eventRef.Play();

            //Mission.MakeSound(_soundIndex, Vec3.Zero, false, true, -1, -1);//plays oneshot sound at position with given parameters.

            //SoundEvent.PlaySound2D(_soundIndex);

        }
    }
}