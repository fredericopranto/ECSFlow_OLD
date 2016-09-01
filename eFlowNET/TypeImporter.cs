using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Collections.Generic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;

namespace ECSFlow.Fody
{
	class TypeImporter
	{
		readonly ModuleDefinition source;
		readonly ModuleDefinition target;
		readonly Hashtable allowedDuplicateTypes = new Hashtable();

		readonly List<string> allowedDuplicateNameSpaces = new List<string>();

		readonly Dictionary<AssemblyDefinition, int> aspOffsets = new Dictionary<AssemblyDefinition, int>();
		readonly PlatformFixer platformFixer;

		readonly ReflectionHelper reflectionHelper;

		readonly MappingHandler mappingHandler = new MappingHandler();

		readonly IKVMLineIndexer lineIndexer;

		StreamWriter logFile;

		public TypeImporter( ModuleDefinition source, ModuleDefinition target )
		{
			this.source = source;
			this.target = target;
			platformFixer = new PlatformFixer( source.Runtime );
			reflectionHelper = new ReflectionHelper( this );

			lineIndexer = new IKVMLineIndexer( this );
		}

		public TypeDefinition Import( TypeDefinition type, Mono.Collections.Generic.Collection<TypeDefinition> items = null, bool internalize = false )
		{
			var types = items ?? target.Types;
			TypeDefinition nt = target.GetType( type.FullName );
			bool justCreatedType = false;
			if ( nt == null )
			{
				nt = CreateType( type, types, internalize, null );
				justCreatedType = true;
			}
			else if ( DuplicateTypeAllowed( type ) )
			{
				INFO( "Merging " + type );
			}
			else if ( !type.IsPublic || internalize )
			{
				// rename the type previously imported.
				// renaming the new one before import made Cecil throw an exception.
				string other = "<" + Guid.NewGuid() + ">" + nt.Name;
				INFO( "Renaming " + nt.FullName + " into " + other );
				nt.Name = other;
				nt = CreateType( type, types, internalize, null );
				justCreatedType = true;
			}
			else if ( UnionMerge )
			{
				INFO( "Merging " + type );
			}
			else
			{
				ERROR( "Duplicate type " + type );
				throw new InvalidOperationException( "Duplicate type " + type + " from " + type.Scope + ", was also present in " + mappingHandler.GetOrigTypeModule( nt ) );
			}
			mappingHandler.StoreRemappedType( type, nt );

			// nested types first (are never internalized)
			foreach ( TypeDefinition nested in type.NestedTypes )
			{
				Import( nested, nt.NestedTypes );
			}
			foreach ( FieldDefinition field in type.Fields )
			{
				CloneTo( field, nt );
			}

			// methods before fields / events
			foreach ( MethodDefinition meth in type.Methods )
			{
				CloneTo( meth, nt, justCreatedType );
			}

			foreach ( EventDefinition evt in type.Events )
			{
				CloneTo( evt, nt, nt.Events );
			}
			foreach ( PropertyDefinition prop in type.Properties )
			{
				CloneTo( prop, nt, nt.Properties );
			}
			return nt;
		}

		public bool UnionMerge { get; set; }

		public bool Log { get; set; }

		public bool LogVerbose { get; set; }

		bool DuplicateTypeAllowed( TypeDefinition type )
		{
			string fullName = type.FullName;
			// Merging module because IKVM uses this class to store some fields.
			// Doesn't fully work yet, as IKVM is nice enough to give all the fields the same name...
			if ( fullName == "<Module>" || fullName == "__<Proxy>" )
			{
				return true;
			}

			// XAML helper class, identical in all assemblies, unused within the assembly, and instanciated through reflection from the outside
			// We could just skip them after the first one, but merging them works just fine
			if ( fullName == "XamlGeneratedNamespace.GeneratedInternalTypeHelper" )
			{
				return true;
			}

			// Merge should be OK since member's names are pretty unique,
			// but renaming duplicate members would be safer...
			if ( fullName == "<PrivateImplementationDetails>" && type.IsPublic )
			{
				return true;
			}

			if ( allowedDuplicateTypes.Contains( fullName ) )
			{
				return true;
			}

			var top = type;
			while ( top.IsNested )
			{
				top = top.DeclaringType;
			}
			string nameSpace = top.Namespace;
			if ( !String.IsNullOrEmpty( nameSpace ) && allowedDuplicateNameSpaces.Any( s => s == nameSpace || nameSpace.StartsWith( s + "." ) ) )
			{
				return true;
			}

			return false;
		}

		static IList<ParameterDefinition> ExtractIndexerParameters( PropertyDefinition prop )
		{
			if ( prop.GetMethod != null )
			{
				return prop.GetMethod.Parameters;
			}
			if ( prop.SetMethod != null )
			{
				return prop.SetMethod.Parameters.ToList().GetRange( 0, prop.SetMethod.Parameters.Count - 1 );
			}
			return null;
		}

		static bool IsIndexer( PropertyDefinition prop )
		{
			if ( prop.Name != "Item" )
			{
				return false;
			}
			var parameters = ExtractIndexerParameters( prop );
			return parameters != null && parameters.Count > 0;
		}

		void CloneTo( PropertyDefinition prop, TypeDefinition nt, Mono.Collections.Generic.Collection<PropertyDefinition> col )
		{
			// ignore duplicate property
			var others = nt.Properties.Where( x => x.Name == prop.Name ).ToList();
			if ( others.Any() )
			{
				bool skip = false;
				if ( !IsIndexer( prop ) || !IsIndexer( others.First() ) )
				{
					skip = true;
				}
				else
				{
					// "Item" property is used to implement Indexer operators
					// It may be specified more than one, with extra arguments to get/set methods
					// Note than one may also define a standard "Item" property, in which case he won't be able to define Indexers

					// Here we try to prevent duplicate indexers, but allow to merge non-duplicated ones (e.g. this[int] & this[string] )
					var args = ExtractIndexerParameters( prop );
					if ( others.Any( x => reflectionHelper.AreSame( args, ExtractIndexerParameters( x ) ) ) )
					{
						skip = true;
					}
				}
				if ( skip )
				{
					IGNOREDUP( "property", prop );
					return;
				}
			}

			PropertyDefinition pd = new PropertyDefinition( prop.Name, prop.Attributes, Import( prop.PropertyType, nt ) );
			col.Add( pd );
			if ( prop.SetMethod != null )
			{
				pd.SetMethod = FindMethodInNewType( nt, prop.SetMethod );
			}
			if ( prop.GetMethod != null )
			{
				pd.GetMethod = FindMethodInNewType( nt, prop.GetMethod );
			}
			if ( prop.HasOtherMethods )
			{
				foreach ( MethodDefinition meth in prop.OtherMethods )
				{
					var nm = FindMethodInNewType( nt, meth );
					if ( nm != null )
					{
						pd.OtherMethods.Add( nm );
					}
				}
			}

			CopyCustomAttributes( prop.CustomAttributes, pd.CustomAttributes, nt );
		}

		void CloneTo( EventDefinition evt, TypeDefinition nt, Mono.Collections.Generic.Collection<EventDefinition> col )
		{
			// ignore duplicate event
			if ( nt.Events.Any( x => x.Name == evt.Name ) )
			{
				IGNOREDUP( "event", evt );
				return;
			}

			EventDefinition ed = new EventDefinition( evt.Name, evt.Attributes, Import( evt.EventType, nt ) );
			col.Add( ed );
			if ( evt.AddMethod != null )
			{
				ed.AddMethod = FindMethodInNewType( nt, evt.AddMethod );
			}
			if ( evt.RemoveMethod != null )
			{
				ed.RemoveMethod = FindMethodInNewType( nt, evt.RemoveMethod );
			}
			if ( evt.InvokeMethod != null )
			{
				ed.InvokeMethod = FindMethodInNewType( nt, evt.InvokeMethod );
			}
			if ( evt.HasOtherMethods )
			{
				foreach ( MethodDefinition meth in evt.OtherMethods )
				{
					var nm = FindMethodInNewType( nt, meth );
					if ( nm != null )
					{
						ed.OtherMethods.Add( nm );
					}
				}
			}

			CopyCustomAttributes( evt.CustomAttributes, ed.CustomAttributes, nt );
		}

