using System;
using System.Collections.Generic;
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
        InicierePropertyEditor editor;

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

        public InicierePropertyEditor Editor { get => editor; set => editor = value; }

        public bool IsFileName => Template.FileNameProperty == this;
        public bool HasEditor => Editor is object;

        public Type Type => Value?.GetType();

        public string Typename => Value is null ? "null" : Type.Name;

        public void MarkAsFileName()
        {
            Template.FileNameProperty = this;
        }

        public override string ToString()
        {
            var val = Value == null ? "NULL" : Value.ToString();
            return $"[{Typename}] '{Name}' = {val};";
        }
    }


}
