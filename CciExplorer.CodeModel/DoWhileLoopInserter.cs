using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Cci;
using Microsoft.Cci.MutableCodeModel;

namespace TourreauGilles.CciExplorer.CodeModel
{
    /// <summary>
    /// 
    /// </summary>
    /// <example>
    ///     int local_0 = 0;
    /// StartLoop:
    ///    LoopCode
    ///    if (Condition)
    ///    {
    ///        goto StartLoop;
    ///    }
    /// EndLoop:
    ///    goto EndLoop;    // ???? (Microsoft's bug ?)
    /// </example>
    internal sealed class DoWhileLoopInserter : BaseCodeTraverser
    {
        public override void Visit(IBlockStatement block)
        {
            BlockStatement blockStatement;

            blockStatement = (BlockStatement)block;

            for (int i = 0; i < blockStatement.Statements.Count; i++)
            {
                ILabeledStatement labelStartWhile;

                labelStartWhile = blockStatement.Statements[i] as ILabeledStatement;
                if (labelStartWhile != null)
                {
                    // Find the conditional statement with the goto
                    IConditionalStatement conditionalLoop;

                    for (int j = i + 1; j < blockStatement.Statements.Count; j++)
                    {
                        // Check if it's the i-statement is a conditional loop
                        conditionalLoop = blockStatement.Statements[j] as IConditionalStatement;

                        if (conditionalLoop != null)
                        {
                            // Check if the true branch is a block statement
                            BlockStatement startLoopBlock;

                            startLoopBlock = conditionalLoop.TrueBranch as BlockStatement;
                            if (startLoopBlock != null)
                            {
                                // Check if the block contains a goto statement and the goto goes to the labeled previously find
                                IGotoStatement startLoopGoto;

                                startLoopGoto = startLoopBlock.Statements[0] as IGotoStatement;
                                if (startLoopGoto != null && startLoopGoto.TargetStatement == labelStartWhile)
                                {
                                    // OK create the do-while loop
                                    DoUntilStatement doWhileStatement;
                                    BlockStatement doWhileBlock;

                                    // Create a do-while block beetween i + 1 and j - 1
                                    doWhileBlock = new BlockStatement();
                                    for (int k = i + 1; k <= j - 1; k++)
                                    {
                                        doWhileBlock.Statements.Add(blockStatement.Statements[k]);
                                    }

                                    doWhileStatement = new DoUntilStatement();
                                    doWhileStatement.Condition = conditionalLoop.Condition;
                                    doWhileStatement.Body = doWhileBlock;

                                    // Remove the goto while loop
                                    for (int k = j; k >= i; k--)
                                    {
                                        blockStatement.Statements.RemoveAt(k);
                                    }

                                    // Insert the while loop
                                    blockStatement.Statements.Insert(i, doWhileStatement);

                                    // Remove the labeled and the goto after the Do/While (Microsoft's Bug ?)
                                    if (i + 2 < blockStatement.Statements.Count)
                                    {
                                        blockStatement.Statements.RemoveAt(i + 2);
                                        blockStatement.Statements.RemoveAt(i + 1);
                                    }

                                    // Exit the "finding conditional" loop
                                    break;
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
