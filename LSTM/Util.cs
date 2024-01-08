using System;
using System.Collections.Generic;
using System.Text;

using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;


namespace LSTMMod
{
    public static class Util
    {
        public static string GetStationName(StationComponent station, int planetId = 0)
        {
            if(planetId == 0)
                planetId = station.planetId;

            PlanetFactory pf = GameMain.galaxy.PlanetById(planetId)?.factory;
            if (pf == null)
            {
                return null;
            }
            string text = pf.ReadExtraInfoOnEntity(station.entityId);

            return text;
        }

        public static string GetCommandValue(string str, string cmd, bool ignoreCase = false)
        {
            if (string.IsNullOrEmpty(str) || string.IsNullOrEmpty(cmd))
            {
                return "";
            }
            StringComparison op = StringComparison.Ordinal;
            if (ignoreCase)
            {
                op = StringComparison.OrdinalIgnoreCase;
            }
            string pre = "[" + cmd + ":";
            int start = str.IndexOf(pre, op);
            if (start >= 0)
            {
                start += pre.Length;
                int len = str.IndexOf("]", start);
                if (len > 0)
                {
                    return str.Substring(start, len - start).Trim();
                }
            }

            return "";
        }
        public static Sprite astroIndicatorIcon
        {
            get
            {
                if (_astroIndicatorIcon == null)
                {
                    UIStarmap starmap = UIRoot.instance.uiGame.starmap;
                    _astroIndicatorIcon = starmap.cursorFunctionButton3.transform.Find("icon")?.GetComponent<Image>()?.sprite;
                }
                return _astroIndicatorIcon;
            }
        }
        internal static Sprite _astroIndicatorIcon;


        public static Color DSPBlue => new Color(0.3821f, 0.8455f, 1f, 0.68f);
        public static Color DSPOrange => new Color(0.9906f, 0.5897f, 0.3691f, 0.68f);

        public static void RemovePersistentCalls(GameObject go)
        {
            Button oldbutton = go.GetComponent<Button>();
            UIButton btn = go.GetComponent<UIButton>();
            if (btn != null && oldbutton != null)
            {
                GameObject.DestroyImmediate(oldbutton);
                btn.button = go.AddComponent<Button>();
            }
        }
        public static T CreateGameObject<T>(string name, float width = 0f, float height = 0f) where T : Component
        {
            GameObject go = new GameObject(name);
            RectTransform rect = go.AddComponent<RectTransform>();
            rect.localScale = Vector3.one;
            if (width > 0f || height > 0)
            {
                rect.sizeDelta = new Vector2(width, height);
            }
            T item = go.AddComponent<T>();

            return item;
        }
        public static Text CreateText(string label, int fontSize = 14, string objectName = "text")
        {
            Text txt_;
            Text stateText = UIRoot.instance.uiGame.assemblerWindow.stateText;
            txt_ = GameObject.Instantiate<Text>(stateText);
            txt_.gameObject.name = objectName;
            txt_.text = label;
            txt_.color = new Color(1f, 1f, 1f, 0.4f);
            txt_.alignment = TextAnchor.MiddleLeft;
            //txt_.supportRichText = false;
            txt_.fontSize = fontSize;
            return txt_;
        }

