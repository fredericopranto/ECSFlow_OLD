using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Cci;
using Microsoft.Cci.ILToCodeModel;
using TourreauGilles.CciExplorer.CodeModel;
using Microsoft.Cci.MutableCodeModel;
using ECSFlowDriverNET;

namespace TourreauGilles.CciExplorer.CSharp
{
    public class CSharpSourceGenerator : SourceVisitor
    {
        private readonly IMetadataReaderHost metadataHost;
        private ECSFlowAssembly eFlowAssembly;
        private const string ItemPropertyName = "Item";
        public IPlatformType PlatformType
        {
            get { return this.metadataHost.PlatformType; }
        }

        public CSharpSourceGenerator(IMetadataReaderHost metadataHost, ICSharpSourceWriter writer)
        {
            this.metadataHost = metadataHost;
            //this.writer = writer;
        }

        public CSharpSourceGenerator(IMetadataReaderHost metadataHost, ECSFlowAssembly eFlowAssembly)
        {
            this.metadataHost = metadataHost;
            this.eFlowAssembly = eFlowAssembly;
        }

        private void GenerateVisibility(ITypeDefinitionMember memberDefinition)
        {
            //this.writer.WriteKeyword(CSharpLanguage.GetVisibility(memberDefinition));
        }

        public static string GetVisibility(ITypeDefinitionMember memberDefinition)
        {
            return CSharpLanguage.GetVisibility(memberDefinition);
        }

        private void GenerateVisibilityAndModifier(IMethodDefinition method)
        {
            if (method.ContainingTypeDefinition.IsInterface == false)
            {
                this.GenerateVisibility(method);

                string modifier;
                modifier = CSharpLanguage.GetModifier(method);

                if (modifier != null)
                {
                    //this.writer.WriteSpace();
                    //this.writer.WriteKeyword(modifier);
                }

                //this.writer.WriteSpace();
            }
        }

        private void GenerateCommaSeparateItems<T>(IEnumerable<T> items, Action<T> visitMethod)
        {
            IList<T> list;

            list = items.ToList();

            if (list.Count > 0)
            {
                visitMethod(list[0]);

                for (int i = 1; i < list.Count(); i++)
                {
                    //this.writer.Write(CSharpLanguage.CommaToken);
                    //this.writer.WriteSpace();

                    visitMethod(list[i]);
                }
            }
        }

        public override void Visit(IAddition addition)
        {
            this.Visit(addition, CSharpLanguage.AdditionToken);
        }

        public override void Visit(IAndOperation andExpression)
        {
            this.Visit(andExpression, CSharpLanguage.AndToken);
        }

        public override void Visit(IArrayIndexer arrayIndexer)
        {
            foreach (IExpression indice in arrayIndexer.Indices)
            {
                //this.writer.Write(CSharpLanguage.LeftSquareBracket);
                this.Visit(indice);
                //this.writer.Write(CSharpLanguage.RightSquareBracket);
            }
        }

        public override void Visit(IArrayTypeReference arrayTypeReference)
        {
            this.Visit(arrayTypeReference.ElementType);
            //this.writer.Write(CSharpLanguage.LeftSquareBracket);
            //this.writer.Write(CSharpLanguage.RightSquareBracket);
        }

        public override void Visit(IAssignment assignment)
        {
            this.Visit(assignment.Target);

            //this.writer.WriteSpace();
            //this.writer.Write(CSharpLanguage.AssignToken);
            //this.writer.WriteSpace();

            this.Visit(assignment.Source);
        }

        public void Visit(IBinaryOperation binaryOperation, string operatorToken)
        {
            //this.writer.WriteLeftParenthesis();

            this.Visit(binaryOperation.LeftOperand);

            //this.writer.WriteSpace();
            //this.writer.Write(operatorToken);
            //this.writer.WriteSpace();

            this.Visit(binaryOperation.RightOperand);

            //this.writer.WriteRightParenthesis();
        }

        public override void Visit(IBitwiseAnd bitwiseAnd)
        {
            this.Visit(bitwiseAnd, CSharpLanguage.BitwiseAndToken);
        }

        public override void Visit(IBitwiseOr bitwiseOr)
        {
            this.Visit(bitwiseOr, CSharpLanguage.BitwiseOrToken);
        }

        public override void Visit(IBlockStatement block)
        {
            //this.writer.OpenBlock();
            base.Visit(block);
            //this.writer.CloseBlock();
        }

        public override void Visit(IBoundExpression boundExpression)
        {
            if (boundExpression.Instance != null)
            {
                this.Visit(boundExpression.Instance);
            }

            ILocalDefinition local = boundExpression.Definition as ILocalDefinition;

            if (local != null)
            {
                this.VisitReference(local);
            }
            else
            {
                IParameterDefinition parameter = boundExpression.Definition as IParameterDefinition;
                if (parameter != null)
                {
                    this.VisitReference(parameter);
                }
                else
                {
                    if (boundExpression.Instance == null)
                    {
                        // Write the static class
                        this.Visit(boundExpression.Type);
                    }

                    //this.writer.Write(CSharpLanguage.PointToken);

                    IFieldReference field = (IFieldReference)boundExpression.Definition;

                    this.Visit(field);
                }
            }
        }

        public override void Visit(IBreakStatement breakStatement)
        {
            //this.writer.WriteKeyword(CSharpLanguage.BreakToken);
            //this.writer.WriteEndInstruction();
        }

        public override void Visit(ICastIfPossible castIfPossible)
        {
            //this.writer.WriteLeftParenthesis();

            this.Visit(castIfPossible.ValueToCast);

            //this.writer.WriteSpace();
            //this.writer.WriteKeyword(CSharpLanguage.AsToken);
            //this.writer.WriteSpace();

            this.Visit(castIfPossible.TargetType);

            //this.writer.WriteRightParenthesis();
        }

        public override void Visit(ICatchClause catchClause)
        {
            //this.writer.WriteKeyword(CSharpLanguage.CatchToken);

            if (catchClause.ExceptionContainer != Dummy.LocalVariable)
            {
                //this.writer.WriteSpace();

                //this.writer.WriteLeftParenthesis();
                this.Visit(catchClause.ExceptionContainer);
                //this.writer.WriteRightParenthesis();
            }

            this.Visit(catchClause.Body);
        }

        public override void Visit(ICheckIfInstance checkIfInstance)
        {
            //this.writer.WriteLeftParenthesis();

            this.Visit(checkIfInstance.Operand);

            //this.writer.WriteSpace();
            //this.writer.WriteKeyword(CSharpLanguage.IsToken);
            //this.writer.WriteSpace();

            this.Visit(checkIfInstance.TypeToCheck);

            //this.writer.WriteRightParenthesis();
        }

        public override void Visit(ICompileTimeConstant constant)
        {
            //TODO: comentado por Frederico
            //this.VisitConstant(constant.Type, constant.Value);
        }

        public override void Visit(IConditional conditional)
        {
            //this.writer.WriteLeftParenthesis();

            // Write the condition
            //this.writer.WriteLeftParenthesis();
            this.Visit(conditional.Condition);
            //this.writer.WriteRightParenthesis();

            // Write the true expression
            //this.writer.WriteSpace();
            //this.writer.Write(CSharpLanguage.QuestionMarkToken);
            //this.writer.WriteSpace();
            //this.writer.WriteLeftParenthesis();
            this.Visit(conditional.ResultIfTrue);
            //this.writer.WriteRightParenthesis();

            // Write the false expression
            //this.writer.WriteSpace();
            //this.writer.Write(CSharpLanguage.ColonToken);
            //this.writer.WriteSpace();
            //this.writer.WriteLeftParenthesis();
            this.Visit(conditional.ResultIfFalse);
            //this.writer.WriteRightParenthesis();

            //this.writer.WriteRightParenthesis();
        }

