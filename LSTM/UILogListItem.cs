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
        public Text demandText;

        [SerializeField]
        public Text supplyText;

        [SerializeField]
        public Text distText;

        [SerializeField]
        public Text timeText;

        [SerializeField]
        public Image labelIcon;


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


            src.labelText.text = "";
            src.labelText.enabled = false;
            //demandText
            //item.demandText = src.labelText;
            item.demandText = GameObject.Instantiate<Text>(src.valueText, src.valueText.transform.parent);
            item.demandText.gameObject.name = "demandText";

            item.demandText.text = "";
            item.demandText.supportRichText = true;
            item.demandText.supportRichText = false;
            item.demandText.fontSize = 14;
            item.demandText.alignment = TextAnchor.MiddleRight;
            //item.demandText.rectTransform.anchoredPosition = new Vector2(8f + leftPadding, 0f);
            rect = Util.NormalizeRectWithTopLeft(item.demandText, 28f + leftPadding, 2f);
            rect.sizeDelta = new Vector2(190f, 22f);

            //supplyText
            item.supplyText = src.valueText;
            item.supplyText.text = "";
            item.supplyText.color = item.demandText.color;
            item.supplyText.supportRichText = false;
            item.supplyText.fontSize = 14;
            item.supplyText.alignment = TextAnchor.MiddleLeft;
            rect = Util.NormalizeRectWithTopLeft(item.supplyText, 300f + leftPadding, 2f);
            rect.sizeDelta = new Vector2(190f, 22f);

            //distText
            item.distText = GameObject.Instantiate<Text>(item.supplyText, item.supplyText.transform.parent);
            item.distText.gameObject.name = "distText";
            item.distText.alignment = TextAnchor.MiddleCenter;
            rect = Util.NormalizeRectWithTopLeft(item.distText, 236f + leftPadding, 2f);
            rect.sizeDelta = new Vector2(50f, 22f);

            item.timeText = GameObject.Instantiate<Text>(item.supplyText, item.supplyText.transform.parent);
            item.timeText.gameObject.name = "timeText";
            item.timeText.alignment = TextAnchor.MiddleCenter;
            rect = Util.NormalizeRectWithTopLeft(item.timeText, 495f + leftPadding, 2f);
            rect.sizeDelta = new Vector2(42f, 22f);

            //labelIcon
            item.labelIcon = src.iconImage;
            if (item.labelIcon != null)
            {
                item.labelIcon.gameObject.name = "labelIcon";
                item.labelIcon.enabled = true;
                rect = Util.NormalizeRectWithTopLeft(item.labelIcon, 8f, 12f);
                rect.pivot = new Vector2(0.5f, 0.5f);
                rect.sizeDelta = new Vector2(24f, 24f);
                rect.localScale = new Vector3(0.3f, 0.3f, 1f);

                item.labelIcon.material = null; //これのせいでめっちゃ光る
                UIStationWindow stationWindow = UIRoot.instance.uiGame.stationWindow;
                circleSprite = stationWindow.storageUIPrefab.transform.Find("storage-icon-empty/white")?.GetComponent<Image>()?.sprite;
                item.labelIcon.sprite = circleSprite;
            }

            item.demandText.color = Util.DSPOrange;
            item.supplyText.color = Util.DSPBlue;
            item.timeText.color = new Color(0.5f, 0.5f, 0.5f, 0.5f);

            GameObject.Destroy(data.gameObject.GetComponent<Image>());
            GameObject.Destroy(data.gameObject.GetComponent<Button>());
            GameObject.Destroy(src.iconHide);
            GameObject.Destroy(src.iconButton);
            //item.iconButton = src.iconButton;
            //GameObject.Destroy(src.valueText.gameObject);
            GameObject.Destroy(src);

        }
        void Start()
        {
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
            if (d.isFromDemand)
            {
                demandText.text = DisplayNameForPlanet(d.fromPlanetData);
                supplyText.text = DisplayNameForPlanet(d.toPlanetData);
                distText.text = "-> " + logData.distanceString + " ->";

            }
            else
            {
                demandText.text = DisplayNameForPlanet(d.toPlanetData);
                supplyText.text = DisplayNameForPlanet(d.fromPlanetData);
                distText.text = "<- " + logData.distanceString + " <-";
            }
            if (itemId != 0)
            {
                labelIcon.sprite = LDB.signals.IconSprite(itemId);
                labelIcon.rectTransform.localScale = Vector3.one;
                labelIcon.color = Color.white;
            }
            timeText.text = d.time;
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
            //locateBtn.gameObject.SetActive(true);
        }

        public void OnPointerExit(PointerEventData _eventData)
        {
            //locateBtn.gameObject.SetActive(false);
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
