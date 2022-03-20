//using System;
//using System.Text;
using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Linq;

namespace LSTMMod
{

    public class UIStatisticsWindowAgent : MonoBehaviour
    {
        public static UIStatisticsWindowAgent instance = null;

        public UIStatisticsWindow window;
        public UIProductEntryAgent[] entries;
        public int entriesLen;
        public UIButton balanceBtn;

        void Start()
        {
            ShowStatInStatisticsWindowChanged();
            if (LSTM.showStatInStatisticsWindow.Value && LSTM._showStatInStatisticsWindow.Value)
            {
                ShowBalance();
            }
            else
            {
                LSTM._showStatInStatisticsWindow.Value = false;
            }
            LSTM.showStatInStatisticsWindow.SettingChanged += (sender, args) => {
                ShowStatInStatisticsWindowChanged();
            };
        }

        public void ShowStatInStatisticsWindowChanged()
        {
            if (LSTM.showStatInStatisticsWindow.Value)
            {
                balanceBtn.gameObject.SetActive(true);
            }
            else
            {
                balanceBtn.gameObject.SetActive(false);
                HideBalance();
            }
        }

        public void ShowBalance()
        {
            for (int i = 0; i < entriesLen; i++)
            {
                entries[i].ShowBalance();
            }
            if (balanceBtn != null)
            {
                balanceBtn.highlighted = true;
            }
            LSTM._showStatInStatisticsWindow.Value = true;
        }

        public void HideBalance()
        {
            for (int i = 0; i < entriesLen; i++)
            {
                entries[i].HideBalance();
            }
            if (balanceBtn != null)
            {
                balanceBtn.highlighted = false;
            }
            LSTM._showStatInStatisticsWindow.Value = false;
        }

        public void OnBalanceBtnClick(int obj)
        {
            if (balanceBtn.highlighted)
            {
                HideBalance();
            }
            else
            {
                ShowBalance();
            }
        }

        public static void PreCreate()
        {
            //before UIStatisticsWindow _OnCreate()
            UIStatisticsWindow statisticsWindow = UIRoot.instance.uiGame.statWindow;

            //showBalanceButton
            UIProductEntry productEntry = statisticsWindow.productEntry;
            UIButton btn = Util.MakeSmallTextButton("LSTM", 38f, 20f);
            RectTransform rect = Util.NormalizeRectD(btn.gameObject);
            rect.SetParent(productEntry.transform, false);
            rect.anchoredPosition = new Vector3(6f, -6f);
            rect.localScale = Vector3.one;
            btn.gameObject.SetActive(false);

            UIProductEntryAgent p = productEntry.gameObject.AddComponent<UIProductEntryAgent>();
            p.productEntry = productEntry;
            p.showBalanceButton = btn.button;
            p.graphTrans = productEntry.statGraph.GetComponent<RectTransform>();
            p.graphTransDefaultWidth = p.graphTrans.sizeDelta.x;

            //for OnPointerEnter OnPointerExit
            Image img = productEntry.gameObject.AddComponent<Image>();
            img.color = Color.clear;
            img.alphaHitTestMinimumThreshold = 0f;

            //UIStatBalance
            p.statBalance = UIStatBalance.CreatePreｆab();
            //p.statBalance = GameObject.Instantiate<UIStatBalance>(prefab);

            p.statBalance.name = "station-balance";
            p.statBalance.transform.SetParent(productEntry.transform, false);
            //p.demandBalance.leftBar.color = p.demandBalance.demandColor;
            (p.statBalance.gameObject.transform as RectTransform).anchoredPosition = new Vector2(458, 0);
            p.statBalance.gameObject.SetActive(true);

            //test
            p.graphTrans.sizeDelta = new Vector2(p.graphTrans.sizeDelta.x - 60, p.graphTrans.sizeDelta.y);

        }