        public override void Visit(IConditionalStatement conditionalStatement)
        {
            //this.writer.WriteKeyword(CSharpLanguage.IfToken);
            //this.writer.WriteSpace();

            // Write the condition
            if (conditionalStatement.Condition is IBoundExpression)
            {
                //this.writer.WriteLeftParenthesis();
                this.Visit(conditionalStatement.Condition);
                //this.writer.WriteRightParenthesis();
            }
            else
            {
                this.Visit(conditionalStatement.Condition);
            }

            // Write true block
            this.Visit(conditionalStatement.TrueBranch);

            // Write false block if needed
            if (!(conditionalStatement.FalseBranch is IEmptyStatement))
            {
                //this.writer.WriteKeyword(CSharpLanguage.ElseToken);

                this.Visit(conditionalStatement.FalseBranch);
            }
        }

        public override void Visit(IConversion conversion)
        {
            //this.writer.Write(CSharpLanguage.LeftParenthesisToken);
            this.Visit(conversion.TypeAfterConversion);
            //this.writer.Write(CSharpLanguage.RightParenthesisToken);

            this.Visit(conversion.ValueToConvert);
        }

        public override void Visit(ICustomAttribute customAttribute)
        {
            //this.writer.Write(CSharpLanguage.LeftSquareBracket);

            this.Visit(customAttribute.Type);

            //this.writer.WriteLeftParenthesis();
            this.Visit(customAttribute.Arguments);

            if (customAttribute.NumberOfNamedArguments > 0)
            {
                if (customAttribute.Arguments.Count() > 0)
                {
                    // Add a comma
                    //this.writer.Write(CSharpLanguage.CommaToken);
                    //this.writer.WriteSpace();
                }

                this.Visit(customAttribute.NamedArguments);
            }

            //this.writer.WriteRightParenthesis();

            //this.writer.Write(CSharpLanguage.RightSquareBracket);
        }

        public override void Visit(ICreateArray createArray)
        {
            // "new "
            //this.writer.WriteKeyword(CSharpLanguage.NewToken);
            //this.writer.WriteSpace();

            // "type"
            this.Visit(createArray.ElementType);

            // Write each dimension size
            foreach (IExpression size in createArray.Sizes)
            {
                //this.writer.Write(CSharpLanguage.LeftSquareBracket);
                this.Visit(size);
                //this.writer.Write(CSharpLanguage.RightSquareBracket);
            }
        }

        public override void Visit(ICreateObjectInstance createObjectInstance)
        {
            //this.writer.WriteKeyword(CSharpLanguage.NewToken);
            //this.writer.WriteSpace();
            this.Visit(createObjectInstance.Type);

            this.Visit(createObjectInstance.Arguments);
        }

        public override void Visit(IDefaultValue defaultValue)
        {
            if (defaultValue.DefaultValueType.IsValueType == false)
            {
                //this.writer.WriteKeyword(CSharpLanguage.NullToken);
            }
            else
            {
                //this.writer.WriteKeyword(CSharpLanguage.DefaultToken);
                //this.writer.WriteLeftParenthesis();
                this.Visit(defaultValue.DefaultValueType);
                //this.writer.WriteRightParenthesis();
            }
        }

        public override void Visit(IDivision division)
        {
            this.Visit(division, CSharpLanguage.DivisionToken);
        }

        /// <summary>
        /// Visita os Atributos
        /// </summary>
        /// <param name="customAttributes"></param>
        public override void Visit(IEnumerable<ICustomAttribute> customAttributes)
        {
            foreach (ICustomAttribute attribute in customAttributes)
            {
                this.Visit(attribute);
            }
        }

        public override void Visit(IEnumerable<IFieldDefinition> fields)
        {
            foreach (IFieldDefinition field in fields)
            {
                this.Visit(field);
                //this.writer.NewLine();
            }
        }

        public override void Visit(IEnumerable<IEventDefinition> events)
        {
            foreach (IEventDefinition evt in events)
            {
                this.Visit(evt);
                //this.writer.NewLine();
            }
        }

        public override void Visit(IEnumerable<IExpression> expressions)
        {
            IExpression[] arguments;

            //this.writer.Write(CSharpLanguage.LeftParenthesisToken);

            arguments = expressions.ToArray();
            if (arguments.Length > 0)
            {
                this.Visit(arguments[0]);

                for (int i = 1; i < arguments.Length; i++)
                {
                    //this.writer.Write(CSharpLanguage.CommaToken);
                    //this.writer.WriteSpace();

                    this.Visit(arguments[i]);
                }
            }

            //this.writer.Write(CSharpLanguage.RightParenthesisToken);
        }

        public override void Visit(IEnumerable<IGenericMethodParameter> genericParameters)
        {
            IGenericMethodParameter[] temp;

            temp = genericParameters.ToArray();
            if (temp.Length > 0)
            {
                //this.writer.Write(CSharpLanguage.LeftAngleBracket);

                this.GenerateCommaSeparateItems(genericParameters, g => this.Visit(g));

                //this.writer.Write(CSharpLanguage.RightAngleBracket);
            }
        }

        public override void Visit(IEnumerable<IGenericTypeParameter> genericParameters)
        {
            IGenericTypeParameter[] temp;

            temp = genericParameters.ToArray();
            if (temp.Length > 0)
            {
                //this.writer.Write(CSharpLanguage.LeftAngleBracket);

                this.GenerateCommaSeparateItems(genericParameters, g => this.Visit(g));

                //this.writer.Write(CSharpLanguage.RightAngleBracket);
            }
        }

        public override void Visit(IEnumerable<IMetadataExpression> expressions)
        {
            this.GenerateCommaSeparateItems(expressions, e => this.Visit(e));
        }

        public override void Visit(IEnumerable<IMetadataNamedArgument> namedArguments)
        {
            this.GenerateCommaSeparateItems(namedArguments, a => this.Visit(a));
        }

        /// <summary>
        /// Visita os Methods - eFlowMethod
        /// </summary>
        /// <param name="methods"></param>
        public override void Visit(IEnumerable<IMethodDefinition> methods)
        {
            foreach (IMethodDefinition method in methods)
            {
                ECSFlowMethod eFlowMethod = new ECSFlowMethod();
                eFlowMethod.Name = MemberHelper.GetMethodSignature(method, NameFormattingOptions.OmitContainingNamespace | NameFormattingOptions.ReturnType | NameFormattingOptions.Signature);
                eFlowMethod.FullName = method.ToString();
                eFlowMethod.Visibility = GetVisibility(method);
                eFlowMethod.MethodDefinition = method;

                this.eFlowAssembly.Types.Last<ECSFlowType>().Methods.Add(eFlowMethod);

                this.Visit(method);
            }
        }

        public override void Visit(IEnumerable<IParameterDefinition> parameters)
        {
            //this.writer.Write(CSharpLanguage.LeftParenthesisToken);
            GenerateCommaSeparateItems(parameters, p => this.Visit(p));
            //this.writer.Write(CSharpLanguage.RightParenthesisToken);
        }

        public override void Visit(IEnumerable<IPropertyDefinition> properties)
        {
            foreach (IPropertyDefinition property in properties)
            {
                this.Visit(property);
                //this.writer.NewLine();
            }
        }

