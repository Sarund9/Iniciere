using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Iniciere
{
    public delegate void FnInicierePropertyEditor(Rect area, TemplateProperty property, object state);
    
    public static class StandartDecorators
    {

        [IniciereDecorator("FileName")]
        public static void FileNameDecorator(DecoratorContext ctx)
        {
            ctx.Property.Value = "New_File"; //TODO: Default Name Input Option
            //ctx.Property.LitValue = "New_File";
            ctx.Property.MarkAsFileName();
        }
        [IniciereDecorator("Text")]
        public static void TextDecorator(DecoratorContext ctx, string editorName)
        {
            ctx.Property.Value = "";
            //Debug.Log($"EDITOR: {editorName}");
            ctx.Property.Editor = new TextEditor(editorName);
            //ctx.Property.CreateEditor(DrawGUI, null);
        }
        

        [IniciereDecorator("Toggle")]
        public static void ToggleDecorator(DecoratorContext ctx, string editorName = null)
        {
            //Debug.Log($"Toggle decorator : '{editorName}'");
            ctx.Property.Value = false;
            ctx.Property.Editor = new PropertyToggleEditor(editorName);
        }
        
        //[IniciereDecorator("OptionalText")]
        public static void OptionalText(DecoratorContext ctx)
        {
            ctx.Property.Editor = new OptTextEditor();
        }
        
        //[IniciereDecorator("ClassType")]
        public static void ClassType(DecoratorContext ctx, string msg = null, Type requiredImpl = null)
        {
            ctx.Property.Editor = new ClassTypeEditor(msg, requiredImpl);
        }

        //[IniciereDecorator("Namespace")]
        //public static void NamespaceDecorator(DecoratorContext ctx)
        //{
        //    ctx.Property.Value = ""; //TODO: Default Name Input Option
        //    //ctx.Property.LitValue = "New_File";
        //    ctx.Property.Editor = new PropertyNamespaceEditor(false);
        //}
        //[IniciereDecorator("EditorNamespace")]
        //public static void EditorNamespaceDecorator(DecoratorContext ctx)
        //{
        //    ctx.Property.Value = ""; //TODO: Default Name Input Option
        //    //ctx.Property.LitValue = "New_File";
        //    ctx.Property.Editor = new PropertyNamespaceEditor(true);
        //}
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

    [Serializable]
    public class DecoratorTypeInstance
    {
        //[SerializeField]
        //UBox serialized = new UBox();

        public DecoratorTypeInstance(MethodInfo method, IniciereDecoratorAttribute atr)
        {
            Method = method;
            Atr = atr;
            var ps = Method.GetParameters();
            UsesParams = ps[ps.Length - 1]
                .IsDefined(typeof(ParamArrayAttribute), false);
            ParamCount = ps.Length - 1;
        }

        public MethodInfo Method { get; private set; }
        public IniciereDecoratorAttribute Atr { get; private set; }

        public bool UsesParams { get; private set; }
        public int ParamCount { get; private set; }

        public string Name => Atr.Name;

        //public void OnBeforeSerialize()
        //{
        //    serialized.Set(this);
        //}

        //public void OnAfterDeserialize()
        //{
        //    var clone = serialized.Get() as DecoratorTypeInstance;
        //    Method = clone.Method;
        //    Atr = clone.Atr;
        //}
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
}
