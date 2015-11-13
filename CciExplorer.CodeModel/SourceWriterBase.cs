using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Cci;

namespace TourreauGilles.CciExplorer
{
    public abstract class SourceWriterBase : ISourceWriter
    {
        private int currentIndentation;

        private const string SpaceToken = " ";

        private bool isNewLine;

        protected SourceWriterBase()
        {
            this.isNewLine = true;
        }

        public bool IsNewLine
        {
            get { return this.isNewLine; }
        }

        protected int CurrentIndentation
        {
            get { return this.currentIndentation; }
        }

        public void NewLineIfNeeded()
        {
            if (this.isNewLine == false)
            {
                this.NewLine();
            }
        }

        public virtual void Indent()
        {
            this.NewLineIfNeeded();

            this.currentIndentation += 4;
        }

        public virtual void Unindent()
        {
            this.NewLineIfNeeded();

            this.currentIndentation -= 4;
        }

        public void WriteSpace()
        {
            this.Write(SpaceToken);
        }

        public void Write(string format, params object[] arguments)
        {
            this.WriteCore(format, arguments);
            this.isNewLine = false;
        }

        protected abstract void WriteCore(string format, params object[] arguments);

        public void WriteKeyword(string keyword)
        {
            this.WriteKeywordCore(keyword);
            this.isNewLine = false;
        }

        protected virtual void WriteKeywordCore(string keyword)
        {
            this.Write(keyword);
        }

        public void WriteReference(IReference reference, string format, params object[] arguments)
        {
            this.WriteReferenceCore(reference, format, arguments);
            this.isNewLine = false;
        }

        protected abstract void WriteReferenceCore(IReference reference, string format, params object[] arguments);

        public void WriteChar(char character)
        {
            this.WriteCharCore(character);
            this.isNewLine = false;
        }

        protected virtual void WriteCharCore(char character)
        {
            this.Write("\'{0}\'", character);
        }

        public void WriteString(string str)
        {
            this.WriteStringCore(str);
            this.isNewLine = false;
        }

        protected virtual void WriteStringCore(string str)
        {
            this.Write("\"{0}\"", str);
        }

        public void NewLine()
        {
            this.NewLineCore();
            this.isNewLine = true;
        }

        protected abstract void NewLineCore();
    }
}
