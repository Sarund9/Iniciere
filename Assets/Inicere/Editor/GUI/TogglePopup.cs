using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Iniciere
{
    public class TogglePopup : EditorWindow
    {
        static readonly Color lineColor_dark = new Color(0.40f, 0.40f, 0.40f);
        static readonly Color lineColor_light = new Color(0.40f, 0.40f, 0.40f);

        Dictionary<string, bool> items;
        Vector2 scroll;

        string search;

        bool allValues;

        public event Action<Dictionary<string, bool>> OnClose;
        public event Action<int, bool> OnItemChanged;

        public static TogglePopup Create(EditorWindow parent, Dictionary<string, bool> items, string title, Rect buttonRect, float width, float maxHeight = 340)
        {
            var win = CreateInstance<TogglePopup>();
            win.titleContent = new GUIContent(title);

            // TODO: remove count, after Automatic Size Changes
            var initHeight = EditorGUIUtility.singleLineHeight * (items.Count() + 2) + 17;

            initHeight = Mathf.Min(initHeight, maxHeight);

            win.ShowAsDropDown(buttonRect, new Vector2(width, initHeight));
            
            var p = win.position;
            p.position = parent.position.position + buttonRect.position + (Vector2.up * buttonRect.height);
            win.position = p;

            win.items = items;

            return win;
        }

        private void OnDisable()
        {
            //Debug.Log("DISABLE");
            OnClose?.Invoke(items);
        }

        private void OnGUI()
        {
            search = GUILayout.TextField(search, "SearchTextField");

            DrawBorderLines();

            scroll = GUILayout.BeginScrollView(scroll);

            int i = 0;
            Queue<(string, bool)> changes = new Queue<(string, bool)>();
            foreach (var item in items)
            {
                // TODO: Filter Search
                using (new GUILayout.HorizontalScope())
                {
                    EditorGUI.BeginChangeCheck();
                    var val = GUILayout.Toggle(item.Value, "", GUILayout.Width(10));
                    GUILayout.Label(item.Key);
                    if (EditorGUI.EndChangeCheck())
                    {
                        // TODO: Undo Functionality (better record after window is closed)
                        //Undo.RecordObject()
                        //items[item.Key] = val;
                        changes.Enqueue((item.Key, val));
                        OnItemChanged?.Invoke(i, val);
                    }
                }
                i++;
            }

            while (changes.Count > 0)
            {
                var item = changes.Dequeue();
                items[item.Item1] = item.Item2;
            }

            GUILayout.EndScrollView();

            using (new GUILayout.HorizontalScope())
            {
                if (GUILayout.Button("All"))
                {
                    SetAll(true);
                }
                if (GUILayout.Button("None"))
                {
                    SetAll(false);
                }
            }
        }

        void SetAll(bool val)
        {
            int i = 0;
            string[] keys = new string[items.Count];
            foreach (var key in items.Keys)
            {
                keys[i] = key;
                i++;
            }
            for (int k = 0; k < keys.Length; k++)
            {
                items[keys[k]] = val;
                OnItemChanged?.Invoke(k, val);
            }
        }

        private static void DrawBorderLines()
        {
            Color color;
            if (EditorGUIUtility.isProSkin)
                color = lineColor_dark;
            else
                color = lineColor_light;

            EditorGUI.DrawRect(new Rect
            {
                x = 0,
                y = 0,
                width = 1,
                height = Screen.height,
            }, color);
            EditorGUI.DrawRect(new Rect
            {
                x = Screen.width - 1,
                y = 0,
                width = 1,
                height = Screen.height,
            }, color);
            EditorGUI.DrawRect(new Rect
            {
                x = 0,
                y = 0,
                width = Screen.width,
                height = 1,
            }, color);
            EditorGUI.DrawRect(new Rect
            {
                x = 0,
                y = Screen.height - 1,
                width = Screen.width,
                height = 1,
            }, color);
        }
    }

    [Serializable]
    public class ToggleItem
    {
        public string name;
        public bool value;

        public ToggleItem()
        {}
        public ToggleItem(string name, bool value)
        {
            this.name = name;
            this.value = value;
        }

        public static implicit operator (string, bool)(ToggleItem i) =>
            (i.name, i.value);

        public static implicit operator ToggleItem((string, bool) t) =>
            new ToggleItem(t.Item1, t.Item2);
    }
}


#region OLD_CODE
/*
const int SIZE = 20;
const int MARGIN = 10;

Rect rect = new Rect
{
    x = Screen.width - SIZE,
    y = SIZE,
    width = SIZE,
    height = SIZE,
};

if (GUI.Button(rect, "X"))
{
    Close();
}

*/
#endregion