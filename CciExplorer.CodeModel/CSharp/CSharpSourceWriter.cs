using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;
using Microsoft.Cci;

namespace TourreauGilles.CciExplorer.CSharp
{
    public class CSharpSourceWriter : ICSharpSourceWriter
    {
        private ISourceWriter innerSourceWriter;

        public CSharpSourceWriter(ISourceWriter innerSourceWriter)
        {
            this.innerSourceWriter = innerSourceWriter;
        }

        public bool IsNewLine
        {
            get { return this.innerSourceWriter.IsNewLine; }
        }

        public void WriteEndInstruction()
        {
            if (this.IsNewLine == false)
            {
                this.Write(CSharpLanguage.SemiColonToken);
                this.NewLine();
            }
        }

        public void OpenBlock()
        {
            this.NewLine();
            this.Write(CSharpLanguage.LeftCurlyToken);
            this.Indent();
        }

        public void CloseBlock()
        {
            this.Unindent();
            this.Write(CSharpLanguage.RightCurlyToken);
            this.NewLine();
        }

        public void Indent()
        {
            this.innerSourceWriter.Indent();
        }

        public void Unindent()
        {
            this.innerSourceWriter.Unindent();
        }

        public void WriteLeftParenthesis()
        {
            this.Write(CSharpLanguage.LeftParenthesisToken);
        }

        public void WriteRightParenthesis()
        {
            this.Write(CSharpLanguage.RightParenthesisToken);
        }

        public void WriteKeyword(string keyword)
        {
            this.innerSourceWriter.WriteKeyword(keyword);
        }

        public void WriteString(string str)
        {
            this.innerSourceWriter.WriteString(str);
        }

        public void WriteChar(char character)
        {
            this.innerSourceWriter.WriteChar(character);
        }

        public void WriteReference(IReference reference, string format, params object[] arguments)
        {
            this.innerSourceWriter.WriteReference(reference, format, arguments);
        }

        public void Write(string format, params object[] arguments)
        {
            this.innerSourceWriter.Write(format, arguments);
        }

        public void WriteSpace()
        {
            this.innerSourceWriter.WriteSpace();
        }

        public void NewLine()
        {
            this.innerSourceWriter.NewLine();
        }
    }
}
