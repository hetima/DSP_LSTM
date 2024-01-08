using BepInEx;
using BepInEx.Logging;
using BepInEx.Configuration;
using HarmonyLib;
//using System;
//using System.Text;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Linq;
using System.Reflection.Emit;
using System.Reflection;

namespace LSTMMod
{

    [BepInPlugin(__GUID__, __NAME__, "0.8.7")]
    public class LSTM : BaseUnityPlugin
    {
        public const string __NAME__ = "LSTM";
        public const string __GUID__ = "com.hetima.dsp." + __NAME__;

        public static LSTM instance = null;
        public static LSTMNavi navi = null;
        public static UIBalanceWindow _win;
        public static UILogWindow _logWindow;
        public static UIConfigWindow _configWin;
        public static StationSignRenderer stationSignRenderer;

        public static ConfigEntry<KeyboardShortcut> mainWindowHotkey;
        public static ConfigEntry<KeyboardShortcut> logWindowHotkey;
        public static ConfigEntry<KeyboardShortcut> switchDisplayModeHotkey;

        public static ConfigEntry<bool> dropSorterKeyEracesNavi;
        public static ConfigEntry<bool> showButtonInStationWindow;
        public static ConfigEntry<bool> showButtonInStatisticsWindow;
        public static ConfigEntry<bool> showButtonInStarmap;

        public static ConfigEntry<bool> showStatInStatisticsWindow;
        public static ConfigEntry<bool> actAsStandardPanel;
        public static ConfigEntry<bool> indicatesWarperSign;
        public static ConfigEntry<bool> reactClosePanelKeyE;
        public static ConfigEntry<bool> showMaterialPicker;
        public static ConfigEntry<bool> setConstructionPointToGround;
        public static ConfigEntry<bool> showStationInfo;
        public static ConfigEntry<bool> showStationInfoOnlyInPlanetView;
        public static ConfigEntry<float> stationInfoIconSize;
        public static ConfigEntry<bool> enableNaviToEverywhere;
        public static ConfigEntry<bool> hideStoragedSlot;

        public static ConfigEntry<bool> enableTLRemoteCluster;
        public static ConfigEntry<bool> enableTLLocalCluster;
        public static ConfigEntry<bool> enableTLConsiderOppositeRange;
        public static ConfigEntry<bool> enableTLDCBalance;
        public static ConfigEntry<float> TLDCSupplyMultiplier;
        public static ConfigEntry<float> TLDCDemandMultiplier;
        public static ConfigEntry<float> TLDCSupplyDenominator;
        public static ConfigEntry<bool> enableTLRemoteDemandDelay;
        public static ConfigEntry<bool> enableTLLocalDemandDelay;
        
        public static ConfigEntry<bool> enableTLSmartTransport;
        public static ConfigEntry<bool> enableOneTimeDemand;
        public static ConfigEntry<bool> oneTimeDemandIgnoreSupplyRange;
        public static ConfigEntry<bool> suppressOpenInventory;
        public static ConfigEntry<bool> enableTrafficLog;
        public static bool enableTrafficLogInThisSession;
        public static ConfigEntry<int> trafficLogDisplayMax;

        public static ConfigEntry<bool> _showStatInStatisticsWindow;

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
            logWindowHotkey = Config.Bind("Keyboard Shortcuts", "logWindowHotkey", KeyboardShortcut.Deserialize(""),
                "Hotkey to open/close Traffic Log window");
            switchDisplayModeHotkey = Config.Bind("Keyboard Shortcuts", "switchDisplayModeHotkey", KeyboardShortcut.Deserialize("Tab"),
                "Hotkey to switch display mode of LSTM window");
            showButtonInStationWindow = Config.Bind("Interface", "showButtonInStationWindow", true,
                "Add open LSTM button to Station Window (needs restart)");
            showButtonInStatisticsWindow = Config.Bind("Interface", "showButtonInStatisticsWindow", false,
                "Add open LSTM button to Statistics Window");
            showButtonInStarmap = Config.Bind("Interface", "showButtonInStarmap", true,
                "Add open LSTM button to detail panel on starmap");
            showStatInStatisticsWindow = Config.Bind("Interface", "showStatInStatisticsWindow", true,
                "Add station stat to Statistics Window");
            setConstructionPointToGround = Config.Bind("Other", "setConstructionPointToGround", false,
                "set the construction point to ground instead of top of the tower (currently disabled)");

