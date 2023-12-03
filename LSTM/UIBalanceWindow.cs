
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using System;
using System.Text;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;


namespace LSTMMod
{
    //Local  item指定  惑星内輸送 itemでフィルタ ローカル需給で分類
    //Local  指定なし   惑星内輸送 全部 ローカル需給で分類
    //Global item指定  全惑星間輸送 itemでフィルタ リモート需給で分類
    //Global 指定なし   惑星内輸送 全部 リモート需給で分類

    public struct BalanceData
    {
        public int itemId;
        public int planetId;
        public bool isLocal;
        public int starId;

        public BalanceData(int _itemId,  int _planetId, bool _isLocal) : this(_itemId, _planetId, _isLocal, 0)
        {
        }

        public BalanceData(int _itemId, int _planetId, bool _isLocal, int _starId)
        {
            itemId = _itemId;
            planetId = _planetId;
            isLocal = _isLocal;
            starId = _starId;
        }
    }

    public struct DisplayMode
    {
        public bool useStationName;
    }

    public enum EStoreType
    {
        Normal,
        Gas,
        GasStubSupply,
        GasStubStorage,
    }
    public struct BalanceListData
    {
        public StationComponent cmp;
        public int index;
        public int planetId;
        public int itemId;
        public int maxCount;
        public int distanceForSort;
        public bool isLocal;
        //public bool useStationName;
        public EStoreType storeType;
    }

    public class UIBalanceWindow : ManualBehaviour, IPointerEnterHandler, IPointerExitHandler, IEventSystemHandler, MyWindow
    {
        public RectTransform windowTrans;
        public bool keepOpen = false;
        internal bool _eventLock;

        public BalanceData balanceData;

        public int tmpPlanetId;
        public int tmpStarId;


        bool _useStationNameFallback;
        public bool UseStationName
        {
            get
            {
                if (displayModes[currentDisplayMode].useStationName)
                {
                    return true;
                }
                return _useStationNameFallback;
            }
        }

        internal DisplayMode[] displayModes;
        internal int currentDisplayMode;

        public int totalSupplyCount;
        public int totalSupplyCapacity;
        public int totalDemandCount;
        public int totalDemandCapacity;

        public UIListView supplyListView;
        public UIListView demandListView;
        public UIMaterialPicker materialView;

        public bool isPointEnter;

        private bool focusPointEnter;

        public static UIBalanceWindow CreateInstance()
        {
            UIBalanceWindow win = MyWindowCtl.CreateWindow<UIBalanceWindow>("LSTMBalanceWindow", "LSTM");
            return win;
        }

        public bool isFunctionWindow()
        {
            return true;
        }

        public void SetUpAndOpen(int _itemId, int _planetId, bool _isLocal, int _starId = 0)
        {
            balanceData = new BalanceData(_itemId, _planetId, _isLocal, _starId);
            materialView.RefreshWithProduct(balanceData.itemId);
            SetUpData();

            UIRoot.instance.uiGame.ShutPlayerInventory();
            MyWindowCtl.OpenWindow(this);
        }

        public void OpenWithoutSetting()
        {
            SetUpData();

            UIRoot.instance.uiGame.ShutPlayerInventory();
            MyWindowCtl.OpenWindow(this);
        }

        public override void _OnCreate()
        {
            _eventLock = true;
            windowTrans = MyWindowCtl.GetRectTransform(this);
            windowTrans.sizeDelta = new Vector2(700, 640);
            balanceData = new BalanceData(0, 0, false, 0);
            displayModes = new DisplayMode[2];
            displayModes[0].useStationName = false;
            displayModes[1].useStationName = true;
            currentDisplayMode = 0;
            CreateUI();
        }

        public override void _OnDestroy()
        {
        }

        public override bool _OnInit()
        {
            //UIStorageGrid srcWin = UIRoot.instance.uiGame.inventory;
            windowTrans.anchoredPosition = new Vector2(-480, 0);
            return true;
        }

        public override void _OnFree()
        {
        }

        public override void _OnRegEvent()
        {
            itemButton.onClick += OnSelectItemButtonClick;
            itemResetButton.onClick += OnItemResetButtonClick;
            planetResetButton.onClick += OnPlanetResetButtonClick;
            localButton.onClick += OnLocalButtonClick;
            remoteButton.onClick += OnRemoteButtonClick;
            starSystemButton.onClick += OnStarSystemButtonClick;
            demandListView.m_ScrollRect.onValueChanged.AddListener(OnDemanScrollRectChanged);
            supplyListView.m_ScrollRect.onValueChanged.AddListener(OnSupplyScrollRectChanged);
        }

        public override void _OnUnregEvent()
        {
            itemButton.onClick -= OnSelectItemButtonClick;
            itemResetButton.onClick -= OnItemResetButtonClick;
            planetResetButton.onClick -= OnPlanetResetButtonClick;
            localButton.onClick -= OnLocalButtonClick;
            remoteButton.onClick -= OnRemoteButtonClick;
            starSystemButton.onClick -= OnStarSystemButtonClick;
            demandListView.m_ScrollRect.onValueChanged.RemoveListener(OnDemanScrollRectChanged);
            supplyListView.m_ScrollRect.onValueChanged.RemoveListener(OnSupplyScrollRectChanged);
        }

        public override void _OnOpen()
        {
            RefreshstarSystemComboBox();
        }

        public override void _OnClose()
        {
            supplyListView.Clear();
            demandListView.Clear();
            isPointEnter = false;
        }