		MethodDefinition FindMethodInNewType( TypeDefinition nt, MethodDefinition methodDefinition )
		{
			var ret = reflectionHelper.FindMethodDefinitionInType( nt, methodDefinition );
			if ( ret == null )
			{
				WARN( "Method '" + methodDefinition.FullName + "' not found in merged type '" + nt.FullName + "'" );
			}
			return ret;
		}

		internal void IGNOREDUP( string ignoredType, object ignoredObject )
		{
			// TODO: put on a list and log a summary
			//INFO("Ignoring duplicate " + ignoredType + " " + ignoredObject);
		}

		void CloneTo( FieldDefinition field, TypeDefinition nt )
		{
			if ( nt.Fields.Any( x => x.Name == field.Name ) )
			{
				IGNOREDUP( "field", field );
				return;
			}
			FieldDefinition nf = new FieldDefinition( field.Name, field.Attributes, Import( field.FieldType, nt ) );
			nt.Fields.Add( nf );

			if ( field.HasConstant )
			{
				nf.Constant = field.Constant;
			}

			if ( field.HasMarshalInfo )
			{
				nf.MarshalInfo = field.MarshalInfo;
			}

			if ( field.InitialValue != null && field.InitialValue.Length > 0 )
			{
				nf.InitialValue = field.InitialValue;
			}

			if ( field.HasLayoutInfo )
			{
				nf.Offset = field.Offset;
			}

			CopyCustomAttributes( field.CustomAttributes, nf.CustomAttributes, nt );
		}

		TypeReference Import( TypeReference reference, IGenericParameterProvider context )
		{
			TypeDefinition type = GetMergedTypeFromTypeRef( reference );
			if ( type != null )
			{
				return type;
			}

			reference = platformFixer.FixPlatformVersion( reference );
			try
			{
				return context == null ? target.Import( reference ) : target.Import( reference, context );
			}
			catch ( ArgumentOutOfRangeException ) // working around a bug in Cecil
			{
				ERROR( "Problem adding reference: " + reference.FullName );
				throw;
			}
		}

		/// <summary>
		/// Clones a parameter into a newly created method
		/// </summary>
		void CloneTo( ParameterDefinition param, MethodDefinition context, Mono.Collections.Generic.Collection<ParameterDefinition> col )
		{
			ParameterDefinition pd = new ParameterDefinition( param.Name, param.Attributes, Import( param.ParameterType, context ) );
			if ( param.HasConstant )
			{
				pd.Constant = param.Constant;
			}
			if ( param.HasMarshalInfo )
			{
				pd.MarshalInfo = param.MarshalInfo;
			}
			if ( param.HasCustomAttributes )
			{
				CopyCustomAttributes( param.CustomAttributes, pd.CustomAttributes, context );
			}
			col.Add( pd );
		}

		void CloneTo( MethodDefinition meth, TypeDefinition type, bool typeJustCreated )
		{
			// ignore duplicate method for merged duplicated types
			if ( !typeJustCreated &&
				 type.Methods.Count > 0 &&
				 type.Methods.Any( x =>
					 ( x.Name == meth.Name ) &&
					 ( x.Parameters.Count == meth.Parameters.Count ) &&
					 ( x.ToString() == meth.ToString() ) ) ) // TODO: better/faster comparation of parameter types?
			{
				IGNOREDUP( "method", meth );
				return;
			}
			// use void placeholder as we'll do the return type import later on (after generic parameters)
			MethodDefinition nm = new MethodDefinition( meth.Name, meth.Attributes, target.TypeSystem.Void );
			nm.ImplAttributes = meth.ImplAttributes;

			type.Methods.Add( nm );

			CopyGenericParameters( meth.GenericParameters, nm.GenericParameters, nm );

			if ( meth.HasPInvokeInfo )
			{
				if ( meth.PInvokeInfo == null )
				{
					// Even if this was allowed, I'm not sure it'd work out
					//nm.RVA = meth.RVA;
				}
				else
				{
					nm.PInvokeInfo = new PInvokeInfo( meth.PInvokeInfo.Attributes, meth.PInvokeInfo.EntryPoint, meth.PInvokeInfo.Module );
				}
			}

			foreach ( ParameterDefinition param in meth.Parameters )
			{
				CloneTo( param, nm, nm.Parameters );
			}

			foreach ( MethodReference ov in meth.Overrides )
			{
				nm.Overrides.Add( Import( ov, nm ) );
			}

			CopySecurityDeclarations( meth.SecurityDeclarations, nm.SecurityDeclarations, nm );
			CopyCustomAttributes( meth.CustomAttributes, nm.CustomAttributes, nm );

			nm.ReturnType = Import( meth.ReturnType, nm );
			nm.MethodReturnType.Attributes = meth.MethodReturnType.Attributes;
			if ( meth.MethodReturnType.HasConstant )
			{
				nm.MethodReturnType.Constant = meth.MethodReturnType.Constant;
			}
			if ( meth.MethodReturnType.HasMarshalInfo )
			{
				nm.MethodReturnType.MarshalInfo = meth.MethodReturnType.MarshalInfo;
			}
			if ( meth.MethodReturnType.HasCustomAttributes )
			{
				CopyCustomAttributes( meth.MethodReturnType.CustomAttributes, nm.MethodReturnType.CustomAttributes, nm );
			}

			if ( meth.HasBody )
			{
				CloneTo( meth.Body, nm );
			}
			meth.Body = null; // frees memory

			nm.IsAddOn = meth.IsAddOn;
			nm.IsRemoveOn = meth.IsRemoveOn;
			nm.IsGetter = meth.IsGetter;
			nm.IsSetter = meth.IsSetter;
			nm.CallingConvention = meth.CallingConvention;
		}

		private void CopySecurityDeclarations( Mono.Collections.Generic.Collection<SecurityDeclaration> input, Mono.Collections.Generic.Collection<SecurityDeclaration> output, IGenericParameterProvider context)
		{
			foreach (SecurityDeclaration sec in input)
			{
				SecurityDeclaration newSec = null;
				if (PermissionsetHelper.IsXmlPermissionSet(sec))
				{
					newSec = PermissionsetHelper.Xml2PermissionSet(sec, target);
				}
				if (newSec == null)
				{
					newSec = new SecurityDeclaration(sec.Action);
					foreach (SecurityAttribute sa in sec.SecurityAttributes)
					{
						SecurityAttribute newSa = new SecurityAttribute(Import(sa.AttributeType, context));
						if (sa.HasFields)
						{
							foreach (CustomAttributeNamedArgument cana in sa.Fields)
							{
								newSa.Fields.Add(Copy(cana, context));
							}
						}
						if (sa.HasProperties)
						{
							foreach (CustomAttributeNamedArgument cana in sa.Properties)
							{
								newSa.Properties.Add(Copy(cana, context));
							}
						}
						newSec.SecurityAttributes.Add(newSa);
					}
				}
				output.Add(newSec);
			}
		}

		private CustomAttributeNamedArgument Copy(CustomAttributeNamedArgument namedArg, IGenericParameterProvider context)
		{
			return new CustomAttributeNamedArgument(namedArg.Name, Copy(namedArg.Argument, context));
		}

		private CustomAttributeArgument Copy(CustomAttributeArgument arg, IGenericParameterProvider context)
		{
			return new CustomAttributeArgument(Import(arg.Type, context), ImportCustomAttributeValue(arg.Value, context));
		}