            actAsStandardPanel = Config.Bind("Interface", "actAsStandardPanel", true,
                "true: close with other panels by esc key. false: one more esc needed");
            dropSorterKeyEracesNavi = Config.Bind("Keyboard Shortcuts", "dropSorterKeyEracesNavi", false,
                "clear navi line when \"Remove Copied Sorter Previews\" shortcut is pressed");
            indicatesWarperSign = Config.Bind("Interface", "indicatesWarperSign", false,
                "show sign on the list if station has warper.");
            reactClosePanelKeyE = Config.Bind("Keyboard Shortcuts", "reactClosePanelKeyE", true,
                "close window when close panel key(E) is pressed.");
            showMaterialPicker = Config.Bind("Interface", "showMaterialPicker", true,
                "Add Material Picker for quick item switching to LSTM window");
            showStationInfo = Config.Bind("Interface", "showStationInfo", false,
                "Show station contents and empty slot count as icon. Also affected by in-game building icon display setting");
            showStationInfoOnlyInPlanetView = Config.Bind("Interface", "showStationInfoOnlyInPlanetView", false,
                "showStationInfo is only displayed in planet view");
            stationInfoIconSize = Config.Bind("Interface", "stationInfoIconSize", 10f,
                new ConfigDescription("station Info icon size. min=5.0 max=15.0. default is 10.0f", new AcceptableValueRange<float>(5.0f, 15.0f)));
            enableNaviToEverywhere = Config.Bind("Other", "enableNaviToEverywhere", false,
                "double-click on Planet View to display navigation to anywhere");
            hideStoragedSlot = Config.Bind("Interface", "hideStoragedSlot", true,
            "hide storaged slot in list view"); 

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
            enableTLLocalDemandDelay = Config.Bind("TrafficLogic", "TLLocalDemandDelay", false,
                "enable TrafficLogic:Local Demand Delay");
            enableTLSmartTransport = Config.Bind("TrafficLogic", "TLSmartTransport", false,
                "enable TrafficLogic:Smart Transport");
            enableOneTimeDemand = Config.Bind("TrafficLogic", "enableOneTimeDemand", false,
                "enable One-Time Demand");
            oneTimeDemandIgnoreSupplyRange = Config.Bind("TrafficLogic", "oneTimeDemandIgnoreSupplyRange", false,
                "One-Time Demand ignores supply range");
            _showStatInStatisticsWindow = Config.Bind("Z", "_showStatInStatisticsWindow", true,
                "Internal setting. Do not change directly");
            suppressOpenInventory = Config.Bind("Other", "suppressOpenInventory", false,
                "Suppress open inventory when opening station window");
            enableTrafficLog = Config.Bind("Other", "enableTrafficLog", false,
                "Enable traffic log window (needs restart game)");
            trafficLogDisplayMax = Config.Bind("Other", "trafficLogDisplayMax", 2000,
                new ConfigDescription("Maximum rows that can be displayed in the log window. min=100 max=9999. original behavior is 2000", new AcceptableValueRange<int>(100, 9999)));


            enableTrafficLogInThisSession = enableTrafficLog.Value;
            Harmony harmony = new Harmony(__GUID__);
            harmony.PatchAll(typeof(Patch));
            harmony.PatchAll(typeof(LSTMStarDistance.Patch));
            harmony.PatchAll(typeof(MyWindowCtl.Patch));
            harmony.PatchAll(typeof(TrafficLogic.Patch));
            harmony.PatchAll(typeof(UIStatisticsWindowAgent.Patch));
            //harmony.PatchAll(typeof(ConstructionPoint.Patch));
            harmony.PatchAll(typeof(UIStationStorageAgent.Patch));

            if (enableTrafficLogInThisSession)
            {
                harmony.PatchAll(typeof(TrafficLog.Patch));
            }
            
        }

        public static void Log(string str)
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

        public static void OpenBalanceWindow(int itemId, int planetId = 0, int starId = 0)
        {
            _win.SetUpAndOpen(itemId, planetId, false, starId);
        }

        public static void OpenBalanceWindow()
        {
            _win.SetUpAndOpen(0, 0, true);
        }