        public override void _OnUpdate()
        {

            if (VFInput.escape && !UIRoot.instance.uiGame.starmap.active && !VFInput.inputing)
            {
                VFInput.UseEscape();
                if (LSTM._configWin.active)
                {
                    LSTM._configWin._Close();
                }
                else if (menuComboBox.isDroppedDown)
                {
                    menuComboBox.isDroppedDown = false;
                }
                else
                {
                    base._Close();
                }
            }
            if (_eventLock)
            {
                return;
            }

            //デフォルトだとスプリッターの形変更などと被るけど、LSTM出したまま建築することもないだろうからとりあえず無視
            if (LSTM.switchDisplayModeHotkey.Value.IsDown() && !VFInput.inputing)
            {
                SwitchDisplayMode();
            }

            if (_demandList.Count>0)
            {
                AddToListView(demandListView, 5, _demandList);
            }
            if (_supplyList.Count > 0)
            {
                AddToListView(supplyListView, 5, _supplyList);
            }

            bool valid = true;
            int step = Time.frameCount % 30;
            if (step == 0)
            {
                valid = RefreshListView(demandListView);
            }
            else if (step == 15)
            {
                valid = RefreshListView(supplyListView);
            }
            else
            {
                RefreshListView(demandListView, true);
                RefreshListView(supplyListView, true);
            }
            if (!valid)
            {
                SetUpData();
            }
            if (!menuComboBox.isDroppedDown && menuTarget != null)
            {
                //menuTarget.UnlockAppearance();
                menuTarget = null;
            }
        }

        public void Filter(int itemId, int planetId = 0, bool refreshMaterial = true)
        {
            if (itemId != balanceData.itemId)
            {
                balanceData = new BalanceData(itemId, balanceData.planetId, balanceData.isLocal, balanceData.starId);
                SetUpData();
            }
            //else if (planetId != balanceData.planetId)
            //{
            //    balanceData = new BalanceData(balanceData.itemId, planetId, balanceData.isLocal, balanceData.starId);
            //    SetUpData();
            //}
            if (refreshMaterial)
            {
                materialView.RefreshWithProduct(balanceData.itemId);
            }
        }


        public void SwitchToLocal()
        {
            if (balanceData.isLocal)
            {
                return;
            }
            if (balanceData.starId != 0)
            {
                tmpStarId = balanceData.starId;
            }
            balanceData = new BalanceData(balanceData.itemId, balanceData.planetId, !balanceData.isLocal);
            SetUpData();
        }

        public void SwitchToStarSystem(int starId = 0)
        {
            if (starId != 0)
            {
                balanceData.starId = starId;
            }
            //if (balanceData.starId != 0 && !balanceData.isLocal)
            //{
            //    return;
            //}
            if (balanceData.starId == 0 && tmpStarId != 0)
            {
                balanceData.starId = tmpStarId;
            }
            if (balanceData.starId == 0)
            {
                balanceData.starId = GameMain.localStar != null ? GameMain.localStar.id : 0;
            }
            balanceData = new BalanceData(balanceData.itemId, balanceData.planetId, false, balanceData.starId);
            SetUpData();
        }

        public void SwitchToGlobal()
        {
            if (balanceData.starId == 0 && !balanceData.isLocal)
            {
                return;
            }
            if (balanceData.starId != 0)
            {
                tmpStarId = balanceData.starId;
            }
            balanceData = new BalanceData(balanceData.itemId, balanceData.planetId, false, 0);
            SetUpData();
        }

        public void SwitchDisplayMode()
        {
            DisplayMode currentMode = displayModes[currentDisplayMode];
            currentDisplayMode++;
            currentDisplayMode %= displayModes.Length;
            DisplayMode newMode = displayModes[currentDisplayMode];
            if (currentMode.useStationName != newMode.useStationName)
            {
                for (int i = 0; i < demandListView.ItemCount; i++)
                {
                    UIBalanceListEntry e = demandListView.GetItem<UIBalanceListEntry>(i);
                    if (e != null && e.itemId > 0)
                    {
                        e.nameDirty = true;
                    }
                }
                for (int i = 0; i < supplyListView.ItemCount; i++)
                {
                    UIBalanceListEntry e = supplyListView.GetItem<UIBalanceListEntry>(i);
                    if (e != null && e.itemId > 0)
                    {
                        e.nameDirty = true;
                    }
                }
            }
        }

        public void SetUpData()
        {
            _eventLock = true;
            tmpPlanetId = 0;
            _demandList.Clear();
            _supplyList.Clear();
            _storageList.Clear();
            _gasList.Clear();
            supplyListView.Clear();
            demandListView.Clear();

            SetUpItemList();
            SetUpItemUI();
            if ((!balanceData.isLocal && balanceData.itemId > 0) || balanceData.starId != 0)
            {
                _demandList.Sort((a, b) => a.distanceForSort - b.distanceForSort);
                _supplyList.Sort((a, b) => a.distanceForSort - b.distanceForSort);
                _storageList.Sort((a, b) => a.distanceForSort - b.distanceForSort);
            }
            else
            {
                _demandList.Sort((a, b) => a.itemId - b.itemId);
                _supplyList.Sort((a, b) => a.itemId - b.itemId);
                _storageList.Sort((a, b) => a.itemId - b.itemId);
            }
            //貯蔵はsupply側にしておく
            _supplyList.AddRange(_storageList);
            _storageList.Clear();

            _eventLock = false;
            //見える範囲だけ即更新
            if (_demandList.Count > 0)
            {
                AddToListView(demandListView, 5, _demandList);
            }
            if (_supplyList.Count > 0)
            {
                AddToListView(supplyListView, 5, _supplyList);
            }
            RefreshListView(demandListView);
            RefreshListView(supplyListView);
        }

