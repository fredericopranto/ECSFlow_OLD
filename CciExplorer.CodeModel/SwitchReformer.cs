using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Cci;
using Microsoft.Cci.ILToCodeModel;
using Microsoft.Cci.MutableCodeModel;
using System.Diagnostics.Contracts;

namespace TourreauGilles.CciExplorer.CodeModel
{
    /// <summary>
    /// switch(value)
    /// {
    ///    case 0:
    ///    case 1:
    ///       goto IL_0017;
    ///       
    ///    default:
    ///       goto IL_0030:
    /// }
    /// 
    /// IL_0017:
    ///     Statements of case 0 and 1
    ///     goto IL_003e;
    /// IL_003e:
    /// </summary>
    internal class SwitchReformer : BaseCodeTraverser
    {
        public override void Visit(IBlockStatement block)
        {
            BlockStatement basicBlock;

            basicBlock = (BlockStatement)block;

            for (int i = 0; i < basicBlock.Statements.Count; i++)
            {
                ISwitchStatement switchStatement;

                switchStatement = basicBlock.Statements[i] as ISwitchStatement;
                if (switchStatement != null)
                {
                    foreach (SwitchCase switchCase in switchStatement.Cases)
                    {
                        IStatement[] statements;

                        statements = switchCase.Body.ToArray();

                        if (statements.Length > 0)
                        {
                            // Get the goto statement
                            IGotoStatement gotoStatement;
                            gotoStatement = (IGotoStatement)statements[0];

                            // Find the index of the label statement in the basic block
                            int labelStatementIndex;
                            labelStatementIndex = basicBlock.Statements.IndexOf(gotoStatement.TargetStatement);
                            Contract.Assert(labelStatementIndex != -1);

                            // Clear the body of the case statement
                            switchCase.Body.Clear();

                            // Copy all statements of the labeled statements and remove it in the labeled statement
                            int statementIndex;

                            statementIndex = labelStatementIndex + 1;
                            while ((basicBlock.Statements[statementIndex] is IGotoStatement) == false && (basicBlock.Statements[statementIndex] is ILabeledStatement) == false)
                            {
                                switchCase.Body.Add(basicBlock.Statements[statementIndex]);
                                basicBlock.Statements.RemoveAt(statementIndex);
                            }

                            // Remove labeled and goto statement in the basicblock
                            if ((basicBlock.Statements[statementIndex] is ILabeledStatement) == false)
                            {
                                basicBlock.Statements.RemoveAt(statementIndex);
                            }

                            basicBlock.Statements.Remove(gotoStatement.TargetStatement);

                            // Add a break statement at the end of the case statement
                            switchCase.Body.Add(new BreakStatement());
                        }
                    }

                    // Remove the labeled statement at the end
                    basicBlock.Statements.RemoveAt(i + 1);
                }
            }
        }
    }
}