        public override void Visit(IEnumerable<ISwitchCase> switchCases)
        {
            //this.writer.OpenBlock();
            base.Visit(switchCases);
            //this.writer.CloseBlock();
        }

        public override void Visit(IEquality equality)
        {
            this.Visit(equality, CSharpLanguage.EqualToken);
        }

        public override void Visit(IEventDefinition eventDefinition)
        {
            // Write the visibility and modifiers
            this.GenerateVisibility(eventDefinition);
            //this.writer.WriteSpace();

            // Write the event keyword
            //this.writer.WriteKeyword(CSharpLanguage.EventToken);
            //this.writer.WriteSpace();

            // Write event type
            this.Visit(eventDefinition.Type);
            //this.writer.WriteSpace();

            // Write the event name
            //this.writer.Write(eventDefinition);

            // Write adder and remove block
            //this.writer.OpenBlock();

            //this.writer.WriteKeyword(CSharpLanguage.AddToken);
            this.Visit(eventDefinition.Adder.ResolvedMethod.Body);

            //this.writer.WriteKeyword(CSharpLanguage.RemoveToken);
            this.Visit(eventDefinition.Remover.ResolvedMethod.Body);

            //this.writer.CloseBlock();
        }

        public override void Visit(IExpressionStatement expressionStatement)
        {
            this.Visit(expressionStatement.Expression);

            //this.writer.WriteEndInstruction();
        }

        public override void Visit(IFieldDefinition fieldDefinition)
        {
            this.Visit(fieldDefinition.Attributes);

            // Write the visibility and modifiers
            this.GenerateVisibility(fieldDefinition);

            string modifier;
            modifier = CSharpLanguage.GetModifier(fieldDefinition);

            if (modifier != null)
            {
                //this.writer.WriteSpace();
                //this.writer.WriteKeyword(modifier);
            }

            //this.writer.WriteSpace();

            // Write the type of field
            this.Visit(fieldDefinition.Type);
            //this.writer.WriteSpace();

            // Write the field name
            //this.writer.Write(fieldDefinition);

            //this.writer.WriteEndInstruction();
        }

        public override void Visit(IFieldReference fieldReference)
        {
            //this.writer.WriteReference(fieldReference);
        }

        public override void Visit(ILeftShift leftShift)
        {
            this.Visit(leftShift, CSharpLanguage.LeftShiftToken);
        }

        public override void Visit(ILessThan lessThan)
        {
            this.Visit(lessThan, CSharpLanguage.LessThanToken);
        }

        public override void Visit(ILessThanOrEqual lessThanOrEqual)
        {
            this.Visit(lessThanOrEqual, CSharpLanguage.LessThanOrEqualToken);
        }

        public override void Visit(ILocalDeclarationStatement localDeclarationStatement)
        {
            this.Visit(localDeclarationStatement.LocalVariable);

            if (localDeclarationStatement.InitialValue != null)
            {
                //this.writer.WriteSpace();
                //this.writer.Write(CSharpLanguage.AssignToken);
                //this.writer.WriteSpace();

                this.Visit(localDeclarationStatement.InitialValue);
            }

            //this.writer.WriteEndInstruction();
        }

        public override void Visit(ILocalDefinition localDefinition)
        {
            this.Visit(localDefinition.Type);

            //this.writer.WriteSpace();
            //this.writer.Write(localDefinition);
        }

        public override void Visit(ILogicalNot logicalNot)
        {
            //this.writer.Write(CSharpLanguage.NotToken);

            //this.writer.WriteLeftParenthesis();
            this.Visit(logicalNot.Operand);
            //this.writer.WriteRightParenthesis();
        }

        public override void Visit(IMetadataConstant constant)
        {
            this.VisitConstant(constant.Type, constant.Value);
        }

        public override void Visit(IMetadataCreateArray createArray)
        {
            //this.writer.WriteKeyword(CSharpLanguage.NewToken);
            //this.writer.WriteSpace();

            this.Visit(createArray.ElementType);
            //this.writer.Write(CSharpLanguage.LeftSquareBracket);
            //this.writer.Write(CSharpLanguage.RightSquareBracket);
            //this.writer.WriteSpace();

            //this.writer.Write(CSharpLanguage.LeftCurlyToken);
            this.GenerateCommaSeparateItems(createArray.Initializers, i => this.Visit(i));
            //this.writer.Write(CSharpLanguage.RightCurlyToken);
        }

        public override void Visit(IMetadataExpression expression)
        {
            expression.Dispatch(this);
        }

        public override void Visit(IMetadataNamedArgument namedArgument)
        {
            //this.writer.Write(namedArgument.ArgumentName);

            //this.writer.WriteSpace();
            //this.writer.Write(CSharpLanguage.AssignToken);
            //this.writer.WriteSpace();

            this.Visit(namedArgument.ArgumentValue);
        }

        public override void Visit(IMetadataTypeOf typeOf)
        {
            this.VisitTypeOf(typeOf.TypeToGet);
        }

        /// <summary>
        /// Visita o corpo do Metodo
        /// </summary>
        /// <param name="methodBody"></param>
        public override void Visit(IMethodBody methodBody)
        {
            ISourceMethodBody sourceMethodBody;

            sourceMethodBody = Decompiler.GetCodeModelFromMetadataModel(this.metadataHost, methodBody, null);

            VisitTryCatch(sourceMethodBody.Block);

            // Comentado Frderico - metodo call tratado no program;
            /*if (methodBody.MethodDefinition.IsConstructor == true)
            {
                // Há casos que o primeiro statement de um contrutor não é um MethodCall
                // statement = (IExpressionStatement)sourceMethodBody.Block.Statements.ElementAt(0);
                IEnumerable<IStatement> statements = sourceMethodBody.Block.Statements.ToList().FindAll(
                    (st => st.GetType().Equals(typeof(ExpressionStatement))));

                // Get the first statement
                // The first statement is a constructor call (base() or this() invocation)
                IExpressionStatement statement = (IExpressionStatement)statements.ToList().Find(
                    (st => ((ExpressionStatement)st).Expression.ToString().Equals(typeof(MethodCall).ToString())));

                this.VisitConstructorCall((IMethodCall)statement.Expression);
            }*/

            // Comentado Frderico - Visita todos os statements;
            //this.Visit(sourceMethodBody.Block);
        }

        public override void Visit(IMethodCall methodCall)
        {
            // If the method is a constructor ignore it
            if (methodCall.MethodToCall.ResolvedMethod.IsConstructor == true)
            {
                return;
            }

            if (methodCall.IsStaticCall == false)
            {
                // Non static call, generate instance
                this.Visit(methodCall.ThisArgument);
            }
            else
            {
                // Write the static class
                this.Visit(methodCall.MethodToCall.ContainingType);
            }
            //this.writer.Write(CSharpLanguage.PointToken);

            // Test if the method is a getter or setter
            IPropertyDefinition property;

            property = methodCall.MethodToCall.ResolvedMethod.GetProperty();
            if (property == null)
            {
                // Write parameters
                this.Visit(methodCall.Arguments);
            }
            else
            {
                this.VisitPropertyReference(property, methodCall);
            }
        }

        private void VisitGenericParameters(IEnumerable<IGenericParameter> genericParameters)
        {
            if (genericParameters.Count() > 0)
            {
                //this.writer.Write(CSharpLanguage.LeftAngleBracket);
                this.GenerateCommaSeparateItems(genericParameters, g => this.Visit(g));
                //this.writer.Write(CSharpLanguage.RightAngleBracket);
            }
        }

