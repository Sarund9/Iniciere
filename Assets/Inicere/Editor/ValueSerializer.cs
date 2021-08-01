using System;
using UnityEditor;
using UnityEngine;

namespace Iniciere
{
    public class ValueSerializer : ScriptableObject
    {
        SerializedObject obj;
        object value;

        public static ValueSerializer Get<T>(object value) =>
            Get(value, typeof(T));
        public static ValueSerializer Get(object value, Type type = null)
        {
            if (type != null && value.GetType() != type)
            {
                throw new Exception("Value Serializer");
            }
            var vs = CreateInstance<ValueSerializer>();
            vs.Value = value;
            vs.Type = type;
            vs.obj = new SerializedObject(vs);
            return vs;
        }

        public object Value
        {
            get => value;
            set => this.value = value;
        }
        public Type Type { get; private set; }

        public SerializedProperty GetProperty()
        {
            return obj.FindProperty("value");
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