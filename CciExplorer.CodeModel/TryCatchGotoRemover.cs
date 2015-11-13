using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Cci;
using Microsoft.Cci.ILToCodeModel;
using TourreauGilles.CciExplorer.CSharp;
using Microsoft.Cci.MutableCodeModel;

namespace TourreauGilles.CciExplorer.CodeModel
{
    /// <summary>
    /// Remove all goto/label in a try/catch statement
    /// </summary>
    internal class TryCatchAnalizer : BaseCodeTraverser
    {
        public override void Visit(IBlockStatement block)
        {
            int qtdTry = 0;
            int qtdCatch = 0;
            int qtdCatchGeneric = 0;
            int qtdCatchSpecialized = 0;

            BlockStatement basicBlock;

            basicBlock = block as BlockStatement;

            if (basicBlock != null)
            {
                for (int i = 0; i < basicBlock.Statements.Count; i++)
                {
                    ITryCatchFinallyStatement tryCatchFilterFinallyStatement;

                    tryCatchFilterFinallyStatement = basicBlock.Statements[i] as ITryCatchFinallyStatement;
                    if (tryCatchFilterFinallyStatement != null)
                    {
                        qtdTry++;

                        // It's a try/catch statement get the try block
                        BlockStatement tryBlock;
                        tryBlock = (BlockStatement)tryCatchFilterFinallyStatement.TryBody;

                        // Get the goto statement a the end of the goto statement
                        IGotoStatement gotoStatement;
                        gotoStatement = (IGotoStatement)tryBlock.Statements.Last();

                        // Remove the goto statement
                        tryBlock.Statements.Remove(gotoStatement);

                        // Remove all goto statement in catch blocks
                        foreach (ICatchClause catchClause in tryCatchFilterFinallyStatement.CatchClauses)
                        {
                            qtdCatch++;

                            if (Type.GetType(catchClause.ExceptionType.ResolvedType.ToString()).Equals(typeof(Exception)))
                                qtdCatchGeneric++;
                            else
                                qtdCatchSpecialized++;

                            BlockStatement catchBlock;
                            IGotoStatement catchGotoStatement;

                            catchBlock = (BlockStatement)catchClause.Body;

                            // Get the last statement of the block
                            catchGotoStatement = catchBlock.Statements.Last() as IGotoStatement;

                            // If the last statement is a goto statement, remove it
                            if (catchGotoStatement != null)
                            {
                                catchBlock.Statements.Remove(catchGotoStatement);
                            }
                        }

                        // Remove the attached label on the parent block
                        basicBlock.Statements.Remove(gotoStatement.TargetStatement);
                    }
                }
            }



            base.Visit(block);
        }
    }
}
