namespace TourreauGilles.CciExplorer.CodeModel
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Microsoft.Cci;
    using Microsoft.Cci.ILToCodeModel;
    using Microsoft.Cci.MutableCodeModel;

    /// <summary>
    /// Remove local declaration which are used directly in a return statement
    /// </summary>
    internal sealed class LocalDeclarationStatementRemover : BaseCodeTraverser
    {
        public override void Visit(IBlockStatement block)
        {
            IStatement[] statements;

            statements = block.Statements.ToArray();
            for (int i = statements.Length - 2; i >= 0; i--)
            {
                IReturnStatement returnStatement;

                returnStatement = statements[i + 1] as IReturnStatement;

                if (returnStatement != null)
                {
                    IBoundExpression boundExpression;

                    boundExpression = returnStatement.Expression as IBoundExpression;
                    if (boundExpression != null)
                    {
                        ILocalDeclarationStatement localDeclaration;

                        localDeclaration = statements[i] as ILocalDeclarationStatement;

                        if (localDeclaration != null && (localDeclaration.LocalVariable == boundExpression.Definition))
                        {
                            BlockStatement basicBlock;

                            basicBlock = (BlockStatement)block;

                            basicBlock.Statements.RemoveAt(i + 1);
                            basicBlock.Statements[i] = new ReturnStatement() { Expression = localDeclaration.InitialValue };
                        }
                    }
                }
            }
        }
    }
}
