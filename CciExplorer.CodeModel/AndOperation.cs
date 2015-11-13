using Microsoft.Cci;

namespace TourreauGilles.CciExplorer.CodeModel
{
    public class AndOperation : LogicalBinaryOperation, IAndOperation
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