        public override void Visit(IMethodDefinition method)
        {
            this.Visit(method.Attributes);

            this.GenerateVisibilityAndModifier(method);

            if (method.IsConstructor == false)
            {
                // Write the return type of method
                this.Visit(method.Type);
                //this.writer.WriteSpace();

                // Write the name of the method
                //this.writer.Write(method);

                // Write generic arguments
                this.Visit(method.GenericParameters);
            }
            else
            {
                // Write the name of the type
                INamedTypeReference type;

                type = method.ContainingTypeDefinition as INamedTypeReference;

                //this.writer.Write(type);
            }

            // Write parameters
            this.Visit(method.Parameters);

            // Write generic constraints
            this.VisitGenericConstraints(method.GenericParameters);

            if (method.IsAbstract == true || method.IsExternal == true)
            {
                //this.writer.WriteEndInstruction();
            }
            else
            {
                // Write statements
                this.Visit(method.Body);
            }
        }

        public override void Visit(IModulus modulus)
        {
            this.Visit(modulus, CSharpLanguage.ModulusToken);
        }

        public override void Visit(IMultiplication multiplication)
        {
            this.Visit(multiplication, CSharpLanguage.MultiplicationToken);
        }

        /// <summary>
        /// Visita os Tipos
        /// </summary>
        /// <param name="namespaceTypeDefinition"></param>
        public override void Visit(INamespaceTypeDefinition namespaceTypeDefinition)
        {
            // Envia para analisar o Visit(IEnumerable<ICustomAttribute> customAttributes)
            this.Visit(namespaceTypeDefinition.Attributes);

            // Envia para analisar o VisitNamedTypeDefinition(INamedTypeDefinition namedTypeDefinition)
            this.VisitNamedTypeDefinition((INamedTypeDefinition)namespaceTypeDefinition);
        }

        public override void Visit(INamespaceTypeReference namespaceTypeReference)
        {
            string keywordType;

            keywordType = CSharpLanguage.GetKeywordType(namespaceTypeReference);

            if (keywordType != null)
            {
                //this.writer.WriteReference(namespaceTypeReference, keywordType);
            }
            else
            {
                //this.writer.WriteReference(namespaceTypeReference);
            }
        }

        public override void Visit(INestedTypeDefinition nestedTypeDefinition)
        {
            //this.writer.WriteKeyword(CSharpLanguage.GetVisibility(nestedTypeDefinition));

            this.VisitNamedTypeDefinition(nestedTypeDefinition);

            //this.writer.NewLine();
        }

        public override void Visit(INotEquality notEquality)
        {
            this.Visit(notEquality, CSharpLanguage.NotEqualToken);
        }

        public override void Visit(IGenericParameter genericParameter)
        {
            if (genericParameter.Variance == TypeParameterVariance.Contravariant)
            {
                //this.writer.WriteKeyword(CSharpLanguage.InToken);
                //this.writer.WriteSpace();
            }
            else if (genericParameter.Variance == TypeParameterVariance.Covariant)
            {
                //this.writer.WriteKeyword(CSharpLanguage.OutToken);
                //this.writer.WriteSpace();
            }

            //this.writer.Write(genericParameter);
        }

        public override void Visit(IGenericMethodParameter genericMethodParameter)
        {
            this.Visit((IGenericParameter)genericMethodParameter);
        }

        public override void Visit(IGenericMethodParameterReference genericMethodParameterReference)
        {
            this.Visit(genericMethodParameterReference.ResolvedType);
        }

        public override void Visit(IGenericTypeInstanceReference genericTypeInstanceReference)
        {
            this.Visit(genericTypeInstanceReference.GenericType);

            //this.writer.Write(CSharpLanguage.LeftAngleBracket);
            GenerateCommaSeparateItems(genericTypeInstanceReference.GenericArguments, a => this.Visit(a));
            //this.writer.Write(CSharpLanguage.RightAngleBracket);
        }

        public override void Visit(IGenericTypeParameter genericTypeParameter)
        {
            this.Visit((IGenericParameter)genericTypeParameter);
        }

        public override void Visit(IGenericTypeParameterReference genericTypeParameterReference)
        {
            this.Visit(genericTypeParameterReference.ResolvedType);
        }

        public override void Visit(IGreaterThan greaterThan)
        {
            this.Visit(greaterThan, CSharpLanguage.GreaterThanToken);
        }

        public override void Visit(IGreaterThanOrEqual greaterThanOrEqual)
        {
            this.Visit(greaterThanOrEqual, CSharpLanguage.GreaterThanOrEqualToken);
        }

        public override void Visit(IGotoStatement gotoStatement)
        {
            //this.writer.WriteKeyword(CSharpLanguage.GotoToken);
            //this.writer.WriteSpace();

            //this.writer.Write(gotoStatement.TargetStatement.Label);
            //this.writer.WriteEndInstruction();
        }

        public override void Visit(ILabeledStatement labeledStatement)
        {
            //this.writer.Unindent();

            //this.writer.Write(labeledStatement.Label);
            //this.writer.Write(CSharpLanguage.ColonToken);

            //this.writer.Indent();

            this.Visit(labeledStatement.Statement);
        }

        public override void Visit(IOrOperation orExpression)
        {
            this.Visit(orExpression, CSharpLanguage.OrToken);
        }

        public override void Visit(IParameterDefinition parameterDefinition)
        {
            if (parameterDefinition.IsOut == true)
            {
                //this.writer.WriteKeyword(CSharpLanguage.OutToken);
                //this.writer.WriteSpace();
            }
            else if (parameterDefinition.IsByReference == true)
            {
                //this.writer.WriteKeyword(CSharpLanguage.RefToken);
                //this.writer.WriteSpace();
            }
            else if (parameterDefinition.IsParameterArray == true)
            {
                //this.writer.WriteKeyword(CSharpLanguage.ParamsToken);
                //this.writer.WriteSpace();
            }

            this.Visit(parameterDefinition.Type);

            //this.writer.WriteSpace();
            //this.writer.Write(parameterDefinition);
        }

        public override void Visit(IPropertyDefinition propertyDefinition)
        {
            this.Visit(propertyDefinition.Attributes);

            IMethodDefinition methodReference;

            // Write the visibility and modifiers
            methodReference = propertyDefinition.GetMethodReference();
            this.GenerateVisibilityAndModifier(methodReference);

            // Write the return type of property
            this.Visit(propertyDefinition.Type);
            //this.writer.WriteSpace();

            // Write the property name
            if (propertyDefinition.Name.Value == ItemPropertyName)
            {
                //this.writer.WriteKeyword(CSharpLanguage.ThisToken);

                //this.writer.Write("[");
                this.GenerateCommaSeparateItems(propertyDefinition.GetMethodReference().Parameters, parameter => this.Visit(parameter));
                //this.writer.Write("]");
            }
            else
            {
                //this.writer.Write(propertyDefinition.Name);
            }

            // Write the property block
            //this.writer.OpenBlock();

            // If a getter is specified, write it
            if (propertyDefinition.Getter != null)
            {
                //this.writer.WriteKeyword(CSharpLanguage.GetToken);

                if (propertyDefinition.Getter.ResolvedMethod.IsAbstract == false)
                {
                    this.Visit(propertyDefinition.Getter.ResolvedMethod.Body);
                }
                else
                {
                    //this.writer.WriteEndInstruction();
                }
            }

            if (propertyDefinition.Setter != null)
            {
                //this.writer.WriteKeyword(CSharpLanguage.SetToken);

                if (propertyDefinition.Getter.ResolvedMethod.IsAbstract == false)
                {
                    this.Visit(propertyDefinition.Setter.ResolvedMethod.Body);
                }
                else
                {
                    //this.writer.WriteEndInstruction();
                }
            }

            //this.writer.CloseBlock();
        }

