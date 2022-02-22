using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Iniciere
{
    public class ContextDropdown : EditorWindow
    {
        Action<int> callback;
        IEnumerable<string> options;
        GUIStyle style;

        private void OnGUI()
        {
            int i = 0;
            foreach (var item in options)
            {
                if (GUILayout.Button(item, style))
                {
                    callback(i);
                    Close();
                    return;
                }
                i++;
            }
        }
    }
}