        public static void OpenStationWindow(StationComponent station, int planetId)
        {
            if (GameMain.mainPlayer.factory == null || planetId != GameMain.mainPlayer.factory.planetId || station == null)
            {
                return;
            }

            _win.keepOpen = true;
            GameMain.mainPlayer.controller.actionInspect.SetInspectee(EObjectType.Entity, station.entityId);
            _win.keepOpen = false;
        }

        public static void IntegrationOpenPlanetId(int planetId)
        {
            _win.SetUpAndOpen(0, planetId, false);
        }

        public static void IntegrationOpenItemId(int itemId)
        {
            _win.SetUpAndOpen(itemId, 0, false);
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
        public static void ToggleLogWindow()
        {
            if (_logWindow.active)
            {
                _logWindow._Close();
            }
            else
            {
                _logWindow.OpenWithoutSetting();
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
                        return productEntry.entryData?.itemId ?? 0;
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

            if (VFInput.inputing)
            {
                return;
            }
            if (mainWindowHotkey.Value.IsDown())
            {
                ToggleBalanceWindow();
            }
            else if (logWindowHotkey.Value.IsDown())
            {
                if (enableTrafficLogInThisSession)
                {
                    ToggleLogWindow();
                }
            }
            else if (reactClosePanelKeyE.Value && VFInput._closePanelE)
            {
                if (_win.active)
                {
                    _win._Close();
                }
            }
        }

        ////ホイールでズームしないように
        private void FixedUpdate()
        {
            if ((_win != null && _win.isPointEnter) || (_logWindow != null && _logWindow.isPointEnter))
            {
                VFInput.inScrollView = true;
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
                UIButton btn = Util.MakeIconButtonB(Util.astroIndicatorIcon, 22);
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

                //if (LSTM.showButtonInStationWindow.Value)
                {
                    //before _OnCreate
                    //storageUIPrefabに付けたほうが効率良いけどUIBalanceListEntryでも使う
                    //UIStationStorageAgent.MakeUIStationStorageAgent(stationWindow.storageUIPrefab);

                    //after _OnCreate
                    UIStationStorage[] storageUIs = AccessTools.FieldRefAccess<UIStationWindow, UIStationStorage[]>(stationWindow, "storageUIs");
                    for (int i = 0; i < storageUIs.Length; i++)
                    {
                        UIStationStorageAgent.MakeUIStationStorageAgent(storageUIs[i]);
                    }
                    LSTM.showButtonInStationWindow.SettingChanged += (sender, args) => {
                        foreach (UIStationStorageAgent agent in UIStationStorageAgent.agents)
                        {
                            agent.RefreshValues();
                        }
                    };
                }
            }

            internal static void AddButtonToStarmap()
            {
                UIPlanetDetail planetDetail = UIRoot.instance.uiGame.planetDetail;
                Transform parent = planetDetail?.transform;
                parent = parent?.Find("detail_group");
                if (parent != null)
                {
                    UIButton btn = Util.MakeSmallTextButton("LSTM", 38f, 20f);
                    btn.gameObject.name = "lstm-show-btn";
                    RectTransform rect = Util.NormalizeRectD(btn.gameObject);
                    rect.SetParent(parent, false);
                    rect.anchoredPosition3D = new Vector3(0f, -64f, 0f);
                    rect.localScale = Vector3.one;
                    btn.onClick += OnStarmapButtonClick;
                    starmapBtnGO = btn.gameObject;
                    starmapBtnGO.SetActive(showButtonInStarmap.Value);
                    showButtonInStarmap.SettingChanged += (sender, args) => {
                        starmapBtnGO.SetActive(showButtonInStarmap.Value);
                    };
                }
            }


            [HarmonyPrefix, HarmonyPatch(typeof(UIGame), "_OnCreate")]
            public static void UIGame__OnCreate_Prefix()
            {
                if (!_initialized) {
                    UIStatisticsWindowAgent.PreCreate();
                }
            }

            [HarmonyPostfix, HarmonyPatch(typeof(UIGame), "_OnCreate")]
            public static void UIGame__OnCreate_Postfix()
            {
                if (!_initialized)
                {
                    _initialized = true;
                    _configWin = UIConfigWindow.CreateInstance();
                    _win = UIBalanceWindow.CreateInstance();
                    _logWindow = UILogWindow.CreateInstance();
                    TrafficLog.trafficLogDelegate = _logWindow;
                    AddButtonToStarmap();
                    AddButtonToStationWindow();
                    UIStatisticsWindowAgent.PostCreate();
                    stationSignRenderer = new StationSignRenderer();
                    stationSignRenderer.Init();
                }
            }

            [HarmonyPrefix, HarmonyPatch(typeof(GameMain), "Begin")]
            public static void GameMain_Begin_Prefix()
            {
                //reset
                OneTimeDemand.ResetOneTimeDemandState();
            }

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


            //ここへ来るまでの判定
            //GameMain: FixedUpdate(): if (!this._paused) 
            //Gamedata: GameTick(): if (this.mainPlayer != null && !this.demoTicked)
            [HarmonyPostfix, HarmonyPatch(typeof(PlayerControlGizmo), "GameTick")]
            public static void PlayerControlGizmo_GameTick_Postfix()
            {
                navi.naviLine.GameTick();
            }

            [HarmonyPostfix, HarmonyPatch(typeof(EntitySignRenderer), "Draw")]
            public static void Mod_entitySignPool_Postfix(EntitySignRenderer __instance)
            {
                if (GameMain.localPlanet != null && __instance != null)
                {
                    stationSignRenderer.Draw(__instance.factory);
                }
            }

            private static float _lastClick;
            [HarmonyPostfix, HarmonyPatch(typeof(RaycastLogic), "GameTick")]
            public static void RaycastLogic_GameTick_Postfix()
            {
                if (UIGame.viewMode != EViewMode.Globe || !enableNaviToEverywhere.Value || GameMain.localPlanet == null)
                {
                    return;
                }
                if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
                {
                    float now = Time.time;
                    if (_lastClick + 0.035f < now && _lastClick + 0.5f > now)
                    {
                        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                        if (Phys.RayCastSphere(ray.origin, ray.direction, 1600f, Vector3.zero, GameMain.localPlanet.realRadius, out RCHCPU rch))
                        {
                            navi.Disable();
                            navi.SetPointNavi(rch.point + (rch.normal * 2), GameMain.localPlanet.id);
                        }
                        _lastClick = 0f;
                    }
                    else
                    {
                        _lastClick = now;
                    }
                }

            }

            public static void SuppressOrOpenInventory(UIGame uiGame)
            {
                if (LSTM.suppressOpenInventory.Value)
                {
                    return;
                }
                else
                {
                    uiGame.OpenPlayerInventory();
                }
            }

            [HarmonyTranspiler, HarmonyPatch(typeof(UIGame), "OnPlayerInspecteeChange")]
            public static IEnumerable<CodeInstruction> UIGame_OnPlayerInspecteeChange_Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                List<CodeInstruction> ins = instructions.ToList();

                MethodInfo m_OpenStationWindow = typeof(UIGame).GetMethod(nameof(UIGame.OpenStationWindow));
                MethodInfo m_OpenPlayerInventory = typeof(UIGame).GetMethod(nameof(UIGame.OpenPlayerInventory));
                MethodInfo m_SuppressOrOpenInventory = typeof(LSTM.Patch).GetMethod(nameof(SuppressOrOpenInventory));

                //IL_06a6: ldarg.0      // this
                //IL_06a7: call instance void UIGame::ShutAllFunctionWindow()
                //// [1452 13 - 1452 39]
                //IL_06ac: ldarg.0      // this
                //IL_06ad: call instance void UIGame::OpenPlayerInventory()
                //// [1453 13 - 1453 37]
                //IL_06b2: ldarg.0      // this
                //IL_06b3: call instance void UIGame::OpenStationWindow()

                int patchCount = 0;
                for (int i = ins.Count - 10; i > 10; i--)
                {
                    if (ins[i].opcode == OpCodes.Call && ins[i].operand is MethodInfo o && o == m_OpenStationWindow)
                    {
                        if (ins[i - 2].opcode == OpCodes.Call && ins[i - 2].operand is MethodInfo o2 && o2 == m_OpenPlayerInventory)
                        {
                            ins[i - 2].opcode = OpCodes.Call;
                            ins[i - 2].operand = m_SuppressOrOpenInventory;
                            patchCount++;
                            break;
                        }
                    }
                }
                if (patchCount != 1)
                {
                    LSTM.Log("UIGame_OnPlayerInspecteeChange_Transpiler (OpenStationWindow) seems wrong");
                }

                return ins.AsEnumerable();
            }





        }

    }
}