        public static void PostCreate()
        {
            //entries
            UIStatisticsWindow statisticsWindow = UIRoot.instance.uiGame.statWindow;
            UIStatisticsWindowAgent agent = statisticsWindow.gameObject.AddComponent<UIStatisticsWindowAgent>();
            agent.window = statisticsWindow;
            agent.entriesLen = AccessTools.FieldRefAccess<UIStatisticsWindow, int>(statisticsWindow, "entriesLen");
            agent.entries = new UIProductEntryAgent[agent.entriesLen];

            UIProductEntry[] ary = AccessTools.FieldRefAccess<UIStatisticsWindow, UIProductEntry[]>(statisticsWindow, "entries");
            for (int i = 0; i < agent.entriesLen; i++)
            {
                agent.entries[i] = ary[i]?.gameObject.GetComponent<UIProductEntryAgent>();
                agent.entries[i].gameObject.SetActive(false);
            }


            //balanceBtn
            UIBuildMenu buildMenu = UIRoot.instance.uiGame.buildMenu;
            UIButton src = buildMenu.categoryButtons[6];
            UIButton btn = GameObject.Instantiate<UIButton>(src, statisticsWindow.productPanel.transform);
            btn.button.interactable = true;
            GameObject go = btn.transform.Find("text")?.gameObject;
            if (go != null)
            {
                go.SetActive(false); //GameObject.Destroy(go);
            }
            //onClick が m_PersistentCalls に残ってるので作り直す
            Button oldbutton = btn.button;
            GameObject.DestroyImmediate(oldbutton);
            Button button = btn.gameObject.AddComponent<Button>();
            btn.button = button;
            btn.gameObject.SetActive(true);

            btn.onClick += agent.OnBalanceBtnClick;
            btn.onRightClick += agent.OnBalanceBtnClick;
            btn.tips.delay = 0.8f;
            btn.tips.tipTitle = "ILS Stock Ratio".Translate();
            btn.tips.tipText = "Click:show / hide ILS stock ratio\nRight click:".Translate();
            btn.tips.corner = 3;
            btn.tips.offset = new Vector2(6, 38);
            RectTransform rect = btn.transform as RectTransform;
            rect.anchorMax = new Vector2(0f, 1f);
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchoredPosition = new Vector2(458f, -51f);
            rect.sizeDelta = new Vector2(36, 36);
            RectTransform iconRect = rect.Find("icon")?.transform as RectTransform;
            if (iconRect != null)
            {
                iconRect.sizeDelta = new Vector2(20, 20);
            }
            btn.gameObject.SetActive(false);
            agent.balanceBtn = btn;
            instance = agent;
        }

        public void OnPostUpdate()
        {
            if (!balanceBtn.highlighted)
            {
                return;
            }

            //statisticsWindow.astroFilter -1:all, 0:localPlanet, (astroFilter % 100 == 0):system, :planetId
            HashSet<int> items = new HashSet<int>();

            int step = Time.frameCount;
            for (int i = 0; i < entriesLen; i++)
            {
                if (entries[i].NeedToUpdate(step) && entries[i].itemId > 0)
                {
                    items.Add(entries[i].itemId);
                }
            }
            if (items.Count > 0)
            {
                int starId = 0;
                int planetId = 0;
                if (window.astroFilter == -1)
                {

                }
                else if (window.gameData.localPlanet != null && window.astroFilter == 0)
                {
                    planetId = window.gameData.localPlanet.id;
                }
                else if (window.astroFilter % 100 == 0)
                {
                    starId = window.astroFilter / 100;
                    if (starId == 0)
                    {
                        starId = window.gameData.localStar.id;
                    }
                }
                else
                {
                    planetId = window.astroFilter;
                }

                List<StationStorageScanner> scanners = StationStorageScanner.GatherStationStorage(items, starId, planetId);

                for (int i = 0; i < entriesLen; i++)
                {
                    entries[i].ScannerUpdated(scanners);
                }
            }
        }




        public static class Patch
        {
            [HarmonyPostfix, HarmonyPatch(typeof(UIStatisticsWindow), "_OnUpdate")]
            public static void UIStatisticsWindow__OnUpdate_Postfix()
            {
                if (instance != null && instance.window.isProductionTab)
                {
                    instance.OnPostUpdate();
                }
            }
        }
    }

