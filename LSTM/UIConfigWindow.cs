using System;
using System.Text;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace LSTMMod
{
    public class UIConfigWindow : ManualBehaviour, MyWindow
    {
        public RectTransform windowTrans;
        public RectTransform tab1;
        public RectTransform tab2; 
        public UIButton tabBtn1;
        public UIButton tabBtn2;

        public static UIConfigWindow CreateInstance()
        {
            UIConfigWindow win = MyWindowCtl.CreateWindow<UIConfigWindow>("LSTMConfigWindow", "LSTM Config");
            return win;
        }

        public void TryClose()
        {
            base._Close();
        }

        public bool isFunctionWindow()
        {
            return true;
        }


        public void OpenWindow()
        {
            MyWindowCtl.OpenWindow(this);
        }

        public override void _OnCreate()
        {
            windowTrans = MyWindowCtl.GetRectTransform(this);
            windowTrans.sizeDelta = new Vector2(640f, 428f);

            CreateUI();
        }

        internal void CreateUI()
        {
            //tabs
            RectTransform base_ = windowTrans;

            float y_ = 54;
            float x_ = 36f;
            float tabx_ = 36f;
            int tabIndex_ = 1;
            RectTransform AddTab(string label, out UIButton outBtn)
            {
                GameObject tab = new GameObject();
                RectTransform tabRect = tab.AddComponent<RectTransform>();
                Util.NormalizeRectWithMargin(tabRect, 54f+28f, 36f, 0f, 0f, windowTrans);
                tab.name = "tab-" + tabIndex_.ToString();
                //tab button
                UIDESwarmPanel swarmPanel = UIRoot.instance.uiGame.dysonEditor.controlPanel.hierarchy.swarmPanel;
                UIButton src = swarmPanel.orbitButtons[0];
                UIButton btn = GameObject.Instantiate<UIButton>(src);
                RectTransform btnRect = Util.NormalizeRectWithTopLeft(btn, tabx_, 54f, windowTrans);
                btnRect.sizeDelta = new Vector2(100f, 24f);
                //(btn.transform.Find("frame").transform as RectTransform).sizeDelta = btnRect.sizeDelta;
                btn.transform.Find("frame").gameObject.SetActive(false);
                // btn.transitions[0] btn btn.transitions[1]==text btn.transitions[2]==frame
                if (btn.transitions.Length >= 3)
                {
                    btn.transitions[0].normalColor = new Color(0.1f, 0.1f, 0.1f, 0.68f);
                    btn.transitions[0].highlightColorOverride = new Color(0.9906f, 0.5897f, 0.3691f, 0.4f);
                    btn.transitions[1].normalColor = new Color(1f, 1f, 1f, 0.6f);
                    btn.transitions[1].highlightColorOverride = new Color(0.2f, 0.1f, 0.1f, 0.9f);
                }
                Text btnText = btn.transform.Find("Text").GetComponent<Text>();
                btnText.text = label;
                btnText.fontSize = 16;
                btn.data = tabIndex_;
                tabIndex_++;
                tabx_ += 100f;

                outBtn = btn;
                return tabRect;
            }

            void AddElement(RectTransform rect_, float height)
            {
                if (rect_ != null)
                {
                    Util.NormalizeRectWithTopLeft(rect_, x_, y_, base_);
                }
                y_ += height;
            }

            Text CreateText(string label_)
            {
                Text src_ = MyWindowCtl.GetTitleText(this);
                Text txt_ = GameObject.Instantiate<Text>(src_);
                txt_.gameObject.name = "label";
                txt_.text = label_;
                txt_.color = new Color(1f, 1f, 1f, 0.38f);
                (txt_.transform as RectTransform).sizeDelta = new Vector2(txt_.preferredWidth + 40f, 30f);
                return txt_;
            }

            //General tab
            tab1 = AddTab("General", out tabBtn1);
            base_ = tab1;
            RectTransform rect;
            y_ = 0f;
            x_ = 0f;
            rect = MyKeyBinder.CreateKeyBinder(LSTM.mainWindowHotkey, "Main Hotkey");
            AddElement(rect, 90f);

            rect = MyCheckBox.CreateCheckBox(LSTM.showMaterialPicker, "Show Material Picker");
            AddElement(rect, 26f);
            rect = MyCheckBox.CreateCheckBox(LSTM.indicatesWarperSign, "Indicates Warper Sign");
            AddElement(rect, 26f);
            rect = MyCheckBox.CreateCheckBox(LSTM.reactClosePanelKeyE, "Close Panel With E Key");
            AddElement(rect, 26f);
            rect = MyCheckBox.CreateCheckBox(LSTM.actAsStandardPanel, "Act As Standard Panel");
            AddElement(rect, 26f);
            rect = MyCheckBox.CreateCheckBox(LSTM.suppressOpenInventory, "Suppress Open Inventory Window");
            AddElement(rect, 36f);

            rect = MyCheckBox.CreateCheckBox(LSTM.showStationInfo, "Show Station Info Icon");
            AddElement(rect, 26f);
            x_ += 16;
            rect = MyCheckBox.CreateCheckBox(LSTM.showStationInfoOnlyInPlanetView, "Only In Planet View");
            AddElement(rect, 26f);
            x_ -= 16;
            rect = MyCheckBox.CreateCheckBox(LSTM.showStatInStatisticsWindow, "Show Stat On Statistics Window");
            AddElement(rect, 26f);

            x_ = 290f;
            y_ = 0f;
            Text txt = CreateText("Show Open LSTM Button On");
            AddElement(txt.transform as RectTransform, 28f);
            x_ += 16;
            rect = MyCheckBox.CreateCheckBox(LSTM.showButtonInStationWindow, "Station Window");
            AddElement(rect, 26f);
            rect = MyCheckBox.CreateCheckBox(LSTM.showButtonInStatisticsWindow, " Statistics Window");
            AddElement(rect, 26f);
            rect = MyCheckBox.CreateCheckBox(LSTM.showButtonInStarmap, "Starmap Detail Panel");
            AddElement(rect, 36f);
            x_ -= 16;
            rect = MyCheckBox.CreateCheckBox(LSTM.setConstructionPointToGround, "Set Construction Point To Ground");
            AddElement(rect, 26f);
            rect = MyCheckBox.CreateCheckBox(LSTM.enableNaviToEverywhere, "Double-Click To Navi Everywhere");
            AddElement(rect, 22f);
            x_ += 32;
            txt = CreateText("On Planet View");
            txt.fontSize = 17;
            txt.color = new Color(0.698f, 0.698f, 0.698f, 0.6588f);
            AddElement(txt.transform as RectTransform, 26f);
            x_ -= 32;
            
            rect = MyCheckBox.CreateCheckBox(LSTM.enableTrafficLog, "Enable Traffic Log (needs restart game)");
            AddElement(rect, 26f);
            rect = MyCheckBox.CreateCheckBox(LSTM.hideStoragedSlot, "Hide Storaged Slot");
            AddElement(rect, 26f);
            

            //Traffic Logic tab
            tab2 = AddTab("Traffic Logic", out tabBtn2);
            base_ = tab2;
            y_ = 0f;
            x_ = 0f;
            rect = MyCheckBox.CreateCheckBox(LSTM.enableOneTimeDemand, "One-time demand");
            AddElement(rect, 26f);
            x_ += 16;
            rect = MyCheckBox.CreateCheckBox(LSTM.oneTimeDemandIgnoreSupplyRange, "Ignore Supply Range");
            AddElement(rect, 36f);
            x_ -= 16;

            rect = MyCheckBox.CreateCheckBox(LSTM.enableTLSmartTransport, "Smart Transport");
            AddElement(rect, 26f);
            rect = MyCheckBox.CreateCheckBox(LSTM.enableTLConsiderOppositeRange, "Consider Opposite Range");
            AddElement(rect, 26f);
            rect = MyCheckBox.CreateCheckBox(LSTM.enableTLRemoteDemandDelay, "Remote Demand Delay (98%)");
            AddElement(rect, 26f);
            rect = MyCheckBox.CreateCheckBox(LSTM.enableTLLocalDemandDelay, "Local Demand Delay (99%)");
            AddElement(rect, 26f);
            rect = MyCheckBox.CreateCheckBox(LSTM.enableTLRemoteCluster, "Remote Cluster [C:]");
            AddElement(rect, 26f);
            rect = MyCheckBox.CreateCheckBox(LSTM.enableTLLocalCluster, "Local Cluster [c:]");
            AddElement(rect, 26f);

            x_ = 290f;
            y_ = 0f;
            rect = MyCheckBox.CreateCheckBox(LSTM.enableTLDCBalance, "Remote Distance/Capacity Balance *");
            AddElement(rect, 26f);
            x_ += 32f;
            txt = CreateText("Supply 70%-100% Multiplier");
            AddElement(txt.transform as RectTransform, 32f);
            rect = MySlider.CreateSlider(LSTM.TLDCSupplyMultiplier, 1f, 20f, "'x'0", 200f);
            AddElement(rect, 26f);
            txt = CreateText("Demand 0%-30% Multiplier");
            AddElement(txt.transform as RectTransform, 32f);
            rect = MySlider.CreateSlider(LSTM.TLDCDemandMultiplier, 1f, 20f, "'x'0", 200f);
            AddElement(rect, 26f);
            txt = CreateText("Supply 0%-30% Denominator");
            AddElement(txt.transform as RectTransform, 32f);
            rect = MySlider.CreateSlider(LSTM.TLDCSupplyDenominator, 1f, 20f, "'1/'0", 200f);
            AddElement(rect, 26f);

            x_ -= 32f;

            x_ = 0f;
            y_ = 292f;
            txt = CreateText("* Distance/Capacity Balance will be forced off when Smart Transport is on");
            txt.fontSize = 15;
            AddElement(txt.transform as RectTransform, 26f);

            OnTabButtonClick(1);
        }

        public override void _OnDestroy()
        {
        }

        public override bool _OnInit()
        {
            windowTrans.anchoredPosition = new Vector2(0, 0);
            return true;
        }

        public override void _OnFree()
        {
        }

        public override void _OnRegEvent()
        {
            tabBtn1.onClick += OnTabButtonClick;
            tabBtn2.onClick += OnTabButtonClick;
        }

        public override void _OnUnregEvent()
        {
            tabBtn1.onClick -= OnTabButtonClick;
            tabBtn2.onClick -= OnTabButtonClick;
        }

        public override void _OnOpen()
        {

        }

        public override void _OnClose()
        {

        }

        public override void _OnUpdate()
        {
            if (VFInput.escape && !VFInput.inputing)
            {
                VFInput.UseEscape();
                base._Close();
            }
        }

        public void OnTabButtonClick(int obj)
        {
            if (obj == 1)
            {
                tabBtn2.highlighted = false;
                tab2.gameObject.SetActive(false);

                tabBtn1.highlighted = true;
                tab1.gameObject.SetActive(true);
            }
            else
            {
                tabBtn1.highlighted = false;
                tab1.gameObject.SetActive(false);

                tabBtn2.highlighted = true;
                tab2.gameObject.SetActive(true);
            }
        }
    }
}
