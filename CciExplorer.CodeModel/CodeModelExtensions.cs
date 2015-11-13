using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Cci;
using System.Diagnostics.Contracts;

namespace TourreauGilles.CciExplorer.CodeModel
{
    public static class CodeModelExtensions
    {
        public static IPropertyDefinition GetProperty(this IMethodDefinition method)
        {
            if (method.Name.Value.StartsWith("get_") == true || method.Name.Value.StartsWith("set_") == true)
            {
                string propertyName;

                propertyName = method.Name.Value.Substring(4);

                return method.ContainingTypeDefinition.Properties.Single(p => p.Name.Value == propertyName);
            }

            return null;
        }

        public static IMethodDefinition GetMethodReference(this IPropertyDefinition propertyDefinition)
        {
            if (propertyDefinition.Getter == null)
            {
                return propertyDefinition.Setter.ResolvedMethod;
            }
            else
            {
                return propertyDefinition.Getter.ResolvedMethod;
            }
        }

        public static bool IsGetterMethod(this IMethodDefinition method)
        {
            IPropertyDefinition property;

            property = GetProperty(method);
            if (property == null || property.Getter != method)
            {
                return false;
            }

            return true;
        }

        public static bool IsSetterMethod(this IMethodDefinition method)
        {
            IPropertyDefinition property;

            property = GetProperty(method);
            if (property == null || property.Setter != method)
            {
                return false;
            }

            return true;
        }

        public static bool HasConstraints(this IGenericParameter genericParameter)
        {
            if (genericParameter.MustBeReferenceType == true)
            {
                return true;
            }

            if (genericParameter.MustBeValueType == true)
            {
                return true;
            }

            if (genericParameter.MustHaveDefaultConstructor == true)
            {
                return true;
            }

            if (genericParameter.Constraints.Count() > 0)
            {
                return true;
            }

            return false;
        }

        public static bool IsAssignableFrom(this ITypeDefinition type, ITypeDefinition t)
        {
            if (type.InternedKey == t.InternedKey)
            {
                return true;
            }
            
            foreach (ITypeDefinition baseType in t.BaseClasses.Union(t.Interfaces).Select(tr => tr.ResolvedType))
            {
                if (type.IsAssignableFrom(baseType) == true)
                {
                    return true;
                }
            }
            
            return false;
        }

        public static IDefinition GetDefinition(this IReference reference)
        {
            if (reference is IDefinition)
            {
                return (IDefinition)reference;
            }

            if (reference is ITypeReference)
            {
                return ((ITypeReference)reference).ResolvedType;
            }

            if (reference is IFieldReference)
            {
                return ((IFieldReference)reference).ResolvedField;
            }

            if (reference is IMethodReference)
            {
                return ((IMethodReference)reference).ResolvedMethod;
            }

            throw new NotImplementedException();
        }

        public static string GetDisplayName(this IDefinition definition)
        {
            if (definition is IArrayType)
            {
                return ((IArrayType)definition).ElementType.GetDisplayName() + "[]";
            }
            else if (definition is IGenericTypeInstance)
            {
                return GetDisplayName((IGenericTypeInstance)definition);
            }
            else if (definition is INamespaceTypeDefinition)
            {
                return GetDisplayName((INamespaceTypeDefinition)definition);
            }
            else if (definition is IPointerType)
            {
                return ((IPointerType)definition).TargetType.GetDisplayName() + "*";
            }
            else if (definition is INamespaceDefinition)
            {
                return ((INamespaceDefinition)definition).ToString();
            }
            else if (definition is IMethodDefinition)
            {
                return GetDisplayName((IMethodDefinition)definition);
            }
            else if (definition is IPropertyDefinition)
            {
                return GetDisplayName((IPropertyDefinition)definition);
            }

            return ((INamedEntity)definition).Name.Value;
        }