        internal void SetUpItemUI()
        {
            localButton.highlighted = balanceData.isLocal;
            remoteButton.highlighted = !balanceData.isLocal && balanceData.starId == 0;
            starSystemButton.highlighted = !balanceData.isLocal && balanceData.starId != 0;
            SetStarSystemBoxHighlight(starSystemButton.highlighted);

            if (balanceData.itemId <= 0)
            {
                itemCircle.fillAmount = 0f;
                itemResetButton.gameObject.SetActive(false);
                itemImage.sprite = defaultItemSprite;
                itemText.text = "";
            }
            else
            {
                itemCircle.fillAmount = 1f;
                itemResetButton.gameObject.SetActive(true);
                ItemProto itemProto = LDB.items.Select(balanceData.itemId);
                if (itemProto != null)
                {
                    itemImage.sprite = itemProto.iconSprite;
                    itemText.text = itemProto.name;
                }
            }

            string AppropriatePlanetName()
            {
                if (balanceData.planetId <= 0)
                {
                    planetResetButton.gameObject.SetActive(false);
                    if (tmpPlanetId > 0)
                    {
                        return "(" + GameMain.galaxy.PlanetById(tmpPlanetId).displayName + ")";
                    }
                    else
                    {
                        return "";
                    }
                }
                else
                {
                    planetResetButton.gameObject.SetActive(true);
                    return GameMain.galaxy.PlanetById(balanceData.planetId).displayName;
                }
            }
            string planetName;
            if (!balanceData.isLocal)
            {
                if (balanceData.starId != 0)
                {
                    planetName = GameMain.galaxy.StarById(balanceData.starId).displayName + "空格行星系".Translate();
                }
                else if(balanceData.itemId == 0)
                {
                    planetName = AppropriatePlanetName();
                }
                else
                {
                    planetName = "(All Planets)".Translate();
                }
                planetResetButton.gameObject.SetActive(false);
            }
            else
            {
                planetName = AppropriatePlanetName();
            }

            if (planetText != null)
            {
                planetText.text = planetName;
                RectTransform rect = (RectTransform)planetResetButton.gameObject.transform;
                rect.anchoredPosition = new Vector2(planetText.preferredWidth / 2 + 18f, rect.anchoredPosition.y);
            }
        }


        internal void SetUpItemList()
        {
            _useStationNameFallback = false;
            if (balanceData.isLocal)
            {
                AddFromPlanet(balanceData.planetId, balanceData.itemId, balanceData.isLocal);
            }
            else
            {
                int itemId = balanceData.itemId;
                int planetId = balanceData.planetId;
                //itemId なし starId なし
                if (itemId <= 0 && balanceData.starId == 0)
                {
                    AddFromPlanet(planetId, itemId, balanceData.isLocal);
                    return;
                }

                //planetId あり itemId ありの場合 planetId は無視
                //starId あり itemIdなしの場合全部追加

                //itemId あり
                HashSet<int> gasSupplyPlanets = new HashSet<int>();
                HashSet<int> gasStoragePlanets = new HashSet<int>();
                GalacticTransport galacticTransport = UIRoot.instance.uiGame.gameData.galacticTransport;
                StationComponent[] stationPool = galacticTransport.stationPool;
                int cursor = galacticTransport.stationCursor;
                for (int i = 1; i < cursor; i++)
                {
                    if (stationPool[i] != null && stationPool[i].gid == i) //gid
                    {
                        StationComponent cmp = stationPool[i];

                        if (balanceData.starId != 0 && (cmp.planetId / 100) != balanceData.starId)
                        {
                            continue;
                        }

                        int length = cmp.storage.Length;
                        for (int j = 0; j < length; j++)
                        {
                            if (cmp.storage[j].itemId == 0)
                            {
                                continue;
                            }
                            if (cmp.storage[j].itemId == itemId || (balanceData.starId != 0 && itemId <= 0))
                            {
                                int maxCount;

                                //factory = GameMain.galaxy.PlanetById(cmp.planetId).factory;
                                maxCount = LSTM.RemoteStationMaxItemCount();
                                if (cmp.isCollector || !cmp.isStellar)
                                {
                                    maxCount /= 2;
                                }

                                if (cmp.isCollector)
                                {
                                    if (cmp.storage[j].remoteLogic == ELogisticStorage.Supply)
                                    {
                                        gasSupplyPlanets.Add(cmp.planetId * 10_000 + cmp.storage[j].itemId);
                                    }
                                    else
                                    {
                                        gasStoragePlanets.Add(cmp.planetId * 10_000 + cmp.storage[j].itemId);
                                    }
                                    AddStore(cmp, j, cmp.planetId, cmp.storage[j].itemId, maxCount, EStoreType.Gas);
                                }
                                else
                                {
                                    AddStore(cmp, j, cmp.planetId, cmp.storage[j].itemId, maxCount);
                                }
                                if (itemId > 0)
                                {
                                    break;
                                }
                            }
                        }
                    }
                }

                int gasMaxCount = LSTM.RemoteStationMaxItemCount() / 2;
                foreach (int gas in gasSupplyPlanets)
                {
                    AddStore(null, 0, gas / 10_000, gas % 10_000, gasMaxCount, EStoreType.GasStubSupply);
                }
                foreach (int gas in gasStoragePlanets)
                {
                    AddStore(null, 0, gas / 10_000, gas % 10_000, gasMaxCount, EStoreType.GasStubStorage);
                }
            }

        }

        public void AddFromPlanet(int planetId, int itemId, bool isLocal)
        {
            _useStationNameFallback = true;

            PlanetFactory factory = null;
            if (planetId != 0)
            {
                factory = GameMain.galaxy.PlanetById(planetId).factory;
                if (factory == null)
                {
                    AddFromPlanetFailedGetFactory(planetId, itemId, isLocal);
                    return;
                }
                tmpStarId = planetId / 100;
            }
            else
            {
                factory = GameMain.localPlanet?.factory;
                if (factory == null)
                {
                    AddFromPlanetFailedGetFactory(planetId, itemId, isLocal);
                    return;
                }
                tmpPlanetId = GameMain.localPlanet.id;
                tmpStarId = GameMain.localPlanet.star.id;
            }

            PlanetTransport transport = factory.transport;
            StationComponent[] stationPool = transport.stationPool;
            int cursor = transport.stationCursor;
            for (int i = 1; i < cursor; i++)
            {
                if (stationPool[i] != null && stationPool[i].id == i) //id
                {
                    StationComponent cmp = stationPool[i];

                    int length = cmp.storage.Length;
                    //warperスロットが存在するかと思ってたけどないっぽい
                    //if (length == 6)
                    //{
                    //    length -= 1;
                    //}
                    for (int j = 0; j < length; j++)
                    {
                        if ((itemId <= 0 && cmp.storage[j].itemId > 0) || (itemId > 0 && cmp.storage[j].itemId == itemId))
                        {
                            int maxCount;
                            //ItemProto itemProto2 = LDB.items.Select((int)factory.entityPool[cmp.entityId].protoId);
                            //if (itemProto2 != null)
                            //{
                            //    maxCount = itemProto2.prefabDesc.stationMaxItemCount;
                            //}
                            maxCount = LSTM.RemoteStationMaxItemCount();
                            if (cmp.isCollector || !cmp.isStellar)
                            {
                                maxCount /= 2;
                            }
                            AddStore(cmp, j, factory.planetId, cmp.storage[j].itemId, maxCount);
                        }
                    }
                }
            }
        }

