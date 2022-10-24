using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using Steamworks;
using static UnityEngine.EventSystems.EventTrigger;
using System.Security.Policy;


namespace LSTMMod
{
    public class UILogWindow : ManualBehaviour, IPointerEnterHandler, IPointerExitHandler, IEventSystemHandler, MyWindow, TrafficLogDelegate
    {

        public RectTransform windowTrans;
        public RectTransform contentTrans;

        public UIListView logListView;
        public Text countText;
        public int entryCount;
        public float newest;
        public float oldest;

        public int targetStationGid;
        //public int targetIndex;
        public int targetItemId;
        public int targetPlanetId;
        public int targetStarId;

        internal bool _eventLock;
        public bool isPointEnter;
        private bool focusPointEnter;

        //public Text itemText;
        public UIButton itemButton;
        public UIButton itemResetButton;
        public Image itemImage;
        public Image itemCircle;
        public static Sprite defaultItemSprite = null;

        public static UILogWindow CreateInstance()
        {
            UILogWindow win = MyWindowCtl.CreateWindow<UILogWindow>("LSTMLogWindow", "Traffic Log");

            return win;
        }

        public void BeginGame()
        {
            if (DSPGame.IsMenuDemo)
            {
                return;
            }
            targetItemId = 0;
            targetStationGid = 0;
        }

        //public void SetUpAndOpenItem(int _itemId)
        //{
        //    targetItemId = _itemId;
        //    targetStationGid = 0;
        //    SetUpData();
        //    //UIRoot.instance.uiGame.ShutPlayerInventory();
        //    MyWindowCtl.OpenWindow(this);
        //}

        public void SetUpAndOpenStationSlot(int itemId, int stationGid = 0, int planetId = 0, int StarId = 0)
        {
            targetItemId = itemId;
            targetStationGid = stationGid;
            //targetIndex = stationIndex;
            targetPlanetId = planetId;
            targetStarId = StarId;
            SetUpData();
            //UIRoot.instance.uiGame.ShutPlayerInventory();
            MyWindowCtl.OpenWindow(this);
        }

        public bool isFunctionWindow()
        {
            return false;
        }

        public Vector2 WindowSize()
        {
            float rows = Mathf.Round(8);
            if (rows < 4f)
            {
                rows = 4f;
            }
            if (rows > 16f)
            {
                rows = 16f;
            }
            return new Vector2(640, 174 + 28 * rows);
        }