		private object ImportCustomAttributeValue(object obj, IGenericParameterProvider context)
		{
			if (obj is TypeReference)
				return Import((TypeReference)obj, context);
			if (obj is CustomAttributeArgument)
				return Copy((CustomAttributeArgument)obj, context);
			if (obj is CustomAttributeArgument[])
				return ((CustomAttributeArgument[])obj).Select(a => Copy(a, context)).ToArray();
			return obj;
		}

		private MethodReference Import(MethodReference reference, IGenericParameterProvider context)
		{
			// If this is a Method/TypeDefinition, it will be corrected to a definition again later

			MethodReference importReference = platformFixer.FixPlatformVersion(reference);

			return target.Import(importReference, context);

		}

		void CloneTo( Mono.Cecil.Cil.MethodBody body, MethodDefinition parent )
		{
			Mono.Cecil.Cil.MethodBody nb = new Mono.Cecil.Cil.MethodBody( parent );
			parent.Body = nb;

			nb.MaxStackSize = body.MaxStackSize;
			nb.InitLocals = body.InitLocals;
			nb.LocalVarToken = body.LocalVarToken;

			foreach ( VariableDefinition var in body.Variables )
			{
				nb.Variables.Add( new VariableDefinition( var.Name,
					Import( var.VariableType, parent ) ) );
			}

			// nb.Instructions.SetCapacity(body.Instructions.Count);
			lineIndexer.PreMethodBodyRepack( body, parent );
			foreach ( Instruction instr in body.Instructions )
			{
				lineIndexer.ProcessMethodBodyInstruction( instr );

				Instruction ni;

				if ( instr.OpCode.Code == Code.Calli )
				{
					var call_site = (Mono.Cecil.CallSite)instr.Operand;
					Mono.Cecil.CallSite ncs = new Mono.Cecil.CallSite( Import( call_site.ReturnType, parent ) )
					{
						HasThis = call_site.HasThis,
						ExplicitThis = call_site.ExplicitThis,
						CallingConvention = call_site.CallingConvention
					};
					foreach ( ParameterDefinition param in call_site.Parameters )
					{
						CloneTo( param, parent, ncs.Parameters );
					}
					ni = Instruction.Create( instr.OpCode, ncs );
				}
				else
				{
					switch ( instr.OpCode.OperandType )
					{
						case OperandType.InlineArg:
						case OperandType.ShortInlineArg:
							if ( instr.Operand == body.ThisParameter )
							{
								ni = Instruction.Create( instr.OpCode, nb.ThisParameter );
							}
							else
							{
								int param = body.Method.Parameters.IndexOf( (ParameterDefinition)instr.Operand );
								ni = Instruction.Create( instr.OpCode, parent.Parameters[param] );
							}
							break;
						case OperandType.InlineVar:
						case OperandType.ShortInlineVar:
							int var = body.Variables.IndexOf( (VariableDefinition)instr.Operand );
							ni = Instruction.Create( instr.OpCode, nb.Variables[var] );
							break;
						case OperandType.InlineField:
							ni = Instruction.Create( instr.OpCode, Import( (FieldReference)instr.Operand, parent ) );
							break;
						case OperandType.InlineMethod:
							ni = Instruction.Create( instr.OpCode, Import( (MethodReference)instr.Operand, parent ) );
							FixAspNetOffset( nb.Instructions, (MethodReference)instr.Operand, parent );
							break;
						case OperandType.InlineType:
							ni = Instruction.Create( instr.OpCode, Import( (TypeReference)instr.Operand, parent ) );
							break;
						case OperandType.InlineTok:
							if ( instr.Operand is TypeReference )
							{
								ni = Instruction.Create( instr.OpCode, Import( (TypeReference)instr.Operand, parent ) );
							}
							else if ( instr.Operand is FieldReference )
							{
								ni = Instruction.Create( instr.OpCode, Import( (FieldReference)instr.Operand, parent ) );
							}
							else if ( instr.Operand is MethodReference )
							{
								ni = Instruction.Create( instr.OpCode, Import( (MethodReference)instr.Operand, parent ) );
							}
							else
							{
								throw new InvalidOperationException();
							}
							break;
						case OperandType.ShortInlineBrTarget:
						case OperandType.InlineBrTarget:
							ni = Instruction.Create( instr.OpCode, (Instruction)instr.Operand ); // TODO review
							break;
						case OperandType.InlineSwitch:
							ni = Instruction.Create( instr.OpCode, (Instruction[])instr.Operand ); // TODO review
							break;
						case OperandType.InlineR:
							ni = Instruction.Create( instr.OpCode, (double)instr.Operand );
							break;
						case OperandType.ShortInlineR:
							ni = Instruction.Create( instr.OpCode, (float)instr.Operand );
							break;
						case OperandType.InlineNone:
							ni = Instruction.Create( instr.OpCode );
							break;
						case OperandType.InlineString:
							ni = Instruction.Create( instr.OpCode, (string)instr.Operand );
							break;
						case OperandType.ShortInlineI:
							if ( instr.OpCode == OpCodes.Ldc_I4_S )
							{
								ni = Instruction.Create( instr.OpCode, (sbyte)instr.Operand );
							}
							else
							{
								ni = Instruction.Create( instr.OpCode, (byte)instr.Operand );
							}
							break;
						case OperandType.InlineI8:
							ni = Instruction.Create( instr.OpCode, (long)instr.Operand );
							break;
						case OperandType.InlineI:
							ni = Instruction.Create( instr.OpCode, (int)instr.Operand );
							break;
						default:
							throw new InvalidOperationException();
					}
				}
				ni.SequencePoint = instr.SequencePoint;
				nb.Instructions.Add( ni );
			}
			lineIndexer.PostMethodBodyRepack( parent );

			for ( int i = 0; i < body.Instructions.Count; i++ )
			{
				Instruction instr = nb.Instructions[i];
				if ( instr.OpCode.OperandType != OperandType.ShortInlineBrTarget &&
					 instr.OpCode.OperandType != OperandType.InlineBrTarget )
				{
					continue;
				}

				instr.Operand = GetInstruction( body, nb, (Instruction)body.Instructions[i].Operand );
			}

			foreach ( ExceptionHandler eh in body.ExceptionHandlers )
			{
				ExceptionHandler neh = new ExceptionHandler( eh.HandlerType );
				neh.TryStart = GetInstruction( body, nb, eh.TryStart );
				neh.TryEnd = GetInstruction( body, nb, eh.TryEnd );
				neh.HandlerStart = GetInstruction( body, nb, eh.HandlerStart );
				neh.HandlerEnd = GetInstruction( body, nb, eh.HandlerEnd );

				switch ( eh.HandlerType )
				{
					case ExceptionHandlerType.Catch:
						neh.CatchType = Import( eh.CatchType, parent );
						break;
					case ExceptionHandlerType.Filter:
						neh.FilterStart = GetInstruction( body, nb, eh.FilterStart );
						break;
				}

				nb.ExceptionHandlers.Add( neh );
			}
		}

		private void FixAspNetOffset( Mono.Collections.Generic.Collection<Instruction> instructions, MethodReference operand, MethodDefinition parent)
		{
			if (operand.Name == "WriteUTF8ResourceString" || operand.Name == "CreateResourceBasedLiteralControl")
			{
				var fullName = operand.FullName;
				if (fullName == "System.Void System.Web.UI.TemplateControl::WriteUTF8ResourceString(System.Web.UI.HtmlTextWriter,System.Int32,System.Int32,System.Boolean)" ||
					fullName == "System.Web.UI.LiteralControl System.Web.UI.TemplateControl::CreateResourceBasedLiteralControl(System.Int32,System.Int32,System.Boolean)")
				{
					int offset;
					if (aspOffsets.TryGetValue(parent.Module.Assembly, out offset))
					{
						int prev = (int)instructions[instructions.Count - 4].Operand;
						instructions[instructions.Count - 4].Operand = prev + offset;
					}
				}
			}
		}

