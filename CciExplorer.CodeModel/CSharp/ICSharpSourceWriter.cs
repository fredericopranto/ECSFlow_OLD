using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TourreauGilles.CciExplorer.CSharp
{
    public interface ICSharpSourceWriter : ISourceWriter
    {
        void OpenBlock();

        void CloseBlock();

        void WriteLeftParenthesis();

        void WriteRightParenthesis();

        void WriteEndInstruction();
    }
}
