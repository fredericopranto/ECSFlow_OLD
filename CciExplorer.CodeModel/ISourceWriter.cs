using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Cci;

namespace TourreauGilles.CciExplorer
{
    public interface ISourceWriter
    {
        bool IsNewLine
        {
            get;
        }

        void Write(string format, params object[] arguments);

        void WriteSpace();

        void WriteKeyword(string keyword);

        void WriteReference(IReference reference, string format, params object[] arguments);

        void WriteString(string str);

        void WriteChar(char character);

        void NewLine();

        void Unindent();

        void Indent();
    }
}
