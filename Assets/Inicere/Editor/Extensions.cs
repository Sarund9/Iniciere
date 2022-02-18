using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Iniciere
{
    public static class Extensions
    {
        public static IEnumerable<Type> GetAllDerivedTypes(this AppDomain aAppDomain, Type aType)
        {
            var assemblies = aAppDomain.GetAssemblies();
            foreach (var assembly in assemblies)
            {
                var types = assembly.GetTypes();
                foreach (var type in types)
                    if (type.IsSubclassOf(aType))
                        yield return type;
            }
        }
        public static IEnumerable<TResult> SelectWhere<T, TResult>(
            this IEnumerable<T> col, TryFunc<T, TResult> selector
            )
        {
            foreach (var item in col)
            {
                if (selector(item, out var result))
                    yield return result;
            }
        }

        public static Rect GetEditorMainWindowPos2019()
        {
            var containerWinType = 
                AppDomain.CurrentDomain.GetAllDerivedTypes(typeof(ScriptableObject))
                .Where(t => t.Name == "ContainerWindow").FirstOrDefault();

            if (containerWinType == null)
                throw new MissingMemberException(
                    "Can't find internal type ContainerWindow. Maybe something has changed inside Unity"
                    );

            var showModeField = containerWinType
                .GetField("m_ShowMode", BindingFlags.NonPublic | BindingFlags.Instance);

            var positionProperty = containerWinType
                .GetProperty("position", BindingFlags.Public | BindingFlags.Instance);

            if (showModeField == null || positionProperty == null)
                throw new MissingFieldException(
                    "Can't find internal fields 'm_ShowMode' or 'position'. Maybe something has changed inside Unity");

            var windows = Resources.FindObjectsOfTypeAll(containerWinType);
            foreach (var win in windows)
            {
                var showmode = (int)showModeField.GetValue(win);
                if (showmode == 4) // main window
                {
                    var pos = (Rect)positionProperty.GetValue(win, null);
                    return pos;
                }
            }
            throw new NotSupportedException(
                "Can't find internal main window. Maybe something has changed inside Unity");
        }

        public static void CenterOnMainWin(this EditorWindow aWin)
        {
#if UNITY_2020_1_OR_NEWER
            var main = EditorGUIUtility.GetMainWindowPosition();
#else
            var main = GetEditorMainWindowPos2019();
#endif
            var pos = aWin.position;
            float w = (main.width - pos.width) * 0.5f;
            float h = (main.height - pos.height) * 0.5f;
            pos.x = main.x + w;
            pos.y = main.y + h;
            aWin.position = pos;
        }
        public static Rect ScaleSizeBy(this Rect rect, Vector2 scale, Vector2 pivotPoint)
        {
            Rect result = rect;
            result.x -= pivotPoint.x;
            result.y -= pivotPoint.y;
            result.xMin *= scale.x;
            result.xMax *= scale.x;
            result.yMin *= scale.y;
            result.yMax *= scale.y;
            result.x += pivotPoint.x;
            result.y += pivotPoint.y;
            return result;
        }
        public static Rect Shrink(this Rect rect,
            float left, float right, float top, float bottom)
        {
            Rect result = rect;
            result.x += right;
            result.width -= right;
            result.width -= left;
            result.y += top;
            result.height -= top;
            result.height -= bottom;
            return result;
        }
        public static Rect ShrinkMaxSize(this Rect rect,
            float maxLeft, float maxRight, float maxTop, float maxBottom)
        {
            Rect result = rect;
            Vector2 center = result.position + result.size / 2;

            float distH = result.width / 2;
            float distV = result.height / 2;

            return result
                .Shrink(
                    Mathf.Max(distH - maxLeft, 0),
                    Mathf.Max(distH - maxRight, 0),
                    Mathf.Max(distV - maxTop, 0),
                    Mathf.Max(distV - maxBottom, 0));
        }
        public static Rect Shrink(this Rect rect, float value)
        {
            Rect result = rect;
            result.x += value;
            result.width -= value * 2;
            result.y += value;
            result.height -= value * 2;
            return result;
        }
        public static Rect Shift(this Rect rect, float x, float y)
        {
            Rect result = rect;
            result.x += x;
            result.y += y;
            return result;
        }
        public static IEnumerable<Rect> SplitHorizontal(this Rect rect, int numSplits)
        {
            float newWidth = rect.width / numSplits;
            for (int i = 0; i < numSplits; i++)
            {
                yield return new Rect
                {
                    y = rect.y, height = rect.height,
                    x = rect.x + newWidth * i,
                    width = newWidth,
                };
            }
        }
        public static IEnumerable<Rect> SplitVertical(this Rect rect, int numSplits)
        {
            float newHeight = rect.height / numSplits;
            for (int i = 0; i < numSplits; i++)
            {
                yield return new Rect
                {
                    x = rect.x, width = rect.width,
                    y = rect.y + newHeight * i,
                    height = newHeight,
                };
            }
        }

        public static string ReplaceAt(this string input, int index, char newChar)
        {
            if (input == null)
            {
                throw new ArgumentNullException("input");
            }
            char[] chars = input.ToCharArray();
            chars[index] = newChar;
            return new string(chars);
        }

        public static void AddRange<T, T2>(this Dictionary<T, T2> hash, IEnumerable<(T, T2)> items)
        {
            foreach (var item in items)
                if (!hash.ContainsKey(item.Item1))
                    hash.Add(item.Item1, item.Item2);
        }
        public static void AddRange<T>(this HashSet<T> hash, IEnumerable<T> items)
        {
            foreach (var item in items)
                hash.Add(item);
        }

        //[MenuItem("Assets/Ping")]
        public static string GetPathToProjectWindowFolder()
        {
            Type projectWindowUtilType = typeof(UnityEditor.ProjectWindowUtil);
            MethodInfo getActiveFolderPath = projectWindowUtilType.GetMethod("GetActiveFolderPath", BindingFlags.Static | BindingFlags.NonPublic);
            object obj = getActiveFolderPath.Invoke(null, new object[0]);
            string pathToCurrentFolder = obj.ToString();
            //Debug.Log(pathToCurrentFolder);

            return pathToCurrentFolder;
        }

        public static string AppendToString(this IEnumerable<string> strcol)
        {
            StringBuilder build = new StringBuilder();
            foreach (var str in strcol)
            {
                build.Append(str);
            }
            return build.ToString();
        }
        public static string AppendToString(this IEnumerable<char> ccol)
        {
            return new string(ccol.ToArray());
        }
        //public static bool HasCommonItem<T1, T2>()

        public static T FirstOr<T>(this IEnumerable<T> it, T value)
        {
            var e = it.GetEnumerator();
            if (e.MoveNext())
            {
                return e.Current;
            }
            else
            {
                return value;
            }
        }

        public static IEnumerable<T> SkipLast<T>(this IEnumerable<T> source)
        {
            var it = source.GetEnumerator();
            bool hasRemainingItems;
            bool isFirst = true;
            T item = default;

            do
            {
                hasRemainingItems = it.MoveNext();
                if (hasRemainingItems)
                {
                    if (!isFirst) yield return item;
                    item = it.Current;
                    isFirst = false;
                }
            } while (hasRemainingItems);
        }
        public static string GetExceptionText(this Exception exception)
        {
            if (exception.InnerException is object)
            {
                return exception.ToString() + " |Inner: \n\n"
                    + exception.InnerException.GetExceptionText();
            }
            return exception.ToString();
        }

    }

    public delegate void Procedure<T>(ref T obj);
}
