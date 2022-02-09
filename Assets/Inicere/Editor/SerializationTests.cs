using System;
using UnityEngine;

namespace Iniciere
{
    [CreateAssetMenu(fileName = "SerializationTest", menuName = "SerializationTest")]
    public class SerializationTests : ScriptableObject
    {
        [SerializeField]
        TemplateProperty property;
        [SerializeField]
        UFuncBox box = new UFuncBox();

        [SerializeField]
        bool assign, print;

        private void OnValidate()
        {
            if (assign)
            {
                assign = false;
                //property.Editor = ToggleEditor.New("My Toggle");
                box.Set((Func<string>)Func);
            }
            if (print)
            {
                print = false;
                Debug.Log("The value is: " + box.Get<Func<string>>().Invoke());
            }
        }


        static string Func() { return "Hello Unity Serialization"; }
    }
}
