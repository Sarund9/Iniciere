using System;
using UnityEditor;
using UnityEngine;

namespace Iniciere
{
    [Serializable]
    public class ClassTypeEditor : InicierePropertyEditor
    {
        string msg;
        Type requiredImpl; // TODO: UBox This

        
        public ClassTypeEditor(string msg, Type requiredImpl)
        {
            this.msg = msg;
            this.requiredImpl = requiredImpl;
        }

        public override void DrawGUI(Rect area, TemplateProperty property)
        {

            if (GUI.Button(area, Msg(), "DropDownButton"))
            {
                var win = ClassTypeSearchWindow.Create(EditorWindow.GetWindow<CreateScriptWindow>(), "Select Type", area, 240);

            }

            string Msg() => msg ?? property.Name;
        }
    }
}
