using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Cci;
using Microsoft.Cci.MutableCodeModel;

namespace TourreauGilles.CciExplorer.CodeModel
{
    /// <summary>
    /// Add "condition == true" if needed
    /// </summary>
    internal class ConditionalReformer : SourceVisitor
    {
        public override void Visit(IConditionalStatement conditionalStatement)
        {
            ((ConditionalStatement)conditionalStatement).Condition = ReformConditionalExpression(conditionalStatement.Condition);

            if (conditionalStatement.Condition is IBinaryOperation == false || conditionalStatement.Condition is IUnaryOperation)
            {
                // Create the "true" constant
                CompileTimeConstant constant;

                constant = new CompileTimeConstant();
                constant.Type = conditionalStatement.Condition.Type.PlatformType.SystemBoolean;
                constant.Value = true;

                // Create the equal operation
                Equality equalOperation;
                
                equalOperation = new Equality();
                equalOperation.LeftOperand = conditionalStatement.Condition;
                equalOperation.RightOperand = constant;

                ((ConditionalStatement)conditionalStatement).Condition = equalOperation;
            }

            base.Visit(conditionalStatement);
        }

        public override void Visit(IBitwiseAnd bitwiseAnd)
        {
            this.VisitBinaryOperation(bitwiseAnd);

            base.Visit(bitwiseAnd);
        }

        public override void Visit(IBitwiseOr bitwiseOr)
        {
            this.VisitBinaryOperation(bitwiseOr);

            base.Visit(bitwiseOr);
        }

        public override void Visit(IBoundExpression boundExpression)
        {
            ((BoundExpression)boundExpression).Instance = ReformConditionalExpression(boundExpression.Instance);

            base.Visit(boundExpression);
        }

        public override void Visit(IEquality equality)
        {
            this.VisitBinaryOperation(equality);

            base.Visit(equality);
        }

        public override void Visit(IExpressionStatement expressionStatement)
        {
            ((ExpressionStatement)expressionStatement).Expression = ReformConditionalExpression(expressionStatement.Expression);

            base.Visit(expressionStatement);
        }

        public override void Visit(INotEquality notEquality)
        {
            this.VisitBinaryOperation(notEquality);

            base.Visit(notEquality);
        }

        public override void Visit(ILogicalNot logicalNot)
        {
            ((LogicalNot)logicalNot).Operand = ReformConditionalExpression(logicalNot.Operand);

            base.Visit(logicalNot);
        }

        private void VisitBinaryOperation(IBinaryOperation binaryOperation)
        {
            ((BinaryOperation)binaryOperation).LeftOperand = ReformConditionalExpression(binaryOperation.LeftOperand);
            ((BinaryOperation)binaryOperation).RightOperand = ReformConditionalExpression(binaryOperation.RightOperand);
        }

        private static IExpression ReformConditionalExpression(IExpression expression)
        {
            IConditional conditionalExpression;

            conditionalExpression = expression as IConditional;

            if (conditionalExpression != null)
            {
                if (IsEqualTo(conditionalExpression.ResultIfTrue, 1) == true)
                {
                    return new OrOperation()
                    {
                        LeftOperand = conditionalExpression.Condition,
                        RightOperand = conditionalExpression.ResultIfFalse
                    };
                }
                else if (IsEqualTo(conditionalExpression.ResultIfFalse, 0) == true)
                {
                    return new AndOperation()
                    {
                        LeftOperand = conditionalExpression.Condition,
                        RightOperand = conditionalExpression.ResultIfTrue
                    };
                }
            }

            return expression;
        }

        private static bool IsEqualTo(IExpression expression, int value)
        {
            ICompileTimeConstant constExpression;

            constExpression = expression as ICompileTimeConstant;
            if (constExpression != null)
            {
                IConvertible convertible;

                convertible = constExpression.Value as IConvertible;

                if (convertible.ToInt32(null) == value)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
