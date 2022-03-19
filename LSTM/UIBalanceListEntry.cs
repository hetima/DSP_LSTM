using HarmonyLib;
using System;
using System.Text;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;


namespace LSTMMod
{
    public class UIBalanceListEntry : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public StationComponent station;
        public int index;
        public int itemId;
        public int planetId;
        public string StationName
        {
            get
            {
                string text = string.IsNullOrEmpty(station.name) ? (station.isStellar ? ("星际站点号".Translate() + station.gid.ToString()) : ("本地站点号".Translate() + station.id.ToString())) : station.name;
                return text;
            }
        }
        public bool nameDirty;
        public UIBalanceWindow window;


        public int stationMaxItemCount = 0;

        public bool isLocal;
        public EStoreType storeType;

        [SerializeField]
        public Image leftBarLocal;

        [SerializeField]
        public Image leftBarRemote;

        [SerializeField]
        public Text countValueText;

        [SerializeField]
        public Text orderValueText;

        [SerializeField]
        public Text maxValueText;

        [SerializeField]
        public Image countBar;

        [SerializeField]
        public Image orderBar;

        [SerializeField]
        public Image locateImage;

        [Header("Colors & Settings")]
        [SerializeField]
        public Color supplyColor;

        [SerializeField]
        public Color demandColor;

        [SerializeField]
        public Color noneSpColor;

        [SerializeField]
        public Color orderInColor;

        [SerializeField]
        public Color orderOutColor;

        [SerializeField]
        public Color orderInTextColor;

        [SerializeField]
        public Color orderOutTextColor;

        [SerializeField]
        public Color orderNoneTextColor;

        [SerializeField]
        public Color locateImageColor;

        [SerializeField]
        public Color locateImageColorLocal;

        [SerializeField]
        public Image itemImage;
        [SerializeField]
        public Slider maxSlider;

        [SerializeField]
        public Text nameText;

        [SerializeField]
        public Text shipCountText;

        [SerializeField]
        public UIButton itemButton;
        
        [SerializeField]
        public UIButton locateBtn;

        [SerializeField]
        public UIButton filterBtn;

