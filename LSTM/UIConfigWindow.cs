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

        public static UIConfigWindow CreateWindow()
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

        protected override void _OnCreate()
        {
            windowTrans = MyWindowCtl.GetRectTransform(this);
            windowTrans.sizeDelta = new Vector2(640f, 360f);

            CreateUI();
        }

        internal void CreateUI()
        {
            float y_ = 54;
            float x_ = 36f;
            void AddElement(RectTransform rect_, float height)
            {
                if (rect_ != null)
                {
                    Util.NormalizeRectWithTopLeft(rect_, x_, y_, windowTrans);
                }
                y_ += height;
                if (y_ > windowTrans.sizeDelta.y)
                {
                    y_ = 60f;
                    x_ += windowTrans.sizeDelta.x / 2;
                }
            }

            Text CreateText(string label_)
            {
                Text src_ = MyWindowCtl.GetTitleText(this);
                Text txt_ = GameObject.Instantiate<Text>(src_);
                txt_.gameObject.name = "label";
                txt_.text = label_;
                (txt_.transform as RectTransform).sizeDelta = new Vector2(txt_.preferredWidth + 40f, 30f);
                return txt_;
            }

            RectTransform rect;

            rect = MyKeyBinder.CreateKeyBinder(LSTM.mainWindowHotkey, "Main Hotkey");
            AddElement(rect, 90f);

            rect = MyCheckBox.CreateCheckBox(LSTM.showMaterialPicker, "Show Material Picker");
            AddElement(rect, 26f);
            rect = MyCheckBox.CreateCheckBox(LSTM.indicatesWarperSign, "Indicates Warper Sign");
            AddElement(rect, 26f);
            rect = MyCheckBox.CreateCheckBox(LSTM.reactClosePanelKeyE, "Close Panel With E Key");
            AddElement(rect, 26f);
            rect = MyCheckBox.CreateCheckBox(LSTM.actAsStandardPanel, "Act As Standard Panel");
            AddElement(rect, 36f);
            rect = MyCheckBox.CreateCheckBox(LSTM.showStationInfo, "Show Station Info Icon");
            AddElement(rect, 26f);
            rect = MyCheckBox.CreateCheckBox(LSTM.setConstructionPointToGround, "Set Construction Point To Ground");
            AddElement(rect, 26f);

            x_ = 320f;
            y_ = 54;
            Text txt = CreateText("Show Open LSTM Button On");
            AddElement(txt.transform as RectTransform, 28f);
            x_ += 16;
            rect = MyCheckBox.CreateCheckBox(LSTM.showButtonInStationWindow, "Show Button In Station Window");
            AddElement(rect, 26f);
            rect = MyCheckBox.CreateCheckBox(LSTM.showButtonInStatisticsWindow, "Show Button In Statistics Window");
            AddElement(rect, 26f);
            rect = MyCheckBox.CreateCheckBox(LSTM.showButtonInStarmap, "Show Button In Starmap");
            AddElement(rect, 36f);
            x_ -= 16;
            rect = MyCheckBox.CreateCheckBox(LSTM.showStatInStatisticsWindow, "Show Stat In Statistics Window");
            AddElement(rect, 26f);




        }

        protected override void _OnDestroy()
        {
        }

        protected override bool _OnInit()
        {
            windowTrans.anchoredPosition = new Vector2(0, 0);
            return true;
        }

        protected override void _OnFree()
        {
        }

        protected override void _OnRegEvent()
        {

        }

        protected override void _OnUnregEvent()
        {

        }

        protected override void _OnOpen()
        {

        }

        protected override void _OnClose()
        {

        }

        protected override void _OnUpdate()
        {
            if (VFInput.escape && !VFInput.inputing)
            {
                VFInput.UseEscape();
                base._Close();
            }
        }
    }
}