        public static string GetDisplayName(INamespaceTypeDefinition namespaceTypeDefinition)
        {
            // Add generic parameter
            ITypeReference[] parameters;
            parameters = namespaceTypeDefinition.GenericParameters.ToArray();

            StringBuilder sb;
            sb = new StringBuilder(namespaceTypeDefinition.Name.Value);

            if (parameters.Length > 0)
            {
                sb.Append("<");
                sb.Append(parameters[0].GetDisplayName());

                for (int i = 1; i < parameters.Length; i++)
                {
                    sb.Append(", ");
                    sb.Append(parameters[i].GetDisplayName());
                }

                sb.Append(">");
            }

            return sb.ToString();
        }

        public static string GetDisplayName(IGenericTypeInstance genericTypeDefinition)
        {
            return genericTypeDefinition.GenericType.GetDisplayName();
        }

        public static string GetDisplayName(this IPropertyDefinition method)
        {
            return null;
        }

        public static string GetDisplayName(this IMethodDefinition method)
        {
            // Generate the method name
            StringBuilder sb;

            sb = new StringBuilder(method.Name.Value);

            // Generate generic arguments
            sb.Append(GetDisplayName(method.GenericParameters));

            // Generate parameters
            sb.Append("(");

            if (method.ParameterCount > 0)
            {
                IParameterDefinition[] parameters;

                parameters = method.Parameters.ToArray();
                sb.Append(parameters[0].Type.GetDisplayName());

                for (int i = 1; i < parameters.Length; i++)
                {
                    sb.Append(", ");
                    sb.Append(parameters[i].Type.GetDisplayName());
                }
            }

            sb.Append(")");

            if (method.IsConstructor == false)
            {
                sb.Append(" : ");
                sb.Append(method.Type.GetDisplayName());
            }

            return sb.ToString();
        }

        public static string GetDisplayName(this IReference reference)
        {
            Contract.Requires<ArgumentNullException>(reference != null);

            INamedTypeReference typeReference;

            typeReference = reference as INamedTypeReference;
            if (typeReference != null && typeReference.TypeCode != PrimitiveTypeCode.NotPrimitive)
            {
                return typeReference.Name.Value;
            }

            return reference.GetDefinition().GetDisplayName();
        }

        public static string GetName(this IReference reference)
        {
            Contract.Requires<ArgumentNullException>(reference != null);

            return reference.GetDefinition().GetName();
        }

        public static string GetName(this IDefinition definition)
        {
            Contract.Requires<ArgumentNullException>(definition != null);

            if (definition is INamedEntity)
            {
                return ((INamedEntity)definition).Name.Value;
            }

            if (definition is IGenericTypeInstance)
            {
                return GetName((IGenericTypeInstance)definition);
            }

            throw new NotSupportedException();
        }

        public static string GetName(this IGenericTypeInstance typeGeneric)
        {
            return typeGeneric.GenericType.GetDisplayName();
        }

        public static IEnumerable<IFieldDefinition> GetEnumFieldValues(this ITypeReference enumReference)
        {
            return enumReference.ResolvedType.GetEnumFieldValues();
        }

        public static IEnumerable<IFieldDefinition> GetEnumFieldValues(this ITypeDefinition enumDefinition)
        {
            Contract.Requires<ArgumentException>(enumDefinition.IsEnum == true);

            return enumDefinition.Fields.Where(f => f.IsStatic == true).OrderBy(f => f.CompileTimeValue.Value);
        }

        public static IFieldDefinition GetEnumFieldValue(this ITypeReference enumReference, object value)
        {
            return enumReference.ResolvedType.GetEnumFieldValue(value);
        }

        public static IFieldDefinition GetEnumFieldValue(this ITypeDefinition enumDefinition, object value)
        {
            return enumDefinition.GetEnumFieldValues().Where(f => f.CompileTimeValue.Value.Equals(value) == true).SingleOrDefault();
        }

        private static string GetDisplayName(IEnumerable<IGenericMethodParameter> genericParameters)
        {
            IGenericMethodParameter[] temp;

            temp = genericParameters.ToArray();
            if (temp.Length > 0)
            {
                StringBuilder sb;

                sb = new StringBuilder("<");
                sb.Append(temp[0].GetDisplayName());

                for (int i = 1; i < temp.Length; i++)
                {
                    sb.Append(",");
                    sb.Append(temp[i].GetDisplayName());
                }

                sb.Append(">");

                return sb.ToString();
            }

            return string.Empty;
        }
    }
}