		private FieldReference Import(FieldReference reference, IGenericParameterProvider context)
		{
			FieldReference importReference = platformFixer.FixPlatformVersion(reference);

			return target.Import(importReference, context);
		}

		internal static Instruction GetInstruction(MethodBody oldBody, MethodBody newBody, Instruction i)
		{
			int pos = oldBody.Instructions.IndexOf(i);
			if (pos > -1 && pos < newBody.Instructions.Count)
				return newBody.Instructions[pos];

			return null /*newBody.Instructions.Outside*/;
		}

		TypeDefinition CreateType( TypeDefinition type, Mono.Collections.Generic.Collection<TypeDefinition> col, bool internalize, string rename )
		{
			TypeDefinition nt = new TypeDefinition( type.Namespace, rename ?? type.Name, type.Attributes );
			col.Add( nt );

			// only top-level types are internalized
			if ( internalize && ( nt.DeclaringType == null ) && nt.IsPublic )
			{
				nt.IsPublic = false;
			}

			CopyGenericParameters( type.GenericParameters, nt.GenericParameters, nt );
			if ( type.BaseType != null )
			{
				nt.BaseType = Import( type.BaseType, nt );
			}

			if ( type.HasLayoutInfo )
			{
				nt.ClassSize = type.ClassSize;
				nt.PackingSize = type.PackingSize;
			}
			// don't copy these twice if UnionMerge==true
			// TODO: we can move this down if we chek for duplicates when adding
			CopySecurityDeclarations( type.SecurityDeclarations, nt.SecurityDeclarations, nt );
			CopyTypeReferences( type.Interfaces, nt.Interfaces, nt );
			CopyCustomAttributes( type.CustomAttributes, nt.CustomAttributes, nt );
			return nt;
		}

		void CopyGenericParameters( Mono.Collections.Generic.Collection<GenericParameter> input, Mono.Collections.Generic.Collection<GenericParameter> output, IGenericParameterProvider nt )
		{
			foreach ( GenericParameter gp in input )
			{
				GenericParameter ngp = new GenericParameter( gp.Name, nt );

				ngp.Attributes = gp.Attributes;
				output.Add( ngp );
			}
			// delay copy to ensure all generics parameters are already present
			Copy( input, output, ( gp, ngp ) => CopyTypeReferences( gp.Constraints, ngp.Constraints, nt ) );
			Copy( input, output, ( gp, ngp ) => CopyCustomAttributes( gp.CustomAttributes, ngp.CustomAttributes, nt ) );
		}

		static void Copy<T>( Mono.Collections.Generic.Collection<T> input, Mono.Collections.Generic.Collection<T> output, Action<T, T> action )
		{
			if ( input.Count != output.Count )
			{
				throw new InvalidOperationException();
			}
			for ( int i = 0; i < input.Count; i++ )
			{
				action.Invoke( input[i], output[i] );
			}
		}

		void CopyTypeReferences( Mono.Collections.Generic.Collection<TypeReference> input, Mono.Collections.Generic.Collection<TypeReference> output, IGenericParameterProvider context )
		{
			foreach ( TypeReference ta in input )
			{
				output.Add( Import( ta, context ) );
			}
		}

		void CopyCustomAttributes( Mono.Collections.Generic.Collection<CustomAttribute> input, Mono.Collections.Generic.Collection<CustomAttribute> output, IGenericParameterProvider context )
		{
			CopyCustomAttributes( input, output, true, context );
		}

		void CopyCustomAttributes( Mono.Collections.Generic.Collection<CustomAttribute> input, Mono.Collections.Generic.Collection<CustomAttribute> output, bool allowMultiple, IGenericParameterProvider context )
		{
			foreach ( CustomAttribute ca in input )
			{
				var caType = ca.AttributeType;
				var similarAttributes = output.Where( attr => reflectionHelper.AreSame( attr.AttributeType, caType ) ).ToList();
				if ( similarAttributes.Count != 0 )
				{
					if ( !allowMultiple )
					{
						continue;
					}
					if ( !CustomAttributeTypeAllowsMultiple( caType ) )
					{
						continue;
					}
					if ( similarAttributes.Any( x =>
						reflectionHelper.AreSame( x.ConstructorArguments, ca.ConstructorArguments ) &&
						reflectionHelper.AreSame( x.Fields, ca.Fields ) &&
						reflectionHelper.AreSame( x.Properties, ca.Properties )
						) )
					{
						continue;
					}
				}
				output.Add( Copy( ca, context ) );
			}
		}

		private CustomAttribute Copy(CustomAttribute ca, IGenericParameterProvider context)
		{
			CustomAttribute newCa = new CustomAttribute(Import(ca.Constructor));
			foreach (var arg in ca.ConstructorArguments)
				newCa.ConstructorArguments.Add(Copy(arg, context));
			foreach (var arg in ca.Fields)
				newCa.Fields.Add(Copy(arg, context));
			foreach (var arg in ca.Properties)
				newCa.Properties.Add(Copy(arg, context));
			return newCa;
		}

		private MethodReference Import(MethodReference reference)
		{
			MethodReference importReference = platformFixer.FixPlatformVersion(reference);
			return target.Import(importReference);
		}

		bool CustomAttributeTypeAllowsMultiple( TypeReference type )
		{
			if ( type.FullName == "IKVM.Attributes.JavaModuleAttribute" || type.FullName == "IKVM.Attributes.PackageListAttribute" )
			{
				// IKVM module attributes, although they don't allow multiple, IKVM supports the attribute being specified multiple times
				return true;
			}
			TypeDefinition typeDef = type.Resolve();
			if ( typeDef != null )
			{
				var ca = typeDef.CustomAttributes.FirstOrDefault( x => x.AttributeType.FullName == "System.AttributeUsageAttribute" );
				if ( ca != null )
				{
					var prop = ca.Properties.FirstOrDefault( y => y.Name == "AllowMultiple" );
					if ( prop.Argument.Value is bool )
					{
						return (bool)prop.Argument.Value;
					}
				}
			}
			// default is false
			return false;
		}

		public TypeDefinition GetMergedTypeFromTypeRef( TypeReference reference )
		{
			return mappingHandler.GetRemappedType( reference );
		}

		internal void ERROR( string msg )
		{
			AlwaysLog( "ERROR: " + msg );
		}

		internal void WARN( string msg )
		{
			AlwaysLog( "WARN: " + msg );
		}

		internal void INFO( string msg )
		{
			LogOutput( "INFO: " + msg );
		}

		internal void VERBOSE( string msg )
		{
			if ( LogVerbose )
			{
				LogOutput( "INFO: " + msg );
			}
		}

		void AlwaysLog( object str )
		{
			string logStr = str.ToString();
			Console.WriteLine( logStr );
			if ( logFile != null )
			{
				logFile.WriteLine( logStr );
			}
		}

		internal void LogOutput( object str )
		{
			if ( Log )
			{
				AlwaysLog( str );
			}
		}

		public ModuleDefinition Target
		{
			get { return target; }
		}
	}

	class ReflectionHelper
	{
		readonly TypeImporter importer;

		internal ReflectionHelper( TypeImporter importer )
		{
			this.importer = importer;
		}

		internal MethodDefinition FindMethodDefinitionInType( TypeDefinition type, MethodReference method )
		{
			return type.Methods.FirstOrDefault(
											   x => x.Name == method.Name &&
													AreSame( x.Parameters, method.Parameters ) &&
													AreSame( x.ReturnType, method.ReturnType ) &&
													x.GenericParameters.Count == method.GenericParameters.Count
				);
		}

