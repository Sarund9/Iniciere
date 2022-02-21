using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace Iniciere
{
    public class CreateScriptWindow : EditorWindow
    {
        List<TemplateInfo> templates;
        int selectedTemplate = -1;
        TemplateInfo SelectedTemplate => selectedTemplate < 0 ? null : templates[selectedTemplate];

        string search = "";

        readonly Dictionary<string, bool> filters_lang = new Dictionary<string, bool>();
        readonly Dictionary<string, bool> filters_fileEx = new Dictionary<string, bool>();
        readonly Dictionary<string, bool> filters_cat = new Dictionary<string, bool>();
        readonly Dictionary<string, bool> filters_flags = new Dictionary<string, bool>();

        BuildFileGUI buildFileUI = new BuildFileGUI();

        [MenuItem("Assets/Create/Script", priority = 80)] //80 is C# Script Priority
        public static void OpenWindow()
        {
            var win = CreateInstance<CreateScriptWindow>();

            win.minSize = new Vector2(460, 580);
            win.maxSize = new Vector2(2000, 1600);

            //win.position = new Rect(200, 200);
#if UNITY_2020_1_OR_NEWER
            Rect pos = EditorGUIUtility.GetMainWindowPosition();
#else
            Rect pos = Extensions.GetEditorMainWindowPos2019();
#endif

            Vector2 scale = new Vector2(0.4f, 0.7f);
            Vector2 pivot = new Vector2(pos.width / 2, pos.height / 2);
            win.position = pos.ScaleSizeBy(scale, pivot);

            win.titleContent = new GUIContent("Create new Script file", "Create a new Script File");
            
            //win.ShowModalUtility();
            win.ShowUtility();
        }

        public static IEnumerable<T> FindAssetsByType<T>() where T : UnityEngine.Object
        {
            string[] guids = AssetDatabase.FindAssets($"t:{typeof(T)}");
            for (int i = 0; i < guids.Length; i++)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guids[i]);
                var assets = AssetDatabase.LoadAllAssetsAtPath(assetPath)
                    .OfType<T>();

                foreach (var item in assets)
                    yield return item;
            }
        }

        private void OnEnable()
        {
            templates = FindAssetsByType<TemplateInfo>()
                .Where(x => !x.IsFailed)
                .ToList();
            
            selectedTemplate = -1;
            filters_flags.Clear();
            filters_cat.Clear();
            filters_lang.Clear();
            filters_fileEx.Clear();

            foreach (var info in templates)
            {
                foreach (var flag in info.Flags)
                    filters_flags[flag] = true;

                foreach (var flag in info.Categories)
                    filters_cat[flag] = true;

                foreach (var flag in info.Langs)
                    filters_lang[flag] = true;

                foreach (var flag in info.FileExts)
                    filters_fileEx[flag] = true;
            }
        }

        private void OnGUI()
        {
            Event e = Event.current;

            UnityEngine.Random.InitState(99);

            float halfScreen = position.width / 2;

            Rect halfTheScreenL = new Rect(0, 0, halfScreen, position.height);

            GUILayout.BeginArea(halfTheScreenL);

            search = GUILayout.TextField(search, "SearchTextField", GUILayout.Width(halfScreen - 5));

            #region TMP_FILTERS
            GUILayout.Space(2f);
            //Draw(GUILayoutUtility.GetLastRect());

            GUILayout.BeginHorizontal();
            //using (new GUILayout.HorizontalScope())
            {
                float subdivideHalf = halfScreen / 2;
                Rect button1 = GUILayoutUtility.GetRect(20, 20, GUILayout.MaxWidth(subdivideHalf));
                button1 = button1.ScaleSizeBy(new Vector2(.9f, 1), button1.size / 2);
                if (GUI.Button(button1, "Language", "DropDownButton"))
                {
                    var t = TogglePopup.Create(this, filters_lang, "Test", button1, button1.width);
                    t.OnItemChanged += OnItemChanged;
                }

                Rect button2 = GUILayoutUtility.GetRect(20, 20, GUILayout.MaxWidth(subdivideHalf));
                button2 = button2.ScaleSizeBy(new Vector2(.9f, 1), button2.size / 2);
                if (GUI.Button(button2, "File Exts", "DropDownButton"))
                {
                    var t = TogglePopup.Create(this, filters_fileEx, "Test", button2, button2.width);
                    t.OnItemChanged += OnItemChanged;
                }

                //Draw(button1);
                //Draw(button2);
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(4f);
            //Draw(GUILayoutUtility.GetLastRect());

            GUILayout.BeginHorizontal();
            //using (new GUILayout.HorizontalScope())
            {
                float subdivideHalf = halfScreen / 2;
                Rect button1 = GUILayoutUtility.GetRect(20, 20, GUILayout.MaxWidth(subdivideHalf));
                button1 = button1.ScaleSizeBy(new Vector2(.9f, 1), button1.size / 2);
                if (GUI.Button(button1, "Category", "DropDownButton"))
                {
                    var t = TogglePopup.Create(this, filters_cat, "Test", button1, button1.width);
                    t.OnItemChanged += OnItemChanged;
                }

                Rect button2 = GUILayoutUtility.GetRect(20, 20, GUILayout.MaxWidth(subdivideHalf));
                button2 = button2.ScaleSizeBy(new Vector2(.9f, 1), button2.size / 2);
                if (GUI.Button(button2, "Flags", "DropDownButton"))
                {
                    var t = TogglePopup.Create(this, filters_flags, "Test", button2, button2.width);
                    t.OnItemChanged += OnItemChanged;
                }

                //Draw(button1);
                //Draw(button2);
            }
            GUILayout.EndHorizontal();

            void OnItemChanged(int i, bool value)
            {
                if (selectedTemplate == i && SkipTempalte(i))
                {
                    //Debug.Log($"Selection Gone I[{i}] S[{selectedTemplate}]");
                    selectedTemplate = -1;
                }
                Repaint();
            }

            #endregion

            #region TMP_DISPLAY
            GUILayout.Space(2f);

            //var r_tmpList = GUILayoutUtility.GetRect(1, 1, GUILayout.MaxWidth(halfScreen));
            //r_tmpList.y += 4;
            //r_tmpList.height = Screen.height - (r_tmpList.y + 1);
            //r_tmpList = r_tmpList.Shrink(10);
            Rect r_tmpList = GUILayoutUtility.GetRect(1, 1,
                GUILayout.MaxWidth(halfScreen),
                GUILayout.ExpandHeight(true))
                .Shrink(10);

            EditorGUI.DrawRect(r_tmpList, new Color(0.2f, 0.2f, 0.2f));

            const float TMP_ITEM_HEIGHT = 80;
            const float TMP_ITEM_MARGIN = 2.2f;
            Color tmpColor1 = new Color(0.3f, 0.3f, 0.3f); // TODO: EditorBackround thing
            Color tmpColor2 = new Color(0.4f, 0.4f, 0.4f);
            bool colToUse = false;
            bool selected = false;
            for (int i = 0, t = 0; i < templates.Count; i++)
            {
                if (SkipTempalte(i))
                    continue;
                

                Rect rect = r_tmpList;
                rect.x += TMP_ITEM_MARGIN;
                rect.width -= TMP_ITEM_MARGIN * 2;
                if (t == 0)
                    rect.y += TMP_ITEM_MARGIN;
                rect.height = TMP_ITEM_HEIGHT;
                rect.y += TMP_ITEM_HEIGHT * t;

                //if (e.type == EventType.MouseDown && e.button == 0 && r_tmpList.Contains(e.mousePosition))
                //{
                //    selectedTemplate = -1;
                //    Debug.Log("UNSELECTED");
                //}

                EditorGUI.DrawRect(rect, GetColor());
                TemplateDisplay(i, rect);

                t++;
                // ============= \\
                Color GetColor()
                {
                    if (selectedTemplate == i)
                    {
                        colToUse = !colToUse;
                        return new Color(0.1f, 0.1f, 0.4f);
                    }
                    else return (colToUse = !colToUse) ? tmpColor1 : tmpColor2;
                    //return selectedTemplate == i ?
                    //    new Color(0.1f, 0.1f, 0.4f) :
                    //    (colToUse = !colToUse) ? tmpColor1 : tmpColor2;
                }
            }
            if (!selected && e.type == EventType.MouseDown && e.button == 0 && r_tmpList.Contains(e.mousePosition))
            {
                selectedTemplate = -1;
                Repaint();
            }

            //Draw(r_tmpList);


            void TemplateDisplay(int index, Rect rect)
            {
                var template = templates[index];
                Rect label = rect.Shrink(0, 0, 0, TMP_ITEM_HEIGHT - EditorGUIUtility.singleLineHeight);
                GUI.Label(label, new GUIContent(template.TmpName));

                if (e.type == EventType.MouseDown && e.button == 0 && rect.Contains(e.mousePosition))
                {
                    selectedTemplate = index;
                    selected = true;
                    //Debug.Log("SELECTED");
                    Repaint();
                }
            }


            #endregion

            GUILayout.EndArea();


            Rect halfTheScreenR = new Rect(position.width / 2, 0, position.width / 2, position.height);
            GUILayout.BeginArea(halfTheScreenR);

            buildFileUI.Draw(SelectedTemplate, Close, true);
            GUILayout.Space(5f); // Margin

            GUILayout.EndArea();

            #region OLD
            /*
            Rect rect_SearchFilters, rect_TmpList, rect_Info, rect_Inspector;
            CalculateAreas();

            const float DIM = 0.6f;
            EditorGUI.DrawRect(rect_SearchFilters, Color.red * DIM);
            EditorGUI.DrawRect(rect_TmpList, Color.blue * DIM);
            EditorGUI.DrawRect(rect_Info, Color.green * DIM);
            EditorGUI.DrawRect(rect_Inspector, Color.cyan * DIM);

            void CalculateAreas()
                {
                    using (new GUILayout.HorizontalScope())
                    {
                        using (new GUILayout.VerticalScope())
                        {
                            rect_SearchFilters = GUILayoutUtility.GetRect(
                                Screen.width / 2,
                                SEARCH_HEIGHT,
                                GUILayout.MaxWidth(Screen.width / 2 - 10), GUILayout.MaxHeight(SEARCH_HEIGHT))
                                .Shrink(10);
                            using (new GUILayout.AreaScope(rect_SearchFilters))
                                DrawFilters();

                            rect_TmpList = GUILayoutUtility.GetRect(
                                Screen.width / 2,
                                Screen.height,
                                GUILayout.MaxWidth(Screen.width / 2 - 10), GUILayout.MinHeight(600))
                                .Shrink(10, 10, 5, 10);
                            using (new GUILayout.AreaScope(rect_TmpList))
                                DrawTemplateList();

                        }

                        using (new GUILayout.VerticalScope())
                        {
                            rect_Info = GUILayoutUtility.GetRect(
                                Screen.width / 2,
                                SEARCH_HEIGHT,
                                GUILayout.MaxWidth(Screen.width / 2 - 10), GUILayout.MaxHeight(SEARCH_HEIGHT))
                                .Shrink(10);
                            using (new GUILayout.AreaScope(rect_Info))
                                DrawTemplateInfo();

                            rect_Inspector = GUILayoutUtility.GetRect(
                                Screen.width / 2,
                                Screen.height,
                                GUILayout.MaxWidth(Screen.width / 2 - 10), GUILayout.MinHeight(600))
                                .Shrink(10, 10, 5, 10);
                            using (new GUILayout.AreaScope(rect_Inspector))
                                DrawTemplateInspector();

                        }
                    }
                }
            //*/
            #endregion
        }

        private bool SkipTempalte(int index)
        {
            // Search Bar

            if (search.Length > 0 && !templates[index].TmpName.ToLower().Contains(search.ToLower()))
            {
                return true;
            }

            //if (!ContainsAny(templates[index].FileExts, x => filters_fileEx[x])) return true;

            if (Invalidate(templates[index].FileExts, filters_fileEx))
                return true;

            if (Invalidate(templates[index].Langs, filters_lang))
                return true;

            if (Invalidate(templates[index].Categories, filters_cat))
                return true;

            if (Invalidate(templates[index].Flags, filters_flags))
                return true;



            return false;
            // ============ \\
            static bool Invalidate(List<string> tags, Dictionary<string, bool> filters)
            {
                if (tags.Count == 0) return false;
                bool exclude = true;
                foreach (var tag in tags)
                {
                    if (!filters.ContainsKey(tag))
                    {
                        Debug.Log($"TAG '{tag}' is not in flags");
                        continue;
                    }
                    if (filters[tag])
                    {
                        exclude = false;
                        break;
                    }
                }
                return exclude;
            }
        }



    }
}


