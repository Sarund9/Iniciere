using System;

namespace Iniciere
{
    [Serializable]
    public class TemplateProperty
    {
        public TemplateProperty(string name)
        {
            Name = name;
        }
        public string Name { get; }
        public object Value { get; set; }

        public Type Type => Value?.GetType();

        public string Typename => Value is null ? "null" : Type.Name;

        public override string ToString()
        {
            var val = Value == null ? "NULL" : Value.ToString();
            return $"[{Typename}] '{Name}' = {val};";
        }
    }


}
