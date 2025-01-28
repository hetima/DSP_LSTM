using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace LSTMMod
{
    public interface MyWindow
    {
        //ShutAllFunctionWindow でなにかチェックしたい場合
        void TryClose();
        bool isFunctionWindow();
    }

    public static class MyWindowCtl
    {
        //MyWindowインターフェイスを使うならtrueにする
        public static bool useMyWindowInterface = false;


        public static List<ManualBehaviour> _windows = new List<ManualBehaviour>(4);
        internal static bool _created = false;
        internal static bool _inited = false;

        public static T CreateWindow<T>(string name, string title = "") where T : Component
        {
            var srcWin = UIRoot.instance.uiGame.inserterWindow;
            GameObject src = srcWin.gameObject;
            GameObject go = GameObject.Instantiate(src, srcWin.transform.parent);
            go.name = name;
            go.SetActive(false);
            GameObject.Destroy(go.GetComponent<UIInserterWindow>());
            ManualBehaviour win = go.AddComponent<T>() as ManualBehaviour;
            //shadow 
            for (int i = 0; i < go.transform.childCount; i++)
            {
                GameObject child = go.transform.GetChild(i).gameObject;
                if (child.name == "panel-bg")
                {
                    Button btn = child.GetComponentInChildren<Button>();
                    //close-btn
                    if (btn != null)
                    {
                        btn.onClick.AddListener(win._Close);
                    }
                    else
                    {

                    }
                }
                else if (child.name != "shadow" && child.name != "panel-bg")
                {
                    GameObject.Destroy(child);
                }
            }

            SetTitle(win, title);


            win._Create();
            if (_inited)
            {
                win._Init(win.data);
            }
            _windows.Add(win);
            return win as T;
        }

        public static void SetTitle(ManualBehaviour win, string title)
        {
            Text txt = GetTitleText(win);
            if (txt)
            {
                txt.text = title;
            }
        }
        public static Text GetTitleText(ManualBehaviour win)
        {
            return win.gameObject.transform.Find("panel-bg/title-text")?.gameObject.GetComponent<Text>();
        }


        public static RectTransform GetRectTransform(ManualBehaviour win)
        {
            return win.GetComponent<RectTransform>();
        }


        public static void SetRect(ManualBehaviour win, RectTransform rect)
        {
            RectTransform rectTransform = win.GetComponent<RectTransform>();
            //rectTransform.position =
            //rectTransform.sizeDelta = rect;
        }


        public static void OpenWindow(ManualBehaviour win)
        {
            win._Open();
            win.transform.SetAsLastSibling();
        }

        public static void CloseWindow(ManualBehaviour win)
        {
            win._Close();
        }

        public static class Patch
        {

            //_Create -> _Init
            [HarmonyPostfix, HarmonyPatch(typeof(UIGame), "_OnCreate")]
            public static void UIGame__OnCreate_Postfix()
            {
                _created = true;
            }

            [HarmonyPostfix, HarmonyPatch(typeof(UIGame), "_OnDestroy")]
            public static void UIGame__OnDestroy_Postfix()
            {
                foreach (var win in _windows)
                {
                    win._Destroy();
                }
                _windows.Clear();
            }

            [HarmonyPostfix, HarmonyPatch(typeof(UIGame), "_OnInit")]
            public static void UIGame__OnInit_Postfix(UIGame __instance)
            {
                foreach (var win in _windows)
                {
                    win._Init(win.data);
                }
                _inited = true;
            }

            [HarmonyPostfix, HarmonyPatch(typeof(UIGame), "_OnFree")]
            public static void UIGame__OnFree_Postfix()
            {
                foreach (var win in _windows)
                {
                    win._Free();
                }
            }

            [HarmonyPostfix, HarmonyPatch(typeof(UIGame), "_OnUpdate")]
            public static void UIGame__OnUpdate_Postfix()
            {
                if (GameMain.isPaused || !GameMain.isRunning)
                {
                    return;
                }
                foreach (var win in _windows)
                {
                    win._Update();
                }
            }

            //[HarmonyPostfix, HarmonyPatch(typeof(UIGame), "get__isAnyFunctionWindowActive")]
            //public static void UIGame__isAnyFunctionWindowActive_Postfix(ref bool __result)
            //{
            //    foreach (var win in _windows)
            //    {
            //        if (win.active)
            //        {
            //            if (useMyWindowInterface && !(win as MyWindow).isFunctionWindow())
            //            {
            //                continue;
            //            }
            //            __result = true;
            //        }
            //    }
            //}

            [HarmonyPostfix, HarmonyPatch(typeof(UIGame), "ShutAllFunctionWindow")]
            public static void UIGame_ShutAllFunctionWindow_Postfix()
            {
                foreach (var win in _windows)
                {
                    if(useMyWindowInterface)
                    {
                        if ((win as MyWindow).isFunctionWindow())
                        {
                            (win as MyWindow).TryClose();
                        }
                    }
                    else
                    {
                        win._Close();
                    }
                }
            }
        }
    }
}
