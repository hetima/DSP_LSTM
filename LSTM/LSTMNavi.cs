//using System;
//using System.Text;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using HarmonyLib;

namespace LSTMMod
{
    public class LSTMNavi : NaviLineDelegate
    {

        //ガス惑星手動採集ボタンをコピーして横にずらす
        public static GameObject MakeNaviPop(LSTMNavi navi)
        {
            UIFunctionPanel functionPanel = UIRoot.instance.uiGame.functionPanel;
            UIButton extractButton = functionPanel.extractButton;
            UIButton btn = GameObject.Instantiate<UIButton>(extractButton, extractButton.transform.parent);
            btn.gameObject.name = "lstm-clear-navi-btn";
            Vector3 pos = btn.transform.localPosition;
            pos.x -= 200f;
            btn.transform.localPosition = pos;
            btn.gameObject.SetActive(false);

            //元の onClick は m_PersistentCalls にあるっぽいので作り直す
            Button oldbutton = btn.button;
            GameObject.DestroyImmediate(oldbutton); //Immediateしないと入れ替えられない
            Button button = btn.gameObject.AddComponent<Button>();
            btn.button = button;
            //btn.button.onClick.RemoveAllListeners();
            btn.button?.onClick.AddListener(navi.NaviLineTipClicked);

            Text txt = btn.transform.Find("text")?.GetComponent<Text>();
            if (txt)
            {
                GameObject.Destroy(txt.gameObject.GetComponent<Localizer>());
                txt.fontSize += 2;
                txt.text = "Click here to clear navi".Translate();
            }

            Image img = btn.transform.Find("icon")?.GetComponent<Image>();
            if(img != null) img.sprite = LSTM.astroIndicator;



            return btn.gameObject;
        }

        //右側に出てくるチップにボタンを付けてクリックできるようにする 不採用
        public static UIKeyTipNode MakeNaviTip(LSTMNavi navi)
        {
            UIKeyTipNode naviTip = UIRoot.instance.uiGame.keyTips.RegisterTip("Navi", "Click here to erase");
            GameObject go = new GameObject("button");
            Image image = go.AddComponent<Image>();
            image.color = new Color(1f, 0.25f, 0f, 0.27f);
            var button = go.AddComponent<Button>();
            button.targetGraphic = image;
            button.onClick.AddListener(new UnityAction(navi.NaviLineTipClicked));
            
            go.transform.SetParent(naviTip.transform, false);
            Vector2 size = ((RectTransform)naviTip.transform).sizeDelta;
            RectTransform rect = Util.NormalizeRectC(go, size.x, size.y);
            rect.anchoredPosition = Vector2.zero;
            return naviTip;
        }

        public void NaviLineTipClicked()
        {
            naviLine.Disable(true);
            //naviTip.desired = false;
            naviPop?.SetActive(false);
        }

        public NaviLine naviLine;
        //public UIKeyTipNode naviTip;
        public GameObject naviPop;

        internal bool _initialized = false;

        public LSTMNavi()
        {
            naviLine = new NaviLine();
            naviLine.autoDisappear = true;
            naviLine._delegate = this;
        }
        public void NaviLineWillAppear(NaviLine naviLine)
        {
            if (!_initialized)
            {
                _initialized = true;
                //naviTip = MakeNaviTip(this);
                naviPop = MakeNaviPop(this);
            }
            //if (naviTip != null) naviTip.desired = true;
            naviPop?.SetActive(true);
        }
        public void NaviLineWillDisappear(NaviLine naviLine)
        {
            //if (naviTip != null) naviTip.desired = false;
            naviPop?.SetActive(false);

        }

        public void NaviLineDidGameTick(NaviLine naviLine)
        {
            if (naviPop == null)
            { 
                return; 
            }

            if (UIGame.viewMode >= EViewMode.Globe)
            {
                //naviTip.desired = false;
                naviPop.SetActive(false);
            }
            else
            {
                if (LSTM.dropSorterKeyEracesNavi.Value && VFInput._cancelTemplateInserter.onDown && !VFInput.inputing)
                {
                    Disable();
                }
                //naviTip.desired = (naviLine.lineGizmo != null);
                naviPop.SetActive(naviLine.lineGizmo != null);
            }
        }


        public void SetStationNavi(StationComponent station, int planetId)
        {

            EntityData[] pool = GameMain.galaxy.PlanetById(planetId)?.factory?.entityPool;
            if (pool == null || pool[station.entityId].stationId != station.id)
            {
                return;
            }
            Vector3 pos = pool[station.entityId].pos;
            naviLine.planetId = planetId;
            naviLine.entityId = station.entityId;
            naviLine.endPoint = pos + (pos.normalized * 8);
        }

        public void Disable()
        {
            naviLine.Disable(true);
        }
    }
}
