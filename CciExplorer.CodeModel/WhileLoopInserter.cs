using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Cci;
using Microsoft.Cci.MutableCodeModel;

namespace TourreauGilles.CciExplorer.CodeModel
{
    /// <summary>
    /// Find a create 
    /// </summary>
    /// <example>
    ///    int local_0 = 0;
    ///    goto EndLabel;
    /// StartLabel:
    ///    // Loop code
    /// EndLabel:
    ///   if (LoopCondition)
    ///   {
    ///        goto StartLabel;
    ///   }
    /// </example>
    internal sealed class WhileLoopInserter : BaseCodeTraverser
    {
        public override void Visit(IBlockStatement block)
        {
            BlockStatement blockStatement;

            blockStatement = (BlockStatement)block;

            for (int i = 0; i < blockStatement.Statements.Count; i++)
            {
                IGotoStatement gotoEndWhile;

                gotoEndWhile = blockStatement.Statements[i] as IGotoStatement;
                if (gotoEndWhile != null)
                {
                    // Find the index of the labeled statement
                    int indexEndWhile;
                    indexEndWhile = blockStatement.Statements.IndexOf(gotoEndWhile.TargetStatement);
                    indexEndWhile++;

                    if (indexEndWhile < blockStatement.Statements.Count)
                    {
                        // Check if the statement at the indexEndWhile position is a conditional statement
                        IConditionalStatement conditionalWhile;
                        conditionalWhile = blockStatement.Statements[indexEndWhile] as IConditionalStatement;

                        if (conditionalWhile != null)
                        {
                            // Check if the conditionalWhile.TrueBranch is a BlockStatement
                            BlockStatement trueBranchBlock;
                            trueBranchBlock = conditionalWhile.TrueBranch as BlockStatement;

                            if (trueBranchBlock != null)
                            {
                                // Check if the conditionalWhile.TrueBranch is a goto statement
                                IGotoStatement gotoStartWhile;
                                gotoStartWhile = trueBranchBlock.Statements.ElementAt(0) as IGotoStatement;

                                if (gotoStartWhile != null)
                                {
                                    // Check if the labeled statement is at an index position less than the labeled gotoEndWhile labeled statement
                                    int indexStartWhile;
                                    indexStartWhile = blockStatement.Statements.IndexOf(gotoStartWhile.TargetStatement);

                                    if (indexStartWhile < indexEndWhile)
                                    {
                                        // OK, create a while loop
                                        WhileDoStatement whileStatement;
                                        BlockStatement whileBlock;

                                        // Create a while block beetween indexStartWhile + 1 and indexEndWhile - 2
                                        whileBlock = new BlockStatement();
                                        for (int j = indexStartWhile + 1; j <= indexEndWhile - 2; j++)
                                        {
                                            whileBlock.Statements.Add(blockStatement.Statements[j]);
                                        }

                                        whileStatement = new WhileDoStatement();
                                        whileStatement.Condition = conditionalWhile.Condition;
                                        whileStatement.Body = whileBlock;

                                        // Remove the goto while loop
                                        for (int j = indexEndWhile; j >= indexStartWhile - 1; j--)
                                        {
                                            blockStatement.Statements.RemoveAt(j);
                                        }

                                        // Insert the while loop
                                        blockStatement.Statements.Insert(i, whileStatement);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
