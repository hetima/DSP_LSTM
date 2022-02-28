using BepInEx;
using BepInEx.Logging;
using BepInEx.Configuration;
using Compatibility;
using HarmonyLib;
//using System;
//using System.Text;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace LSTMMod
{

    [BepInPlugin(__GUID__, __NAME__, "0.3.5.1")]
    [BepInDependency("dsp.nebula-multiplayer-api", BepInDependency.DependencyFlags.SoftDependency)]
    public class LSTM : BaseUnityPlugin
    {
        public const string __NAME__ = "LSTM";
        public const string __GUID__ = "com.hetima.dsp." + __NAME__;

        public static LSTM instance = null;

        public static Sprite astroIndicator {
            get {
                if (_astroIndicator == null)
                {
                    UIStarmap starmap = UIRoot.instance.uiGame.starmap;
                    _astroIndicator = starmap.cursorFunctionButton3.transform.Find("icon")?.GetComponent<Image>()?.sprite;
                }
                return _astroIndicator;
            } 
        }
        internal static Sprite _astroIndicator;
        
        public static LSTMNavi navi = null;
        public static UIBalanceWindow _win;
        public static ConfigEntry<KeyboardShortcut> mainWindowHotkey;

        public static ConfigEntry<bool> dropSorterKeyEracesNavi;
        public static ConfigEntry<bool> showButtonInStationWindow;
        public static ConfigEntry<bool> showButtonInStatisticsWindow;
        public static ConfigEntry<bool> actAsStandardPanel;
        public static ConfigEntry<bool> indicatesWarperSign;

        public static ConfigEntry<bool> enableTLRemoteCluster;
        public static ConfigEntry<bool> enableTLLocalCluster;
        public static ConfigEntry<bool> enableTLConsiderOppositeRange;
        public static ConfigEntry<bool> enableTLDCBalance;
        public static ConfigEntry<float> TLDCSupplyMultiplier;
        public static ConfigEntry<float> TLDCDemandMultiplier;
        public static ConfigEntry<float> TLDCSupplyDenominator;
        public static ConfigEntry<bool> enableTLRemoteDemandDelay;
        public static ConfigEntry<bool> enableTLSmartTransport;
        

        new internal static ManualLogSource Logger;


        void Awake()
        {
            Logger = base.Logger;
            //Logger.LogInfo("Awake");
            instance = this;
            navi = new LSTMNavi();

            MyWindowCtl.useMyWindowInterface = true;

            mainWindowHotkey = Config.Bind("Keyboard Shortcuts", "mainWindowHotkey", KeyboardShortcut.Deserialize("T + LeftControl"),
                "Hotkey to open/close LSTM window");
            showButtonInStationWindow = Config.Bind("Interface", "showButtonInStationWindow", true,
                "Add open LSTM button to Station Window (needs restart)");
            showButtonInStatisticsWindow = Config.Bind("Interface", "showButtonInStatisticsWindow", false,
                "Add open LSTM button to Statistics Window (needs restart)");
            actAsStandardPanel = Config.Bind("Interface", "actAsStandardPanel", true,
                "true: close with other panels by esc key. false: one more esc needed");
            dropSorterKeyEracesNavi = Config.Bind("Keyboard Shortcuts", "dropSorterKeyEracesNavi", false,
                "clear navi line when \"Remove Copied Sorter Previews\" shortcut is pressed");
            indicatesWarperSign = Config.Bind("Interface", "indicatesWarperSign", false,
                "show sign on the list if station has warper.");

            enableTLRemoteCluster = Config.Bind("TrafficLogic", "TLRemoteCluster", false,
                "enable TrafficLogic:Remote Cluster");
            enableTLLocalCluster = Config.Bind("TrafficLogic", "TLLocalCluster", false,
               "enable TrafficLogic:Local Cluster");
            enableTLConsiderOppositeRange = Config.Bind("TrafficLogic", "TLConsiderOppositeRange", false,
                "enable TrafficLogic:Consider Opposite Range");
            enableTLDCBalance = Config.Bind("TrafficLogic", "TLDCBalance", false,
                "enable TrafficLogic:Remote Distance/Capacity Balance");
            TLDCSupplyMultiplier = Config.Bind("TrafficLogic", "TLDCSupplyMultiplier", 1f,
                "enable TrafficLogic:Multiplier for Remote Supply Distance/Capacity Balance (1-100)");
            TLDCDemandMultiplier = Config.Bind("TrafficLogic", "TLDCDemandMultiplier", 1f,
                "enable TrafficLogic:Multiplier for Remote Demand Distance/Capacity Balance (1-100)");
            TLDCSupplyDenominator = Config.Bind("TrafficLogic", "TLDCSupplyDenominator", 1f,
                "enable TrafficLogic:Denominator for Remote Supply Distance/Capacity Balance (1-100)");
            enableTLRemoteDemandDelay = Config.Bind("TrafficLogic", "TLRemoteDemandDelay", false,
                "enable TrafficLogic:Remote Demand Delay");
            enableTLSmartTransport = Config.Bind("TrafficLogic", "TLSmartTransport", false,
                "enable TrafficLogic:Smart Transport");

            var harmony = new Harmony(__GUID__);
            harmony.PatchAll(typeof(Patch));
            harmony.PatchAll(typeof(LSTMStarDistance.Patch));
            harmony.PatchAll(typeof(MyWindowCtl.Patch));
            harmony.PatchAll(typeof(TrafficLogic.Patch));


            if (BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("dsp.nebula-multiplayer-api"))
                NebulaCompat.Init();
        }

        public void Log(string str)
        {
            Logger.LogInfo(str);
        }


        public static int RemoteStationMaxItemCount()
        {
            return 10000 + GameMain.history.remoteStationExtraStorage;
        }

        public static void OpenBalanceWindow(StationComponent targetStation, int index, bool isLocal, PlanetFactory targetFactory = null)
        {
            int planetId;
            planetId = targetFactory != null ? targetFactory.planetId : 0;
            _win.SetUpAndOpen(targetStation.storage[index].itemId, isLocal ? planetId : 0, isLocal);
        }

        public static void OpenBalanceWindow(int itemId)
        {
            _win.SetUpAndOpen(itemId, 0, false);
        }

        public static void OpenBalanceWindow()
        {
            _win.SetUpAndOpen(0, 0, true);
        }


        public static void LocateStation(StationComponent station, int planetId)
        {
            navi.Disable();

            if (planetId <= 0)
            {
                return;
            }
            _win.keepOpen = true;
            int local = GameMain.localPlanet != null ? GameMain.localPlanet.id : 0;


            if (station != null)
            {
                navi.SetStationNavi(station, planetId);
            }

            if (local != planetId || UIRoot.instance.uiGame.starmap.active)
            {
                if (GameMain.history.universeObserveLevel >= 4)
                {
                    UIRoot.instance.uiGame.ShutPlayerInventory();
                    UIRoot.instance.uiGame.ShutAllFunctionWindow();
                    //UIRoot.instance.uiGame.ShutAllFullScreens();
                    UIRoot.instance.uiGame.OpenStarmap();
                    int starIdx = planetId / 100 - 1;
                    int planetIdx = planetId % 100 - 1;
                    UIStarmap map = UIRoot.instance.uiGame.starmap;

                    PlanetData planet = GameMain.galaxy.PlanetById(planetId);
                    if (planet != null)
                    {
                        map.focusPlanet = null;
                        map.focusStar = map.starUIs[starIdx];
                        map.OnCursorFunction2Click(0);
                        if (map.focusStar == null)
                        {
                            map.focusPlanet = map.planetUIs[planetIdx];
                            map.OnCursorFunction2Click(0);
                            //map.SetViewStar(star.star, true);
                            map.focusPlanet = map.planetUIs[planetIdx]; //Function Panelを表示させるため
                            map.focusStar = null;
                        }
                    }
                }
                else
                {
                    UIMessageBox b = UIMessageBox.Show("Upgrades Required".Translate(), "To use this feature, Universe Exploration 4 is required.".Translate(), "OK", 0);
                }
            }
            _win.keepOpen = false;
        }
        

        public static void OnStarmapButtonClick(int obj)
        {
            //VFAudio.Create("ui-click-0", null, Vector3.zero, true, 2);
            PlanetData planet = UIRoot.instance.uiGame.planetDetail.planet;
            if (planet != null)
            {
                _win.SetUpAndOpen(0, planet.id, false /*planet.type != EPlanetType.Gas*/);
            }
        }


        public static void OnStationWinLocateButtonClick(int obj)
        {
            //VFAudio.Create("ui-click-0", null, Vector3.zero, true, 2);
            UIStationWindow stationWindow = UIRoot.instance.uiGame.stationWindow;
            StationComponent stationComponent = stationWindow.transport.stationPool[stationWindow.stationId];
            LocateStation(stationComponent, stationWindow.transport.planet.id);
        }

        public static void ToggleBalanceWindow()
        {
            int itemId = ItemIdHintUnderMouse();
            if (itemId > 0)
            {
                _win.SetUpAndOpen(itemId, 0, false);
                return;
            }

            if (_win.active)
            {
                _win._Close();
            }
            else
            {
                _win.OpenWithoutSetting();
            }
        }

        //UIItemTip依存にすると楽だがチップが出るまでのタイムラグがあるので操作感が悪い
        public static int ItemIdHintUnderMouse()
        {
            List<RaycastResult> targets = new List<RaycastResult>();
            PointerEventData pointer = new PointerEventData(EventSystem.current);
            pointer.position = Input.mousePosition;
            EventSystem.current.RaycastAll(pointer, targets);
            foreach (RaycastResult target in targets)
            {
                UIButton btn = target.gameObject.GetComponentInParent<UIButton>();
                if (btn?.tips != null && btn.tips.itemId > 0)
                {
                    return btn.tips.itemId;
                }

                UIBalanceWindow balWin = target.gameObject.GetComponentInParent<UIBalanceWindow>();
                if (balWin != null)
                {
                    return 0;
                }

                UIReplicatorWindow repWin = target.gameObject.GetComponentInParent<UIReplicatorWindow>();
                if (repWin != null)
                {
                    int mouseRecipeIndex = AccessTools.FieldRefAccess<UIReplicatorWindow, int>(repWin, "mouseRecipeIndex");
                    RecipeProto[] recipeProtoArray = AccessTools.FieldRefAccess<UIReplicatorWindow, RecipeProto[]>(repWin, "recipeProtoArray");
                    if (mouseRecipeIndex < 0)
                    {
                        return 0;
                    }
                    RecipeProto recipeProto = recipeProtoArray[mouseRecipeIndex];
                    if (recipeProto != null)
                    {
                        return recipeProto.Results[0];
                    }
                    return 0;
                }

                UIStorageGrid grid = target.gameObject.GetComponentInParent<UIStorageGrid>();
                if (grid != null)
                {
                    StorageComponent storage = AccessTools.FieldRefAccess<UIStorageGrid, StorageComponent>(grid, "storage");
                    int mouseOnX = AccessTools.FieldRefAccess<UIStorageGrid, int>(grid, "mouseOnX");
                    int mouseOnY = AccessTools.FieldRefAccess<UIStorageGrid, int>(grid, "mouseOnY");
                    if (mouseOnX >= 0 && mouseOnY >= 0 && storage != null)
                    {
                        int num6 = mouseOnX + mouseOnY * grid.colCount;
                        return storage.grids[num6].itemId;
                    }
                    return 0;
                }

                UIProductEntry productEntry = target.gameObject.GetComponentInParent<UIProductEntry>();
                if (productEntry != null)
                {
                    if (productEntry.productionStatWindow.isProductionTab)
                    {
                        return productEntry.entryData.itemId;
                    }
                    return 0;
                }
            }
            return 0;
        }

        private void Update()
        {
            if (!GameMain.isRunning || GameMain.isPaused || GameMain.instance.isMenuDemo )
            {
                return;
            }
            if (mainWindowHotkey.Value.IsDown())
            {
                ToggleBalanceWindow();
            }
        }


        static class Patch
        {
            internal static bool _initialized = false;
            internal static GameObject starmapBtnGO;
            internal static GameObject stationWindowControls;

            internal static void AddButtonToStationWindow()
            {
                UIStationWindow stationWindow = UIRoot.instance.uiGame.stationWindow;

                //navi btn
                UIButton btn = Util.MakeIconButtonB(astroIndicator, 22);
                if (btn != null)
                {
                    btn.gameObject.name = "lstm-locate-btn";
                    RectTransform rect = Util.NormalizeRectD(btn.gameObject);

                    rect.SetParent(stationWindow.windowTrans, false);
                    rect.anchoredPosition = new Vector3(534f, -60f);
                    btn.onClick += OnStationWinLocateButtonClick;
                    btn.tips.tipTitle = "Locate Station";
                    btn.tips.tipText = "Show navigation to this station";
                    btn.tips.corner = 8;
                    btn.tips.offset = new Vector2(0f, 8f);
                    stationWindowControls = btn.gameObject;
                    stationWindowControls.SetActive(true);
                }

                if (LSTM.showButtonInStationWindow.Value)
                {
                    //before _OnCreate
                    //storageUIPrefabに付けたほうが効率良いけどUIBalanceListEntryでも使う
                    //UIStationStorageParasite.MakeUIStationStorageParasite(stationWindow.storageUIPrefab);

                    //after _OnCreate
                    UIStationStorage[] storageUIs = AccessTools.FieldRefAccess<UIStationWindow, UIStationStorage[]>(stationWindow, "storageUIs");
                    for (int i = 0; i < storageUIs.Length; i++)
                    {
                        UIStationStorageParasite.MakeUIStationStorageParasite(storageUIs[i]);
                    }
                }
            }

            internal static void AddButtonToStarmap()
            {
                UIPlanetDetail planetDetail = UIRoot.instance.uiGame.planetDetail;
                GameObject go = planetDetail.typeText.gameObject;
                if (go != null)
                {
                    Transform parent = go.transform.parent;
                    UIButton btn = Util.MakeSmallTextButton("LSTM", 38f, 20f);
                    btn.gameObject.name = "lstm-show-btn";
                    RectTransform rect = Util.NormalizeRectD(btn.gameObject);
                    rect.SetParent(parent, false);
                    rect.anchoredPosition = new Vector3(-2f, -36f);
                    rect.localScale = Vector3.one;
                    btn.onClick += OnStarmapButtonClick;
                    starmapBtnGO = btn.gameObject;
                    starmapBtnGO.SetActive(true);
                }
            }


            [HarmonyPrefix, HarmonyPatch(typeof(UIGame), "_OnCreate")]
            public static void UIGame__OnCreate_Prefix()
            {
                if (LSTM.showButtonInStatisticsWindow.Value)
                {
                    ProductEntryParasite.AddButtonToStatisticsWindow();
                }
            }

            [HarmonyPostfix, HarmonyPatch(typeof(UIGame), "_OnCreate")]
            public static void UIGame__OnCreate_Postfix()
            {
                if (!_initialized)
                {
                    _initialized = true;
                    _win = MyWindowCtl.CreateWindow<UIBalanceWindow>("LSTMBalanceWindow", "LSTM");

                    AddButtonToStarmap();
                    AddButtonToStationWindow();

                }
            }

            //[HarmonyPrefix, HarmonyPatch(typeof(GameMain), "Begin")]
            //public static void GameMain_Begin_Prefix()
            //{
            //}

            //[HarmonyPrefix, HarmonyPatch(typeof(GameMain), "End")]
            //public static void GameMain_End_Prefix()
            //{
            //}

            [HarmonyPrefix, HarmonyPatch(typeof(UIGame), "OnPlayerInspecteeChange")]
            public static void UIGame_OnPlayerInspecteeChange_Prefix(UIGame __instance, EObjectType objType, int objId)
            {
                int planetId = GameMain.mainPlayer.planetId;
                if (planetId > 0 && objType == EObjectType.Entity && objId > 0)
                {
                    if (navi.naviLine.entityId == objId && navi.naviLine.planetId == planetId && UIGame.viewMode < EViewMode.Globe)
                    {
                        navi.Disable();
                    }
                }
            }

            [HarmonyPrefix, HarmonyPatch(typeof(PlanetFactory), "RemoveEntityWithComponents")]
            public static void PlanetFactory_RemoveEntityWithComponents_Prefix(PlanetFactory __instance, int id)
            {
                if (navi.naviLine.entityId == id && navi.naviLine.planetId == __instance.planetId)
                {
                    navi.Disable();
                }
                if (__instance.entityPool[id].stationId != 0) //before RemoveStationComponent
                {
                    _win._Close();
                }
            }

            [HarmonyPostfix, HarmonyPatch(typeof(UIStationWindow), "OnStationIdChange")]
            public static void UIStationWindow_OnStationIdChange_Postfix(UIStationWindow __instance)
            {
                if (stationWindowControls != null && __instance.active && __instance.stationId != 0 && __instance.factory != null)
                {
                    StationComponent stationComponent = __instance.transport.stationPool[__instance.stationId];
                    if (stationComponent != null || stationComponent.id == __instance.stationId)
                    {
                        if (stationComponent.isVeinCollector)
                        {
                            stationWindowControls.SetActive(false);
                        }
                        else
                        {
                            stationWindowControls.SetActive(true);
                        }
                    }
                }
            }

            [HarmonyPostfix, HarmonyPatch(typeof(UIStationStorage), "RefreshValues")]
            public static void UIStationStorage_RefreshValues_Postfix(UIStationStorage __instance)
            {
                if (LSTM.showButtonInStationWindow.Value)
                {
                    __instance.GetComponent<UIStationStorageParasite>()?.RefreshValues();
                }
            }

            //ここへ来るまでの判定
            //GameMain: FixedUpdate(): if (!this._paused) 
            //Gamedata: GameTick(): if (this.mainPlayer != null && !this.demoTicked)
            [HarmonyPostfix, HarmonyPatch(typeof(PlayerControlGizmo), "GameTick")]
            public static void PlayerControlGizmo_GameTick_Postfix()
            {
                navi.naviLine.GameTick();
            }


            //ホイールでズームしないように
            [HarmonyPostfix, HarmonyPatch(typeof(VFInput), "get__cameraZoomIn")]
            public static void VFInput__cameraZoomIn_Postfix(ref float __result)
            {
                if (_win.isPointEnter)
                {
                    __result = 0f;
                }
            }
            [HarmonyPostfix, HarmonyPatch(typeof(VFInput), "get__cameraZoomOut")]
            public static void VFInput__cameraZoomOut_Postfix(ref float __result)
            {
                if (_win.isPointEnter)
                {
                    __result = 0f;
                }
            }

        }

    }

    public class ProductEntryParasite : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField]
        public UIProductEntry productEntry;
        [SerializeField]
        public Button showBalanceButton;

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

        public static void AddButtonToStatisticsWindow()
        {
            //UIStatisticsWindow _OnCreate() より先に
            UIStatisticsWindow statisticsWindow = UIRoot.instance.uiGame.statWindow;
            UIProductEntry productEntry = statisticsWindow.productEntry;
            UIButton btn = Util.MakeSmallTextButton("LSTM", 38f, 20f);
            RectTransform rect = Util.NormalizeRectD(btn.gameObject);
            rect.SetParent(productEntry.transform, false);
            rect.anchoredPosition = new Vector3(6f, -6f);
            rect.localScale = Vector3.one;
            btn.gameObject.SetActive(false);
            ProductEntryParasite p = productEntry.gameObject.AddComponent<ProductEntryParasite>();
            p.productEntry = productEntry;
            p.showBalanceButton = btn.button;

            //OnPointerEnter OnPointerExit のため
            Image img = productEntry.gameObject.AddComponent<Image>();
            img.color = Color.clear;
            img.alphaHitTestMinimumThreshold = 0f;
        }

    }



    public class UIStationStorageParasite : MonoBehaviour
    {
        public UIStationStorage uiStorage;
        
        [SerializeField]
        public UIButton localBtn;
        [SerializeField]
        public UIButton remoteBtn;

        void Start()
        {
            if(localBtn != null) localBtn.onClick += OpenLocalBalance;
            if (remoteBtn != null) remoteBtn.onClick += OpenRemoteBalance;
        }

        public void RefreshValues()
        {
            if (!LSTM.showButtonInStationWindow.Value || uiStorage.station == null || uiStorage.index >= uiStorage.station.storage.Length || uiStorage.station.storage[uiStorage.index].itemId <= 0 || uiStorage.popupBoxRect.gameObject.activeSelf)
            {
                localBtn.gameObject.SetActive(false);
                remoteBtn.gameObject.SetActive(false);
                return;
            }

            if (uiStorage.station.isCollector)
            {
                localBtn.gameObject.SetActive(false);
            }
            else
            {
                localBtn.gameObject.SetActive(true);
            }

            if (uiStorage.station.isStellar || uiStorage.station.isCollector)
            {
                remoteBtn.gameObject.SetActive(true);
            }
            else
            {
                remoteBtn.gameObject.SetActive(false);
            }
        }


        public void OpenLocalBalance(int obj)
        {
            //VFAudio.Create("ui-click-0", null, Vector3.zero, true, 2);
            OpenBalance(true);
        }
        public void OpenRemoteBalance(int obj)
        {
            //VFAudio.Create("ui-click-0", null, Vector3.zero, true, 2);
            OpenBalance(false);
        }

        public void OpenBalance(bool isLocal)
        {
            StationComponent cmp = uiStorage.station;
            if (cmp == null)
            {
                return;
            }
            int index = uiStorage.index;
            UIStationWindow stationWindow = UIRoot.instance.uiGame.stationWindow;
            
            LSTM.OpenBalanceWindow(cmp, index, isLocal, stationWindow.factory);
        }

        public static UIStationStorageParasite MakeUIStationStorageParasite(UIStationStorage stationStorage)
        {
            GameObject parent = stationStorage.gameObject;
            GameObject go = new GameObject("lstm-open-barance-button");

            UIStationStorageParasite parasite = parent.AddComponent<UIStationStorageParasite>();
            go.transform.parent = parent.transform;
            go.transform.localPosition = new Vector3(523, -60, 0);
            go.transform.localScale = new Vector3(1, 1, 1);
            //RectTransform rect = (RectTransform)go.transform;
            //rect.sizeDelta = new Vector2(16, 32);

            Sprite s = Util.LoadSpriteResource("ui/textures/sprites/icons/resume-icon");
            parasite.remoteBtn = Util.MakeIconButton(go.transform, s, 0, 0);
            parasite.localBtn = Util.MakeIconButton(go.transform, s, 0, 32);

            if (parasite.localBtn != null && parasite.remoteBtn != null)
            {

                parasite.localBtn.gameObject.name = "lstm-open-barance-local";
                parasite.remoteBtn.gameObject.name = "lstm-open-barance-remote";
                //btn.uiBtn.gameObject.transform.Find("bg").gameObject.SetActive(false); //or destroy
                //btn.uiBtn.gameObject.transform.Find("sd").gameObject.SetActive(false);
                parasite.uiStorage = stationStorage;
            }
            else
            {
                LSTM.instance.Log("UIStationStorageParasite is null");
            }

            return parasite;
        }
    }
}
