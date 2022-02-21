using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Iniciere
{
    [Serializable]
    public class TemplateProperty
    {
        [SerializeField]
        string name;
        [SerializeField]
        TemplateInfo template;
        [SerializeField]
        UBox valueBox = new UBox();

        [SerializeField]
        UBox editor = new UBox();

        [SerializeField]
        bool isFileName;

        public TemplateProperty(string name, TemplateInfo templateInfo)
        {
            this.name = name;
            template = templateInfo;
        }
        public string Name => name;
        public object Value
        {
            get => valueBox.Get();
            set => valueBox.Set(value);
        }
        

        public TemplateInfo Template => template;

        public InicierePropertyEditor Editor
        {
            get => editor.Get() as InicierePropertyEditor;
            set => editor.Set(value);
        }

        public bool IsFileName => isFileName;
        public bool HasEditor => Editor is object;

        public Type Type => Value?.GetType();

        public string Typename => Value is null ? "null" : Type.Name;

        public void MarkAsFileName()
        {
            isFileName = true;
        }

        public override string ToString()
        {
            var val = Value == null ? "NULL" : Value.ToString();
            return $"{Typename} {Name} = {val};";
        }

    }


}
