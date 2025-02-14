using System.Collections.Generic;
using System;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.Settlements.Workshops;
using TaleWorlds.CampaignSystem.Settlements.Locations;
using TaleWorlds.CampaignSystem.AgentOrigins;
using TaleWorlds.CampaignSystem.Encounters;
using TaleWorlds.Core;
using TaleWorlds.Localization;
using TaleWorlds.ObjectSystem;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.SaveSystem;
using TaleWorlds.Library;
using SandBox;
using TaleWorlds.MountAndBlade;
using SandBox.Conversation;
using TaleWorlds.CampaignSystem.CampaignBehaviors;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.CampaignSystem.Overlay;


namespace FirstMod
{
    public class ArtisanBeerBehavior : CampaignBehaviorBase
    {
        //注册事件
        public override void RegisterEvents()
        {
            CampaignEvents.WorkshopTypeChangedEvent.AddNonSerializedListener(this, OnWorkshopTypeChangedEvent);
            CampaignEvents.DailyTickTownEvent.AddNonSerializedListener(this, DailyTick);
            CampaignEvents.LocationCharactersAreReadyToSpawnEvent.AddNonSerializedListener(this, new Action<Dictionary<string, int>>(this.LocationCharactersAreReadyToSpawn));
            CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener(this, OnSessionLaunched);
            CampaignEvents.OnItemProducedEvent.AddNonSerializedListener(this,OnItemProduced);
        }

        private void OnItemProduced(ItemObject item, Settlement settlement, int amount)
        {
            if(item.StringId == "beer")
                InformationManager.DisplayMessage(new InformationMessage(String.Format("{0} was produced in {1}", item.Name,settlement.Name)));
        }

        private void OnWorkshopTypeChangedEvent(Workshop workshop)
        {
            
        }

        ItemObject _artisanBeer;
        CharacterObject _artisanBrewer;
        private void OnSessionLaunched(CampaignGameStarter starter)
        {
            _artisanBeer = MBObjectManager.Instance.GetObject<ItemObject>("artisan_beer");
            _artisanBrewer = MBObjectManager.Instance.GetObject<CharacterObject>("artisan_brewer");
            this.AddDialogs(starter);
            AddGameMenus(starter);
        }

        private void AddGameMenus(CampaignGameStarter starter)
        {
            for(int i = 0; i < 5; i++)
            {
                AddWorkshopButton(starter,i);
            }
            starter.AddGameMenu("town_workshop", "酿酒厂",  (MenuCallbackArgs args) => { },
                GameOverlays.MenuOverlayType.SettlementWithBoth);
            starter.AddGameMenuOption("town_workshop", "town_workshop_inventory", "库存", (MenuCallbackArgs args) =>
            {
                args.optionLeaveType = GameMenuOption.LeaveType.Trade;
                return true;
            }, (MenuCallbackArgs args) =>
            {
                List<InquiryElement> list = new List<InquiryElement>();
                var artisanWorkshop = ArtisanWorkshop(_selectedWorkshop);
                for(int i = 0; i < artisanWorkshop.inventoryStock; i++)
                {
                    list.Add(new InquiryElement(_artisanBeer, _artisanBeer.Name.ToString(), new ImageIdentifier(_artisanBeer)));
                }
                MultiSelectionInquiryData data = new MultiSelectionInquiryData(
                    "库存",
                    "选择一个物品",
                    list,
                    true,
                    1,
                    1000,
                    "Take",
                    "离开",
                    (List<InquiryElement> list) =>
                    {
                        var artisanWorkshop = ArtisanWorkshop(_selectedWorkshop);
                        artisanWorkshop.AddToStock(-list.Count);
                        MobileParty.MainParty.ItemRoster.AddToCounts(_artisanBeer, list.Count);
                    },
                    (List<InquiryElement> list) =>{ }
                );
                MBInformationManager.ShowMultiSelectionInquiry(data, false);
            });
            starter.AddGameMenuOption("town_workshop","wotn_workshop_management","Manage Workshop",(MenuCallbackArgs args) =>
            {
                args.optionLeaveType = GameMenuOption.LeaveType.Submenu;
                return true;
            },(MenuCallbackArgs args) =>
            {
                GameStateManager.Current.PushState(GameStateManager.Current.CreateState<ArtisanWorkshopManagementState>(_selectedWorkshop,ArtisanWorkshop(_selectedWorkshop)));
            });
            starter.AddGameMenuOption("town_workshop", "town_workshop_id", "离开", (MenuCallbackArgs args) =>
            {
                args.optionLeaveType = GameMenuOption.LeaveType.Leave;
                return true;
            }, (MenuCallbackArgs args) => GameMenu.SwitchToMenu("town"), true);
        }
        Workshop _selectedWorkshop;
        private void AddWorkshopButton(CampaignGameStarter starter,int i)
        {
            starter.AddGameMenuOption("town", "town_workshop" + i, " 管理" +i+ "号酿酒厂",
                            (MenuCallbackArgs args) =>
                            {
                                args.optionLeaveType = GameMenuOption.LeaveType.Submenu;
                                if (i >= Settlement.CurrentSettlement.Town.Workshops.Length) return false;
                                Workshop workshop = Settlement.CurrentSettlement.Town.Workshops[i];
                                if ( workshop.WorkshopType.StringId == "brewery" && workshop.Owner == Hero.MainHero) return true;
                                return false;
                            },
                            delegate (MenuCallbackArgs x)
                            {
                                GameMenu.SwitchToMenu("town_workshop");
                                _selectedWorkshop = Settlement.CurrentSettlement.Town.Workshops[i];
                            }, false, 9, false, null);
        }

