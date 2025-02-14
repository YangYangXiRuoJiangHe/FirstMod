using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Core;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.GauntletUI.Data;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View.MissionViews;
using TaleWorlds.ObjectSystem;

namespace FirstMod
{
    public partial class ArtisanBeerMissionView : MissionView
    {
        GauntletLayer _layer;
        IGauntletMovie _movie;
        ArtisanBeerMissionVM _dataSource;

        public override void OnMissionScreenInitialize()
        {
            base.OnMissionScreenInitialize();
            _dataSource = new ArtisanBeerMissionVM(Mission); // 提前初始化数据源
            if (_layer == null && MissionScreen != null) // 确保只执行一次
            {
                _layer = new GauntletLayer(1, "GauntletLayer", false);
                _movie = _layer.LoadMovie("ArtisanBeerHUD", _dataSource);
                base.MissionScreen.AddLayer(_layer);
            }

        }
        public override void OnMissionScreenFinalize()
        {
            base.OnMissionScreenFinalize();
            base.MissionScreen.RemoveLayer(_layer);
            _layer = null;
            _movie = null;
            _dataSource = null;
        }
        public override void OnMissionModeChange(MissionMode oldMissionMode, bool atStart)
        {
            base.OnMissionModeChange(oldMissionMode, atStart);
            _dataSource?.OnMissionModeChanged(Mission);
        }
    }

    public class ArtisanBeerMissionVM : ViewModel
    {
        Mission _mission;

        public ArtisanBeerMissionVM(Mission mission)
        {
            _mission = mission;
            var itemRoster = MobileParty.MainParty.ItemRoster;
            var artisanBeerObject = MBObjectManager.Instance.GetObject<ItemObject>("artisan_beer");
            BeerAmount = itemRoster.GetItemNumber(artisanBeerObject);


            OnMissionModeChanged(mission);
        }
        public void OnMissionModeChanged(Mission mission)
        {
            IsVisable = mission.Mode is MissionMode.Battle or MissionMode.Stealth;
        }
        int _beerAmount;
        [DataSourceProperty]
        public int BeerAmount
        {
            get
            {
                return this._beerAmount;
            }
            set
            {
                if (value != this._beerAmount)
                {
                    this._beerAmount = value;
                    base.OnPropertyChangedWithValue(value, "BeerAmount");
                }
            }
        }
        bool _isVisable;
        [DataSourceProperty]
        public bool IsVisable

        {
            get
            {
                return this._isVisable;
            }
            set
            {
                if (value != this._isVisable)
                {
                    this._isVisable = value;
                    base.OnPropertyChangedWithValue(value, "IsVisable");
                }
            }
        }
    }

}