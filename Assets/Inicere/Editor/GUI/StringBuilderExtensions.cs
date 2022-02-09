using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Iniciere
{
    public static class StringBuilderExtensions
    {
        public static bool IsAt(this StringBuilder build, string value, int index)
        {
            if (string.IsNullOrEmpty(value))
            {
                throw new ArgumentException("IsAt: value cannot be Null or Empty");
            }
            
            int c = 0;
            for (int i = index; i < build.Length; i++)
            {
                if (build[i] == value[c])
                {
                    c++;
                    if (value.Length >= c)
                    {
                        return true;
                    }
                }
                else
                {
                    return false;
                }
            }
            return false;
        }
    }
}