        public static UIBalanceListEntry CreatePreｆab()
        {
            UIStationWindow stationWindow = UIRoot.instance.uiGame.stationWindow;

            UIStationStorage storageUIPrefab = stationWindow.storageUIPrefab;
            UIStationStorage src =  GameObject.Instantiate<UIStationStorage>(storageUIPrefab);
            UIBalanceListEntry prefab = src.gameObject.AddComponent<UIBalanceListEntry>();
            prefab.leftBarLocal = src.leftBar;
            prefab.countValueText = src.countValueText;
            prefab.orderValueText = src.orderValueText;
            prefab.maxValueText = src.maxValueText;
            prefab.countBar = src.countBar;
            prefab.orderBar = src.orderBar;
            prefab.supplyColor = src.supplyColor;
            prefab.demandColor = src.demandColor;
            prefab.noneSpColor = src.noneSpColor;
            prefab.orderInColor = src.orderInColor;
            prefab.orderOutColor = src.orderOutColor;
            prefab.orderInTextColor = src.orderInTextColor;
            prefab.orderOutTextColor = src.orderOutTextColor;
            prefab.orderNoneTextColor = src.orderNoneTextColor;
            prefab.itemImage = src.itemImage;
            prefab.maxSlider = src.maxSlider;
            prefab.itemButton = src.itemButton;

            GameObject.Destroy(src);

            GameObject go = prefab.gameObject;
            RectTransform rect = Util.NormalizeRect(go);
            rect.sizeDelta = new Vector2(rect.sizeDelta.x, rect.sizeDelta.y + 14);
            go.transform.Find("bg/empty-tip")?.gameObject.SetActive(false);
            go.transform.Find("storage-icon/take-button")?.gameObject.SetActive(false);

            GameObject label = go.transform.Find("current-count-label/current-count-text")?.gameObject;
            if (label != null)
            {
                GameObject nameLabel = GameObject.Instantiate(label);
                nameLabel.name = "lstm-name-lbl";
                prefab.nameText = nameLabel.GetComponent<Text>();
                prefab.nameText.alignment = TextAnchor.MiddleLeft;
                RectTransform rect2 = Util.NormalizeRectB(nameLabel);
                nameLabel.transform.SetParent(go.transform, false);
                rect2.offsetMax = Vector2.zero;
                rect2.offsetMin = new Vector2(14f, 54f);

                GameObject shipCount = GameObject.Instantiate(nameLabel);
                shipCount.name = "lstm-ship-count";
                prefab.shipCountText = shipCount.GetComponent<Text>();
                prefab.shipCountText.alignment = TextAnchor.MiddleRight;
                rect2 = (RectTransform)shipCount.transform;
                shipCount.transform.SetParent(go.transform, false);
                rect2.pivot = new Vector2(1f, 0.5f);
                rect2.localPosition = new Vector2(300f, 68f);
            }

            //locate button
            UIButton btn = Util.MakeIconButtonB(LSTM.astroIndicator, 22);
            if (btn != null)
            {
                btn.gameObject.name = "lstm-locate-btn";
                rect = Util.NormalizeRectD(btn.gameObject);
                rect.SetParent(go.transform, false);
                rect.anchoredPosition = new Vector2(240f, -6f);
                //btn.onClick +=
                btn.tips.tipTitle = "Locate Station".Translate();
                btn.tips.tipText = "Show the star to which the station belongs or navigation to this station".Translate();
                btn.tips.corner = 3;
                btn.tips.offset = new Vector2(18f, -20f);
                btn.gameObject.SetActive(false);
                prefab.locateBtn = btn;

                prefab.locateImage = btn.gameObject.transform.Find("icon")?.GetComponent<Image>();
                prefab.locateImageColor = new Color(0.8f, 0.8f, 0.8f, 0.55f);
                prefab.locateImageColorLocal = new Color(0.3821f, 0.8455f, 1f, 0.55f);
            }

            //filter button
            Sprite sprite = Util.LoadSpriteResource("ui/textures/sprites/icons/filter-icon");
            btn = Util.MakeIconButtonB(sprite, 22);
            if (btn != null)
            {
                btn.gameObject.name = "lstm-filter-btn";
                rect = Util.NormalizeRectD(btn.gameObject);
                rect.SetParent(go.transform, false);
                rect.anchoredPosition = new Vector2(38f, -56f);
                //btn.onClick +=
                btn.tips.tipTitle = "Item Filter".Translate();
                btn.tips.tipText = "Filter list with this item".Translate();
                btn.tips.corner = 3;
                btn.tips.offset = new Vector2(18f, -20f);

                Image img = btn.transform.Find("bg")?.GetComponent<Image>();
                if (img)
                {
                    img.color = new Color(0.216f, 0.216f, 0.216f, 0.8f);
                }
                btn.gameObject.SetActive(false);
                prefab.filterBtn = btn;
            }

            //left bar
            //offset.max.yを減らすと上が削れる
            //offset.mix.yを増やすと下が削れる
            //offset.max.xを増やすと幅が広がる
            prefab.leftBarRemote = GameObject.Instantiate<Image>(prefab.leftBarLocal, prefab.leftBarLocal.transform.parent);
            //縦に重ねると見にくかった
            //rect = prefab.leftBarRemote.rectTransform;
            //rect.offsetMin = new Vector2(0, 26);
            //rect.offsetMax = new Vector2(4f, 0);
            //prefab.leftBarRemote.gameObject.name = "bar-remote";
            //rect = prefab.leftBarLocal.rectTransform;
            //rect.offsetMax = new Vector2(4f, -62);

            rect = prefab.leftBarRemote.rectTransform;
            rect.offsetMax = new Vector2(3f, 0);
            rect.offsetMin = new Vector2(0, 0);
            prefab.leftBarRemote.gameObject.name = "bar-remote";
            rect = prefab.leftBarLocal.rectTransform;
            rect.offsetMax = new Vector2(8f, -28);
            rect.offsetMin = new Vector2(4, 0);



            go = prefab.gameObject;
            for (int i = go.transform.childCount - 1; i >= 0; --i)
            {
                GameObject child = go.transform.GetChild(i).gameObject;
                if (child.name.Contains("button") || child.name.Contains("popup") || child.name.Contains("empty"))
                {
                    child.SetActive(false);
                    GameObject.Destroy(child);
                }
                else
                {
                    if (child.GetComponent<EventTrigger>() != null)
                    {
                        GameObject.Destroy(child.GetComponent<EventTrigger>());
                    }
                    Vector3 lpos = child.transform.localPosition;
                    child.SetActive(true);
                    if (child.name == "slider-bg")
                    {
                        Image img = child.GetComponent<Image>();
                        if (img)
                        {
                            img.color = new Color(1f, 1f, 1f, 0.1f);
                        }
                        child.transform.localScale = new Vector3(0.66f, 1f, 1f);
                        lpos.x -= 32;
                        lpos.y -= 10;
                        child.transform.localPosition = lpos;
                    }
                    else if (child.name.Contains("label"))
                    {
                        lpos.x -= 30;
                        lpos.y -= 12;
                        child.transform.localPosition = lpos;
                    }
                    else if (child.name.Contains("icon"))
                    {
                        child.transform.localScale = new Vector3(0.75f, 0.75f, 0.75f);
                        lpos.x = 36;
                        lpos.y = 30;
                        child.transform.localPosition = lpos;
                    }
                }
            }

            return prefab;
        }

