
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using System;
using System.Text;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
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
        public BalanceData(int _itemId,  int _planetId, bool _isLocal)
        {
            itemId = _itemId;
            planetId = _planetId;
            isLocal = _isLocal;
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

        public bool isPointEnter;

        private bool focusPointEnter;

        public bool isFunctionWindow()
        {
            return true;
        }

        public void SetUpAndOpen(int _itemId, int _planetId, bool _isLocal)
        {
            balanceData = new BalanceData(_itemId, _planetId, _isLocal);
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

        protected override void _OnCreate()
        {
            _eventLock = true;
            windowTrans = MyWindowCtl.GetRectTransform(this);
            windowTrans.sizeDelta = new Vector2(700, 640);
            balanceData = new BalanceData(0, 0, false);
            displayModes = new DisplayMode[2];
            displayModes[0].useStationName = false;
            displayModes[1].useStationName = true;
            currentDisplayMode = 0;
            CreateListViews();
            CreateUI();
        }

        protected override void _OnDestroy()
        {
        }

        protected override bool _OnInit()
        {
            //UIStorageGrid srcWin = UIRoot.instance.uiGame.inventory;
            windowTrans.anchoredPosition = new Vector2(-480, 0);
            return true;
        }

        protected override void _OnFree()
        {
        }

        protected override void _OnRegEvent()
        {
            itemButton.onClick += OnSelectItemButtonClick;
            itemResetButton.onClick += OnItemResetButtonClick;
            planetResetButton.onClick += OnPlanetResetButtonClick;
            localButton.onClick += OnLocalButtonClick;
            remoteButton.onClick += OnRemoteButtonClick;

        }

        protected override void _OnUnregEvent()
        {
            itemButton.onClick -= OnSelectItemButtonClick;
            itemResetButton.onClick -= OnItemResetButtonClick;
            planetResetButton.onClick -= OnPlanetResetButtonClick;
            localButton.onClick -= OnLocalButtonClick;
            remoteButton.onClick -= OnRemoteButtonClick;
        }

        protected override void _OnOpen()
        {

        }

        protected override void _OnClose()
        {
            supplyListView.Clear();
            demandListView.Clear();
            isPointEnter = false;
        }

        protected override void _OnUpdate()
        {

            if (VFInput.escape && !UIRoot.instance.uiGame.starmap.active && !VFInput.inputing)
            {
                VFInput.UseEscape();
                base._Close();
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
        }

        public void Filter(int itemId, int planetId = 0)
        {
            if (itemId != balanceData.itemId)
            {
                balanceData = new BalanceData(itemId, balanceData.planetId, balanceData.isLocal);
                SetUpData();
            }
            //else if (planetId != balanceData.planetId)
            //{
            //    balanceData = new BalanceData(balanceData.itemId, planetId, balanceData.isLocal);
            //    SetUpData();
            //}
        }


        public void SwitchToLocal()
        {
            if (balanceData.isLocal)
            {
                return;
            }
            balanceData = new BalanceData(balanceData.itemId, balanceData.planetId, !balanceData.isLocal);
            SetUpData();
        }

        public void SwitchToGlobal()
        {
            if (!balanceData.isLocal)
            {
                return;
            }
            balanceData = new BalanceData(balanceData.itemId, balanceData.planetId, !balanceData.isLocal);
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
            if (!balanceData.isLocal && balanceData.itemId > 0)
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
            remoteButton.highlighted = !balanceData.isLocal;

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
            string planetName;
            if (balanceData.itemId > 0 && !balanceData.isLocal)
            {
                planetName = "(All Planets)".Translate();
                planetResetButton.gameObject.SetActive(false);
            }
            else if (balanceData.planetId <= 0)
            {
                planetResetButton.gameObject.SetActive(false);
                if (tmpPlanetId > 0)
                {
                    planetName = "(" + GameMain.galaxy.PlanetById(tmpPlanetId).displayName + ")";
                }
                else
                {
                    planetName = "";
                }
            }
            else
            {
                planetResetButton.gameObject.SetActive(true);
                planetName = GameMain.galaxy.PlanetById(balanceData.planetId).displayName;
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
                //itemId なし
                if (itemId <= 0)
                {
                    AddFromPlanet(planetId, itemId, balanceData.isLocal);
                    return;
                }

                //planetId あり itemId ありの場合 planetId は無視

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
                        int length = cmp.storage.Length;
                        if (length > 5)
                        {
                            length -= 1;
                        }
                        for (int j = 0; j < length; j++)
                        {
                            if(cmp.storage[j].itemId == itemId)
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
                                        gasSupplyPlanets.Add(cmp.planetId);
                                    }
                                    else
                                    {
                                        gasStoragePlanets.Add(cmp.planetId);
                                    }
                                    AddStore(cmp, j, cmp.planetId, itemId, maxCount, EStoreType.Gas);
                                }
                                else
                                {
                                    AddStore(cmp, j, cmp.planetId, itemId, maxCount);
                                }
                                break;
                            }
                        }
                    }
                }

                int gasMaxCount = LSTM.RemoteStationMaxItemCount() / 2;
                foreach (var gasPlanetId in gasSupplyPlanets)
                {
                    AddStore(null, 0, gasPlanetId, itemId, gasMaxCount, EStoreType.GasStubSupply);
                }
                foreach (var gasPlanetId in gasStoragePlanets)
                {
                    AddStore(null, 0, gasPlanetId, itemId, gasMaxCount, EStoreType.GasStubStorage);
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
                    return;
                }
            }
            else
            {
                factory = GameMain.localPlanet?.factory;
                if (factory == null)
                {
                    return;
                }
                tmpPlanetId = GameMain.localPlanet.id;
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
                    if (length > 5)
                    {
                        length -= 1;
                    }
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
                            list = _storageList;
                            break;
                        case ELogisticStorage.Supply:
                            list = _supplyList;
                            break;
                        case ELogisticStorage.Demand:
                            list = _demandList;
                            break;
                        default:
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
                    distanceForSort = (int)(distancef * 100);
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
        public Text demandText;
        public Text supplyText;


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
            supplyText = go.GetComponent<Text>();
            supplyText.text = "Supply".Translate();
            supplyText.color = Util.DSPBlue;
            rect = Util.NormalizeRectC(go);
            rect.SetParent(windowTrans, false);
            rect.sizeDelta = new Vector2(80, rect.sizeDelta.y);
            rect.anchoredPosition = new Vector2(-310f, 168f); //demand

            go = GameObject.Instantiate(go, windowTrans);
            go.name = "demand-label";
            demandText = go.GetComponent<Text>();
            demandText.text = "Demand".Translate();
            demandText.color = Util.DSPOrange;

            rect.pivot = new Vector2(1f, 0.5f); //supply
            rect.anchoredPosition = new Vector2(310f, 168f); //supply


            //local remote button
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
            rect.sizeDelta = new Vector2(60f, 32f);
            rect.anchoredPosition = new Vector2(270f, 240f); //remote
            txt.text = "Global";
            go.SetActive(true);

            go = GameObject.Instantiate(go, windowTrans);
            go.name = "remote-button";
            rect.anchoredPosition = new Vector2(270f, 202f); //local
            txt.text = "Local";

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
            Transform transform = UIRoot.instance.uiGame.inventory.rectTrans;
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
        }

        private void OnLocalButtonClick(int obj)
        {
            SwitchToLocal();
        }

        private void OnRemoteButtonClick(int obj)
        {
            SwitchToGlobal();
        }

        private void OnItemResetButtonClick(int obj)
        {
            balanceData = new BalanceData(0, balanceData.planetId, balanceData.isLocal);
            SetUpData();
        }
        private void OnPlanetResetButtonClick(int obj)
        {
            balanceData = new BalanceData(balanceData.itemId, 0, balanceData.isLocal);
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
            balanceData = new BalanceData(itemProto.ID, balanceData.planetId, balanceData.isLocal);
            SetUpData();
        }




        protected override void _OnLateUpdate()
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
    }
}