		// nasty copy from MetadataResolver.cs for now
		internal bool AreSame( IList<ParameterDefinition> a, IList<ParameterDefinition> b )
		{
			var count = a.Count;

			if ( count != b.Count )
			{
				return false;
			}

			if ( count == 0 )
			{
				return true;
			}

			for ( int i = 0; i < count; i++ )
			{
				if ( !AreSame( a[i].ParameterType, b[i].ParameterType ) )
				{
					return false;
				}
			}

			return true;
		}

		internal bool AreSame( TypeSpecification a, TypeSpecification b )
		{
			if ( !AreSame( a.ElementType, b.ElementType ) )
			{
				return false;
			}

			if ( a.IsGenericInstance )
			{
				return AreSame( (GenericInstanceType)a, (GenericInstanceType)b );
			}

			if ( a.IsRequiredModifier || a.IsOptionalModifier )
			{
				return AreSame( (IModifierType)a, (IModifierType)b );
			}

			if ( a.IsArray )
			{
				return AreSame( (ArrayType)a, (ArrayType)b );
			}

			return true;
		}

		internal bool AreSame( ArrayType a, ArrayType b )
		{
			if ( a.Rank != b.Rank )
			{
				return false;
			}

			// TODO: dimensions

			return true;
		}

		internal bool AreSame( IModifierType a, IModifierType b )
		{
			return AreSame( a.ModifierType, b.ModifierType );
		}

		internal bool AreSame( GenericInstanceType a, GenericInstanceType b )
		{
			if ( !a.HasGenericArguments )
			{
				return !b.HasGenericArguments;
			}

			if ( !b.HasGenericArguments )
			{
				return false;
			}

			if ( a.GenericArguments.Count != b.GenericArguments.Count )
			{
				return false;
			}

			for ( int i = 0; i < a.GenericArguments.Count; i++ )
			{
				if ( !AreSame( a.GenericArguments[i], b.GenericArguments[i] ) )
				{
					return false;
				}
			}

			return true;
		}

		internal bool AreSame( GenericParameter a, GenericParameter b )
		{
			return a.Position == b.Position;
		}

		internal bool AreSame( TypeReference a, TypeReference b )
		{
			if ( a == b )
			{
				return true;
			}
			if ( a == null || b == null )
			{
				return false;
			}
			a = importer.GetMergedTypeFromTypeRef( a ) ?? a;
			b = importer.GetMergedTypeFromTypeRef( b ) ?? b;

			if ( a.MetadataType != b.MetadataType )
			{
				return false;
			}

			if ( a.IsGenericParameter )
			{
				return AreSame( (GenericParameter)a, (GenericParameter)b );
			}

			if ( a is TypeSpecification )
			{
				return AreSame( (TypeSpecification)a, (TypeSpecification)b );
			}

			return a.FullName == b.FullName;
		}

		internal bool AreSame( Mono.Collections.Generic.Collection<CustomAttributeArgument> a, Mono.Collections.Generic.Collection<CustomAttributeArgument> b )
		{
			if ( a.Count != b.Count )
			{
				return false;
			}
			for ( int i = 0; i < a.Count; i++ )
			{
				if ( !AreSame( a[i], b[i] ) )
				{
					return false;
				}
			}
			return true;
		}

		internal bool AreSame( CustomAttributeArgument a, CustomAttributeArgument b )
		{
			if ( !AreSame( a.Type, b.Type ) )
			{
				return false;
			}
			if ( a.Value == b.Value )
			{
				return true;
			}
			if ( a.Value == null )
			{
				return false;
			}
			if ( !a.Value.Equals( b.Value ) )
			{
				return false;
			}
			return true;
		}

		internal bool AreSame( Mono.Collections.Generic.Collection<CustomAttributeNamedArgument> a, Mono.Collections.Generic.Collection<CustomAttributeNamedArgument> b )
		{
			if ( a.Count != b.Count )
			{
				return false;
			}
			foreach ( var argA in a )
			{
				var argB = b.FirstOrDefault( x => x.Name == argA.Name );
				if ( argB.Name == null )
				{
					return false;
				}
				if ( !AreSame( argA.Argument, argB.Argument ) )
				{
					return false;
				}
			}
			return true;
		}


	}

	class MappingHandler
	{
		internal class Pair
		{
			internal readonly string scope;
			internal readonly string name;

			public Pair( string scope, string name )
			{
				this.scope = scope;
				this.name = name;
			}

			public override int GetHashCode()
			{
				return scope.GetHashCode() + name.GetHashCode();
			}

			public override bool Equals( object obj )
			{
				if ( obj == this )
				{
					return true;
				}
				if ( !( obj is Pair ) )
				{
					return false;
				}
				Pair p = (Pair)obj;
				return p.scope == scope && p.name == name;
			}
		}

		readonly IDictionary<Pair, TypeDefinition> mappings = new Dictionary<Pair, TypeDefinition>();
		readonly IDictionary<Pair, TypeReference> exportMappings = new Dictionary<Pair, TypeReference>();

		internal TypeDefinition GetRemappedType( TypeReference r )
		{
			TypeDefinition other;
			if ( r.Scope != null && mappings.TryGetValue( GetTypeKey( r ), out other ) )
			{
				return other;
			}
			return null;
		}

		internal void StoreRemappedType( TypeDefinition orig, TypeDefinition renamed )
		{
			if ( orig.Scope != null )
			{
				mappings[GetTypeKey( orig )] = renamed;
			}
		}

		internal void StoreExportedType( IMetadataScope scope, String fullName, TypeReference exportedTo )
		{
			if ( scope != null )
			{
				exportMappings[GetTypeKey( scope, fullName )] = exportedTo;
			}
		}

		static Pair GetTypeKey( TypeReference reference )
		{
			return GetTypeKey( reference.Scope, reference.FullName );
		}

		static Pair GetTypeKey( IMetadataScope scope, String fullName )
		{
			return new Pair( GetScopeName( scope ), fullName );
		}

		internal static string GetScopeName( IMetadataScope scope )
		{
			if ( scope is AssemblyNameReference )
			{
				return ( (AssemblyNameReference)scope ).Name;
			}
			if ( scope is ModuleDefinition )
			{
				return ( (ModuleDefinition)scope ).Assembly.Name.Name;
			}
			throw new Exception( "Unsupported scope: " + scope );
		}

		internal static string GetScopeFullName( IMetadataScope scope )
		{
			if ( scope is AssemblyNameReference )
			{
				return ( (AssemblyNameReference)scope ).FullName;
			}
			if ( scope is ModuleDefinition )
			{
				return ( (ModuleDefinition)scope ).Assembly.Name.FullName;
			}
			throw new Exception( "Unsupported scope: " + scope );
		}

		public TypeReference GetExportedRemappedType( TypeReference type )
		{
			TypeReference other;
			if ( type.Scope != null && exportMappings.TryGetValue( GetTypeKey( type ), out other ) )
			{
				return other;
			}
			return null;
		}

		internal string GetOrigTypeModule( TypeDefinition nt )
		{
			return mappings.Where( p => p.Value == nt ).Select( p => p.Key.scope ).FirstOrDefault();
		}
	}

	class PlatformFixer
	{
		private TargetRuntime sourceRuntime;
		private TargetRuntime targetRuntime;
		private string targetPlatformDirectory;
		/// <summary>Loaded assemblies are stored here to prevent them loading more than once.</summary>
		private Hashtable platformAssemblies = new Hashtable();

		public PlatformFixer(TargetRuntime runtime)
		{
			sourceRuntime = runtime;
		}

		public void ParseTargetPlatformDirectory(TargetRuntime runtime, string platformDirectory)
		{
			targetRuntime = runtime;
			targetPlatformDirectory = platformDirectory;

			if (string.IsNullOrEmpty(targetPlatformDirectory) && (runtime != sourceRuntime))
				GetPlatformPath(runtime);
			if (!string.IsNullOrEmpty(targetPlatformDirectory))
			{
				if (!Directory.Exists(targetPlatformDirectory))
					throw new ArgumentException("Platform directory not found: \"" + targetPlatformDirectory + "\".");
				// TODO: only tested for Windows, not for Mono!
//if (!File.Exists(Path.Combine(targetPlatformDirectory, "mscorlib.dll")))
  //                  throw new ArgumentException("Invalid platform directory: \"" + targetPlatformDirectory + "\" (mscorlib.dll not found).");
			}
		}