        public override void Visit(IReturnStatement returnStatement)
        {
            if (returnStatement.Expression != null)
            {
                //this.writer.WriteKeyword(CSharpLanguage.ReturnToken);
                //this.writer.WriteSpace();

                base.Visit(returnStatement);

                //this.writer.WriteEndInstruction();
            }
        }

        public override void Visit(IRightShift rightShift)
        {
            this.Visit(rightShift, CSharpLanguage.RightShiftToken);
        }

        public override void Visit(ISubtraction subtraction)
        {
            this.Visit(subtraction, CSharpLanguage.SubtractionToken);
        }

        public override void Visit(ISwitchCase switchCase)
        {
            if (switchCase.IsDefault == true)
            {
                //this.writer.WriteKeyword(CSharpLanguage.DefaultToken);
                //this.writer.WriteSpace();
            }
            else
            {
                //this.writer.WriteKeyword(CSharpLanguage.CaseToken);
                //this.writer.WriteSpace();

                this.Visit(switchCase.Expression);
            }

            //this.writer.Write(CSharpLanguage.ColonToken);

            //this.writer.Indent();
            this.Visit(switchCase.Body);
            //this.writer.Unindent();

            if (switchCase.IsDefault == false)
            {
                //this.writer.NewLine();
            }
        }

        public override void Visit(ISwitchStatement switchStatement)
        {
            //this.writer.WriteKeyword(CSharpLanguage.SwitchToken);
            //this.writer.WriteSpace();

            //this.writer.WriteLeftParenthesis();
            this.Visit(switchStatement.Expression);
            //this.writer.WriteRightParenthesis();

            this.Visit(switchStatement.Cases);
        }

        public override void Visit(ITargetExpression targetExpression)
        {
            if (targetExpression.Instance != null)
            {
                this.Visit(targetExpression.Instance);
            }

            var loc = targetExpression.Definition as ILocalDefinition;
            if (loc != null)
            {
                this.VisitReference(loc);
            }
            else
            {
                var par = targetExpression.Definition as IParameterDefinition;
                if (par != null)
                {
                    this.VisitReference(par);
                }
                else
                {
                    var fieldReference = targetExpression.Definition as IFieldReference;
                    if (fieldReference != null)
                    {
                        //this.writer.Write(CSharpLanguage.PointToken);
                        this.Visit(fieldReference);
                    }
                    else
                    {
                        var indexer = targetExpression.Definition as IArrayIndexer;
                        if (indexer != null)
                        {
                            this.Visit(indexer);
                        }
                        else
                        {
                            var address = targetExpression.Definition as IAddressDereference;
                            if (address != null)
                            {
                                this.Visit(address);
                            }
                            else
                            {
                                var method = targetExpression.Definition as IMethodReference;
                                if (method != null)
                                {
                                    //this.writer.Write(CSharpLanguage.PointToken);
                                    this.Visit(method);
                                }
                                else
                                {
                                    //this.writer.Write(CSharpLanguage.PointToken);
                                    this.VisitReference((IPropertyDefinition)targetExpression.Definition);
                                }
                            }
                        }
                    }
                }
            }
        }

        public override void Visit(ITryCatchFinallyStatement tryCatchFilterFinallyStatement)
        {
            //this.writer.WriteKeyword(CSharpLanguage.TryToken);
            this.Visit(tryCatchFilterFinallyStatement.TryBody);

            foreach (ICatchClause catchClause in tryCatchFilterFinallyStatement.CatchClauses)
            {
                this.Visit(catchClause);
            }

            if (tryCatchFilterFinallyStatement.FinallyBody != null)
            {
                //this.writer.WriteKeyword(CSharpLanguage.FinallyToken);
                this.Visit(tryCatchFilterFinallyStatement.FinallyBody);
            }
        }

        public override void Visit(IThisReference thisReference)
        {
            //this.writer.WriteKeyword(CSharpLanguage.ThisToken);
        }

        public override void Visit(IThrowStatement throwStatement)
        {
            //this.writer.WriteKeyword(CSharpLanguage.ThrowToken);
            //this.writer.WriteSpace();

            this.Visit(throwStatement.Exception);
            //this.writer.WriteEndInstruction();
        }

        /// <summary>
        /// Registra os Types - eFlowType
        /// </summary>
        /// <param name="typeDefinition"></param>
        public override void Visit(ITypeDefinition typeDefinition)
        {
            ECSFlowType eFlowType = new ECSFlowType();
            eFlowType.Name = typeDefinition.GetDisplayName();
            eFlowType.FullName = typeDefinition.ToString();
            this.eFlowAssembly.Types.Add(eFlowType);

            // Envia para analisar o Visit(INamespaceTypeDefinition namespaceTypeDefinition)
            typeDefinition.Dispatch(this);
        }

        public override void Visit(ITypeOf typeOf)
        {
            this.VisitTypeOf(typeOf.TypeToGet);
        }

        public override void Visit(IUnaryNegation unaryNegation)
        {
            this.Visit(unaryNegation.Operand);
            //this.writer.Write(CSharpLanguage.SubtractionToken);
            //this.writer.Write(CSharpLanguage.SubtractionToken);
        }

        public override void Visit(IUnaryPlus unaryPlus)
        {
            this.Visit(unaryPlus.Operand);
            //this.writer.Write(CSharpLanguage.AdditionToken);
            //this.writer.Write(CSharpLanguage.AdditionToken);
        }

        public override void Visit(IDoUntilStatement doUntilStatement)
        {
            //this.writer.WriteKeyword(CSharpLanguage.DoToken);

            this.Visit(doUntilStatement.Body);

            //this.writer.WriteKeyword(CSharpLanguage.WhileToken);
            //this.writer.WriteSpace();
            this.Visit(doUntilStatement.Condition);
        }

        public override void Visit(IWhileDoStatement whileDoStatement)
        {
            //this.writer.WriteKeyword(CSharpLanguage.WhileToken);
            //this.writer.WriteSpace();

            this.Visit(whileDoStatement.Condition);
            this.Visit(whileDoStatement.Body);
        }

        private void VisitConstant(ITypeReference type, object value)
        {
            if (type.TypeCode == PrimitiveTypeCode.String)
            {
                //this.writer.WriteString((string)value);
            }
            else if (type.TypeCode == PrimitiveTypeCode.Boolean)
            {
                bool b;

                b = (bool)value;

                if (b == true)
                {
                    //this.writer.WriteKeyword(CSharpLanguage.TrueToken);
                }
                else
                {
                    //this.writer.WriteKeyword(CSharpLanguage.FalseToken);
                }
            }
            else if (type.TypeCode == PrimitiveTypeCode.Char)
            {
                //this.writer.WriteChar((char)value);
            }
            else if (type == this.PlatformType.SystemObject && value == null)
            {
                //this.writer.WriteKeyword(CSharpLanguage.NullToken);
            }
            else
            {
                //this.writer.Write("{0}", value);
            }
        }

        private void VisitConstructorCall(IMethodCall constructorCall)
        {
            IThisReference thisReference;

            thisReference = (IThisReference)constructorCall.ThisArgument;

            //this.writer.Write(" : ");

            if (thisReference.Type == constructorCall.MethodToCall.ContainingType)
            {
                // Call to other constructor on the same type
                //this.writer.WriteKeyword(CSharpLanguage.ThisToken);
            }
            else
            {
                // Call to base constructor
                //this.writer.WriteKeyword(CSharpLanguage.BaseToken);
            }

            this.Visit(constructorCall.Arguments);
        }

