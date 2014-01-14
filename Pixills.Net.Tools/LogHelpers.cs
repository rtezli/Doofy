using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pixills.Tools.Log
{
    public class LogHelpers
    {
        public static string EscapeNonPrintable(string s)
        {
            var escaped = new StringBuilder();
            foreach(var c in s)
            {
                if(Char.IsLetterOrDigit(c) || Char.IsPunctuation(c) ||  Char.IsWhiteSpace(c))
                    escaped.Append(c);
                else
                    escaped.Append(".");
            }
            return escaped.ToString();
        }
    }
}