    public class UIProductEntryAgent : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField]
        public UIProductEntry productEntry;
        [SerializeField]
        public Button showBalanceButton;

        [SerializeField]
        public UIStatBalance statBalance;

        [SerializeField]
        public RectTransform graphTrans;
        [SerializeField]
        public float graphTransDefaultWidth;

        public int itemId;
        public int nextUpdateFrame;

        void Start()
        {
            showBalanceButton?.onClick.AddListener(ShowBalanceButtonClicked);
        }

        public void ShowBalanceButtonClicked()
        {
            int itemId = productEntry.entryData.itemId;
            if (itemId > 0)
            {
                //VFAudio.Create("ui-click-0", null, Vector3.zero, true, 2);
                LSTM.OpenBalanceWindow(itemId);
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (LSTM.showButtonInStatisticsWindow.Value)
            {
                showBalanceButton.gameObject.SetActive(true);
            }
        }
        public void OnPointerExit(PointerEventData _eventData)
        {
            showBalanceButton.gameObject.SetActive(false);
        }

        public void ShowBalance()
        {
            statBalance.gameObject.SetActive(true);
            graphTrans.sizeDelta = new Vector2(graphTransDefaultWidth - 100, graphTrans.sizeDelta.y);

        }
        public void HideBalance()
        {
            statBalance.gameObject.SetActive(false);
            graphTrans.sizeDelta = new Vector2(graphTransDefaultWidth, graphTrans.sizeDelta.y);

        }

        public bool NeedToUpdate(int frame)
        {
            int currentItemId = productEntry.entryData.itemId;
            if (frame > nextUpdateFrame || itemId != currentItemId)
            {
                itemId = currentItemId;
                nextUpdateFrame = frame + 60;
                return true;
            }
            return false;
        }

        public void ScannerUpdated(List<StationStorageScanner> scanners)
        {
            foreach (StationStorageScanner scanner in scanners)
            {
                if (itemId == scanner.itemId)
                {
                    statBalance.TakeValueFromScanner(scanner);
                    return;
                }
            }
        }

    }

    public class UIStatBalance : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField]
        public Text demandRatioText;

        [SerializeField]
        public Text supplyRatioText;

        [SerializeField]
        public Text supplyMaxText;

        [SerializeField]
        public Text supplyCountText;

        [SerializeField]
        public Text demandMaxText;

        [SerializeField]
        public Text demandCountText;

        public static string undefString = "<color=#464646ff>--</color>";

        public void TakeValueFromScanner(StationStorageScanner scanner)
        {
            SetDemandRatio(scanner.demandRatio);
            SetSupplyRatio(scanner.supplyRatio);
            demandMaxText.text = scanner.demandMax <= 0 ? "" : Util.KMGFormat(scanner.demandMax);
            demandCountText.text = scanner.demandMax <= 0 ? "" : Util.KMGFormat(scanner.demandCount);
            supplyMaxText.text = scanner.supplyMax <= 0 ? "" : Util.KMGFormat(scanner.supplyMax);
            supplyCountText.text = scanner.supplyMax <= 0 ? "" : Util.KMGFormat(scanner.supplyCount);
        }

        public void SetDemandRatio(float val)
        {
            val *= 100;
            if (val < 0)
            {
                demandRatioText.text = undefString;
                demandRatioText.alignment = TextAnchor.MiddleCenter;
            }
            else if (val > 999)
            {
                demandRatioText.text = "999+<size=18> <color=#464646ff>%</color></size>";
                demandRatioText.alignment = TextAnchor.MiddleRight;
            }
            else
            {
                demandRatioText.text = string.Format("{0:F1}<size=18> <color=#464646ff>%</color></size>", val);
                demandRatioText.alignment = TextAnchor.MiddleRight;
            }
        }

        public void SetSupplyRatio(float val)
        {
            val *= 100;
            if (val < 0)
            {
                supplyRatioText.text = undefString;
                supplyRatioText.alignment = TextAnchor.MiddleCenter;
            }
            else if (val > 999)
            {
                supplyRatioText.text = "999+<size=18> <color=#464646ff>%</color></size>";
                supplyRatioText.alignment = TextAnchor.MiddleRight;
            }
            else
            {
                supplyRatioText.text = string.Format("{0:F1}<size=18> <color=#464646ff>%</color></size>", val);
                supplyRatioText.alignment = TextAnchor.MiddleRight;
            }
        }
        public void OnPointerClick(PointerEventData pointerEventData)
        {
            if (pointerEventData.button == PointerEventData.InputButton.Right)
            {
                //UIRealtimeTip.Popup("right", false, 0);
            }
            //UIRealtimeTip.Popup("click", false, 0);

        }

        public static UIStatBalance CreatePreｆab()
        {
            UIStatisticsWindow statisticsWindow = UIRoot.instance.uiGame.statWindow;
            UIProductEntry productEntry = statisticsWindow.productEntry;
            float height = productEntry.rectTransform.sizeDelta.y;

            GameObject go = new GameObject("station-balance-prefab");
            go.AddComponent<RectTransform>();
            RectTransform rect = Util.NormalizeRect(go);
            rect.sizeDelta = new Vector2(120, height);

            //for IPointerClickHandler
            Image img = go.AddComponent<Image>();
            img.color = Color.clear;
            img.alphaHitTestMinimumThreshold = 0f;

            UIStatBalance prefab = go.AddComponent<UIStatBalance>();

            Text txt;
            txt = Util.MakeGameObject<Text>(go.transform, productEntry.consumeText.gameObject,  2, 0, 72, 40, false, true);
            txt.alignment = TextAnchor.MiddleRight;
            txt.resizeTextForBestFit = true;
            txt.resizeTextMaxSize = 34;
            txt.supportRichText = true;
            prefab.demandRatioText = txt;

            txt = Util.MakeGameObject<Text>(go.transform, productEntry.productText.gameObject,  2, 60, 72, 40, false, true);
            txt.alignment = TextAnchor.MiddleRight;
            txt.resizeTextForBestFit = true;
            txt.resizeTextMaxSize = 34;
            txt.supportRichText = true;
            prefab.supplyRatioText = txt;

            txt = Util.MakeGameObject<Text>(go.transform, productEntry.consumeText.gameObject, 75, 4, 44, 20, false, true);
            txt.alignment = TextAnchor.MiddleCenter;
            txt.resizeTextForBestFit = true;
            txt.resizeTextMaxSize = 18;
            txt.supportRichText = true;
            prefab.demandMaxText = txt;

            txt = Util.MakeGameObject<Text>(go.transform, productEntry.consumeText.gameObject, 75, 25, 44, 20, false, true);
            txt.alignment = TextAnchor.MiddleCenter;
            txt.resizeTextForBestFit = true;
            txt.resizeTextMaxSize = 18;
            txt.supportRichText = true;
            prefab.demandCountText = txt;

            txt = Util.MakeGameObject<Text>(go.transform, productEntry.productText.gameObject, 75, 60, 44, 20, false, true);
            txt.alignment = TextAnchor.MiddleCenter;
            txt.resizeTextForBestFit = true;
            txt.resizeTextMaxSize = 18;
            txt.supportRichText = true;
            prefab.supplyMaxText = txt;

            txt = Util.MakeGameObject<Text>(go.transform, productEntry.productText.gameObject, 75, 81, 44, 20, false, true);
            txt.alignment = TextAnchor.MiddleCenter;
            txt.resizeTextForBestFit = true;
            txt.resizeTextMaxSize = 18;
            txt.supportRichText = true;
            prefab.supplyCountText = txt;

            txt = Util.MakeGameObject<Text>(go.transform, productEntry.consumeLabel.gameObject, 0, 28, 0, 0, false, true); 
            txt.text = "Demand";
            txt = Util.MakeGameObject<Text>(go.transform, productEntry.productLabel.gameObject, 0, 88, 0, 0, false, true);
            txt.text = "Supply";

            GameObject sepSrc = new GameObject();
            rect = sepSrc.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(40f, 0.8f);
            img = sepSrc.AddComponent<Image>();
            img.color = new Color(0.9f, 0.9f, 0.9f, 0.1f);

            Util.MakeGameObject<Image>(go.transform, sepSrc, 77f, 24f, 40f, 0.8f, false, true);
            Util.MakeGameObject<Image>(go.transform, sepSrc, 77f, 80f, 40f, 0.8f, false, true);
            Object.Destroy(sepSrc);

            return prefab;
        }

    }

    public class StationStorageScanner
    {
        public int itemId;
        // 1 == "100.0%", -1 == "--"
        public float demandRatio;
        public float supplyRatio;

        public long demandMax;
        public long demandCount;
        public long supplyMax;
        public long supplyCount;

        
        public StationStorageScanner(int itemId)
        {
            this.itemId = itemId;
        }
        public bool TryAddStorage(StationComponent s)
        {
            for (int i = 0; i < s.storage.Length; i++)
            {
                if (itemId == s.storage[i].itemId)
                {
                    int max = s.storage[i].max;
                    if (s.storage[i].remoteLogic == ELogisticStorage.Supply)
                    {
                        supplyMax += s.storage[i].max;
                        supplyCount += s.storage[i].count;
                    }
                    else if (s.storage[i].remoteLogic == ELogisticStorage.Demand)
                    {
                        demandMax += s.storage[i].max;
                        demandCount += s.storage[i].count;
                    }
                    //else
                    //{
                    //do nothing when ELogisticStorage.None
                    //}
                    return true;
                }
            }
            return false;
        }

        public void PostScan()
        {
            if (supplyMax == 0)
            {
                supplyRatio = -1;
            }
            else
            {
                supplyRatio = (float)supplyCount / (float)supplyMax;
            }

            if (demandMax == 0)
            {
                demandRatio = -1;
            }
            else
            {
                demandRatio = (float)demandCount / (float)demandMax;
            }
        }

        public static List<StationStorageScanner> GatherStationStorage(HashSet<int> items, int starId, int planetId, bool excludesGasGiant = false)
        {
            List<StationStorageScanner> scanners = items.Select(s => new StationStorageScanner(s)).ToList();

            List<PlanetData> targetPlanets = new List<PlanetData>(10);
            if (starId != 0)
            {
                StarData starData = GameMain.galaxy.StarById(starId);
                if (starData != null)
                {
                    for (int j = 0; j < starData.planetCount; j++)
                    {
                        if (excludesGasGiant && starData.planets[j].type == EPlanetType.Gas)
                        {
                            continue;
                        }
                        targetPlanets.Add(starData.planets[j]);
                    }
                }
            }
            else if (planetId != 0)
            {
                PlanetData p = GameMain.galaxy.PlanetById(planetId);
                if (p != null /*&& !(excludesGasGiant && p.type == EPlanetType.Gas)*/)
                {
                    targetPlanets.Add(p);
                }
            }
            else
            {
                //all
                GatherGalaxy(scanners, excludesGasGiant);
                return PostGather(scanners);
            }
            GatherPlanets(scanners, targetPlanets);
            return PostGather(scanners);
        }

        public static List<StationStorageScanner> PostGather(List<StationStorageScanner> scanners)
        {
            foreach (StationStorageScanner scanner in scanners)
            {
                scanner.PostScan();
            }
            return scanners;
        }

        public static void GatherPlanets(List<StationStorageScanner> scanners, List<PlanetData> planets)
        {
            foreach (PlanetData planet in planets)
            {
                if (planet.factory == null)
                {
                    continue;
                }

                PlanetTransport transport = planet.factory.transport;
                for (int i = 1; i < transport.stationCursor; i++)
                {
                    if (transport.stationPool[i] != null && transport.stationPool[i].id == i && transport.stationPool[i].isStellar)
                    {
                        StationComponent s = transport.stationPool[i];
                        foreach (StationStorageScanner scanner in scanners)
                        {
                            scanner.TryAddStorage(s);
                        }
                    }
                }
            }
        }

        public static void GatherGalaxy(List<StationStorageScanner> scanners, bool excludesGasGiant = false)
        {
            GalacticTransport galacticTransport = UIRoot.instance.uiGame.gameData.galacticTransport;
            StationComponent[] stationPool = galacticTransport.stationPool;
            int cursor = galacticTransport.stationCursor;

            for (int i = 1; i < cursor; i++)
            {
                if (stationPool[i] != null && stationPool[i].gid == i) //gid
                {
                    StationComponent s = stationPool[i];
                    if (excludesGasGiant && s.isCollector)
                    {
                        continue;
                    }
                    foreach (StationStorageScanner scanner in scanners)
                    {
                        scanner.TryAddStorage(s);
                    }
                }
            }
        }
    }
}