        protected override void _OnCreate()
        {
            _eventLock = true;

            windowTrans = MyWindowCtl.GetRectTransform(this);
            windowTrans.sizeDelta = WindowSize();

            GameObject go = new GameObject("content");
            contentTrans = go.AddComponent<RectTransform>();
            Util.NormalizeRectWithMargin(contentTrans, 60f, 28f, 20f, 28f, windowTrans);

            //listview
            Image bgImage = Util.CreateGameObject<Image>("list-bg", 100f, 100f);
            bgImage.color = new Color(0f, 0f, 0f, 0.56f);
            Util.NormalizeRectWithMargin(bgImage, 70f, 0f, 20f, 16f, contentTrans);

            logListView = Util.CreateListView(UILogListItem.CreateListViewPrefab, "list-view", null, 16f);
            Util.NormalizeRectWithMargin(logListView.transform, 0f, 0f, 0f, 0f, bgImage.transform);
            //ここでサイズ調整…
            //(logListView.m_ItemRes.com_data.transform as RectTransform).sizeDelta = new Vector2(600f, 24f);

            ////scope buttons
            //float scopex_ = 4f;
            //UIButton AddScope(string label, int data)
            //{
            //    UIDESwarmPanel swarmPanel = UIRoot.instance.uiGame.dysonEditor.controlPanel.hierarchy.swarmPanel;
            //    UIButton src = swarmPanel.orbitButtons[0];
            //    UIButton btn = GameObject.Instantiate<UIButton>(src);
            //    // btn.transitions[0] btn btn.transitions[1]==text btn.transitions[2]==frame
            //    if (btn.transitions.Length >= 2)
            //    {
            //        btn.transitions[0].normalColor = new Color(0.1f, 0.1f, 0.1f, 0.68f);
            //        btn.transitions[0].highlightColorOverride = new Color(0.9906f, 0.5897f, 0.3691f, 0.4f);
            //        btn.transitions[1].normalColor = new Color(1f, 1f, 1f, 0.6f);
            //        btn.transitions[1].highlightColorOverride = new Color(0.2f, 0.1f, 0.1f, 0.9f);
            //    }
            //    Text btnText = btn.transform.Find("Text").GetComponent<Text>();
            //    btnText.text = label;
            //    if (btnText.font.name == "MPMK85") //JP MOD
            //    {
            //        btnText.fontSize = 16;
            //    }
            //    else // "DIN"
            //    {
            //        btnText.fontSize = 14;
            //    }
            //    btn.data = data;

            //    RectTransform btnRect = Util.NormalizeRectWithTopLeft(btn, scopex_, 0f, contentTrans);
            //    btnRect.sizeDelta = new Vector2(btnText.preferredWidth + 14f, 22f);
            //    btn.transform.Find("frame").gameObject.SetActive(false);
            //    //(btn.transform.Find("frame").transform as RectTransform).sizeDelta = btnRect.sizeDelta;
            //    scopex_ += btnRect.sizeDelta.x + 0f;

            //    return btn;
            //}
            //scopeButtons = new List<UIButton>(8);
            ////scopeButtons.Add(AddScope("Star", (int)Scope.Star));
            //scopeButtons.Add(AddScope("All Planet", (int)Scope.Planet));
            //scopeButtons.Add(AddScope("Current Star", (int)Scope.CurrentStar));
            //scopeButtons.Add(AddScope("Has Factory", (int)Scope.HasFactory));
            ////scopeButtons.Add(AddScope("★", (int)Scope.Fav));
            //scopeButtons.Add(AddScope("Recent", (int)Scope.Recent));
            //scope = Scope.Planet;
            //foreach (var btn in scopeButtons)
            //{
            //    btn.highlighted = (btn.data == (int)scope);
            //}

            countText = Util.CreateText("", 14, "result-count");
            Util.NormalizeRectWithBottomLeft(countText, 2f, 0f, contentTrans);

            if (defaultItemSprite == null)
            {
                defaultItemSprite = Util.LoadSpriteResource("Icons/Tech/1414");
            }

            Transform bgTrans;
            RectTransform rect;
            UIAssemblerWindow assemblerWindow = UIRoot.instance.uiGame.assemblerWindow;

            //icon
            bgTrans = assemblerWindow.resetButton.transform.parent; //circle-back
            if (bgTrans != null)
            {
                go = GameObject.Instantiate(bgTrans.gameObject);
                Transform btnTrans = go.transform.Find("product-icon");
                if (btnTrans != null)
                {
                    itemResetButton = go.transform.Find("stop-btn")?.GetComponent<UIButton>();
                    go.transform.Find("cnt-text")?.gameObject.SetActive(false);
                    //GameObject.Destroy(go.GetComponent<EventTrigger>());
                    GameObject.Destroy(go.transform.Find("circle-fg-1")?.gameObject);
                    GameObject.Destroy(go.transform.Find("product-icon-1")?.gameObject);
                    GameObject.Destroy(go.transform.Find("cnt-text-1")?.gameObject);

                    itemButton = btnTrans.GetComponent<UIButton>();
                    itemButton.tips.tipTitle = "Select Item".Translate();
                    itemButton.tips.tipText = "Select item to display".Translate();
                    itemButton.tips.corner = 3;
                    itemButton.tips.offset = new Vector2(16, 16);
                    itemCircle = go.transform.Find("circle-fg")?.GetComponent<Image>();
                    itemCircle.color = Util.DSPBlue;
                    itemImage = btnTrans.GetComponent<Image>();
                    itemImage.sprite = defaultItemSprite;
                    rect = Util.NormalizeRectD(go);
                    //rect.localScale = new Vector3(1f, 1f, 1f);
                    rect.SetParent(windowTrans, false);
                    rect.anchoredPosition = new Vector2(30f, -54f);
                    go.name = "item-button";
                    go.SetActive(true);
                }

            }

            //reload btn
            UIButton btn = Util.MakeSmallTextButton("Reload", 44f, 20f);
            btn.transform.SetParent(contentTrans, false);
            btn.onClick += OnReloadButtonClick;
            btn.tips.delay = 0.8f;
            btn.tips.tipTitle = "Reload".Translate();
            btn.tips.tipText = "Click to reload data".Translate();
            btn.tips.corner = 3;
            btn.tips.offset = new Vector2(6, 38);
            rect = btn.transform as RectTransform;
            rect.anchorMax = new Vector2(0f, 1f);
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchoredPosition = new Vector2(580f, -46f);
            btn.gameObject.SetActive(true);



            //menu
            //CreateMenuBox();

            _eventLock = false;
        }

