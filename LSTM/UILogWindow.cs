using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;


namespace LSTMMod
{
    public class UILogWindow : ManualBehaviour, IPointerEnterHandler, IPointerExitHandler, IEventSystemHandler, MyWindow
    {

        public RectTransform windowTrans;
        public RectTransform contentTrans;

        public UIListView planetListView;
        public Text countText;

        public int targetStationGid;
        public int targetIndex;
        public int targetItemId;

        internal bool _eventLock;
        public bool isPointEnter;
        private bool focusPointEnter;

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

        public void SetUpAndOpenItem(int _itemId)
        {
            targetItemId = _itemId;
            SetUpData();
            //UIRoot.instance.uiGame.ShutPlayerInventory();
            MyWindowCtl.OpenWindow(this);
        }

        public void SetUpAndOpenStationSlot(int stationGid, int stationIndex)
        {
            targetStationGid = stationGid;
            targetIndex = stationIndex;
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
            Image bg = Util.CreateGameObject<Image>("list-bg", 100f, 100f);
            bg.color = new Color(0f, 0f, 0f, 0.56f);
            Util.NormalizeRectWithMargin(bg, 70f, 0f, 20f, 16f, contentTrans);

            planetListView = Util.CreateListView(UILogListItem.CreateListViewPrefab, "list-view", null, 16f);
            Util.NormalizeRectWithMargin(planetListView.transform, 0f, 0f, 0f, 0f, bg.transform);
            //ここでサイズ調整…
            //(planetListView.m_ItemRes.com_data.transform as RectTransform).sizeDelta = new Vector2(600f, 24f);

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


            //menu
            //CreateMenuBox();

            _eventLock = false;
        }

        private void OnScrollRectChanged(Vector2 val)
        {
            if (planetListView.m_ScrollRect.verticalScrollbar.size < 0.1f)
            {
                planetListView.m_ScrollRect.verticalScrollbar.size = 0.1f;
            }
            else if (planetListView.m_ScrollRect.verticalScrollbar.size >= 0.99f)
            {
                planetListView.m_ScrollRect.verticalScrollbar.size = 0.001f;
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
            planetListView.m_ScrollRect.onValueChanged.AddListener(OnScrollRectChanged);
        }

        protected override void _OnUnregEvent()
        {
            planetListView.m_ScrollRect.onValueChanged.RemoveListener(OnScrollRectChanged);
        }

        protected override void _OnOpen()
        {

        }
        protected override void _OnClose()
        {
            planetListView.Clear();
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

            if (_logList.Count > 0)
            {
                AddToListView(planetListView, 5, _logList);
            }

            bool valid = true;
            int step = Time.frameCount % 30;

            if (Input.GetKeyDown(KeyCode.Tab))
            {
                //int current = targetItemId;
                //targetItemId = itemSelection.NextTargetItemId(targetItemId, PLFN.showPowerState.Value);
                //if (targetItemId != current)
                //{
                //    valid = RefreshListView(planetListView);
                //}
            }
            else if (step == 0)
            {
                valid = RefreshListView(planetListView);
                //UIListViewのStart()で設定されるのでその後に呼ぶ必要がある
                planetListView.m_ScrollRect.scrollSensitivity = 28f;
            }
            else
            {
                RefreshListView(planetListView, true);
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

        internal List<TrafficLogData> _logList = new List<TrafficLogData>(2000);
        public void SetUpData()
        {
            _eventLock = true;
            _logList.Clear();
            planetListView.Clear();
            //targetItemId = itemSelection.lastSelectedItemId;

            SetUpItemList();
            //_logList.Sort((a, b) => a.distanceForSort - b.distanceForSort);

            _eventLock = false;
            if (_logList.Count > 0)
            {
                AddToListView(planetListView, 20, _logList);
            }
            RefreshListView(planetListView);

        }

        public void SetUpItemList()
        {
            int count = 0;
            countText.text = "";

            if (targetStationGid!=0)
            {
                foreach (var item in TrafficLog.TrafficLogDataForStationSlot(targetStationGid, targetIndex))
                {
                    if (item == null)
                    {
                        break;
                    }
                    AddStore(item);
                    count++;
                }
                countText.text = "Result: " + count.ToString();

            }
            else if (targetItemId != 0)
            {
                foreach (var item in TrafficLog.TrafficLogDataForItem(targetItemId))
                {
                    if (item == null)
                    {
                        break;
                    }
                    AddStore(item);
                    count++;
                }
                countText.text = "Result: " + count.ToString();

            }

        }

        internal void AddStore(TrafficLogData data, int sortIndex = -1)
        {
            _logList.Add(data);
        }

        internal int AddToListView(UIListView listView, int count, List<TrafficLogData> list)
        {
            if (list.Count < count)
            {
                count = list.Count;
            }
            if (count == 0)
            {
                return count;
            }

            for (int i = 0; i < count; i++)
            {
                TrafficLogData d = list[0];
                list.RemoveAt(0);
                UILogListItem e = listView.AddItem<UILogListItem>();
                e.Init(in d, this);
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



    }
}