        public void AddFromPlanetFailedGetFactory(int planetId, int itemId, bool isLocal)
        {
            //nothing to do
        }



        internal List<BalanceListData> _demandList = new List<BalanceListData>(200);
        internal List<BalanceListData> _supplyList = new List<BalanceListData>(200);
        internal List<BalanceListData> _storageList = new List<BalanceListData>(200);
        public List<BalanceListData> _gasList = new List<BalanceListData>(800);

        //追加の前に溜め込む
        //ガス惑星まとめるようにしたらコードがカオス
        internal void AddStore(StationComponent cmp, int index, int planetId, int itemId, int maxCount = 10000, EStoreType storeType = EStoreType.Normal)
        {
            List<BalanceListData> list;
            
            int distanceForSort;
            if (storeType== EStoreType.Gas)
            {
                list = _gasList;
                distanceForSort = 0;
            }
            else
            {
                if (storeType == EStoreType.GasStubSupply)
                {
                    list = _supplyList;
                }
                else if (storeType == EStoreType.GasStubStorage)
                {
                    list = _storageList;
                }
                else
                {
                    StationStore store = cmp.storage[index];
                    ELogisticStorage ltype = balanceData.isLocal ? store.localLogic : store.remoteLogic;

                    switch (ltype)
                    {
                        case ELogisticStorage.None:
                            if (LSTM.hideStoragedSlot.Value)
                            {
                                return;
                            }
                            list = _storageList;
                            break;
                        case ELogisticStorage.Supply:
                            list = _supplyList;
                            break;
                        case ELogisticStorage.Demand:
                            list = _demandList;
                            break;
                        default:
                            if (LSTM.hideStoragedSlot.Value)
                            {
                                return;
                            }
                            list = _storageList;
                            break;
                    }
                }

                int localPlanetId = GameMain.localPlanet != null ? GameMain.localPlanet.id : 0;
                if (localPlanetId == planetId)
                {
                    distanceForSort = 0;
                }
                else if (localPlanetId / 100 == planetId / 100)
                {
                    distanceForSort = planetId % 100;
                }
                else
                {
                    float distancef = LSTMStarDistance.StarDistanceFromHere(planetId / 100);
                    distanceForSort = (int)(distancef * 100) + (planetId % 100);
                }
            }
            BalanceListData d = new BalanceListData()
            {
                cmp = cmp,
                index = index,
                planetId = planetId,
                itemId = itemId,
                maxCount = maxCount,
                isLocal = balanceData.isLocal,
                //useStationName = useStationName,
                distanceForSort = distanceForSort,
                storeType = storeType,
            };
            list.Add(d);
        }

        //溜め込んでいたものを追加する
        internal int AddToListView(UIListView listView, int count, List<BalanceListData> list)
        {
            if(list.Count < count)
            {
                count = list.Count;
            }
            if (count == 0)
            {
                return count;
            }

            for (int i = 0; i < count; i++)
            {
                BalanceListData d = list[0];
                list.RemoveAt(0);
                UIBalanceListEntry e = listView.AddItem<UIBalanceListEntry>();

                e.window = this;
                e.station = d.cmp;
                e.index = d.index;
                e.itemId = d.itemId;
                e.planetId = d.planetId;
                e.isLocal = balanceData.isLocal;
                e.stationMaxItemCount = d.maxCount;
                e.storeType = d.storeType;
                e.SetUpValues(UseStationName);

            }
            return count;
        }


