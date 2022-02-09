using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace Iniciere
{
    public class ClassTypeSearchWindow : EditorWindow
    {
        static readonly Color lineColor_dark = new Color(0.40f, 0.40f, 0.40f);
        static readonly Color lineColor_light = new Color(0.40f, 0.40f, 0.40f);

        Task<ClassTypeList> constructing;
        ClassTypeList typeList;
        bool show;

        static IEnumerable<Type> typeCache;

        string search;
        Vector2 scroll;

        public static ClassTypeSearchWindow Create(
            EditorWindow parent,
            string title, Rect buttonRect,
            float width, float maxHeight = 340)
        {
            var win = CreateInstance<ClassTypeSearchWindow>();
            win.titleContent = new GUIContent(title);

            var initHeight = EditorGUIUtility.singleLineHeight * 16;
            initHeight = Mathf.Min(initHeight, maxHeight);
            win.ShowAsDropDown(buttonRect, new Vector2(width, initHeight));

            var p = win.position;
            p.position =
                parent.position.position + buttonRect.position +
                (Vector2.up * buttonRect.height);
            win.position = p;

            win.constructing = ClassTypeList.GetNewAsync(GetTypes());

            return win;
        }

        private void OnGUI()
        {
            search = GUILayout.TextField(search, "SearchTextField");

            show = GUILayout.Toggle(show, "Show");

            DrawBorderLines();

            if (typeList is null)
            {
                GUILayout.Label("Loading...");
            }
            else if (show)
            {
                foreach (ClassTypeList.Node node in typeList.Roots)
                {
                    DrawNodeRecursive(node);
                }
            }

            scroll = GUILayout.BeginScrollView(scroll);
            GUILayout.EndScrollView();
        }

        void DrawNodeRecursive(ClassTypeList.Node node)
        {
            if (node is null || node.type is null || node.type.Name is null)
                return;

            using (new EditorGUI.IndentLevelScope())
            {
                
                GUILayout.Label(node.type.Name);

                for (int i = 0; i < node.Children.Count; i++)
                {
                    //TypeList.Node childNode = node.Children[i];
                    DrawNodeRecursive(node.Children[i]);
                }
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
        private void OnInspectorUpdate()
        {
            if (constructing is object)
            {
                if (constructing.IsFaulted)
                {
                    Debug.LogError("Error creating ClassTypeSearchWindow"
                        + constructing.Exception.InnerException.Message);
                    constructing = null;
                }
                else if (constructing.IsCompleted)
                {
                    typeList = constructing.Result;

                    constructing = null;
                }
            }
        }

        static IEnumerable<Type> GetTypes()
        {
            if (typeCache is null)
            {
                typeCache = AppDomain
                    .CurrentDomain
                    .GetAssemblies()
                    .SelectMany(a => a.GetTypes())
                    .Where(t => t.IsClass && t != typeof(object));
            }
            return typeCache;
        }
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