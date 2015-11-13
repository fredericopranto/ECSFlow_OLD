using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Cci;
using Microsoft.Cci.MutableCodeModel;

namespace TourreauGilles.CciExplorer.CodeModel
{
    /// <summary>
    /// Correct assignment operator when the two operand are not the same type.
    /// </summary>
    internal class AssignmentReformer : BaseCodeTraverser
    {
        public override void Visit(IAssignment assignment)
        {
            if (assignment.Target.Type.ResolvedType.IsAssignableFrom(assignment.Source.Type.ResolvedType) == false)
            {
                CompileTimeConstant constant;

                constant = assignment.Source as CompileTimeConstant;
                if (constant != null)
                {
                    ((Assignment)assignment).Source = GetSourceExpression(constant, assignment.Target.Type);
                }
            }
        }

        public override void Visit(ILocalDeclarationStatement localDeclarationStatement)
        {
            if (localDeclarationStatement.InitialValue != null)
            {
                if (localDeclarationStatement.LocalVariable.Type.InternedKey != localDeclarationStatement.InitialValue.Type.InternedKey)
                {
                    CompileTimeConstant constant;

                    constant = localDeclarationStatement.InitialValue as CompileTimeConstant;
                    if (constant != null)
                    {
                        ((LocalDeclarationStatement)localDeclarationStatement).InitialValue = GetSourceExpression(constant, localDeclarationStatement.LocalVariable.Type);
                    }
                }
            }
        }

        private static IExpression GetSourceExpression(CompileTimeConstant constant, ITypeReference targetType)
        {
            if (constant.Value == null)
            {
                return constant;
            }

            if (targetType.IsEnum == true)
            {
                IFieldDefinition enumField;

                enumField = targetType.GetEnumFieldValue(constant.Value);
                if (enumField != null)
                {
                    // The InitialValue is a static reference to the enumField
                    BoundExpression boundExpression;
                    boundExpression = new BoundExpression();

                    boundExpression.Instance = null;
                    boundExpression.Definition = new FieldReference()
                    {
                        ContainingType = enumField.ContainingType,
                        Name = enumField.Name
                    };

                    return boundExpression;
                }
                else
                {
                    // Cast de value in the enum type
                    Conversion castExpression;

                    castExpression = new Conversion()
                    {
                        TypeAfterConversion = targetType,
                        ValueToConvert = constant
                    };

                    return castExpression;
                }
            }
            else
            {
                constant.Value = ConvertValue(constant.Value, targetType);
                constant.Type = targetType;

                return constant;
            }
        }

        private static object ConvertValue(object value, ITypeReference destinationType)
        {
            if (destinationType.TypeCode == PrimitiveTypeCode.Boolean)
            {
                return Convert.ToBoolean(value);
            }

            if (destinationType.TypeCode == PrimitiveTypeCode.Char)
            {
                return Convert.ToChar(value);
            }

            if (destinationType.TypeCode == PrimitiveTypeCode.Int8)
            {
                return Convert.ToSByte(value);
            }

            if (destinationType.TypeCode == PrimitiveTypeCode.Int16)
            {
                return Convert.ToInt16(value);
            }

            if (destinationType.TypeCode == PrimitiveTypeCode.Int64)
            {
                return Convert.ToInt64(value);
            }

            if (destinationType.TypeCode == PrimitiveTypeCode.UInt8)
            {
                return Convert.ToByte(value);
            }

            if (destinationType.TypeCode == PrimitiveTypeCode.UInt16)
            {
                return Convert.ToUInt16(value);
            }

            if (destinationType.TypeCode == PrimitiveTypeCode.UInt32)
            {
                return Convert.ToUInt32(value);
            }

            throw new NotSupportedException();
        }
    }
}