        private Workshop FindCurrentWorkshop(Agent agent)
        {
            if (Settlement.CurrentSettlement != null && Settlement.CurrentSettlement.IsTown)
            {
                CampaignAgentComponent component = agent.GetComponent<CampaignAgentComponent>();
                AgentNavigator agentNavigator = (component != null) ? component.AgentNavigator : null;
                if (agentNavigator != null)
                {
                    foreach (Workshop workshop in Settlement.CurrentSettlement.Town.Workshops)
                    {
                        if (workshop.Tag == agentNavigator.SpecialTargetTag)
                        {
                            return workshop;
                        }
                    }
                }
            }
            return null;
        }

        public ArtisanWorkshopState ConversationArtisanWorkshop()
        {
            var workshop = FindCurrentWorkshop(ConversationMission.OneToOneConversationAgent);
            if (workshop != null)
            {
                return ArtisanWorkshop(workshop);
            }
            return null; 
        }

        public int ConversationArtisanWorkshopStock()
        {
            var artisanWorkshop =  ConversationArtisanWorkshop();
            if (artisanWorkshop != null) return artisanWorkshop.inventoryStock;
            return 0;
        }

        private void AddDialogs(CampaignGameStarter starter)
        {
            {
                starter.AddPlayerLine("tavernkeeper_talk_ask_artisan_beer", "tavernkeeper_talk", "tavernkeeper_artisan_beer", "你这有手工啤酒卖吗?", null, null);
                starter.AddDialogLine("tavernkeeper_talk_artisan_beer_a", "tavernkeeper_artisan_beer", "tavernkeeper_talk", "当然有，但不出售给外人", () =>
                {
                    foreach (var workshop in Settlement.CurrentSettlement.Town.Workshops)
                    {
                        if (workshop.WorkshopType.StringId == "brewery") return true;
                    }
                    return false;
                }, null);
                starter.AddDialogLine("tavernkeeper_talk_artisan_beer_b", "tavernkeeper_artisan_beer", "tavernkeeper_talk", "我这全是工业酒精勾兑的，专卖给那些大老粗，向您这样的勇士，我可不敢卖给您", null, null);
            }
            {
                starter.AddDialogLine("artisan_brewer_talk_outofstock", "start", "end", "日安先生，您想要在这买一些手工啤酒吗？不幸的是我们没有库存了，请之后再来",
                    //() => CharacterObject.OneToOneConversationCharacter.Occupation == Occupation.Tavernkeeper, null);
                    () => CharacterObject.OneToOneConversationCharacter == _artisanBrewer && ConversationArtisanWorkshopStock() <= 0, null);
                starter.AddDialogLine("artisan_brewer_talk_instock", "start", "artisan_brewer", "日安先生，您想要购买一些手工啤酒吗？只要200枚金币一瓶",
                    () => CharacterObject.OneToOneConversationCharacter == _artisanBrewer, null);

                starter.AddPlayerLine("artisan_brewer_buy", "artisan_brewer", "artisan_brewer_purchaced", "是的，我想拿一个", null, () => {
                    Hero.MainHero.ChangeHeroGold(-200);
                    MobileParty.MainParty.ItemRoster.AddToCounts(_artisanBeer, 1);
                    var artisanworkshop = ConversationArtisanWorkshop();
                    artisanworkshop.AddToStock(-1);
                }, 100, (out TextObject explanation) =>
                {
                    if (Hero.MainHero.Gold < 200)
                    {
                        explanation = new TextObject("没有足够的钱");
                        return false;
                    }
                    else
                    {
                        explanation = TextObject.Empty;
                        return true;
                    }
                });
                starter.AddDialogLine("artisan_brewer_thanks_for_business", "artisan_brewer_purchaced", "end", "感谢您的慷慨！", null, null);


                starter.AddPlayerLine("artisan_brewer_buy_refuse", "artisan_brewer", "artisan_brewer_declined", "不买", null, null);
                starter.AddDialogLine("artisan_brewer_your_loss", "artisan_brewer_declined", "end", "好吧，那祝你一切顺利", null, null);


            }
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
                        //这里的townsman需要获取culture，但实际上仅用来判断男女，在这里我的酿酒NPC为男的，因此我用false
                        //string actionSetCode = ActionSetCode.GenerateActionSetNameWithSuffix(monsterWithSuffix, townsman.IsFemale, "_villager_in_tavern");
                        //喝酒动画找不到了，现在有的仅是_villager_in_tavern的一般动画集合，我尝试去找在酒馆中的三个镇民NPC的喝酒动作，但没有找到，似乎是在这个动画集中但在引擎中被调用
                        //这个函数并没有被删除，仅被注释，可以到源文件中去将注释去掉，但这样就修改了游戏源文件。
                        string actionSetCode = ActionSetCode.GenerateActionSetNameWithSuffix(monsterWithSuffix, false, "_villager_in_aserai_tavern");
                        string value = "artisan_beer_drinking_animation";
                        var agentData = new AgentData(new
                            SimpleAgentOrigin(_artisanBrewer, -1, null, default(UniqueTroopDescriptor))).Monster(monsterWithSuffix).Age(MBRandom.RandomInt(minValue, maxValue));
                        CharacterObject caravanMaster = Settlement.CurrentSettlement.Culture.CaravanMaster;
                        LocationCharacter locationCharacter = new LocationCharacter(agentData,
                            new LocationCharacter.AddBehaviorsDelegate(SandBoxManager.Instance.AgentBehaviorManager.AddWandererBehaviors),
                            workshop.Tag, true, LocationCharacter.CharacterRelations.Friendly, actionSetCode, true, false, null, false, false, true)
                        {
                            PrefabNamesForBones = {
                                {
                                    agentData.AgentMonster.MainHandItemBoneIndex,
                                    value
                                }
                            }
                        };
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
                    var artisanWorkshop = ArtisanWorkshop(workshop);
                    artisanWorkshop.AddToStock(artisanWorkshop.dailyProductionAmount);
                }
            }
        }
        
        //private void OnWorkshopTypeChangedEvent(Workshop workshop,Hero oldOwningHero,WorkshopType type)
        //{
        //}

        public ArtisanWorkshopState ArtisanWorkshop(Workshop workshop)
        {
            string id = workshop.Settlement.StringId + "_" + workshop.Tag;
            if (artisanWorkshops.TryGetValue(id, out var state))
            {
                return state;
            }
            else
            {
                state = new ArtisanWorkshopState() { dailyProductionAmount = 1, inventoryCapacity = 10, inventoryStock = 0 };
                artisanWorkshops.Add(id, state);
                return state;
            }
        }

        public Dictionary<string, ArtisanWorkshopState> artisanWorkshops = new Dictionary<string, ArtisanWorkshopState>();
        //保存行为
        public override void SyncData(IDataStore dataStore)
        {
            dataStore.SyncData("artisanWorkshops", ref artisanWorkshops);
        }
    }

    public class ArtisanWorkshopSaveableTypeDefiner : SaveableTypeDefiner
    {
        public ArtisanWorkshopSaveableTypeDefiner() : base(536_882_256)
        {
        }
        protected override void DefineClassTypes()
        {
            AddClassDefinition(typeof(ArtisanWorkshopState), 1);
        }
        protected override void DefineContainerDefinitions()
        {
            ConstructContainerDefinition(typeof(Dictionary<string, ArtisanWorkshopState>)); ;
        }
    }

    public class ArtisanWorkshopState
    {
        [SaveableField(1)]
        public int inventoryStock;
        [SaveableField(2)]
        public int inventoryCapacity;
        [SaveableField(3)]
        public int dailyProductionAmount;

        public void AddToStock(int amount)
        {
            inventoryStock += amount;
            if(inventoryStock > inventoryCapacity)
            {
                inventoryStock = inventoryCapacity;
            }
            if(inventoryStock < 0)
            {
                InformationManager.DisplayMessage(new InformationMessage("Artisan workshop negative inventory stock."));
            }
        }
    }
}