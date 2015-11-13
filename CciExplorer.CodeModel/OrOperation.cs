using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Cci;
using Microsoft.Cci.MutableCodeModel;

namespace TourreauGilles.CciExplorer.CodeModel
{
    public class OrOperation : LogicalBinaryOperation, IOrOperation
    {
        public override void Dispatch(ICodeVisitor visitor)
        {
            ISourceVisitor sourceVisitor;

            sourceVisitor = visitor as ISourceVisitor;
            if (sourceVisitor != null)
            {
                sourceVisitor.Visit(this);
            }
        }
    }
}