        private void OnScrollRectChanged(Vector2 val)
        {
            if (logListView.m_ScrollRect.verticalScrollbar.size < 0.1f)
            {
                logListView.m_ScrollRect.verticalScrollbar.size = 0.1f;
            }
            else if (logListView.m_ScrollRect.verticalScrollbar.size >= 0.99f)
            {
                logListView.m_ScrollRect.verticalScrollbar.size = 0.001f;
            }
        }
        protected override void _OnDestroy()
        {

        }

        protected override bool _OnInit()
        {
            windowTrans.anchoredPosition = new Vector2(370f, -446f + (windowTrans.sizeDelta.y / 2)); // pivot=0.5 なので /2
            //PLFN.mainWindowSize.SettingChanged += (sender, args) => {
            //    windowTrans.sizeDelta = WindowSize();
            //};
            //menuTarget = null;
            return true;
        }

        protected override void _OnFree()
        {

        }

        protected override void _OnRegEvent()
        {
            itemButton.onClick += OnSelectItemButtonClick;
            itemResetButton.onClick += OnItemResetButtonClick;
            logListView.m_ScrollRect.onValueChanged.AddListener(OnScrollRectChanged);

        }

        protected override void _OnUnregEvent()
        {
            itemButton.onClick -= OnSelectItemButtonClick;
            itemResetButton.onClick -= OnItemResetButtonClick;
            logListView.m_ScrollRect.onValueChanged.RemoveListener(OnScrollRectChanged);
        }

        protected override void _OnOpen()
        {

        }
        protected override void _OnClose()
        {
            isPointEnter = false;
        }
        protected override void _OnUpdate()
        {
            if (VFInput.escape && !UIRoot.instance.uiGame.starmap.active && !VFInput.inputing)
            {
                VFInput.UseEscape();
                if (LSTM._configWin.active)
                {
                    LSTM._configWin._Close();
                }
                //else if (menuComboBox.isDroppedDown)
                //{
                //    menuComboBox.isDroppedDown = false;
                //}
                else
                {
                    base._Close();
                }
            }
            if (_eventLock)
            {
                return;
            }

            bool valid = true;
            int step = Time.frameCount % 30;

            if (Input.GetKeyDown(KeyCode.Tab))
            {
                //int current = targetItemId;
                //targetItemId = itemSelection.NextTargetItemId(targetItemId, PLFN.showPowerState.Value);
                //if (targetItemId != current)
                //{
                //    valid = RefreshListView(logListView);
                //}
            }
            else if (step == 0)
            {
                valid = RefreshListView(logListView);
                //UIListViewのStart()で設定されるのでその後に呼ぶ必要がある
                logListView.m_ScrollRect.scrollSensitivity = 28f;
            }
            else
            {
                RefreshListView(logListView, true);
            }
            if (!valid)
            {
                SetUpData();
            }

            //if (!menuComboBox.isDroppedDown && menuTarget != null)
            //{
            //    menuTarget.UnlockAppearance();
            //    menuTarget = null;
            //}
        }

