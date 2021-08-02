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
            ctx.Property.Value = "File";
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

                        if (p.Length != 1 || p[0].ParameterType != typeof(DecoratorContext) || p[0].IsOut)
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
        }

        public MethodInfo Method { get; }
        public IniciereDecoratorAttribute Atr { get; }

        public string Name => Atr.Name;
    }

    public class DecoratorExecInstance
    {
        public DecoratorExecInstance(DecoratorTypeInstance decor)
        {
            Decor = decor;
        }
        public DecoratorTypeInstance Decor { get; }


        public void Execute(DecoratorContext ctx)
        {
            var mi = Decor.Method;

            mi.Invoke(null, new object[] { ctx });
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