		protected void GetPlatformPath(TargetRuntime runtime)
		{
			// TODO: obviously, this only works for Windows, not for Mono!
			string platformBasePath = Path.GetFullPath(Path.Combine(Environment.SystemDirectory, "..\\Microsoft.NET\\Framework\\"));
			List<string> platformDirectories = new List<string>(Directory.GetDirectories(platformBasePath));
			switch (runtime)
			{
				case (TargetRuntime.Net_1_0):
					targetPlatformDirectory = platformDirectories.First(x => Path.GetFileName(x).StartsWith("v1.0."));
					break;
				case (TargetRuntime.Net_1_1):
					targetPlatformDirectory = platformDirectories.First(x => Path.GetFileName(x).StartsWith("v1.1."));
					break;
				case (TargetRuntime.Net_2_0):
					targetPlatformDirectory = platformDirectories.First(x => Path.GetFileName(x).StartsWith("v2.0."));
					break;
				case (TargetRuntime.Net_4_0):
					targetPlatformDirectory = platformDirectories.First(x => Path.GetFileName(x).StartsWith("v4.0."));
					break;
				default:
					throw new NotImplementedException();
			}
		}

		private AssemblyDefinition TryGetPlatformAssembly(AssemblyNameReference sourceAssemblyName)
		{
			try
			{
				string platformFile = Path.Combine(targetPlatformDirectory, sourceAssemblyName.Name + ".dll");
				AssemblyDefinition platformAsm = null;
				platformAsm = (AssemblyDefinition)platformAssemblies[platformFile];
				if (platformAsm == null)
				{
					if (File.Exists(platformFile))
					{
						// file exists, must be a platform file so exchange it // TODO: is this OK?
						platformAsm = AssemblyDefinition.ReadAssembly(platformFile);
						platformAssemblies[platformFile] = platformAsm;
					}
				}
				return platformAsm;
			}
			catch
			{
				return null;
			}
		}

		public AssemblyNameReference FixPlatformVersion(AssemblyNameReference assyName)
		{
			if (targetPlatformDirectory == null)
				return assyName;

			AssemblyDefinition fixedDef = TryGetPlatformAssembly(assyName);
			if (fixedDef != null)
				return fixedDef.Name;

			return assyName;
		}

		public TypeReference FixPlatformVersion(TypeReference reference)
		{
			if (targetPlatformDirectory == null)
				return reference;

			AssemblyNameReference scopeAsm = reference.Scope as AssemblyNameReference;
			if (scopeAsm != null)
			{
				AssemblyDefinition platformAsm = TryGetPlatformAssembly(scopeAsm);
				if (platformAsm != null)
				{
					TypeReference newTypeRef;
					if (reference is TypeSpecification)
					{
						TypeSpecification refSpec = reference as TypeSpecification;
						TypeReference fet = FixPlatformVersion(refSpec.ElementType);
						if (reference is ArrayType)
						{
							var array = (ArrayType)reference;
							var imported_array = new ArrayType(fet);
							if (array.IsVector)
								return imported_array;

							var dimensions = array.Dimensions;
							var imported_dimensions = imported_array.Dimensions;

							imported_dimensions.Clear();

							for (int i = 0; i < dimensions.Count; i++)
							{
								var dimension = dimensions[i];

								imported_dimensions.Add(new ArrayDimension(dimension.LowerBound, dimension.UpperBound));
							}

							return imported_array;
						}
						else if (reference is PointerType)
							return new PointerType(fet);
						else if (reference is ByReferenceType)
							return new ByReferenceType(fet);
						else if (reference is PinnedType)
							return new PinnedType(fet);
						else if (reference is SentinelType)
							return new SentinelType(fet);
						else if (reference is OptionalModifierType)
							return new OptionalModifierType(FixPlatformVersion(((OptionalModifierType)reference).ModifierType), fet);
						else if (reference is RequiredModifierType)
							return new RequiredModifierType(FixPlatformVersion(((RequiredModifierType)reference).ModifierType), fet);
						else if (reference is GenericInstanceType)
						{
							var instance = (GenericInstanceType)reference;
							var element_type = FixPlatformVersion(instance.ElementType);
							var imported_instance = new GenericInstanceType(element_type);

							var arguments = instance.GenericArguments;
							var imported_arguments = imported_instance.GenericArguments;

							for (int i = 0; i < arguments.Count; i++)
								imported_arguments.Add(FixPlatformVersion(arguments[i]));

							return imported_instance;
						}
						else if (reference is FunctionPointerType)
							throw new NotImplementedException();
						else
							throw new InvalidOperationException();
					}
					else
					{
						newTypeRef = new TypeReference(reference.Namespace, reference.Name, reference.Module,
							platformAsm.Name);
					}
					foreach (var gp in reference.GenericParameters)
						newTypeRef.GenericParameters.Add(FixPlatformVersion(gp, newTypeRef));
					newTypeRef.IsValueType = reference.IsValueType;
					if (reference.DeclaringType != null)
						newTypeRef.DeclaringType = FixPlatformVersion(reference.DeclaringType);
					return newTypeRef;
				}
			}
			return reference;
		}


		MethodSpecification FixPlatformVersionOnMethodSpecification(MethodReference method)
		{
			if (!method.IsGenericInstance)
				throw new NotSupportedException();

			var instance = (GenericInstanceMethod)method;
			var element_method = FixPlatformVersion(instance.ElementMethod);
			var imported_instance = new GenericInstanceMethod(element_method);

			var arguments = instance.GenericArguments;
			var imported_arguments = imported_instance.GenericArguments;

			for (int i = 0; i < arguments.Count; i++)
				imported_arguments.Add(FixPlatformVersion(arguments[i]));

			return imported_instance;
		}


		public MethodReference FixPlatformVersion(MethodReference reference)
		{
			if (targetPlatformDirectory == null)
				return reference;

			if (reference.IsGenericInstance)
			{
				return FixPlatformVersionOnMethodSpecification(reference);
			}

			MethodReference fixedRef = new MethodReference(reference.Name, FixPlatformVersion(reference.ReturnType), FixPlatformVersion(reference.DeclaringType));
			fixedRef.HasThis = reference.HasThis;
			fixedRef.ExplicitThis = reference.ExplicitThis;
			fixedRef.CallingConvention = reference.CallingConvention;
			foreach (ParameterDefinition pd in reference.Parameters)
				fixedRef.Parameters.Add(FixPlatformVersion(pd));
			foreach (GenericParameter gp in reference.GenericParameters)
				fixedRef.GenericParameters.Add(FixPlatformVersion(gp, fixedRef));
			return fixedRef;
		}

		public FieldReference FixPlatformVersion(FieldReference reference)
		{
			if (targetPlatformDirectory == null)
				return reference;

			FieldReference fixedRef = new FieldReference(reference.Name, FixPlatformVersion(reference.FieldType), FixPlatformVersion(reference.DeclaringType));
			return fixedRef;
		}

		private ParameterDefinition FixPlatformVersion(ParameterDefinition pd)
		{
			ParameterDefinition npd = new ParameterDefinition(pd.Name, pd.Attributes, FixPlatformVersion(pd.ParameterType));
			npd.Constant = pd.Constant;
			foreach (CustomAttribute ca in pd.CustomAttributes)
				npd.CustomAttributes.Add(FixPlatformVersion(ca));
			npd.MarshalInfo = pd.MarshalInfo;
			return npd;
		}