        public void TryClose()
        {
            base._Close();
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

        //internal List<TrafficLogData> _logList = new List<TrafficLogData>(2000);

        public bool sholdShowLogData(UILogListItem listItem)
        {
            TrafficLogData logData = listItem.logData;
            if (targetStarId != 0)
            {
                if ((logData.fromPlanet / 100 != targetStarId) && (logData.toPlanet / 100 != targetStarId))
                {
                    return false;
                }
            }
            if (targetPlanetId != 0)
            {
                if ((logData.fromPlanet != targetPlanetId) && (logData.toPlanet != targetPlanetId))
                {
                    return false;
                }
            }
            if (targetItemId != 0)
            {
                if (logData.itemId != targetItemId)
                {
                    return false;
                }
            }
            if (targetStationGid != 0)
            {
                if ((logData.toStationGid != targetStationGid)
                    && (logData.fromStationGid != targetStationGid))
                {
                    return false;
                }
            }
            return true;
        }

        public void TrafficLogReseted()
        {
            _eventLock = true;
            logListView.Clear();
            _logList.Clear();
            _eventLock = false;
        }
        public void TrafficLogAdded(TrafficLogData logData)
        {
            AddStore(logData);
        }

        public void SetUpData()
        {
            _eventLock = true;
            //targetItemId = itemSelection.lastSelectedItemId;

            SetUpItemList();
            SetUpItemUI();
            //_logList.Sort((a, b) => a.distanceForSort - b.distanceForSort);

            _eventLock = false;

            RefreshListView(logListView);

        }

        internal void SetUpItemUI()
        {
            int itemId = targetItemId;
            if (itemId <= 0)
            {
                itemCircle.fillAmount = 0f;
                itemResetButton.gameObject.SetActive(false);
                itemImage.sprite = defaultItemSprite;
                //itemText.text = "";
            }
            else
            {
                itemCircle.fillAmount = 1f;
                itemResetButton.gameObject.SetActive(true);
                ItemProto itemProto = LDB.items.Select(itemId);
                if (itemProto != null)
                {
                    itemImage.sprite = itemProto.iconSprite;
                    //itemText.text = itemProto.name;
                }
            }
        }

        public void SetUpItemList()
        {
            int entryMax = 9999;
            entryCount = 0;
            countText.text = "";
            newest = 0f;
            oldest = Time.realtimeSinceStartup;

            if (_logList.Count >= entryMax)
            {
                logListView.Clear();
            }
            else
            {
                int total = _logList.Count + logListView.ItemCount;
                if (total > entryMax)
                {
                    logListView.RemoveRange(total - entryMax, logListView.ItemCount);
                }
            }
            foreach (TrafficLogData item in _logList)
            {
                AddToListView(item);
            }
            _logList.Clear();

            for (int i = 0; i < logListView.ItemCount; i++)
            {
                UILogListItem item = logListView.GetItem<UILogListItem>(i);
                if (item == null)
                {
                    break;
                }
                if (sholdShowLogData(item))
                {
                    item.gameObject.SetActive(true);
                    entryCount++;
                    if (newest < item.logData.realtimeSinceStartup)
                    {
                        newest = item.logData.realtimeSinceStartup;
                    }
                    if (oldest > item.logData.realtimeSinceStartup)
                    {
                        oldest = item.logData.realtimeSinceStartup;
                    }
                }
                else
                {
                    item.gameObject.SetActive(false);
                }
            }
            RefleshCountText();
        }

        public void RefleshCountText()
        {
            string tpmString = "";
            if (entryCount > 1)
            {
                float duration = newest - oldest;
                if (duration > 30)
                {
                    float tpm = (float)entryCount / (duration / 60f);
                    tpmString = "    (" + tpm.ToString("F1") + " per min)";
                }

            }
            countText.text = "Result: " + entryCount.ToString() + tpmString;
        }

        internal List<TrafficLogData> _logList = new List<TrafficLogData>(2000);
        internal void AddStore(TrafficLogData data, int sortIndex = -1)
        {
            _logList.Add(data);
        }

        internal void AddToListView(TrafficLogData logData)
        {
            //RectTransform contentRect = (RectTransform)logListView.m_ContentPanel.transform;

            UILogListItem item = logListView.InsertItem<UILogListItem>(0);
            item.Init(in logData, this);
            //if (sholdShowLogData(item))
            //{
            //    item.gameObject.SetActive(true);
            //    entryCount++;
            //}
            //else
            //{
            //    item.gameObject.SetActive(false);
            //}
            //if (contentRect.anchoredPosition.y != 0)
            //{
            //    contentRect.anchoredPosition = new Vector2(contentRect.anchoredPosition.x, contentRect.anchoredPosition.y + 28f);
            //}
            //newest = item.logData.realtimeSinceStartup;
            //RefleshCountText();
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
                UILogListItem e = (UILogListItem)data.com_data;
                if (e != null)
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
        private void OnReloadButtonClick(int obj)
        {
            _eventLock = true;
            SetUpItemList();
            _eventLock = false;
        }

        private void OnStarResetButtonClick(int obj)
        {
            targetStarId = 0;
            targetPlanetId = 0;
            targetStationGid = 0;
            SetUpData();
        }
        private void OnPlanetResetButtonClick(int obj)
        {
            targetPlanetId = 0;
            targetStationGid = 0;
            SetUpData();
        }

        private void OnStationResetButtonClick(int obj)
        {
            targetStationGid = 0;
            SetUpData();
        }

        private void OnItemResetButtonClick(int obj)
        {
            targetItemId = 0;
            targetStationGid = 0;
            SetUpData();
        }

        private void OnSelectItemButtonClick(int obj)
        {
            if (UIItemPicker.isOpened)
            {
                UIItemPicker.Close();
                return;
            }
            UIItemPicker.Popup(windowTrans.anchoredPosition + new Vector2(-220f, 180f), new Action<ItemProto>(this.OnItemPickerReturn));
        }

        private void OnItemPickerReturn(ItemProto itemProto)
        {
            if (itemProto == null)
            {
                return;
            }

            targetItemId = itemProto.ID;
            targetStationGid = 0;
            SetUpData();
        }

    }
}