        private void VisitDelegate(INamedTypeDefinition delegateDefinition)
        {
            IMethodDefinition method;

            method = delegateDefinition.Methods.Where(m => m.Name == this.metadataHost.NameTable.Invoke).Single();

            //this.writer.WriteKeyword(CSharpLanguage.DelegateToken);
            //this.writer.WriteSpace();

            this.Visit(method.Type);
            //this.writer.WriteSpace();

            //this.writer.Write(delegateDefinition.Name.Value);
            this.Visit(delegateDefinition.GenericParameters);
            this.Visit(method.Parameters);

            // Write generic constraints
            this.VisitGenericConstraints(delegateDefinition.GenericParameters);

            //this.writer.WriteEndInstruction();
        }

        private void VisitEnum(INamedTypeDefinition enumDefinition)
        {
            //this.writer.WriteKeyword(CSharpLanguage.EnumToken);
            //this.writer.WriteSpace();
            //this.writer.Write(enumDefinition);

            //this.writer.OpenBlock();

            IFieldDefinition[] enumFields;

            enumFields = enumDefinition.GetEnumFieldValues().ToArray();
            if (enumFields.Length > 0)
            {
                this.VisitEnumField(enumFields[0]);

                for (int i = 1; i < enumFields.Length; i++)
                {
                    //this.writer.Write(",");
                    //this.writer.NewLine();

                    this.VisitEnumField(enumFields[i]);
                }
            }

            //this.writer.CloseBlock();
        }

        private void VisitEnumField(IFieldDefinition enumField)
        {
            //this.writer.Write(enumField);

            //this.writer.Write(" = ");
            this.Visit(enumField.CompileTimeValue);
        }

        private void VisitGenericConstraints(IEnumerable<IGenericParameter> genericParameters)
        {
            if (genericParameters.Count() > 0)
            {
                foreach (IGenericParameter genericParameter in genericParameters)
                {
                    if (genericParameter.HasConstraints() == true)
                    {
                        //this.writer.Indent();
                        this.VisitGenericConstraint(genericParameter);
                        //this.writer.Unindent();
                    }
                }
            }
        }

        private void VisitGenericConstraint(IGenericParameter genericParameter)
        {
            bool commaNeeded;

            //this.writer.WriteSpace();
            //this.writer.WriteKeyword(CSharpLanguage.WhereToken);
            //this.writer.WriteSpace();

            //this.writer.Write(genericParameter);
            //this.writer.WriteSpace();
            //this.writer.Write(CSharpLanguage.ColonToken);
            //this.writer.WriteSpace();

            // Write class or struct constaint
            if (genericParameter.MustBeReferenceType == true)
            {
                //this.writer.WriteKeyword(CSharpLanguage.ClassToken);
                commaNeeded = true;
            }
            else if (genericParameter.MustBeValueType == true)
            {
                //this.writer.WriteKeyword(CSharpLanguage.StructToken);
                commaNeeded = true;
            }
            else
            {
                commaNeeded = false;
            }

            // Write base class constraints
            if (genericParameter.Constraints.Count() > 0)
            {
                if (commaNeeded == true)
                {
                    //this.writer.Write(CSharpLanguage.CommaToken);
                    //this.writer.WriteSpace();
                }
                else
                {
                    commaNeeded = true;
                }

                this.GenerateCommaSeparateItems(genericParameter.Constraints, c => this.Visit(c));
            }

            // Write new() constraint
            if (genericParameter.MustHaveDefaultConstructor == true)
            {
                if (commaNeeded == true)
                {
                    //this.writer.Write(CSharpLanguage.CommaToken);
                    //this.writer.WriteSpace();
                }

                //this.writer.WriteKeyword(CSharpLanguage.NewToken);
                //this.writer.Write(CSharpLanguage.LeftParenthesisToken);
                //this.writer.Write(CSharpLanguage.RightParenthesisToken);
            }
        }

        private void VisitNamedTypeDefinition(INamedTypeDefinition namedTypeDefinition)
        {
            // If delegate, call the VisitDelegate and exit the method
            if (namedTypeDefinition.IsDelegate == true)
            {
                this.VisitDelegate(namedTypeDefinition);
                return;
            }

            if (namedTypeDefinition.IsClass == true)
            {
                this.eFlowAssembly.Types.Last<ECSFlowType>().Kind = CSharpLanguage.ClassToken;
            }
            else if (namedTypeDefinition.IsStruct == true)
            {
                this.eFlowAssembly.Types.Last<ECSFlowType>().Kind = CSharpLanguage.StructToken;
            }
            else if (namedTypeDefinition.IsInterface == true)
            {
                this.eFlowAssembly.Types.Last<ECSFlowType>().Kind = CSharpLanguage.InterfaceToken;
            }
            else if (namedTypeDefinition.IsEnum == true)
            {
                this.VisitEnum(namedTypeDefinition);
                return;
            }

            this.Visit(namedTypeDefinition.GenericParameters);

            // Write inherited class + interfaces (ignore Object, ValueType, Enum) class
            ITypeReference[] inheritedTypes;
            inheritedTypes = namedTypeDefinition.BaseClasses
                .Union(namedTypeDefinition.Interfaces)
                .ToArray();

            if (inheritedTypes.Length > 0)
            {
                GenerateCommaSeparateItems(inheritedTypes, t => this.Visit(t));
            }

            // Envia para analisar o Visit(IEnumerable<IMethodDefinition> methods)
            this.Visit(namedTypeDefinition.Members.OfType<INestedTypeDefinition>().OrderBy(f => f.Name.Value));

            // Envia para analisar o Visit(IEnumerable<IMethodDefinition> methods)
            this.Visit(namedTypeDefinition.Members.OfType<IFieldDefinition>().OrderBy(f => f.Name.Value));

            // Envia para analisar o Visit(IEnumerable<IMethodDefinition> methods)
            this.Visit(namedTypeDefinition.Members.OfType<IMethodDefinition>().Where(m => m.IsConstructor == true).OrderBy(m => m.Name.Value));

            // Envia para analisar o Visit(IEnumerable<IMethodDefinition> methods)
            // Analisa os métodos das classes Abstratas
            this.Visit(namedTypeDefinition.Members.OfType<IMethodDefinition>().Where(m => m.IsSpecialName == false).OrderBy(m => m.Name.Value));

            //TODO: comentado por Frederico
            //this.Visit(namedTypeDefinition.Members.OfType<IPropertyDefinition>().OrderBy(p => p.Name.Value));

            this.Visit(namedTypeDefinition.Members.OfType<IEventDefinition>().OrderBy(e => e.Name.Value));
        }

        private void VisitPropertyReference(IPropertyDefinition property, IMethodCall methodCall)
        {
            //this.writer.WriteReference(property);

            // If the method is a setter...
            if (methodCall.MethodToCall.ResolvedMethod.IsSetterMethod() == true)
            {
                // ... write the assign operator
                //this.writer.WriteSpace();
                //this.writer.Write(CSharpLanguage.AssignToken);
                //this.writer.WriteSpace();

                // and write the value
                this.Visit(methodCall.Arguments.ElementAt(0));
            }
        }

        public override void VisitReference(ILocalDefinition local)
        {
            //this.writer.Write(local);
        }

        public override void VisitReference(IParameterDefinition parameter)
        {
            //this.writer.Write(parameter);
        }

