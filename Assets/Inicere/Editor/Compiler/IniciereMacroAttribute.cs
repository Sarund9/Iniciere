using System;

namespace Iniciere
{
    [AttributeUsage(
        AttributeTargets.Method, Inherited = false, AllowMultiple = false
        )]
    public sealed class IniciereMacroAttribute : Attribute
    {
        //public IniciereMacroAttribute(params Type[] input)
        //{
        //    InputSign = input;
        //}
        public IniciereMacroAttribute(string macroName)//, params Type[] input) : this(input)
        {
            Name = macroName;
        }

        public string Name { get; }
        public Type[] InputSign { get; }

        public bool HasName => Name is object;
    }

}
