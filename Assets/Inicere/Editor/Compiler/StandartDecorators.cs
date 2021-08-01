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
        static void FileNameDecorator(DecoratorContext ctx)
        {

        }
        class FileNameEditor
        {

        }
    }

    [AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = false)]
    sealed class IniciereDecoratorAttribute : Attribute
    {
        
        public IniciereDecoratorAttribute(string name)
        {
            Name = name;
        }

        public string Name { get; }
    }

    public class DecoratorContext
    {
        TemplateProperty property;

        public void AddPropertyEditor()
        {

        }
    }

    public class DecoratorTypeInstance
    {
        public DecoratorTypeInstance(MethodInfo method, IniciereMacroAttribute atr)
        {
            Method = method;
            Atr = atr;
        }

        public MethodInfo Method { get; }
        public IniciereMacroAttribute Atr { get; }

        public string Name => Atr.Name;
    }

    public class DecoratorExecInstance
    {
        public DecoratorExecInstance(DecoratorTypeInstance decor)
        {
            Decor = decor;
        }
        public DecoratorTypeInstance Decor { get; }

    }

    public abstract class InicierePropertyEditor
    {
        public abstract void DrawGUI(Rect area, TemplateProperty property);
        public virtual float GetHeight(Rect area, TemplateProperty property) => EditorGUIUtility.singleLineHeight;
    }
}