        void Start()
        {
            maxSlider.enabled = false;
            filterBtn.onClick += OnFilterButtonClick;
            locateBtn.onClick += OnLocateButtonClick;
            locateBtn.onRightClick += OnLocateButtonRightClick;
            locateBtn.gameObject.SetActive(false);
            filterBtn.gameObject.SetActive(false);
        }

        public void SetUpValues(bool useStationName)
        {
            nameDirty = false;

            if (storeType == EStoreType.GasStubStorage || storeType == EStoreType.GasStubSupply)
            {
                itemImage.sprite = UIBalanceWindow.gasGiantSprite;
                itemImage.color = new Color(0.5f, 0.5f, 0.5f, 0.6f);
                ((RectTransform)locateBtn.transform).anchoredPosition = new Vector2(200f, -6f);
            }
            else
            {
                ItemProto itemProto = LDB.items.Select(itemId);
                if (itemProto != null)
                {
                    itemImage.sprite = itemProto.iconSprite;
                }
                int local = GameMain.localPlanet != null ? GameMain.localPlanet.id : 0;
                if (local == planetId)
                {
                    locateImage.color = locateImageColorLocal;
                }
                else
                {
                    locateImage.color = locateImageColor;
                }
            }

            if (stationMaxItemCount == 0)
            {
                stationMaxItemCount = LSTM.RemoteStationMaxItemCount();
            }
            else if (stationMaxItemCount < LSTM.RemoteStationMaxItemCount())
            {
                RectTransform rect = (RectTransform)maxSlider.gameObject.transform;
                rect.sizeDelta = new Vector2(rect.sizeDelta.x / ((float)LSTM.RemoteStationMaxItemCount() / (float)stationMaxItemCount), rect.sizeDelta.y);
            }

            SetUpNameText(useStationName);

        }