        public static UIListView CreateListView(Action<UIData> dataFunc, string goName = "", Transform parent = null, float vsWidth = 10f)
        {
            UIListView result;
            UIListView src = UIRoot.instance.uiGame.tutorialWindow.entryList;
            GameObject go = GameObject.Instantiate(src.gameObject);
            if (parent != null)
            {
                go.transform.SetParent(parent, false);
            }
            if (!string.IsNullOrEmpty(goName))
            {
                go.name = goName;
            }

            result = go.GetComponent<UIListView>();
            if (dataFunc != null)
            {
                RectTransform itemResTrans = result.m_ItemRes.gameObject.transform as RectTransform;

                //transform.anchorMin = new Vector2(0f, 1f);
                //transform.anchorMax = new Vector2(0f, 1f);
                //transform.localScale = new Vector2(0f, 1f);
                //itemResTrans.sizeDelta = transform.sizeDelta;

                //result.m_ItemRes.transform.DetachChildren
                for (int i = itemResTrans.childCount - 1; i >= 0; i--)
                {
                    GameObject.Destroy(itemResTrans.GetChild(i).gameObject);
                }
                itemResTrans.DetachChildren();
                GameObject.Destroy(itemResTrans.GetComponent<UITutorialListEntry>());
                //GameObject.Destroy(itemResTrans.GetComponent<Image>());//
                //GameObject.Destroy(itemResTrans.GetComponent<Button>());//

                dataFunc(result.m_ItemRes);

                result.RowHeight = (int)itemResTrans.sizeDelta.y;
                result.m_ItemRes.sel_highlight = null;

                result.m_ContentPanel.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
                result.m_ContentPanel.constraintCount = 1;
                result.ColumnWidth = (int)itemResTrans.sizeDelta.x;
                result.RowHeight = (int)itemResTrans.sizeDelta.y;
            }
            result.HorzScroll = false;
            result.VertScroll = true;
            result.CullOutsideItems = false;
            result.ColumnSpacing = 0;
            result.RowSpacing = 4;

            Image barBg = result.m_ScrollRect.verticalScrollbar.GetComponent<Image>();
            if (barBg != null)
            {
                barBg.color = new Color(0f, 0f, 0f, 0.62f);
            }

            //あんまり上手くはないけどとりあえず変わる
            RectTransform vsRect = result.m_ScrollRect.verticalScrollbar.transform as RectTransform;
            vsRect.sizeDelta = new Vector2(vsWidth, vsRect.sizeDelta.y);

            return result;
        }

        //parent 左上原点, cmp 左上基準 Y軸も正の数で渡す
        public static RectTransform NormalizeRectWithTopLeft(Component cmp, float left, float top, Transform parent = null)
        {
            RectTransform rect = cmp.transform as RectTransform;
            if (parent != null)
            {
                rect.SetParent(parent, false);
            }
            rect.anchorMax = new Vector2(0f, 1f);
            rect.anchorMin = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition3D = new Vector3(left, -top, 0f);
            return rect;
        }

        public static RectTransform NormalizeRectWithBottomLeft(Component cmp, float left, float bottom, Transform parent = null)
        {
            RectTransform rect = cmp.transform as RectTransform;
            if (parent != null)
            {
                rect.SetParent(parent, false);
            }
            rect.anchorMax = new Vector2(0f, 0f);
            rect.anchorMin = new Vector2(0f, 0f);
            rect.pivot = new Vector2(0f, 0f);
            rect.anchoredPosition3D = new Vector3(left, bottom, 0f);
            return rect;
        }

        public static RectTransform NormalizeRectWithMargin(Component cmp, float top, float left, float bottom, float right, Transform parent = null)
        {
            RectTransform rect = cmp.transform as RectTransform;
            if (parent != null)
            {
                rect.SetParent(parent, false);
            }
            rect.anchoredPosition3D = Vector3.zero;
            rect.localScale = Vector3.one;
            rect.anchorMax = Vector2.one;
            rect.anchorMin = Vector2.zero;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.offsetMax = new Vector2(-right, -top);
            rect.offsetMin = new Vector2(left, bottom);
            return rect;
        }

        //俺たちは雰囲気でUnity座標系をやっている
        public static RectTransform NormalizeRect(GameObject go, float width = 0, float height = 0)
        {
            RectTransform rect = (RectTransform)go.transform;
            rect.anchorMax = Vector2.zero;
            rect.anchorMin = Vector2.zero;
            rect.pivot = Vector2.zero;
            if (width > 0 && height > 0)
            {
                rect.sizeDelta = new Vector2(width, height);
            }
            return rect;
        }

