using System;
using TaleWorlds.CampaignSystem.Settlements.Workshops;
using TaleWorlds.Core;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade.View.Screens;
using TaleWorlds.ScreenSystem;

namespace FirstMod
{
    public class ArtisanWorkshopManagementState : GameState
    {
        public Workshop workshop;
        public ArtisanWorkshopState artisanWorkshop;
        public ArtisanWorkshopManagementState(Workshop workshop, ArtisanWorkshopState artisanWorkshop)
        {
            this.workshop = workshop;
            this.artisanWorkshop = artisanWorkshop;
        }
        public ArtisanWorkshopManagementState() { throw new ArgumentException(); }
    }
    [GameStateScreen(typeof(ArtisanWorkshopManagementState))]
    public class WorkshopManagementScreen : ScreenBase, IGameStateListener
    {
        ArtisanWorkshopManagementState _artisanWorkshopManagementState;
        public WorkshopManagementScreen(ArtisanWorkshopManagementState artisanWorkshopManagementState)
        {
            _artisanWorkshopManagementState = artisanWorkshopManagementState;
        }
        GauntletLayer _layer;
        WorkshopManagementVM _dataSoruce;
        void IGameStateListener.OnActivate()
        {
            _layer = new GauntletLayer(1,"GauntletLayer",true);
            _dataSoruce = new WorkshopManagementVM(_artisanWorkshopManagementState);
            _layer.LoadMovie("WorkshopManagement",_dataSoruce);
            _layer.InputRestrictions.SetInputRestrictions(true,InputUsageMask.All);
            _layer.IsFocusLayer = true;
            ScreenManager.TrySetFocus(_layer);
            AddLayer(_layer);
        }

        void IGameStateListener.OnDeactivate()
        {
            _layer.InputRestrictions.ResetInputRestrictions();
            RemoveLayer(_layer);
            _dataSoruce = null;
        }
        protected override void OnFrameTick(float dt)
        {
            base.OnFrameTick(dt);
            if(_layer.Input.IsKeyReleased(TaleWorlds.InputSystem.InputKey.Escape))
            {
                _dataSoruce.ExecuteCancel();
            }
            if(_layer.Input.IsKeyReleased(TaleWorlds.InputSystem.InputKey.Enter))
            {
                _dataSoruce.ExecuteDone();
            }
        }

        void IGameStateListener.OnFinalize()
        {
        }

        void IGameStateListener.OnInitialize()
        {
        }
    }

    public class WorkshopManagementVM : ViewModel
    {
        ArtisanWorkshopManagementState _artisanWorkshopManagementState;

        public WorkshopManagementVM(ArtisanWorkshopManagementState artisanWorkshopManagementState)
        {
            this._artisanWorkshopManagementState = artisanWorkshopManagementState;
            _artisanBeerProduction = _artisanWorkshopManagementState.artisanWorkshop.dailyProductionAmount;
            UpdateRegularBeerProductionLaber();
        }

        void UpdateRegularBeerProductionLaber()
        {
            int effiency = 120 - _artisanBeerProduction * 20;
            RegularBeerProduction = String.Format("{0}%",effiency);
        }

        int _artisanBeerProduction;
        [DataSourceProperty]
        public int ArtisanBeerProduction
        {
            get
            {
                return this._artisanBeerProduction;
            }
            set
            {
                if (value != this._artisanBeerProduction)
                {
                    this._artisanBeerProduction = value;
                    base.OnPropertyChangedWithValue(value, "ArtisanBeerProduction");
                    UpdateRegularBeerProductionLaber();
                }
            }
        }
        string _regularBeerProduction;
        [DataSourceProperty]
        public string RegularBeerProduction
        {
            get
            {
                return this._regularBeerProduction;
            }
            set
            {
                if (value != this._regularBeerProduction)
                {
                    this._regularBeerProduction = value;
                    base.OnPropertyChangedWithValue(value, "RegularBeerProduction");
                }
            }
        }
        [DataSourceProperty]
        public string CancelLabel => GameTexts.FindText("str_cancel", null).ToString();
        [DataSourceProperty]
        public string DoneLabel => GameTexts.FindText("str_done", null).ToString();
        [DataSourceProperty]
        public string ArtisanBeerLable => new TextObject("{=MW3wUeJ4cNn9u}Artisan Beer", null).ToString();
        [DataSourceProperty]
        public string RegularBeerLable => new TextObject("{=lIhzP2TNWwE0G}Regular Beer", null).ToString();
        [DataSourceProperty]
        public string TitleText => new TextObject("{=OKNK41jjzTCIV}Brewer Management", null).ToString();
        [DataSourceProperty]
        public string ProductionRatioLable => new TextObject("{=2tY9SOU7RdtA9}Production Ratio", null).ToString();
        public void ExecuteCancel()
        {
            GameStateManager.Current.PopState();
        }

        public void ExecuteDone()
        {

            _artisanWorkshopManagementState.artisanWorkshop.dailyProductionAmount = ArtisanBeerProduction;
            GameStateManager.Current.PopState();
        }
    }
}