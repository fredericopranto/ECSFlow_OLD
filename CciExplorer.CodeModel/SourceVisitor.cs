using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Cci;

namespace TourreauGilles.CciExplorer.CodeModel
{
    public abstract class SourceVisitor : BaseCodeTraverser, ISourceVisitor
    {
        protected SourceVisitor()
        {
        }

        public override void Visit(IExpression expression)
        {
            if (expression is IAndOperation)
            {
                this.Visit((IAndOperation)expression);
            }
            else if (expression is IOrOperation)
            {
                this.Visit((IOrOperation)expression);
            }
            else
            {
                base.Visit(expression);
            }
        }

        public virtual void Visit(IAndOperation andExpression)
        {
            this.Visit(andExpression.LeftOperand);
            this.Visit(andExpression.RightOperand);
        }

        public virtual void Visit(IOrOperation orExpression)
        {
            this.Visit(orExpression.LeftOperand);
            this.Visit(orExpression.RightOperand);
        }
    }
}
