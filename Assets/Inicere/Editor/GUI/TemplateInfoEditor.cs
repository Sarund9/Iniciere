using UnityEditor;

namespace Iniciere
{
    [CustomEditor(typeof(TemplateInfo))]
    public class TemplateInfoEditor : Editor
    {
        TemplateInfo obj;

        private void OnEnable()
        {
            obj = (TemplateInfo)target;
        }

        public override void OnInspectorGUI()
        {
            TemplateGUI.Draw(obj);
        }
    }
}