#region OLD_CODE
/*

GUILayout.FlexibleSpace();
//var r1 =
GUILayoutUtility.GetRect(position.height, 20);
//Draw(r1);
using (new GUILayout.HorizontalScope())
{
    float screenQuarter = position.width / 4;
    Rect butonsRect = GUILayoutUtility
        .GetRect(20, 20);

    //EditorGUI.DrawRect(butonsRect, Color.red);

    Rect button = butonsRect
        .Shrink(0, screenQuarter + 10, 0, 0);
    Rect button2 = button
        .Shift(-button.width - 4, 0);
    //button.x += 
    //Draw(r2);
    //GUILayout.FlexibleSpace();

    EditorGUI.BeginDisabledGroup(SelectedTemplate is null);

    if (GUI.Button(button, "Create"))
    {
        //CREATE SCRIPT (PROGRESS BAR?)
        var path = Extensions.GetPathToProjectWindowFolder();
        ScriptBuilder.CreateScript(SelectedTemplate, path);

    }
    if (GUI.Button(button2, "Create & Close"))
    {
        //CREATE SCRIPT (PROGRESS BAR?)
        var path = Extensions.GetPathToProjectWindowFolder();
        ScriptBuilder.CreateScript(SelectedTemplate, path);
        Close();
    }
    EditorGUI.EndDisabledGroup();
}

//List<TemplateLocation> templates = new List<TemplateLocation>();

private void OnInspectorUpdate()
{
    for (int i = 0; i < precompiling.Count; i++)
    {
        if (precompiling[i].IsFaulted)
        {
            Debug.LogError($"Iniciere Compilation Faulted: {precompiling[i].Exception}");
            precompiling.RemoveAt(i);
            i--;
            //throw precompiling[i].Exception;
        }
        else if (precompiling[i].IsCompleted)
        {
            //Debug.Log("COMPLETED");
            var info = precompiling[i].Result;
            if (info != null)
            {
                //Debug.Log($"Template precompiled: {info.Name}");
                templates.Add(info);

                filters_lang.AddRange(info.Langs.Select(x => (x, true)));

                filters_fileEx.AddRange(info.FileExts.Select(x => (x, true)));

                filters_cat.AddRange(info.Categories.Select(x => (x, true)));

                filters_flags.AddRange(info.Flags.Select(x => (x, true)));
            }
            precompiling.RemoveAt(i);
            i--;
        }
                
    }
}

readonly List<Task<TemplateInfo>> precompiling = new List<Task<TemplateInfo>>();

void ProcessAsync(Action action, Action callback = null)
{
    thread = new Thread(Exec);
    thread.Start(action);
    void Exec(object del)
    {
        (del as Action).Invoke();
        callback?.Invoke();
        Thread.Sleep(1);
    }
}

private void TemplateColChanged(object sender, NotifyCollectionChangedEventArgs e)
{
    if (e.Action == NotifyCollectionChangedAction.Add)
    {
        //e.NewItems[0]
    }
}

private void CreateGUI()
{
    //rootVisualElement = new 
}

private void CreateGUI()
{
            
}

private async Task FindFiles(Action callback = null)
{
    IEnumerable<string> filepaths = await Task.Run(() => InicereScriptFinder.FindFilePaths());

    IEnumerable<TemplateLocation> templates = InicereScriptFinder.FindTemplatesLite(filepaths);

    foreach (var item in templates)
    {
        TemplateInfo info = TemplateInfo.New(item);
        precompiling.Add(PrecompileTemplate(item, info));
    }
    //return templates;
}
private async Task<TemplateInfo> PrecompileTemplate(TemplateLocation template, TemplateInfo info)
{
            
    int result = await Task.Run(() => Compiler.Precompile(template, info));

    if (result != 0)
    {
        //Task.
        return null;
    }

    return info;
}

private void OnDestroy() //TODO: Cancel Compile
{
            
}

void CancelCompilations()
{
    foreach (var item in precompiling)
    {
        //TODO: Cancel Compile
    }
}

static Color rdbgcolor;
void Draw(Rect rect)
{
    rdbgcolor = UnityEngine.Random.ColorHSV(0, 1, 0.7f, 0.9f, 0.1f, 0.7f, .5f, .5f);
    EditorGUI.DrawRect(rect, rdbgcolor);
}

EditorRightSide();

void EditorRightSide()
{
    if (SelectedTemplate != null && !selectedLastEvent)
    {
        selectedLastEvent = true;
        return;
    }
    if (SelectedTemplate == null)
    {
        selectedLastEvent = false;
        return;
    }

    #region TMP_INFO

    //if (SelectedTemplate != null)
    {
        var style_TemplateTitle = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = 19,
            padding = new RectOffset(0, 0, 5, 0),
        };
        GUILayout.Label(new GUIContent(SelectedTemplate.TmpName, SelectedTemplate.ShortDescription), style_TemplateTitle, GUILayout.ExpandWidth(true));
        EditorGUILayout.SelectableLabel(SelectedTemplate.LongDescription);

        using (new GUILayout.HorizontalScope(GUILayout.MaxHeight(30)))
        {

            Rect baseRect = GUILayoutUtility.GetRect(10, 50);

            Rect[] rects = baseRect
                .SplitHorizontal(4)
                .Select(r => r.Shrink(3, 3, 0, 0))
                .ToArray();

            TagDisplay(rects[0], SelectedTemplate.FileExts, "File Exts");
            TagDisplay(rects[1], SelectedTemplate.Langs, "Language");
            TagDisplay(rects[2], SelectedTemplate.Categories, "Categories");
            TagDisplay(rects[3], SelectedTemplate.Flags, "Flags");
        }

    }

    static void TagDisplay(Rect rect, List<string> tags, string title)
    {
        StringBuilder str = new StringBuilder("\n");

        foreach (var tag in tags)
        {
            str.Append($"{tag}, ");
        }
        if (str.Length > 2)
            str.Remove(str.Length - 2, 2);
        else
            str.Append("none");

        float lineHeight = EditorGUIUtility.singleLineHeight;
        int numItems = tags.Count;

        float height = lineHeight * numItems;

        var boxstyle = new GUIStyle("HelpBox")
        {
            fontSize = 12,
        };

        GUI.Label(rect, new GUIContent(str.ToString()), boxstyle);

        var titlestyle = new GUIStyle("Toolbar")
        {
            fontSize = 12,
            fixedHeight = lineHeight,
            fontStyle = FontStyle.Bold,
        };

        var titleRect = rect
            .Shrink(0, 0, 0, rect.height - lineHeight);

        GUI.Label(titleRect, title, titlestyle);
    }

    #endregion


    EditorGUI.BeginChangeCheck();

    var fileNameProperty = SelectedTemplate.FileNameProperty;
    if (fileNameProperty != null)
    {
        var str = EditorGUILayout.TextField("File Name", fileNameProperty.Value.ToString());

        if (EditorGUI.EndChangeCheck())
        {
            fileNameProperty.Value = str;
        }
    }

    #region INSPECTOR
    using (new GUILayout.ScrollViewScope(inspectorScroll))
    {
        var properties = SelectedTemplate.Properties;
        //var sobj = new UnityEditor.SerializedObject();
        foreach (var prop in properties)
        {
            if (prop.IsFileName)
                continue;
            if (prop.HasEditor)
                HandleProperty(prop);
        }
        void HandleProperty(TemplateProperty templateProperty)
        {
            float height = templateProperty.Editor.GetHeight(templateProperty);

            var rect = GUILayoutUtility.GetRect(0, height);

            templateProperty.Editor.DrawGUI(rect, templateProperty);
        }
    }

    #endregion


}



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