        internal bool RefreshListView(UIListView listView, bool onlyNewlyEmerged = false)
        {
            if (_eventLock)
            {
                return true;
            }

            RectTransform contentRect = (RectTransform)listView.m_ContentPanel.transform;
            float top = -contentRect.anchoredPosition.y;
            float height = ((RectTransform)contentRect.parent).rect.height;
            float bottom = top - height - 120f; //項目の高さ適当な決め打ち

            for (int i = 0; i < listView.ItemCount; i++)
            {
                UIData data = listView.GetItemNode(i);
                UIBalanceListEntry e = (UIBalanceListEntry)data.com_data;
                if (e != null && e.itemId > 0 )
                {
                    float itemPos = ((RectTransform)data.transform).localPosition.y;
                    bool shown = itemPos <= top && itemPos > bottom;
                    if (onlyNewlyEmerged && !shown)
                    {
                        continue;
                    }
                    if (!e.RefreshValues(shown, onlyNewlyEmerged))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        internal void RefreshstarSystemComboBox()
        {
            GameData gameData = UIRoot.instance.uiGame.gameData;
            List<string> items = starSystemComboBox.Items;
            List<int> itemsData = starSystemComboBox.ItemsData;
            items.Clear();
            itemsData.Clear();
            items.Add("统计当前星球".Translate());
            itemsData.Add(0);

            int factoryCount = gameData.factoryCount;
            for (int i = 0; i < factoryCount; i++)
            {
                StarData star = gameData.factories[i].planet.star;
                if (!itemsData.Contains(star.id))
                {
                    string item = star.displayName + "空格行星系".Translate();
                    itemsData.Add(star.id);
                    items.Add(item);
                }
            }
        }

        public void SetStarSystemBoxHighlight(bool flag)
        {
            if (starSystemBoxBg == null || starSystemBoxMark == null)
            {
                return;
            }
            if (flag)
            {
                starSystemBoxBg.color = new Color(1f, 0.85f, 0.62f, 0.92f);
                starSystemBoxMark.color = new Color(0.7f, 0.3f, 0.2f, 0.9f);
            }
            else
            {
                starSystemBoxBg.color = new Color(0.65f, 0.91f, 1f, 0.41f);
                starSystemBoxMark.color = new Color(1f, 1f, 1f, 0.9f);
            }
        }

        public void OnStarSystemBoxItemIndexChange()
        {
            if (_eventLock)
            {
                return;
            }
            int num = starSystemComboBox.itemIndex;
            if (num < 0) //recursion
            {
                return;
            }
            int starId = starSystemComboBox.ItemsData[num];
            if (starId == 0)
            {
                starId = GameMain.localStar != null ? GameMain.localStar.id : 0;
            }
            tmpStarId = 0;
            SwitchToStarSystem(starId);
            starSystemComboBox.itemIndex = -1; //recursion
        }


        public Text itemText;
        public UIButton itemButton;
        public UIButton itemResetButton;
        public Image itemImage;
        public Image itemCircle;
        public Text planetText;
        public UIButton planetButton;
        public UIButton planetResetButton;
        //public Image planetImage;
        //public Image planetCircle;

        public UIButton localButton;
        public UIButton remoteButton;
        public UIButton starSystemButton;
        public Text demandLabel;
        public Text supplyLabel;

        public UIComboBox starSystemComboBox;
        internal Image starSystemBoxBg;
        internal Image starSystemBoxMark;


        public static Sprite defaultItemSprite = null;
        public static Sprite gasGiantSprite = null;

        internal void CreateListViews()
        {
            UIListView src = UIRoot.instance.uiGame.tutorialWindow.entryList;
            GameObject go = GameObject.Instantiate(src.gameObject);
            go.name = "list-view-left";

            RectTransform rect = Util.NormalizeRectC(go, 310, 444);
            rect.SetParent(windowTrans, false);
            rect.anchoredPosition = new Vector2(-168, -68);

            demandListView = go.GetComponent<UIListView>();

            demandListView.m_ItemRes.com_data = UIBalanceListEntry.CreatePreｆab();
            Transform parent = demandListView.m_ItemRes.gameObject.transform;
            Transform transform = demandListView.m_ItemRes.com_data.gameObject.transform;

            //demandListView.m_ItemRes.transform.DetachChildren
            for (int i = parent.childCount - 1; i >= 0; --i)
            {
                GameObject.Destroy(parent.GetChild(i).gameObject);
            }
            parent.DetachChildren();
            transform.SetParent(parent);

            Image barBg = demandListView.m_ScrollRect.verticalScrollbar.GetComponent<Image>();
            if (barBg != null)
            {
                barBg.color = new Color(0f, 0f, 0f, 0.2f);
            }

            GameObject go2 = GameObject.Instantiate(go);
            go2.name = "list-view-right";
            RectTransform rect2 = (RectTransform)go2.transform;
            rect2.SetParent(windowTrans, false);
            rect2.anchoredPosition = new Vector2(152, -68);
            supplyListView = go2.GetComponent<UIListView>();


            InitListView(supplyListView);
            InitListView(demandListView);
        }

        internal void InitListView(UIListView listView)
        {
            listView.HorzScroll = false;
            listView.VertScroll = true;
            listView.m_ItemRes.sel_highlight = null;

            listView.CullOutsideItems = false;
            listView.ColumnSpacing = 0;
            listView.RowSpacing = 4;
            listView.m_ItemRes.com_data.gameObject.SetActive(true);

            Transform parent = listView.m_ItemRes.gameObject.transform;
            GameObject.Destroy(parent.GetComponent<Image>());
            GameObject.Destroy(parent.GetComponent<Button>());
            GameObject.Destroy(parent.GetComponent<UITutorialListEntry>());


            Transform transform = listView.m_ItemRes.com_data.gameObject.transform;
            transform.localPosition = Vector3.zero;
            transform.localScale = Vector3.one;
            listView.RowHeight = (int)(transform as RectTransform).sizeDelta.y;

        }

        internal void CreateStarSystemBox()
        {
            // Main Button : Image,Button
            // -Pop sign : Image
            // -Text : Text
            // Dropdown List ScrollBox : ScrollRect

            UIStatisticsWindow statisticsWindow = UIRoot.instance.uiGame.statWindow;
            UIComboBox src = statisticsWindow.productAstroBox;
            UIComboBox box = GameObject.Instantiate<UIComboBox>(src, windowTrans);
            box.gameObject.name = "system-box";


            RectTransform //boxRect = box.transform as RectTransform;
            boxRect = Util.NormalizeRectC(box.gameObject);
            boxRect.anchoredPosition = new Vector2(213f, 220f);

            //boxRect.anchoredPosition = new Vector2(-130, -30);//tmp

            RectTransform btnRect = box.transform.Find("Main Button")?.transform as RectTransform;
            if (btnRect != null)
            {
                btnRect.pivot = new Vector2(1f, 0f);
                btnRect.anchorMax = Vector2.zero;
                btnRect.anchorMin = Vector2.zero;
                btnRect.anchoredPosition = new Vector2(boxRect.sizeDelta.x, 0f);
                btnRect.sizeDelta = new Vector2(20, boxRect.sizeDelta.y);

                Button btn = btnRect.GetComponent<Button>();

                //0.6549 0.9137 1 0.4118  1 1 1 1 off
                //(1f, 0.85f, 0.62f, 0.92f) 0 0 0 0.93 on (0.7 0.3 0.2 0.9)
                starSystemBoxBg = btnRect.GetComponent<Image>();
                starSystemBoxMark = btnRect.Find("Pop sign")?.GetComponent<Image>();
                if (starSystemBoxMark != null)
                {
                    (starSystemBoxMark.transform as RectTransform).anchoredPosition = new Vector2(-10f, 0f);
                }
                btnRect.Find("Text")?.gameObject.SetActive(false);
            }

            box.DropDownCount = 16;
            box.onItemIndexChange.AddListener(OnStarSystemBoxItemIndexChange);
            starSystemComboBox = box;
        }

        internal void CreateUI()
        {
            if (defaultItemSprite == null)
            {
                defaultItemSprite = Util.LoadSpriteResource("Icons/Tech/1414");
            }
            if (gasGiantSprite == null)
            {
                gasGiantSprite = Util.LoadSpriteResource("Icons/Tech/1606");
            }

            CreateListViews();

            GameObject go;
            Transform bg;
            RectTransform rect;
            UIAssemblerWindow assemblerWindow = UIRoot.instance.uiGame.assemblerWindow;

            //icon
            bg = assemblerWindow.resetButton.transform.parent; //circle-back
            if (bg != null)
            {
                go = GameObject.Instantiate(bg.gameObject);
                Transform btn = go.transform.Find("product-icon");
                if (btn != null)
                {
                    itemResetButton = go.transform.Find("stop-btn")?.GetComponent<UIButton>();
                    go.transform.Find("cnt-text")?.gameObject.SetActive(false);
                    //GameObject.Destroy(go.GetComponent<EventTrigger>());
                    GameObject.Destroy(go.transform.Find("circle-fg-1")?.gameObject);
                    GameObject.Destroy(go.transform.Find("product-icon-1")?.gameObject);
                    GameObject.Destroy(go.transform.Find("cnt-text-1")?.gameObject);

                    itemButton = btn.GetComponent<UIButton>();
                    itemButton.tips.tipTitle = "Select Item".Translate();
                    itemButton.tips.tipText = "Select item to display".Translate();
                    itemButton.tips.corner = 3;
                    itemButton.tips.offset = new Vector2(16, 16);
                    itemCircle = go.transform.Find("circle-fg")?.GetComponent<Image>();
                    itemCircle.color = Util.DSPBlue;
                    itemImage = btn.GetComponent<Image>();
                    itemImage.sprite = defaultItemSprite;
                    rect = Util.NormalizeRectC(go);
                    //rect.localScale = new Vector3(1f, 1f, 1f);
                    rect.SetParent(windowTrans, false);
                    rect.anchoredPosition = new Vector2(-276f, 224f);
                    go.name = "item-button";
                    go.SetActive(true);
                }

            }

            //demand supply label
            Text stateText = assemblerWindow.stateText;

            go = GameObject.Instantiate(stateText.gameObject);
            go.name = "supply-label";
            supplyLabel = go.GetComponent<Text>();
            supplyLabel.text = "Supply".Translate();
            supplyLabel.color = Util.DSPBlue;
            rect = Util.NormalizeRectC(go);
            rect.SetParent(windowTrans, false);
            rect.sizeDelta = new Vector2(80, rect.sizeDelta.y);
            //rect.pivot = new Vector2(1f, 0.5f);
            rect.anchoredPosition = new Vector2(0f, 168f);

            go = GameObject.Instantiate(go, windowTrans);
            go.name = "demand-label";
            demandLabel = go.GetComponent<Text>();
            demandLabel.text = "Demand".Translate();
            demandLabel.color = Util.DSPOrange;
            rect = go.transform as RectTransform;
            rect.anchoredPosition = new Vector2(-310f, 168f);


            //local remote starSystem button
            UIPowerGeneratorWindow generatorWindow = UIRoot.instance.uiGame.generatorWindow;
            go = GameObject.Instantiate(generatorWindow.gammaMode1Button.gameObject);
            Text txt = go.transform.Find("button-text")?.GetComponent<Text>();
            GameObject.Destroy(go.transform.Find("button-text")?.GetComponent<Localizer>());
            go.name = "local-button";
            localButton = go.GetComponent<UIButton>();
            localButton.tips.tipTitle = "";
            localButton.tips.tipText = "";
            rect = Util.NormalizeRectC(go);
            rect.SetParent(windowTrans, false);
            rect.sizeDelta = new Vector2(66f, 30f);
            rect.anchoredPosition = new Vector2(280f, 186f);
            if (txt != null) txt.text = "Local";
            go.SetActive(true);

            go = GameObject.Instantiate(go, windowTrans);
            go.name = "system-button";
            rect = go.transform as RectTransform;
            rect.sizeDelta = new Vector2(46f, 30f); //
            rect.anchoredPosition = new Vector2(270f, 220f); //(280f, 220f)
            txt = rect.Find("button-text")?.GetComponent<Text>();
            if (txt != null) txt.text = "Sys";
            starSystemButton = go.GetComponent<UIButton>();

            go = GameObject.Instantiate(go, windowTrans);
            go.name = "remote-button";
            rect = go.transform as RectTransform;
            rect.sizeDelta = new Vector2(66f, 30f); //
            rect.anchoredPosition = new Vector2(280f, 254f);
            txt = rect.Find("button-text")?.GetComponent<Text>();
            if(txt != null) txt.text = "Global";
            remoteButton = go.GetComponent<UIButton>();

            //name
            Text titleText = MyWindowCtl.GetTitleText(this);
            if (titleText != null)
            {
                go = GameObject.Instantiate(titleText.gameObject);
                go.name = "item-name";
                itemText = go.GetComponent<Text>();
                itemText.fontSize = 20;
                itemText.alignment = TextAnchor.MiddleCenter;

                rect = Util.NormalizeRectC(go);
                rect.SetParent(windowTrans, false);
                rect.sizeDelta = new Vector2(200f, rect.sizeDelta.y);
                rect.anchoredPosition = new Vector2(0f, 210f); //planetText
                go.SetActive(true);

                go = GameObject.Instantiate(go, windowTrans);
                rect.anchoredPosition = new Vector2(0f, 240f); //itemText
                go.name = "planet-name";
                planetText = go.GetComponent<Text>();
                //ContentSizeFitter?

            }

            //frame
            Transform transform = UIRoot.instance.uiGame.inventoryWindow.windowTrans;
            bg = transform.Find("content-bevel-bg");
            if (bg != null)
            {
                go = GameObject.Instantiate(bg.gameObject);
                rect = (RectTransform)go.transform;

                go.transform.SetParent(windowTrans, false);
                rect.localScale = Vector3.one;
                rect.anchoredPosition = Vector2.zero;
                //rect2.anchorMax = new Vector2(1f, 0.8f);
                rect.offsetMax = new Vector2(-26f, -160f);
                rect.offsetMin = new Vector2(24f, 26f);

                go.transform.SetSiblingIndex(2);
            }

            Sprite s = itemResetButton.transform.Find("x")?.GetComponent<Image>()?.sprite;
            planetResetButton = Util.MakeIconButtonB(s, 22);
            if (planetResetButton != null)
            {
                planetResetButton.gameObject.name = "lstm-planet-reset-btn";
                rect = Util.NormalizeRectC(planetResetButton.gameObject);
                rect.SetParent(windowTrans, false);
                rect.anchoredPosition = new Vector2(150f, 210f);
            }

            //materialView
            materialView = UIMaterialPicker.CreateView(this);
            if (materialView != null)
            {
                materialView.window = this;
                materialView.transform.SetSiblingIndex(0);
                //RectTransform rect2 = (RectTransform)materialView.gameObject.transform;
                //rect2.SetParent(windowTrans, false);
            }

            //config button
            Sprite sprite = null;
            //Sprite sprite = LDB.signals.IconSprite(504); //この時点では取れないみたい
            //こっちから取る
            UIButton[] buildingWarnButtons = UIRoot.instance.optionWindow.buildingWarnButtons;
            if (buildingWarnButtons.Length >= 4)
            {
                UIButton warnBtn = buildingWarnButtons[3];
                Image warnImg = warnBtn.transform.Find("image")?.GetComponent<Image>();
                if (warnImg != null)
                {
                    sprite = warnImg.sprite;
                }
            }
            UIButton configBtn = Util.MakeIconButtonC(sprite, 32f);
            configBtn.button.onClick.AddListener(new UnityAction(this.OnConfigButtonClick));
            rect = Util.NormalizeRectC(configBtn.gameObject);
            rect = Util.NormalizeRectWithTopLeft(configBtn, 158f, 18f, windowTrans);
            configBtn.gameObject.name = "config-button";
            configBtn.gameObject.SetActive(true);
            Image configImg = configBtn.gameObject.GetComponent<Image>();
            configImg.color = new Color(1f, 1f, 1f, 0.17f);
            if (configBtn.transitions.Length > 0)
            {
                configBtn.transitions[0].mouseoverColor = new Color(1f, 1f, 1f, 0.67f);
                configBtn.transitions[0].normalColor = new Color(1f, 1f, 1f, 0.17f);
                configBtn.transitions[0].pressedColor = new Color(1f, 1f, 1f, 0.5f);
            }

            if (LSTM.enableTrafficLogInThisSession)
            {
                //Log btn
                UIButton btn = Util.MakeSmallTextButton("LOG", 44f, 20f);
                btn.gameObject.name = "log-btn";
                Util.NormalizeRectWithTopLeft(btn, 298f, 144f, windowTrans);
                btn.onClick += OnLogButtonClick;
                btn.tips.delay = 0.8f;
                btn.tips.tipTitle = "Log".Translate();
                btn.tips.tipText = "Open Log with current state".Translate();
                btn.tips.corner = 3;
                btn.tips.offset = new Vector2(6, 38);
                btn.gameObject.SetActive(true);
            }

            CreateStarSystemBox();
            //menu
            CreateMenuBox();
        }

        private void OnDemanScrollRectChanged(Vector2 val)
        {
            AdjustScrollbar(demandListView.m_ScrollRect.verticalScrollbar);
        }

        private void OnSupplyScrollRectChanged(Vector2 val)
        {
            AdjustScrollbar(supplyListView.m_ScrollRect.verticalScrollbar);
        }

        private void AdjustScrollbar(Scrollbar bar)
        {
            if (bar.size < 0.1f)
            {
                bar.size = 0.1f;
            }
            else if (bar.size >= 0.99f)
            {
                bar.size = 0.001f;
            }
        }

        private void OnConfigButtonClick()
        {
            LSTM._configWin.OpenWindow();
        }

        private void OnLogButtonClick(int obj)
        {
            if (LSTM.enableTrafficLogInThisSession)
            {
                int planetId = balanceData.planetId;
                if (planetId == 0 && tmpPlanetId > 0 && balanceData.starId == 0)
                {
                    planetId = tmpPlanetId;
                }
                LSTM._logWindow.SetUpAndOpenStationSlot(balanceData.itemId, 0, balanceData.planetId, balanceData.starId);
            }
        }

        private void OnLocalButtonClick(int obj)
        {
            SwitchToLocal();
        }
        private void OnStarSystemButtonClick(int obj)
        {
            SwitchToStarSystem();
        }

        private void OnRemoteButtonClick(int obj)
        {
            SwitchToGlobal();
        }

        private void OnItemResetButtonClick(int obj)
        {
            balanceData = new BalanceData(0, balanceData.planetId, balanceData.isLocal, balanceData.starId);
            materialView.RefreshWithProduct(balanceData.itemId);
            SetUpData();
        }
        private void OnPlanetResetButtonClick(int obj)
        {
            balanceData = new BalanceData(balanceData.itemId, 0, balanceData.isLocal, 0/* or balanceData.starId?*/); 
            SetUpData();
        }

        private void OnSelectItemButtonClick(int obj)
        {
            if (UIItemPicker.isOpened)
            {
                UIItemPicker.Close();
                return;
            }
            UIItemPicker.Popup(windowTrans.anchoredPosition + new Vector2(-360f, 180f), new Action<ItemProto>(this.OnItemPickerReturn));
        }

        private void OnItemPickerReturn(ItemProto itemProto)
        {
            if (itemProto == null)
            {
                return;
            }
            //targetFactory = null;
            balanceData = new BalanceData(itemProto.ID, balanceData.planetId, balanceData.isLocal, balanceData.starId);
            materialView.RefreshWithProduct(balanceData.itemId);
            SetUpData();
        }



        public override void _OnLateUpdate()
        {
        }

        //ホイールズーム対策用 UITutorialWindowなどの真似
        public void OnPointerEnter(PointerEventData _eventData)
        {
            this.isPointEnter = true;
        }
        public void OnPointerExit(PointerEventData _eventData)
        {
            this.isPointEnter = false;
        }
        public void OnApplicationFocus(bool focus)
        {
            if (!focus)
            {
                this.focusPointEnter = this.isPointEnter;
                this.isPointEnter = false;
            }
            if (focus)
            {
                this.isPointEnter = this.focusPointEnter;
            }
        }

        public void TryClose()
        {
            if (keepOpen || !LSTM.actAsStandardPanel.Value)
            {
                return;
            }
            base._Close();
        }

        //menu
        public UIComboBox menuComboBox;
        UIBalanceListEntry menuTarget;
        public enum EMenuCommand
        {
            OneTimeDemand = 0,
            StationSlotLog,
        }

        public void ShowMenu(UIBalanceListEntry item)
        {
            if (menuComboBox.isDroppedDown)
            {
                menuComboBox.isDroppedDown = false;
                return;
            }

            if (item.storeType != EStoreType.Normal || !item.station.isStellar)
            {
                return;
            }

            RectTransform rect = menuComboBox.m_DropDownList;
            //anchorMax = new Vector2(1f, 0f);
            //anchorMin = new Vector2(0f, 0f);
            //pivot = new Vector2(0f, 1f);

            UIRoot.ScreenPointIntoRect(Input.mousePosition, rect.parent as RectTransform, out Vector2 pos);
            pos.x = pos.x + 20f;
            pos.y = pos.y + 30f;
            menuComboBox.m_DropDownList.anchoredPosition = pos;

            menuTarget = item;
            //menuTarget.LockAppearance();
            RefreshMenuBox(item);
            if (menuComboBox.DropDownCount > 0)
            {
                menuComboBox.OnPopButtonClick();
            }
        }

        internal void RefreshMenuBox(UIBalanceListEntry item)
        {
            List<string> items = menuComboBox.Items;
            List<int> itemsData = menuComboBox.ItemsData;
            items.Clear();
            itemsData.Clear();
            int itemCount = 0;
            if (item == null)
            {
                menuComboBox.DropDownCount = 0;
                return;
            }
            if (LSTM.enableTrafficLogInThisSession && item.station.isStellar)
            {
                items.Add("Traffic Log");
                itemsData.Add((int)EMenuCommand.StationSlotLog);
                itemCount++;
            }

            if (!item.station.isCollector)
            {
                if (LSTM.enableOneTimeDemand.Value && menuTarget.station.storage[menuTarget.index].remoteDemandCount > 0)
                {
                    items.Add("One-time Demand");
                    itemsData.Add((int)EMenuCommand.OneTimeDemand);
                    itemCount++;
                }
            }
            //if ()
            //{
            //    items.Add("label");
            //    itemsData.Add((int)EMenuCommand.);
            //    itemCount++;
            //}

            menuComboBox.DropDownCount = itemCount;

        }


        public void OnMenuBoxItemIndexChange()
        {
            if (_eventLock)
            {
                return;
            }
            int num = menuComboBox.itemIndex;
            if (num < 0) //recursion
            {
                return;
            }
            if (menuTarget != null)
            {
                EMenuCommand itemData = (EMenuCommand)menuComboBox.ItemsData[num];
                switch (itemData)
                {
                    case EMenuCommand.OneTimeDemand:
                        if (!OneTimeDemand.AddOneTimeDemand(menuTarget.station, menuTarget.index))
                        {
                            UIRealtimeTip.Popup("Supplier not found", false, 0);
                        }
                        break;
                    case EMenuCommand.StationSlotLog:
                        LSTM._logWindow.SetUpAndOpenStationSlot(menuTarget.itemId, menuTarget.station.gid);
                        break;
                    default:
                        break;
                }

                //menuTarget.UnlockAppearance();
                menuTarget = null;
            }

            menuComboBox.itemIndex = -1; //recursion
        }

        internal void CreateMenuBox()
        {
            // Main Button : Image,Button
            // -Pop sign : Image
            // -Text : Text
            // Dropdown List ScrollBox : ScrollRect

            UIStatisticsWindow statisticsWindow = UIRoot.instance.uiGame.statWindow;
            UIComboBox src = statisticsWindow.productAstroBox;
            UIComboBox box = GameObject.Instantiate<UIComboBox>(src, windowTrans);
            box.gameObject.name = "menu-box";

            RectTransform boxRect = Util.NormalizeRectWithTopLeft(box, 20f, 20f, windowTrans);

            RectTransform btnRect = box.transform.Find("Main Button")?.transform as RectTransform;
            if (btnRect != null)
            {
                btnRect.pivot = new Vector2(1f, 0f);
                btnRect.anchorMax = Vector2.zero;
                btnRect.anchorMin = Vector2.zero;
                btnRect.anchoredPosition = new Vector2(boxRect.sizeDelta.x, 0f);
                btnRect.sizeDelta = new Vector2(20, boxRect.sizeDelta.y);

                Button btn = btnRect.GetComponent<Button>();
                btnRect.Find("Text")?.gameObject.SetActive(false);
                btnRect.gameObject.SetActive(false);
            }

            box.onItemIndexChange.AddListener(OnMenuBoxItemIndexChange);
            menuComboBox = box;

            //Dropdown List ScrollBox
            RectTransform vsRect = menuComboBox.m_Scrollbar.transform as RectTransform;
            vsRect.sizeDelta = new Vector2(0, vsRect.sizeDelta.y);

            RefreshMenuBox(null);
        }
    }
}