        private void VisitTypeOf(ITypeReference typeToGet)
        {
            //this.writer.WriteKeyword(CSharpLanguage.TypeOfToken);

            //this.writer.WriteLeftParenthesis();
            this.Visit(typeToGet);
            //this.writer.WriteRightParenthesis();
        }

        public void VisitTryCatch(IBlockStatement block)
        {
            int qtdTry = 0;
            int qtdCatch = 0;
            int qtdCatchGeneric = 0;
            int qtdCatchSpecialized = 0;
            int qtdThrowTry = 0;
            int qtdThrowCatch = 0;
            int qtdThrowFinally = 0;
            int qtdFinally = 0;

            DecompiledBlock basicBlock;

            basicBlock = block as DecompiledBlock;

            if (basicBlock != null)
            {
                for (int i = 0; i < basicBlock.Statements.Count; i++)
                {
                    // Recupera blocos Try/Cath/Finally
                    ITryCatchFinallyStatement SuperStatement;
                    SuperStatement = basicBlock.Statements[i] as ITryCatchFinallyStatement;

                    if (SuperStatement != null)
                    {
                        if (SuperStatement.FinallyBody != null)
                        {
                            VisitFinallyBody(ref qtdThrowFinally, ref qtdFinally, ref SuperStatement);
                        }

                        if (SuperStatement.TryBody != null)
                        {
                            VisitTryBody(ref qtdTry, ref qtdThrowTry, SuperStatement);
                        }

                        if (SuperStatement.CatchClauses.Count() > 0)
                        {
                            VisitCatchClause(ref qtdCatchGeneric, ref qtdCatchSpecialized, ref qtdThrowCatch, SuperStatement);
                        }


                    }
                }
            }

            ECSFlowMethod method = this.eFlowAssembly.Types.Last<ECSFlowType>().Methods.Last<ECSFlowMethod>();
            method.QtdTry = qtdTry;
            method.QtdCatchGeneric = qtdCatchGeneric;
            method.QtdCatchSpecialized = qtdCatchSpecialized;
            method.QtdFinally = qtdFinally;
            method.QtdThrow = qtdThrowTry + qtdThrowCatch + qtdThrowFinally;

            this.eFlowAssembly.TryCount += method.QtdTry;
            this.eFlowAssembly.CatchGenericCount += method.QtdCatchGeneric;
            this.eFlowAssembly.CatchSpecializedCount += method.QtdCatchSpecialized;
            this.eFlowAssembly.FinallyCount += method.QtdFinally;
            this.eFlowAssembly.ThrowCount += method.QtdThrow;


            //TODO: comentado
            //base.Visit(block);
        }

        private void VisitTryBody(ref int qtdTry, ref int qtdThrowTry, ITryCatchFinallyStatement SuperStatement)
        {
            //Sublist<BasicBlock<Instruction>> blocks = ((Microsoft.Cci.ILToCodeModel.DecompiledBlock)(SuperStatement.TryBody)).ContainedBlocks;

            //foreach (var block in blocks)
            //{
            //    Sublist<Instruction> instructions = block.Instructions;

            //    foreach (var instruction in instructions)
            //    {
            //        IOperation op = instruction.Operation;
            //        if (op.OperationCode == OperationCode.Throw)
            //        {
            //            IMethodReference MethodReference = op.Value as IMethodReference;
            //            IMethodDefinition MethodDefinition = MethodReference.ResolvedMethod;
            //            INamedTypeDefinition NamedTypeDefinition = MethodDefinition.ContainingTypeDefinition as INamedTypeDefinition;

            //            eFlowMethodException eFlowMethodExceptionTry = new eFlowMethodException();
            //            //Type ExceptionTry = Type.GetType(op.Value.newThrow.Exception.Type.ToString());
            //            Type ExceptionTry = Type.GetType("System.Exception");
            //            eFlowAssembly.RegisterException(ExceptionTry.ToString());
            //            eFlowException ExceptionReference = new eFlowException(ExceptionTry);
            //            eFlowMethodExceptionTry.ExceptionReference = ExceptionReference;
            //            eFlowMethodExceptionTry.Kind = "Try";
            //            eFlowMethodExceptionTry.IsGeneric = ExceptionTry.Equals(typeof(Exception)) || ExceptionTry.Equals(typeof(Object));
            //            eFlowMethodExceptionTry.StartOffSet = ((Microsoft.Cci.ILToCodeModel.DecompiledBlock)(SuperStatement.TryBody)).StartOffset.ToString();
            //            eFlowMethodExceptionTry.EndOffSet = ((Microsoft.Cci.ILToCodeModel.DecompiledBlock)(SuperStatement.TryBody)).EndOffset.ToString();
            //            this.eFlowAssembly.Types.Last<ECSFlowType>().Methods.Last<ECSFlowMethod>().MethodExceptions.Add(eFlowMethodExceptionTry);
            //        }
            //    }
            //}

            List<IStatement> ThrowsIntoTry;
            ThrowsIntoTry = SuperStatement.TryBody.Statements.ToList().FindAll
                (st => st.GetType().Equals(typeof(ThrowStatement)));

            List<IStatement> MethodCallsIntoTry = SuperStatement.TryBody.Statements.ToList().FindAll
                (st => st.GetType().Equals(typeof(ExpressionStatement)));

            //if (MethodCallsIntoTry.Count > 0)
            //{
            //    MethodCall m = (MethodCall)((ExpressionStatement)MethodCallsIntoTry[0]).Expression;
            //    Microsoft.Cci.MetadataReader.MethodBody.MethodBody b = (Microsoft.Cci.MetadataReader.MethodBody.MethodBody)m.MethodToCall.ResolvedMethod.Body;

            //    var specializedOperationExceptionInformation = new List<IOperationExceptionInformation>(b.OperationExceptionInformation);
            //    for (int i = 0, n = specializedOperationExceptionInformation.Count; i < n; i++)
            //    {
            //        var unspecializedOperationException = specializedOperationExceptionInformation[i];
            //    }
            //}

            qtdThrowTry += ThrowsIntoTry.Count;
            qtdTry++;

            foreach (ThrowStatement newThrow in ThrowsIntoTry)
            {
                // Captura e registra as exceções dos Trys
                ECSFlowMethodException eFlowMethodExceptionTry = new ECSFlowMethodException();
                Type ExceptionTry = Type.GetType(newThrow.Exception.Type.ToString());
                if (ExceptionTry != null)
                {
                    eFlowAssembly.RegisterException(ExceptionTry.ToString());

                    ECSFlowException ExceptionReference = new ECSFlowException(ExceptionTry);
                    //eFlowMethodExceptionTry.ExceptionReference = ExceptionReference;
                }
                eFlowMethodExceptionTry.Kind = "Try";
                if (ExceptionTry != null)
                    eFlowMethodExceptionTry.IsGeneric = ExceptionTry.Equals(typeof(Exception)) || ExceptionTry.Equals(typeof(Object));
                eFlowMethodExceptionTry.StartOffSet = ((DecompiledBlock)(SuperStatement.TryBody)).StartOffset.ToString();
                eFlowMethodExceptionTry.EndOffSet = ((DecompiledBlock)(SuperStatement.TryBody)).EndOffset.ToString();
                this.eFlowAssembly.Types.Last().Methods.Last().MethodExceptions.Add(eFlowMethodExceptionTry);
            }
        }

