using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Cci;

namespace TourreauGilles.CciExplorer.CodeModel
{
    public static class SourceWriterExtensions
    {
        public static void Write(this ISourceWriter writer, IName name)
        {
            writer.Write(name.Value);
        }

        public static void Write(this ISourceWriter writer, INamedEntity namedEntity)
        {
            writer.Write(namedEntity.Name);
        }

        public static void WriteReference(this ISourceWriter writer, IReference reference)
        {
            writer.WriteReference(reference, reference.GetName());
        }
    }
}
