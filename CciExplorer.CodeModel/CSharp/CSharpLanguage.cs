using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Cci;

namespace TourreauGilles.CciExplorer.CSharp
{
    internal static class CSharpLanguage
    {
        public const string ClassToken = "class";
        public const string StructToken = "struct";
        public const string InterfaceToken = "interface";
        public const string EnumToken = "enum";
        public const string DelegateToken = "delegate";

        public const string PublicToken = "public";
        public const string PrivateToken = "private";
        public const string ProtectedToken = "protected";
        public const string InternalToken = "internal";

        public const string VirtualToken = "virtual";
        public const string NewToken = "new";
        public const string AbstractToken = "abstract";
        public const string OverrideToken = "override";
        public const string StaticToken = "static";
        public const string ExternToken = "extern";
        public const string ReadOnlyToken = "readonly";

        public const string EventToken = "event";
        public const string AddToken = "add";
        public const string RemoveToken = "remove";

        public const string AdditionToken = "+";
        public const string SubtractionToken = "-";
        public const string DivisionToken = "/";
        public const string MultiplicationToken = "*";
        public const string ModulusToken = "%";

        public const string LessThanToken = "<";
        public const string LessThanOrEqualToken = "<=";
        public const string GreaterThanToken = ">";
        public const string GreaterThanOrEqualToken = ">=";

        public const string LeftShiftToken = "<<";
        public const string RightShiftToken = ">>";

        public const string AndToken = "&&";
        public const string OrToken = "||";

        public const string BitwiseAndToken = "&";
        public const string BitwiseOrToken = "|";

        public const string GetToken = "get";
        public const string SetToken = "set";

        public const string AsToken = "as";
        public const string IsToken = "is";

        public const string GotoToken = "goto";

        public const string VoidToken = "void";
        public const string RefToken = "ref";
        public const string ParamsToken = "params";

        public const string InToken = "in";
        public const string OutToken = "out";

        public const string TypeOfToken = "typeof";

        public const string LeftParenthesisToken = "(";
        public const string RightParenthesisToken = ")";
        
        public const string LeftCurlyToken = "{";
        public const string RightCurlyToken = "}";

        public const string LeftSquareBracket = "[";
        public const string RightSquareBracket = "]";

        public const string LeftAngleBracket = "<";
        public const string RightAngleBracket = ">";

        public const string AssignToken = "=";
        public const string EqualToken = "==";
        public const string NotEqualToken = "!=";
        public const string NotToken = "!";

        public const string PointToken = ".";
        public const string CommaToken = ",";

        public const string ReturnToken = "return";
        public const string ThrowToken = "throw";

        public const string WhereToken = "where";
        
        public const string ThisToken = "this";
        public const string BaseToken = "base";

        public const string TrueToken = "true";
        public const string FalseToken = "false";

        public const string SemiColonToken = ";";
        public const string QuestionMarkToken = "?";
        public const string ColonToken = ":";

        public const string NullToken = "null";
        public const string DefaultToken = "default";

        public const string IfToken = "if";
        public const string ElseToken = "else";

        public const string WhileToken = "while";
        public const string DoToken = "do";

        public const string SwitchToken = "switch";
        public const string CaseToken = "case";
        public const string BreakToken = "break";

        public const string TryToken = "try";
        public const string CatchToken = "catch";
        public const string FinallyToken = "finally";

        public const string BoolToken = "bool";
        public const string ByteToken = "byte";
        public const string CharToken = "char";
        public const string DecimalToken = "decimal";
        public const string DoubleToken = "double";
        public const string FloatToken = "float";
        public const string IntToken = "int";
        public const string LongToken = "long";
        public const string ObjectToken = "object";
        public const string ShortToken = "short";
        public const string StringToken = "string";
        public const string UByteToken = "ubyte";
        public const string UIntToken = "uint";
        public const string UShortToken = "ushort";
        public const string ULongToken = "ulong";

        public static string GetVisibility(ITypeDefinitionMember member)
        {
            switch(member.Visibility)
            {
                case TypeMemberVisibility.Public:
                    return PublicToken;

                case TypeMemberVisibility.Private:
                    return PrivateToken;

                case TypeMemberVisibility.FamilyAndAssembly:
                case TypeMemberVisibility.FamilyOrAssembly:
                    return ProtectedToken + " " + InternalToken;

                case TypeMemberVisibility.Family:
                    return ProtectedToken;

                case TypeMemberVisibility.Assembly:
                    return InternalToken;

                default:
                    throw new NotSupportedException();
            }
        }

        public static string GetModifier(IFieldDefinition fieldDefinition)
        {
            if (fieldDefinition.IsReadOnly == true)
            {
                if (fieldDefinition.IsStatic == true)
                {
                    return ReadOnlyToken + " " + StaticToken;
                }
                else
                {
                    return ReadOnlyToken;
                }
            }

            if (fieldDefinition.IsStatic == true)
            {
                return StaticToken;
            }

            return null;
        }

        public static string GetModifier(IMethodDefinition method)
        {
            if (method.IsAbstract == true)
            {
                return AbstractToken;
            }

            if (method.IsVirtual == true)
            {
                if (method.IsNewSlot == true)
                {
                    return VirtualToken;
                }
                else
                {
                    return OverrideToken;
                }
            }

            if (method.IsStatic == true)
            {
                if (method.IsExternal == true)
                {
                    return StaticToken + " " + ExternToken;
                }
                else
                {
                    return StaticToken;
                }
            }

            return null;
        }

        public static string GetKeywordType(ITypeReference typeReference)
        {
            if (typeReference.InternedKey == typeReference.PlatformType.SystemObject.InternedKey)
            {
                return ObjectToken;
            }

            switch(typeReference.TypeCode)
            {
                case PrimitiveTypeCode.Boolean:
                    return BoolToken;

                case PrimitiveTypeCode.Char:
                    return CharToken;

                case PrimitiveTypeCode.Float32:
                    return FloatToken;

                case PrimitiveTypeCode.Float64:
                    return DoubleToken;

                case PrimitiveTypeCode.Int16:
                    return ShortToken;

                case PrimitiveTypeCode.Int32:
                    return IntToken;

                case PrimitiveTypeCode.Int64:
                    return LongToken;

                case PrimitiveTypeCode.Int8:
                    return ByteToken;

                case PrimitiveTypeCode.String:
                    return StringToken;

                case PrimitiveTypeCode.UInt16:
                    return UShortToken;

                case PrimitiveTypeCode.UInt32:
                    return UIntToken;

                case PrimitiveTypeCode.UInt64:
                    return ULongToken;

                case PrimitiveTypeCode.UInt8:
                    return UByteToken;

                case PrimitiveTypeCode.Void:
                    return VoidToken;

                default:
                    return null;
            }
        }
    }
}
