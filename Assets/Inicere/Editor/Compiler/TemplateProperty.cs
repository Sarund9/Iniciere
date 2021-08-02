using System;
using System.Collections.Generic;
using UnityEngine;

namespace Iniciere
{
    [Serializable]
    public class TemplateProperty
    {
        public TemplateProperty(string name, TemplateInfo templateInfo)
        {
            Name = name;
            Template = templateInfo;
        }
        public string Name { get; }
        public object Value { get; set; }

        public TemplateInfo Template { get; }

        public List<InicierePropertyEditor> CustomEditors { get; } = new List<InicierePropertyEditor>();

        public bool IsFileName { get; set; }

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
