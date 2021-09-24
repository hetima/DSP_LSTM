﻿using System;
using System.Collections.Generic;
using System.Text;

using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;


namespace LSTMMod
{
    public static class Util
    {
        public static Color DSPBlue => new Color(0.3821f, 0.8455f, 1f, 0.7059f);
        public static Color DSPOrange => new Color(0.9906f, 0.5897f, 0.3691f, 0.7059f);

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
            if (sd != null) bg.gameObject.SetActive(false);

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
        
    }
}