		private GenericParameter FixPlatformVersion(GenericParameter gp, IGenericParameterProvider gpp)
		{
			GenericParameter ngp = new GenericParameter(gp.Name, gpp);
			ngp.Attributes = gp.Attributes;
			foreach (TypeReference tr in gp.Constraints)
				ngp.Constraints.Add(FixPlatformVersion(tr));
			foreach (CustomAttribute ca in gp.CustomAttributes)
				ngp.CustomAttributes.Add(FixPlatformVersion(ca));
			if (gp.DeclaringType != null )
				ngp.DeclaringType = FixPlatformVersion(gp.DeclaringType);
			foreach (GenericParameter gp1 in gp.GenericParameters)
				ngp.GenericParameters.Add(FixPlatformVersion(gp1, ngp));
			return ngp;
		}

		private CustomAttribute FixPlatformVersion(CustomAttribute ca)
		{
			CustomAttribute nca = new CustomAttribute(FixPlatformVersion(ca.Constructor));
			foreach (CustomAttributeArgument caa in ca.ConstructorArguments)
				nca.ConstructorArguments.Add(FixPlatformVersion(caa));
			foreach (CustomAttributeNamedArgument cana in ca.Fields)
				nca.Fields.Add(FixPlatformVersion(cana));
			foreach (CustomAttributeNamedArgument cana in ca.Properties)
				nca.Properties.Add(FixPlatformVersion(cana));
			return nca;
		}

		private CustomAttributeArgument FixPlatformVersion(CustomAttributeArgument caa)
		{
			return new CustomAttributeArgument(FixPlatformVersion(caa.Type), caa.Value);
		}

		private CustomAttributeNamedArgument FixPlatformVersion(CustomAttributeNamedArgument cana)
		{
			return new CustomAttributeNamedArgument(cana.Name, FixPlatformVersion(cana.Argument));
		}
	}

	public class PermissionsetHelper
	{
		private static TypeReference GetTypeRef(string nameSpace, string name, string assemblyName, ModuleDefinition targetModule)
		{
			TypeReference typeRef = targetModule.Import(new TypeReference(nameSpace, name, targetModule,
					targetModule.AssemblyReferences.First(x => x.Name == assemblyName)));
			return typeRef;
		}

		public static bool IsXmlPermissionSet(SecurityDeclaration xmlDeclaration)
		{
			if (!xmlDeclaration.HasSecurityAttributes || xmlDeclaration.SecurityAttributes.Count == 0)
				// nothing to convert
				return false;
			if (xmlDeclaration.SecurityAttributes.Count > 1)
				return false;

			SecurityAttribute sa = xmlDeclaration.SecurityAttributes[0];
			if (sa.HasFields)
				return false;
			if (!sa.HasProperties || sa.Properties.Count > 1)
				return false;
			CustomAttributeNamedArgument arg = sa.Properties[0];
			if (arg.Name != "XML" || arg.Argument.Type.FullName != "System.String")
				return false;
			return true;
		}

		public static SecurityDeclaration Permission2XmlSet(SecurityDeclaration declaration, ModuleDefinition targetModule)
		{
			if (!declaration.HasSecurityAttributes || declaration.SecurityAttributes.Count == 0)
				// nothing to convert
				return declaration;
			if (declaration.SecurityAttributes.Count > 1)
				throw new Exception("Cannot convert SecurityDeclaration with more than one attribute");

			SecurityAttribute sa = declaration.SecurityAttributes[0];
			if (sa.HasFields)
				throw new NotSupportedException("Cannot convert SecurityDeclaration with fields");

			TypeReference attrType = sa.AttributeType;
			AssemblyNameReference attrAsm = (AssemblyNameReference)attrType.Scope;
			string className = attrType.FullName + ", " + attrAsm.FullName;

			XmlDocument xmlDoc = new XmlDocument();

			XmlElement permissionSet = xmlDoc.CreateElement("PermissionSet");
			permissionSet.SetAttribute("class", "System.Security.PermissionSet");
			permissionSet.SetAttribute("version", "1");

			XmlElement iPermission = xmlDoc.CreateElement("IPermission");
			iPermission.SetAttribute("class", className);
			iPermission.SetAttribute("version", "1");
			foreach (var arg in sa.Properties)
			{
				iPermission.SetAttribute(arg.Name, arg.Argument.Value.ToString());
			}

			permissionSet.AppendChild(iPermission);
			xmlDoc.AppendChild(permissionSet);

			SecurityDeclaration xmlDeclaration = new SecurityDeclaration(declaration.Action);
			SecurityAttribute attribute = new SecurityAttribute(GetTypeRef("System.Security.Permissions", "PermissionSetAttribute", "mscorlib", targetModule));

			attribute.Properties.Add(new CustomAttributeNamedArgument("XML",
				new CustomAttributeArgument(targetModule.TypeSystem.String, xmlDoc.InnerXml)));

			xmlDeclaration.SecurityAttributes.Add(attribute);
			return xmlDeclaration;
		}

		public static SecurityDeclaration Xml2PermissionSet(SecurityDeclaration xmlDeclaration, ModuleDefinition targetModule)
		{
			if (!xmlDeclaration.HasSecurityAttributes || xmlDeclaration.SecurityAttributes.Count == 0)
				// nothing to convert
				return null;
			if (xmlDeclaration.SecurityAttributes.Count > 1)
				throw new Exception("Cannot convert SecurityDeclaration with more than one attribute");

			SecurityAttribute sa = xmlDeclaration.SecurityAttributes[0];
			if (sa.HasFields)
				throw new NotSupportedException("Cannot convert SecurityDeclaration with fields");
			if (!sa.HasProperties || sa.Properties.Count > 1)
				throw new NotSupportedException("Invalid XML SecurityDeclaration (only 1 property supported)");
			CustomAttributeNamedArgument arg = sa.Properties[0];
			if (arg.Name != "XML" || arg.Argument.Type.FullName != "System.String")
				throw new ArgumentException("Property \"XML\" expected");
			if (string.IsNullOrEmpty(arg.Argument.Value as string))
				return null;
			XmlDocument xmlDoc = new XmlDocument();
			xmlDoc.LoadXml((string)arg.Argument.Value);
			XmlNode permissionSet = xmlDoc.SelectSingleNode("/PermissionSet");
			if (permissionSet == null)
				return null;
			XmlNode permissionSetClass = permissionSet.SelectSingleNode("@class"); // check version?
			if (permissionSetClass == null)
				return null;
			if (permissionSetClass.Value != "System.Security.PermissionSet")
				return null;
			XmlNode iPermission = permissionSet.SelectSingleNode("IPermission");
			if (iPermission == null)
				return null;
			XmlNode iPermissionClass = iPermission.SelectSingleNode("@class"); // check version?
			if (iPermissionClass == null)
				return null;

			// Create Namespace & Name from FullName, AssemblyName can be ignored since we look it up.
			string[] valueParts = iPermissionClass.Value.Split(',');
			Collection<string> classNamespace = new Collection<string>(valueParts[0].Split('.'));
			string assemblyName = valueParts[1].Trim();
			string className = classNamespace[classNamespace.Count - 1];
			classNamespace.RemoveAt(classNamespace.Count - 1);
			SecurityAttribute attribute = new SecurityAttribute(GetTypeRef(string.Join(".", classNamespace.ToArray()), className, assemblyName, targetModule));
			foreach (XmlAttribute xmlAttr in iPermission.Attributes)
			{
				if ((xmlAttr.Name != "class") && (xmlAttr.Name != "version"))
				{
					attribute.Properties.Add(new CustomAttributeNamedArgument(xmlAttr.Name,
						new CustomAttributeArgument(targetModule.TypeSystem.String, xmlAttr.Value)));
				}
			}
			SecurityDeclaration newSd = new SecurityDeclaration(xmlDeclaration.Action);
			newSd.SecurityAttributes.Add(attribute);
			return newSd;
		}
	}

