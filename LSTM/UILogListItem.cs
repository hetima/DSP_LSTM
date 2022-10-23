using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Runtime.InteropServices.ComTypes;


namespace LSTMMod
{
    public class UILogListItem : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        public static Sprite circleSprite;
        public UILogWindow window;
        public TrafficLogData logData;
        public int itemId;

        [SerializeField]
        public Text nameText;

        [SerializeField]
        public Text valueText;

        [SerializeField]
        public Text valueSketchText;

        [SerializeField]
        public Image labelIcon;

        [SerializeField]
        public Image veinIcon;

        [SerializeField]
        public Sprite iconHide;

        [SerializeField]
        public UIButton iconButton;

        [SerializeField]
        public UIButton locateBtn;

        [SerializeField]
        public GameObject baseObj;

        public static void CreateListViewPrefab(UIData data)
        {
            RectTransform rect;
            UILogListItem item = data.gameObject.AddComponent<UILogListItem>();
            data.com_data = item;
            Image bg = Util.CreateGameObject<Image>("list-item", 600f, 24f);
            bg.gameObject.SetActive(true);
            RectTransform baseTrans = Util.NormalizeRectWithTopLeft(bg, 0f, 0f, item.transform);
            item.baseObj = bg.gameObject;

            bg.color = new Color(0.2f, 0.2f, 0.2f, 0.1f);
            (item.transform as RectTransform).sizeDelta = new Vector2(600f, 24f);
            baseTrans.sizeDelta = new Vector2(600f, 24f);
            baseTrans.localScale = Vector3.one;

            UIResAmountEntry src = GameObject.Instantiate<UIResAmountEntry>(UIRoot.instance.uiGame.planetDetail.entryPrafab, baseTrans);
            src.gameObject.SetActive(true);

            float rightPadding = 0f;
            float leftPadding = 22f;

            //locate button
            UIButton btn = Util.MakeIconButtonB(Util.astroIndicatorIcon, 22);
            if (btn != null)
            {
                btn.gameObject.name = "locate-btn";
                rect = Util.NormalizeRectWithTopLeft(btn, 534f - rightPadding, 1f, baseTrans);

                //btn.onClick +=
                btn.tips.tipTitle = "Locate Planet".Translate();
                btn.tips.tipText = "Show the planet on the starmap.".Translate();
                btn.tips.corner = 3;
                btn.tips.offset = new Vector2(18f, -20f);
                item.locateBtn = btn;
            }

            //nameText
            item.nameText = src.labelText;
            item.nameText.text = "";
            item.nameText.fontSize = 16;
            item.nameText.rectTransform.anchoredPosition = new Vector2(8f + leftPadding, 0f);

            //valueText
            item.valueText = src.valueText;
            item.valueText.text = "";
            item.valueText.color = item.nameText.color;
            item.valueText.supportRichText = true;
            rect = Util.NormalizeRectWithTopLeft(item.valueText, 380f - rightPadding - leftPadding, 2f);
            rect.sizeDelta = new Vector2(100f, 24f);

            //valueSketchText
            item.valueSketchText = GameObject.Instantiate<Text>(item.valueText, item.valueText.transform.parent);
            item.valueSketchText.gameObject.name = "valueSketchText";
            item.valueSketchText.alignment = TextAnchor.UpperLeft;
            rect = Util.NormalizeRectWithTopLeft(item.valueSketchText, 504f - rightPadding - leftPadding, 2f);
            rect.sizeDelta = new Vector2(24f, 24f);

            //veinIcon
            item.veinIcon = src.iconImage;
            if (item.veinIcon != null)
            {
                rect = Util.NormalizeRectWithTopLeft(item.veinIcon, 482f - rightPadding - leftPadding, 2f);
                item.veinIcon.enabled = true;

                //labelIcon
                item.labelIcon = GameObject.Instantiate<Image>(item.veinIcon, baseTrans);
                item.labelIcon.gameObject.name = "labelIcon";
                rect = Util.NormalizeRectWithTopLeft(item.labelIcon, 16f, 12f);
                rect.pivot = new Vector2(0.5f, 0.5f);
                rect.sizeDelta = new Vector2(24f, 24f);
                rect.localScale = new Vector3(0.3f, 0.3f, 1f);

                item.labelIcon.material = null; //これのせいでめっちゃ光る
                UIStationWindow stationWindow = UIRoot.instance.uiGame.stationWindow;
                circleSprite = stationWindow.storageUIPrefab.transform.Find("storage-icon-empty/white")?.GetComponent<Image>()?.sprite;
                item.labelIcon.sprite = circleSprite;
            }

            item.iconHide = src.iconHide;
            item.iconButton = src.iconButton;
            //GameObject.Destroy(src.valueText.gameObject);
            GameObject.Destroy(src);

        }
        void Start()
        {
            locateBtn.onClick += OnLocateButtonClick;
            locateBtn.gameObject.SetActive(false);
        }
        public string DisplayNameForPlanet(PlanetData planetData)
        {
            string prefix = "";
            if (true)
            {
                if (planetData.type == EPlanetType.Gas)
                {
                    prefix += "[GAS]";
                    return prefix + planetData.displayName;
                }
            }
            return planetData.displayName;
            
        }

        public void Init(in TrafficLogData d, UILogWindow window_)
        {
            window = window_;
            logData = d;
            itemId = d.itemId;
            //DisplayNameForPlanet()
            nameText.text = DisplayNameForPlanet(d.fromPlanetData);
            valueText.text = DisplayNameForPlanet(d.toPlanetData);
        }
        private void OnLocateButtonClick(int obj)
        {
            //LSTM.LocatePlanet(planetData.id);
        }

        public bool RefreshValues(bool shown, bool onlyNewlyEmerged = false)
        {
            //このSetActive特に不要か
            if (shown != baseObj.activeSelf)
            {
                baseObj.SetActive(shown);
            }
            else if (onlyNewlyEmerged)
            {
                return true;
            }
            if (!shown)
            {
                return true;
            }
            if (itemId != 0)
            {
            }
            else
            {
            }
            valueSketchText.text = logData.time;
            return true;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Right)
            {
                //window.ShowMenu(this);
            }
        }

        public void OnPointerEnter(PointerEventData _eventData)
        {
            locateBtn.gameObject.SetActive(true);
        }

        public void OnPointerExit(PointerEventData _eventData)
        {
            locateBtn.gameObject.SetActive(false);
        }

        public void LockAppearance()
        {
            Button b = GetComponent<Button>();
            if (b != null)
            {
                b.enabled = false;
            }
        }

        public void UnlockAppearance()
        {
            Button b = GetComponent<Button>();
            if (b != null)
            {
                b.enabled = true;
            }
        }
    }
}
