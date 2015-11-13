using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Cci;
using Microsoft.Cci.MutableCodeModel;

namespace TourreauGilles.CciExplorer.CodeModel
{
    internal sealed class UnaryNegationAndPlusInserter : BaseCodeTraverser
    {
        public override void Visit(IBlockStatement block)
        {
            BlockStatement blockStatement;

            blockStatement = (BlockStatement)block;

            for (int i = 0; i < blockStatement.Statements.Count; i++)
            {
                // Find IExpressionStatement
                ExpressionStatement expressionStatement;

                expressionStatement = blockStatement.Statements[i] as ExpressionStatement;
                if (expressionStatement != null)
                {
                    // Check if the expression statement is IAssignment
                    IAssignment assignment;

                    assignment = expressionStatement.Expression as IAssignment;
                    if (assignment != null)
                    {
                        // Check if right of assignment is an addition
                        IAddition addition;

                        addition = assignment.Source as IAddition;
                        if (addition != null)
                        {
                            // Check if right of addition is a constant
                            ICompileTimeConstant constant;

                            constant = addition.RightOperand as ICompileTimeConstant;
                            if (constant != null)
                            {
                                if (constant.Value is int && (int)constant.Value == 1)
                                {
                                    // OK, replace the line with a UnaryPlus statement
                                    UnaryPlus unaryPlus;

                                    unaryPlus = new UnaryPlus();
                                    unaryPlus.Operand = addition.LeftOperand;

                                    expressionStatement.Expression = unaryPlus;
                                }
                            }

                            continue;
                        }

                        // Check if right of assignment is a subtraction
                        ISubtraction subtraction;

                        subtraction = assignment.Source as ISubtraction;
                        if (subtraction != null)
                        {
                            // Check if right of subtraction is a constant
                            ICompileTimeConstant constant;

                            constant = subtraction.RightOperand as ICompileTimeConstant;
                            if (constant != null)
                            {
                                if (constant.Value is int && (int)constant.Value == 1)
                                {
                                    // OK, replace the line with a UnaryNegation statement
                                    UnaryNegation unaryNegation;

                                    unaryNegation = new UnaryNegation();
                                    unaryNegation.Operand = subtraction.LeftOperand;

                                    expressionStatement.Expression = unaryNegation;
                                }
                            }

                            continue;
                        }
                    }
                }
            }

            base.Visit(block);
        }
    }
}