        private void VisitCatchClause(ref int qtdCatchGeneric, ref int qtdCatchSpecialized, ref int qtdThrowCatch, ITryCatchFinallyStatement SuperStatement)
        {
            foreach (ICatchClause catchClause in SuperStatement.CatchClauses)
            {
                // Captura e registra as exceções dos Catchs (>> Inicio Catch Log)
                Type ExceptionCatch;
                ECSFlowMethodException eFlowMethodException = new ECSFlowMethodException();
                //"System.Data.DataTable, System.Data, Version=1.0.5000.0, Culture=neutral, PublicKeyToken=b77a5c561934e089"
                //IUnit unit = (((Microsoft.Cci.MetadataReader.ObjectModelImplementation.NamespaceTypeRefReference)(catchClause.ExceptionType)).ContainingUnitNamespace).Unit;
                ExceptionCatch = Type.GetType(String.Concat(catchClause.ExceptionType.ToString()));

                if (ExceptionCatch != null)
                {
                    eFlowAssembly.RegisterException(catchClause.ExceptionType.ToString());

                    ECSFlowException ExceptionReference = new ECSFlowException(ExceptionCatch);
                    //eFlowMethodException.ExceptionReference = ExceptionReference;
                    if (ExceptionCatch.Equals(typeof(Exception)))
                        qtdCatchGeneric++;
                    else
                        qtdCatchSpecialized++;
                }
                eFlowMethodException.Kind = "Catch";
                if (ExceptionCatch != null)
                    eFlowMethodException.IsGeneric = ExceptionCatch.Equals(typeof(Exception)) || ExceptionCatch.Equals(typeof(Object));
                eFlowMethodException.StartOffSet = ((Microsoft.Cci.ILToCodeModel.DecompiledBlock)(catchClause.Body)).StartOffset.ToString();
                eFlowMethodException.EndOffSet = ((Microsoft.Cci.ILToCodeModel.DecompiledBlock)(catchClause.Body)).EndOffset.ToString();

                // Contabiliza Throws dos blocos Catch
                BlockStatement catchBlock;
                catchBlock = (BlockStatement)catchClause.Body;

                List<IStatement> ThrowsIntoCatch = catchBlock.Statements.FindAll(
                    delegate (IStatement statement)
                    {
                        return statement.GetType().Equals(typeof(ThrowStatement));
                    });

                // Verifica se será preenchido a tag <string> quando há um Throw dentro do Catch
                var HasThrowsIntoCatch = ThrowsIntoCatch.Count != 0;
                if (!HasThrowsIntoCatch)
                {
                    this.eFlowAssembly.Types.Last<ECSFlowType>().Methods.Last<ECSFlowMethod>().MethodExceptions.Add(eFlowMethodException);
                }

                foreach (ThrowStatement newThrow in ThrowsIntoCatch)
                {
                    // Captura e registra as exceções dos Throw dentro dos Catchs 
                    ECSFlowMethodException eFlowMethodExceptionThrowIntoCatch = new ECSFlowMethodException();
                    Type ExceptionCatchIntoCatch = Type.GetType(newThrow.Exception.Type.ToString());
                    if (ExceptionCatchIntoCatch != null)
                    {
                        eFlowAssembly.RegisterException(ExceptionCatchIntoCatch.ToString());
                        eFlowAttributeReference ExceptionReferenceThrowIntoCatch = new eFlowAttributeReference();
                        ExceptionReferenceThrowIntoCatch.Reference = 0;

                        ECSFlowException ExceptionReference = new ECSFlowException(ExceptionCatch);
                        //eFlowMethodExceptionThrowIntoCatch.ExceptionReference = ExceptionReference;
                    }
                    eFlowMethodExceptionThrowIntoCatch.Kind = "ThrowsIntoCatch";
                    eFlowMethodExceptionThrowIntoCatch.IsGeneric = ExceptionCatch.Equals(typeof(Exception));
                    eFlowMethodExceptionThrowIntoCatch.StartOffSet = ((Microsoft.Cci.ILToCodeModel.DecompiledBlock)(catchClause.Body)).StartOffset.ToString();
                    eFlowMethodExceptionThrowIntoCatch.EndOffSet = ((Microsoft.Cci.ILToCodeModel.DecompiledBlock)(catchClause.Body)).EndOffset.ToString();
                    this.eFlowAssembly.Types.Last<ECSFlowType>().Methods.Last<ECSFlowMethod>().MethodExceptions.Add(eFlowMethodExceptionThrowIntoCatch);

                    // Adicionando a descrição do Throw do Catch (>> Finalização Catch Log)
                    // TODO: verificar se há como retornar mais de um throw dentro de um statement
                    eFlowThrowsIntoCatch eFlowThrowsIntoCatch = new eFlowThrowsIntoCatch();
                    eFlowThrowsIntoCatch.String = newThrow.Exception.Type.ToString();
                    //eFlowMethodException.ThrowsIntoCatch = eFlowThrowsIntoCatch;
                    this.eFlowAssembly.Types.Last<ECSFlowType>().Methods.Last<ECSFlowMethod>().MethodExceptions.Add(eFlowMethodException);
                }

                qtdThrowCatch += ThrowsIntoCatch.Count;
            }
        }

        private void VisitFinallyBody(ref int qtdThrowFinally, ref int qtdFinally, ref ITryCatchFinallyStatement SuperStatement)
        {
            List<IStatement> ThrowsIntoFinally;
            ThrowsIntoFinally = SuperStatement.FinallyBody.Statements.ToList().FindAll
                (st => st.GetType().Equals(typeof(ThrowStatement)));

            qtdThrowFinally += ThrowsIntoFinally.Count;
            qtdFinally++;

            foreach (ThrowStatement newThrow in ThrowsIntoFinally)
            {
                // Captura e registra as exceções dos Catchs (>> Inicio Catch Log)
                ECSFlowMethodException eFlowMethodExceptionFinally = new ECSFlowMethodException();
                Type ExceptionFinally = Type.GetType(newThrow.Exception.Type.ToString());
                if (ExceptionFinally != null)
                {
                    eFlowAssembly.RegisterException(ExceptionFinally.ToString());
                    ECSFlowException ExceptionReference = new ECSFlowException(ExceptionFinally);
                    //eFlowMethodExceptionFinally.ExceptionReference = ExceptionReference;
                }
                eFlowMethodExceptionFinally.Kind = "Finally";
                if (ExceptionFinally != null)
                    eFlowMethodExceptionFinally.IsGeneric = ExceptionFinally.Equals(typeof(Exception)) || ExceptionFinally.Equals(typeof(Object));

                eFlowMethodExceptionFinally.StartOffSet = ((Microsoft.Cci.ILToCodeModel.DecompiledBlock)(SuperStatement.FinallyBody)).StartOffset.ToString();
                eFlowMethodExceptionFinally.EndOffSet = ((Microsoft.Cci.ILToCodeModel.DecompiledBlock)(SuperStatement.FinallyBody)).EndOffset.ToString();
                this.eFlowAssembly.Types.Last<ECSFlowType>().Methods.Last<ECSFlowMethod>().MethodExceptions.Add(eFlowMethodExceptionFinally);
            }

            // TODO: Nem sempre há statements tryCatchFilterFinallyStatement. Melhorar o código.
            // TODO: verificar melhor este caso de quando há um finally ter de descobrir o novo bloco
            // Redescobrir um novo bloco TryCatchFinallyStatement quando temos um finally na classe
            try
            {
                IEnumerable<IStatement> statements = SuperStatement.TryBody.Statements.ToList().FindAll((st => st.GetType().Equals(typeof(TryCatchFinallyStatement))));
                if (statements.ToList().Count > 0)
                    SuperStatement = (ITryCatchFinallyStatement)statements.ToList()[0];
            }
            catch
            { }
        }
    }
}