        public void SetUpNameText(bool useStationName)
        {
            //int starId = planetId / 100;
            int localPlanetId = GameMain.localPlanet != null ? GameMain.localPlanet.id : 0;
            string distStr;

            if (localPlanetId == planetId)
            {
                distStr = "";
            }
            else
            {
                float d = LSTMStarDistance.StarDistanceFromHere(planetId / 100);
                if (d > 0)
                {
                    distStr = string.Format("   ({0:F1}ly)", d);
                }
                else
                {
                    //same star system
                    distStr = "   (near)";
                }
            }

            if (useStationName && station != null)
            {
                nameText.text = StationName + distStr;
            }
            else
            {
                PlanetData planet = GameMain.galaxy.PlanetById(planetId);
                if (planet != null)
                {
                    nameText.text = planet.displayName + distStr;
                }
                else
                {
                    nameText.text = distStr;
                }
            }
        }


        public bool RefreshValues(bool shown, bool onlyNewlyEmerged = false)
        {
            if (nameDirty)
            {
                nameDirty = false;
                SetUpNameText(window.UseStationName);
            }

            if (shown != gameObject.activeSelf)
            {
                gameObject.SetActive(shown);
            }
            else if(onlyNewlyEmerged)
            {
                return true;
            }
            if (!shown)
            {
                return true;
            }

            int count = 0;
            int totalOrdered = 0;
            int max = 0;
            float barMax = 0f;
            int divisor = 1; //画像描画用
            if (storeType == EStoreType.Normal)
            {
                StationStore stationStore = station.storage[index];
                if (stationStore.itemId != itemId)
                {
                    return false;
                }
                barMax = (float)stationMaxItemCount;
                //compatibility with some mod?
                if (barMax < stationStore.max)
                {
                    barMax = (float)stationStore.max;
                }
                //輸送船
                string shipCount;
                if (station.isCollector)
                {
                    shipCount = "[GS]";
                }
                else if (station.isVeinCollector)
                {
                    shipCount = "[AM]";
                }
                else
                {
                    int ship;
                    int totalShip;
                    if (isLocal)
                    {
                        ship = station.idleDroneCount;
                        totalShip = ship + station.workDroneCount;
                        shipCount = ship.ToString() + "/" + totalShip.ToString();
                    }
                    else
                    {
                        ship = station.idleShipCount;
                        totalShip = ship + station.workShipCount;
                        shipCount = ship.ToString() + "/" + totalShip.ToString();
                        if (LSTM.indicatesWarperSign.Value && station.warperCount > 0)
                        {
                            shipCount += "*";
                        }
                    }
                }
                shipCountText.text = shipCount;

                count = stationStore.count;
                totalOrdered = stationStore.totalOrdered;
                max = stationStore.max;

                this.leftBarLocal.color = this.GetLogisticColor(stationStore.localLogic);
                this.leftBarRemote.color = this.GetLogisticColor(stationStore.remoteLogic);
                if (station.isCollector)
                {
                    leftBarLocal.gameObject.SetActive(false);
                    //this.leftBarLocal.color =
                }
                else if (!station.isStellar)
                {
                    leftBarRemote.gameObject.SetActive(false);
                }
            }
            else if (storeType == EStoreType.GasStubStorage || storeType == EStoreType.GasStubSupply)
            {
                int stationCount = 0;
                int SufficientStationCount = 0;
                int totalCapability = 0;
                ELogisticStorage logic;
                if (storeType == EStoreType.GasStubSupply)
                {
                    logic = ELogisticStorage.Supply;
                }
                else
                {
                    logic = ELogisticStorage.None;
                }

                leftBarRemote.color = this.GetLogisticColor(logic);
                leftBarLocal.gameObject.SetActive(false);
                leftBarRemote.gameObject.SetActive(true);
                List<BalanceListData> gasList = window._gasList;
                foreach (var item in gasList)
                {
                    if (item.planetId != planetId || item.itemId != itemId)
                    {
                        continue;
                    }
                    
                    StationStore stationStore = item.cmp.storage[item.index];
                    if (stationStore.remoteLogic != logic)
                    {
                        continue;
                    }
                    if (stationStore.count > 1000)
                    {
                        SufficientStationCount++;
                        totalCapability += stationStore.count / 1000;
                    }
                    count += stationStore.count;
                    totalOrdered += stationStore.totalOrdered;
                    max += stationStore.max;
                    barMax += stationMaxItemCount;
                    stationCount++;
                }

                shipCountText.text = "[GS] " /*+ totalCapability.ToString() + "/"*/ + SufficientStationCount.ToString() + "/" + stationCount.ToString();

                divisor = stationCount;
            }
            else
            {
                return false;
            }

            if (divisor == 0)
            {
                return false;
            }

            //在庫
            if (count>=20000)
            {
                countValueText.text = (count/1000).ToString() + "k";
            }
            else
            {
                countValueText.text = count.ToString();
            }

            if (totalOrdered > 0)
            {
                orderValueText.text = "+" + totalOrdered.ToString();
                orderValueText.color = orderInTextColor;
            }
            else if (totalOrdered < 0)
            {
                orderValueText.text = "-" + (-totalOrdered).ToString();
                orderValueText.color = orderOutTextColor;
            }
            else
            {
                orderValueText.text = "0";
                orderValueText.color = orderNoneTextColor;
            }

            if (max >= LSTM.RemoteStationMaxItemCount())
            {
                maxValueText.text = (max / 1000).ToString() + "k";
            }
            else
            {
                maxValueText.text = max.ToString();
            }

            count /= divisor;
            max /= divisor;
            totalOrdered /= divisor;
            barMax /= (float)divisor;
            float barMaxWidth = 200f / ((float)LSTM.RemoteStationMaxItemCount() / barMax);
            //compatibility with some mod?
            if (barMaxWidth > 200f)
            {
                barMaxWidth = 200f;
            }
            float num2 = (float)count / barMax;
            float num3 = (float)totalOrdered / barMax;
            int num4 = (int)(barMaxWidth * num2 + 0.49f);
            int num5 = (int)(barMaxWidth * (Mathf.Abs(num3) + num2) + 0.49f) - num4;
            if (num4 > (int)barMaxWidth)
            {
                num4 = (int)barMaxWidth;
            }
            if (num3 > 0f)
            {
                if (num4 + num5 > (int)barMaxWidth)
                {
                    num5 = (int)barMaxWidth - num4;
                }
            }
            else if (num3 < 0f && num4 - num5 < 0)
            {
                num5 = num4;
            }
            countBar.rectTransform.sizeDelta = new Vector2((float)num4, 0f);
            orderBar.rectTransform.sizeDelta = new Vector2((float)num5, 0f);
            if (num3 > 0f)
            {
                orderBar.rectTransform.anchoredPosition = new Vector2((float)num4, 0f);
                orderBar.color = orderInColor;
            }
            else
            {
                orderBar.rectTransform.anchoredPosition = new Vector2((float)(num4 - num5), 0f);
                orderBar.color = orderOutColor;
            }
            maxSlider.minValue = 0f;
            maxSlider.maxValue = barMax / 100f;
            maxSlider.value = (float)(max / 100);

            return true;
        }

        private Color GetLogisticColor(ELogisticStorage logic)
        {
            if (logic == ELogisticStorage.Supply)
            {
                return this.supplyColor;
            }
            if (logic == ELogisticStorage.Demand)
            {
                return this.demandColor;
            }
            return this.noneSpColor;
        }

        private void OnLocateButtonClick(int obj)
        {
            LSTM.LocateStation(station, planetId);
        }
        private void OnLocateButtonRightClick(int obj)
        {
            LSTM.OpenStationWindow(station, planetId);
        }
        private void OnFilterButtonClick(int obj)
        {
            window.Filter(itemId, 0);
        }

        public void OnPointerEnter(PointerEventData _eventData)
        {
            locateBtn.gameObject.SetActive(true);
            if (itemId != window.balanceData.itemId)
            {
                filterBtn.gameObject.SetActive(true);
            }
        }

        public void OnPointerExit(PointerEventData _eventData)
        {
            locateBtn.gameObject.SetActive(false);
            filterBtn.gameObject.SetActive(false);

        }
    }
}