	 /// <summary>
	/// This feature, when enabled, allows to store debug indexes within the assembly itself.
	/// It was inspired by IKVM (which does it for Java assemblies), and re-uses the same attributes.
	/// It then allows at runtime to display file:line information on all stacktraces, by resolving the IL offset provided.
	/// </summary>
	internal class IKVMLineIndexer
	{
		private readonly TypeImporter typeImporter;
		private bool enabled;
		private LineNumberWriter lineNumberWriter;
		private string fileName;
		private IMetadataScope ikvmRuntimeReference;
		private TypeReference sourceFileAttributeTypeReference;
		private TypeReference lineNumberTableAttributeTypeReference;
		private MethodReference lineNumberTableAttributeConstructor1;
		private MethodReference lineNumberTableAttributeConstructor2;
		private MethodReference sourceFileAttributeConstructor;

		protected ModuleDefinition TargetAssemblyMainModule
		{
			get { return typeImporter.Target; }
		}

		public IKVMLineIndexer(TypeImporter ilTypeImporter)
		{
			typeImporter = ilTypeImporter;
		}

		public void Reset()
		{
			lineNumberWriter = null;
			fileName = null;
		}

		public void PreMethodBodyRepack(MethodBody body, MethodDefinition parent)
		{
			if (!enabled)
				return;

			Reset();
			if (!parent.CustomAttributes.Any(x => x.Constructor.DeclaringType.Name == "LineNumberTableAttribute"))
			{
				lineNumberWriter = new LineNumberWriter(body.Instructions.Count / 4);
			}
		}

		public void ProcessMethodBodyInstruction(Instruction instr)
		{
			if (!enabled)
				return;

			var currentSeqPoint = instr.SequencePoint;
			if (lineNumberWriter != null && currentSeqPoint != null)
			{
				if (fileName == null && currentSeqPoint.Document != null)
				{
					var url = currentSeqPoint.Document.Url;
					if (url != null)
					{
						try
						{
							fileName = new FileInfo(url).Name;
						}
						catch
						{
							// for mono
						}
					}
				}
				if (currentSeqPoint.StartLine == 0xFeeFee && currentSeqPoint.EndLine == 0xFeeFee)
				{
					if (lineNumberWriter.LineNo > 0)
					{
						lineNumberWriter.AddMapping(instr.Offset, -1);
					}
				}
				else
				{
					if (lineNumberWriter.LineNo != currentSeqPoint.StartLine)
					{
						lineNumberWriter.AddMapping(instr.Offset, currentSeqPoint.StartLine);
					}
				}
			}
		}

		public void PostMethodBodyRepack(MethodDefinition parent)
		{
			if (!enabled)
				return;

			if (lineNumberWriter != null && lineNumberWriter.Count > 0)
			{
				CustomAttribute ca;
				if (lineNumberWriter.Count == 1)
				{
					ca =
						new CustomAttribute(lineNumberTableAttributeConstructor1)
						{
							ConstructorArguments = { new CustomAttributeArgument(TargetAssemblyMainModule.TypeSystem.UInt16, (ushort)lineNumberWriter.LineNo) }
						};
				}
				else
				{
					ca =
						new CustomAttribute(lineNumberTableAttributeConstructor2)
						{
							ConstructorArguments = { new CustomAttributeArgument(new ArrayType(TargetAssemblyMainModule.TypeSystem.Byte), lineNumberWriter.ToArray().Select(b => new CustomAttributeArgument(TargetAssemblyMainModule.TypeSystem.Byte, b)).ToArray()) }
						};
				}
				parent.CustomAttributes.Add(ca);
				if (fileName != null)
				{
					var type = parent.DeclaringType;
					var exist = type.CustomAttributes.FirstOrDefault(x => x.Constructor.DeclaringType.Name == "SourceFileAttribute");
					if (exist == null)
					{
						// put the filename on the type first
						type.CustomAttributes.Add(new CustomAttribute(sourceFileAttributeConstructor)
						{
							ConstructorArguments = { new CustomAttributeArgument(TargetAssemblyMainModule.TypeSystem.String, fileName) }
						});
					}
					else if (fileName != (string)exist.ConstructorArguments[0].Value)
					{
						// if already specified on the type, but different (e.g. for partial classes), put the attribute on the method.
						// Note: attribute isn't allowed for Methods, but that restriction doesn't apply to IL generation (or runtime use)
						parent.CustomAttributes.Add(new CustomAttribute(sourceFileAttributeConstructor)
						{
							ConstructorArguments = { new CustomAttributeArgument(TargetAssemblyMainModule.TypeSystem.String, fileName) }
						});
					}
				}
			}
		}
	}

	 // copied from IKVM source
	public sealed class LineNumberWriter
	{
		private System.IO.MemoryStream stream;
		private int prevILOffset;
		private int prevLineNum;
		private int count;

		public LineNumberWriter(int estimatedCount)
		{
			stream = new System.IO.MemoryStream(estimatedCount * 2);
		}

		public void AddMapping(int ilOffset, int linenumber)
		{
			if (count == 0)
			{
				if (ilOffset == 0 && linenumber != 0)
				{
					prevLineNum = linenumber;
					count++;
					WritePackedInteger(linenumber - (64 + 50));
					return;
				}
				else
				{
					prevLineNum = linenumber & ~3;
					WritePackedInteger(((-prevLineNum / 4) - (64 + 50)));
				}
			}
			bool pc_overflow;
			bool lineno_overflow;
			byte lead;
			int deltaPC = ilOffset - prevILOffset;
			if (deltaPC >= 0 && deltaPC < 31)
			{
				lead = (byte)deltaPC;
				pc_overflow = false;
			}
			else
			{
				lead = (byte)31;
				pc_overflow = true;
			}
			int deltaLineNo = linenumber - prevLineNum;
			const int bias = 2;
			if (deltaLineNo >= -bias && deltaLineNo < 7 - bias)
			{
				lead |= (byte)((deltaLineNo + bias) << 5);
				lineno_overflow = false;
			}
			else
			{
				lead |= (byte)(7 << 5);
				lineno_overflow = true;
			}
			stream.WriteByte(lead);
			if (pc_overflow)
			{
				WritePackedInteger(deltaPC - (64 + 31));
			}
			if (lineno_overflow)
			{
				WritePackedInteger(deltaLineNo);
			}
			prevILOffset = ilOffset;
			prevLineNum = linenumber;
			count++;
		}

		public int Count
		{
			get
			{
				return count;
			}
		}

		public int LineNo
		{
			get
			{
				return prevLineNum;
			}
		}

		public byte[] ToArray()
		{
			return stream.ToArray();
		}

		/*
		 * packed integer format:
		 * ----------------------
		 * 
		 * First byte:
		 * 00 - 7F      Single byte integer (-64 - 63)
		 * 80 - BF      Double byte integer (-8192 - 8191)
		 * C0 - DF      Triple byte integer (-1048576 - 1048576)
		 * E0 - FE      Reserved
		 * FF           Five byte integer
		 */
		private void WritePackedInteger(int val)
		{
			if (val >= -64 && val < 64)
			{
				val += 64;
				stream.WriteByte((byte)val);
			}
			else if (val >= -8192 && val < 8192)
			{
				val += 8192;
				stream.WriteByte((byte)(0x80 + (val >> 8)));
				stream.WriteByte((byte)val);
			}
			else if (val >= -1048576 && val < 1048576)
			{
				val += 1048576;
				stream.WriteByte((byte)(0xC0 + (val >> 16)));
				stream.WriteByte((byte)(val >> 8));
				stream.WriteByte((byte)val);
			}
			else
			{
				stream.WriteByte(0xFF);
				stream.WriteByte((byte)(val >> 24));
				stream.WriteByte((byte)(val >> 16));
				stream.WriteByte((byte)(val >> 8));
				stream.WriteByte((byte)(val >> 0));
			}
		}
	}
}
