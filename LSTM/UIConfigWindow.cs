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
            windowTrans.sizeDelta = new Vector2(360f, 260f);

            CreateUI();
        }

        internal void CreateUI()
        {
            float y_ = windowTrans.sizeDelta.y - 80f;
            float x_ = 40f;
            void addElement(RectTransform rect_)
            {
                if (rect_ != null)
                {
                    rect_.SetParent(windowTrans, false);
                    rect_.anchoredPosition = new Vector2(x_, y_);
                }
                y_ -= 30;
                if (y_ < 40f)
                {
                    y_ = windowTrans.sizeDelta.y - 80f;
                    x_ += windowTrans.sizeDelta.x / 2;
                }
            }

            RectTransform rect = MyCheckBox.CreateCheckBox(LSTM.showMaterialPicker, "Show Material Picker");
            addElement(rect);
            rect = MyCheckBox.CreateCheckBox(LSTM.setConstructionPointToGround, "Set Construction Point To Ground");
            addElement(rect);
            rect = MyCheckBox.CreateCheckBox(LSTM.reactClosePanelKeyE, "Close Panel With E Key");
            addElement(rect);
            rect = MyCheckBox.CreateCheckBox(LSTM.indicatesWarperSign, "Indicates Warper Sign");
            addElement(rect);
            rect = MyCheckBox.CreateCheckBox(LSTM.actAsStandardPanel, "Act As Standard Panel");
            addElement(rect);

            //addElement(null);
            //rect = MyCheckBox.CreateCheckBox(LSTM.showButtonInStationWindow, "Show Button In Station Window");
            //addElement(rect);
            //rect = MyCheckBox.CreateCheckBox(LSTM.showButtonInStatisticsWindow, "Show Button In Statistics Window");
            //addElement(rect);
            //rect = MyCheckBox.CreateCheckBox(LSTM.showStatInStatisticsWindow, "Show Stat In Statistics Window");
            //addElement(rect);
            


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
