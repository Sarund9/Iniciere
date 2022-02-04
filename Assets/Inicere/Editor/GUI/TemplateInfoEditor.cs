using UnityEditor;

namespace Iniciere
{
    [CustomEditor(typeof(TemplateInfo))]
    public class TemplateInfoEditor : Editor
    {
        TemplateInfo obj;
        TemplateGUI ui = new TemplateGUI();

        private void OnEnable()
        {
            obj = (TemplateInfo)target;
        }

        public override void OnInspectorGUI()
        {
            ui.Draw(obj);
        }
    }
}