        //offsetでサイズを決める感じ
        public static RectTransform NormalizeRectB(GameObject go)
        {
            RectTransform rect = (RectTransform)go.transform;
            rect.anchorMax = Vector2.one;
            rect.anchorMin = Vector2.zero;
            rect.pivot = new Vector2(0.5f, 0.5f);

            return rect;
        }

        //中央揃え
        public static RectTransform NormalizeRectC(GameObject go, float width = 0, float height = 0)
        {
            RectTransform rect = (RectTransform)go.transform;
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            if (width > 0 && height > 0)
            {
                rect.sizeDelta = new Vector2(width, height);
            }
            return rect;
        }

        //左上
        public static RectTransform NormalizeRectD(GameObject go, float width = 0, float height = 0)
        {
            RectTransform rect = (RectTransform)go.transform;
            rect.anchorMax = Vector2.up;
            rect.anchorMin = Vector2.up;
            rect.pivot = Vector2.up;
            if (width > 0 && height > 0)
            {
                rect.sizeDelta = new Vector2(width, height);
            }
            return rect;
        }

        public static Sprite LoadSpriteResource(string path)
        {
            Sprite s = Resources.Load<Sprite>(path);
            if (s != null)
            {
                return s;
            }
            else
            {
                Texture2D t = Resources.Load<Texture2D>(path);
                if (t != null)
                {
                    s = Sprite.Create(t, new Rect(0, 0, t.width, t.height), new Vector2(0.5f, 0.5f));
                    return s;
                }
            }
            return null;
        }
        


        public static UIButton MakeSmallTextButton(string label = "", float width = 0, float height = 0)
        {
            UIAssemblerWindow assemblerWindow = UIRoot.instance.uiGame.assemblerWindow;
            GameObject go = GameObject.Instantiate(assemblerWindow.copyButton.gameObject);
            UIButton btn = go.GetComponent<UIButton>();
            Transform child = go.transform.Find("Text");
            GameObject.DestroyImmediate(child.GetComponent<Localizer>());
            Text txt = child.GetComponent<Text>();
            txt.text = label;
            btn.tips.tipText = "";
            btn.tips.tipTitle = "";

            if (width > 0 || height > 0)
            {
                RectTransform rect = (RectTransform)go.transform;
                if (width == 0)
                {
                    width = rect.sizeDelta.x;
                }
                if (height == 0)
                {
                    height = rect.sizeDelta.y;
                }
                rect.sizeDelta = new Vector2(width, height);
            }

            go.transform.localScale = Vector3.one;

            return btn;
        }
        public static UIButton MakeIconButtonB(Sprite sprite, float size = 60)
        {
            GameObject go = GameObject.Instantiate(UIRoot.instance.uiGame.researchQueue.pauseButton.gameObject);
            UIButton btn = go.GetComponent<UIButton>();
            RectTransform rect = (RectTransform)go.transform;
            //rect.sizeDelta = new Vector2(size, size);
            float scale = size / 60;
            rect.localScale = new Vector3(scale, scale, scale);
            Image img = go.transform.Find("icon")?.GetComponent<Image>();
            if (img != null)
            {
                img.sprite = sprite;
            }
            btn.tips.tipText = "";
            btn.tips.tipTitle = "";
            return btn;
        }

        public static UIButton MakeIconButtonC(Sprite sprite, float size = 30)
        {
            GameObject src = UIRoot.instance.uiGame.starmap.northButton?.transform.parent.Find("tip")?.gameObject;
            GameObject go = GameObject.Instantiate(src);

            RemovePersistentCalls(go);
            UIButton btn = go.GetComponent<UIButton>();
            RectTransform rect = (RectTransform)go.transform;
            for (int i = rect.childCount - 1; i >= 0; --i)
            {
                GameObject.Destroy(rect.GetChild(i).gameObject);
            }
            rect.DetachChildren();

            if (size > 0)
            {
                rect.sizeDelta = new Vector2(size, size);
                //float scale = size / rect.sizeDelta.y; //y=30
                //rect.localScale = new Vector3(scale, scale, scale);
            }

            Image img = go.GetComponent<Image>();
            if (img != null)
            {
                img.sprite = sprite;
            }
            btn.tips.tipText = "";
            btn.tips.tipTitle = "";
            btn.tips.delay = 0.6f;

            if (btn.transitions != null && btn.transitions.Length > 0)
            {
                btn.transitions = new UIButton.Transition[] { btn.transitions[0] };
            }
            return btn;
        }



