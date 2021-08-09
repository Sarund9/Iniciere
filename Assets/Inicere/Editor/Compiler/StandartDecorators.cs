using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Iniciere
{
    public static class StandartDecorators
    {

        [IniciereDecorator("FileName")]
        public static void FileNameDecorator(DecoratorContext ctx)
        {
            ctx.Property.Value = "New_File"; //TODO: Default Name Input Option
            ctx.Property.MarkAsFileName();
        }
        [IniciereDecorator("Text")]
        public static void TextDecorator(DecoratorContext ctx)
        {
            ctx.Property.Value = "";
            ctx.Property.Editor = new TextEditor();
        }

        class TextEditor : InicierePropertyEditor
        {
            public override void DrawGUI(Rect area, TemplateProperty property)
            {
                EditorGUI.BeginChangeCheck();

                var str = EditorGUI.TextField(area, property.Name, property.Value.ToString());

                if (EditorGUI.EndChangeCheck())
                {
                    property.Value = str;
                }
            }
        }
        
        [IniciereDecorator("Toggle")]
        public static void ToggleDecorator(DecoratorContext ctx, string editorName = null)
        {
            //Debug.Log($"Toggle decorator : '{editorName}'");
            ctx.Property.Value = false;
            ctx.Property.Editor = new ToggleEditor(editorName);
        }
        class ToggleEditor : InicierePropertyEditor
        {
            private string editorName;

            public ToggleEditor(string editorName)
            {
                this.editorName = editorName;
            }

            public override void DrawGUI(Rect area, TemplateProperty property)
            {
                EditorGUI.BeginChangeCheck();

                bool value = EditorGUI.ToggleLeft(area,
                    editorName ?? property.Name,
                    (bool)property.Value);

                if (EditorGUI.EndChangeCheck())
                {
                    property.Value = value;
                }
            }
        }
        
        [IniciereDecorator("OptionalText")]
        public static void OptionalText(DecoratorContext ctx)
        {
            ctx.Property.Editor = new OptTextEditor();
        }
        class OptTextEditor : InicierePropertyEditor
        {
            public override void DrawGUI(Rect area, TemplateProperty property)
            {
                EditorGUI.BeginChangeCheck();
                //Rect text = area.Shrink(0, 10, 0, 0);
                var str = EditorGUI.TextField(area, property.Name, property.Value.ToString());
                if (EditorGUI.EndChangeCheck())
                {
                    property.Value = str;
                }

                EditorGUI.BeginChangeCheck();
                Rect toggle = area.Shrink(area.width - 10, 0, 0, 0);
                toggle.x += area.width - 10;

                if (EditorGUI.EndChangeCheck())
                {
                    property.Value = str;
                }
            }
        }

        [IniciereDecorator("ClassType")]
        public static void ClassType(DecoratorContext ctx, string msg = null, Type requiredImpl = null)
        {
            ctx.Property.Editor = new ClassTypeEditor(msg, requiredImpl);
        }
        class ClassTypeEditor : InicierePropertyEditor
        {
            readonly string msg;
            readonly Type requiredImpl;

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

    [AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = false)]
    public sealed class IniciereDecoratorAttribute : Attribute
    {
        
        public IniciereDecoratorAttribute(string name)
        {
            Name = name;
        }

        public string Name { get; }
    }

    public class DecoratorContext
    {
        

        private DecoratorContext()
        {
        }

        public Assembly[] Assemblies { get; private set; }
        public IEnumerable<Type> Types { get; private set; }
        public IEnumerable<MethodInfo> Methods { get; private set; }
        public IEnumerable<DecoratorTypeInstance> Decorators { get; private set; }

        public TemplateProperty Property { get; private set; }

        public void Prepare(TemplateProperty property)
        {
            Property = property;
        }

        public void AddPropertyEditor()
        {

        }

        public static IEnumerable<DecoratorTypeInstance> InitializeDecoratorSystem(AppDomain domain, out DecoratorContext ctx)
        {
#pragma warning disable IDE0017 // Simplify object initialization
            ctx = new DecoratorContext();
#pragma warning restore IDE0017 // Simplify object initialization

            ctx.Assemblies = domain.GetAssemblies();
            ctx.Types = ctx.Assemblies.SelectMany(a => a.GetTypes());
            ctx.Methods = ctx.Types.SelectMany(t => t.GetMethods());

            //var m = typeof(StandartDecorators).GetMethod("FileNameDecorator");
            //Debug.Log($"METHOD:{m}");
            //var at = m.GetCustomAttribute<IniciereDecoratorAttribute>();
            //Debug.Log($"ATR CONTAINS:{at}");

            return ctx.Decorators = ctx.Methods
                .SelectWhere((MethodInfo m, out DecoratorTypeInstance di) =>
                {
                    var atr = m.GetCustomAttribute<IniciereDecoratorAttribute>();

                    if (atr is object)
                    {
                        di = new DecoratorTypeInstance(m, atr);

                        //Debug.Log($"METHOD: {m.Name}");

                        if (!m.IsStatic || m.ContainsGenericParameters)
                            return false;

                        var p = m.GetParameters();
                        //Debug.Log($"PARAMS {p.Length} '{p[0].ParameterType.Name}'");

                        if (p.Length == 0 || p[0].ParameterType != typeof(DecoratorContext)
                            || p.Any(pm => pm.IsOut))
                            return false;

                        //Debug.Log($"DECOR: {atr.Name}");
                        return true;
                    }
                    else
                    {
                        di = null;
                        return false;
                    }
                });
        }
    }

    public class DecoratorTypeInstance
    {
        public DecoratorTypeInstance(MethodInfo method, IniciereDecoratorAttribute atr)
        {
            Method = method;
            Atr = atr;
            var ps = Method.GetParameters();
            UsesParams = ps[ps.Length - 1]
                .IsDefined(typeof(ParamArrayAttribute), false);
            ParamCount = ps.Length - 1;
        }

        public MethodInfo Method { get; }
        public IniciereDecoratorAttribute Atr { get; }

        public bool UsesParams { get; }
        public int ParamCount { get; }

        public string Name => Atr.Name;
    }

    public class DecoratorExecInstance
    {
        public DecoratorExecInstance(DecoratorTypeInstance decor, object[] parameters)
        {
            Decor = decor;
            Params = parameters;
        }
        public DecoratorTypeInstance Decor { get; }

        public object[] Params { get; }

        public bool Execute(DecoratorContext ctx)
        {
            var mi = Decor.Method;

            int inputLenght = Decor.ParamCount + 1;
            var inputs = new object[inputLenght];

            for (int i = 0; i < Params.Length; i++)
            {
                if (i == Decor.ParamCount - 1 && Decor.UsesParams)
                {
                    int paramsLenght = Params.Length - Decor.ParamCount + 1;
                    var paramArg = new object[paramsLenght];
                    for (int p = 0; p < paramsLenght; p++)
                    {
                        paramArg[p] = Params[i + p];
                    }
                    inputs[i + 1] = paramArg;
                    break;
                }
                inputs[i + 1] = Params[i];
            }
            inputs[0] = ctx;

            //var strprint = inputs.Aggregate(new StringBuilder(), (str, obj) =>
            //{
            //    string value = obj is null ? "NULL" : obj.ToString();
            //    str.Append(value + ", ");
            //    return str;
            //});
            //Debug.Log($"Executing {Params.Length} '{strprint}'");

            try
            {
                mi.Invoke(null, inputs);
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Iniciere Error: decorator '{Decor.Name}'" +
                    $" has incorrect Inputs: \n {ex.Message}");
                return false;
            }
        }
    }

    public abstract class InicierePropertyEditor
    {
        public abstract void DrawGUI(Rect area, TemplateProperty property);
        public virtual float GetHeight(TemplateProperty property) => EditorGUIUtility.singleLineHeight;

        //public virtual float GetHeight(float? prevHeight, TemplateProperty property) =>
        //    prevHeight is null ?
        //        EditorGUIUtility.singleLineHeight
        //        : (float)prevHeight;
    }
}