        public static UIButton MakeIconButton(Transform parent, Sprite sprite, float posX = 0, float posY = 0, bool right = false, bool bottom = false)
        {

            //GameObject go = GameObject.Find("UI Root/Overlay Canvas/In Game/Research Queue/pause");
            GameObject go = UIRoot.instance.uiGame.researchQueue.pauseButton.gameObject;
            if (go == null) return null;
            UIButton btn = MakeGameObject<UIButton>(parent, go, posX, posY, 0, 0, right, bottom);
            if (btn == null) return null;


            var bg = btn.gameObject.transform.Find("bg");
            if (bg != null) bg.gameObject.SetActive(false);
            var sd = btn.gameObject.transform.Find("sd");
            if (sd != null) sd.gameObject.SetActive(false);

            var icon = btn.gameObject.transform.Find("icon");
            if (sprite != null && icon != null)
            {
                Image img = icon.GetComponent<Image>();
                if (img != null)
                {
                    img.sprite = sprite;
                    img.color = new Color(0.94f, 0.74f, 0.24f, 0.6f);
                }
                icon.localScale = new Vector3(1.6f, 1.6f, 1.6f);
            }

            btn.gameObject.transform.localScale = new Vector3(0.28f, 0.28f, 0.28f);

            btn.tips.offset = new Vector2(0, -10);
            btn.tips.corner = 0;
            btn.tips.delay = 0.5f;
            btn.tips.tipText = "";
            btn.tips.tipTitle = "";

            return btn;
        }

        public static T MakeGameObject<T>(Transform parent, GameObject src, float posX = 0, float posY = 0, float width = 0, float height = 0, bool right = false, bool bottom = false)
        {
            if (src == null) return default;
            var go = UnityEngine.Object.Instantiate(src);
            if (go == null)
            {
                return default;
            }

            var rect = (RectTransform)go.transform;
            if (rect != null)
            {
                float yAnchor = bottom ? 0 : 1;
                float xAnchor = right ? 1 : 0;
                rect.anchorMax = new Vector2(xAnchor, yAnchor);
                rect.anchorMin = new Vector2(xAnchor, yAnchor);
                rect.pivot = new Vector2(0, 0);
                if (width == -1) width = rect.sizeDelta.x;
                if (height == -1) height = rect.sizeDelta.y;
                if (width > 0 && height > 0)
                {
                    rect.sizeDelta = new Vector2(width, height);
                }
                rect.SetParent(parent, false);
                rect.anchoredPosition = new Vector2(posX, posY);
            }
            return go.GetComponent<T>();
        }

        public static string KMGFormat(long num)
        {
            if (num >= 100_000_000_000_000)
                return (num / 1_000_000_000_000).ToString("#,0T");
            if (num >= 10_000_000_000_000)
                return (num / 1_000_000_000_000).ToString("0.#") + "T";
            if (num >= 100_000_000_000)
                return (num / 1_000_000_000).ToString("#,0G");
            if (num >= 10_000_000_000)
                return (num / 1_000_000_000).ToString("0.#") + "G";
            if (num >= 100_000_000)
                return (num / 1_000_000).ToString("#,0M");
            if (num >= 10_000_000)
                return (num / 1_000_000).ToString("0.#") + "M";
            if (num >= 100_000)
                return (num / 1_000).ToString("#,0K");
            if (num >= 10_000)
                return (num / 1_000).ToString("0.#") + "K";

            return num.ToString("#,0");
        }

    }
}
