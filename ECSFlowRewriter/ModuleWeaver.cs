using System;
using System.Linq;
using Mono.Cecil;
using ECSFlow.Fody;
using System.Collections.Generic;

/// <summary>
/// 
/// </summary>
public class ModuleWeaver
{
    public Action<string> LogInfo { get; set; }

    /// <summary>
    /// Assemnly representation
    /// </summary>
    public ModuleDefinition ModuleDefinition { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public IAssemblyResolver AssemblyResolver { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public TypeSystem typeSystem;

    /// <summary>
    /// 
    /// </summary>
    public TypeReference ExceptionType;

    /// <summary>
    /// 
    /// </summary>
    public ModuleWeaver()
    {
        LogInfo = m => { };
    }

    /// <summary>
    /// 
    /// </summary>
    public void Execute()
    {
        ImportResources();

        // Process each module type
        IEnumerable<TypeDefinition> types = ModuleDefinition.GetTypes().Where(x => (x.BaseType != null) && !x.IsEnum && !x.IsInterface).ToList();
        foreach (var type in types)
        {
            ProcessMethods(type);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    private void ImportResources()
    {
        // Import .NET base assembly 
        var msCoreLibDefinition = AssemblyResolver.Resolve("mscorlib");
        ExceptionType = ModuleDefinition.ImportReference(msCoreLibDefinition.MainModule.Types.First(x => x.Name == "Exception"));

        // Import Mapping reference
        ModuleDefinition ECSFlowModule = ModuleDefinition.ReadModule("ECSFlowAttributes.dll");
        TypeDefinition MappingType = ECSFlowModule.Types.First(t => t.FullName == "AssemblyToProcessMapping");

        // Get Mapping reference
        foreach (var item in ECSFlowModule.Types)
        {
            ModuleDefinition.ImportReference(GetTypeReference(item));
        }

        //Additional information: Member 'System.Void ECSFlow.Fody.ExceptionChannelAttribute::.ctor
        ///(System.Type,System.String,System.String[],System.String[],System.Boolean)' 
        foreach (var item in typeof(ExceptionChannelAttribute).GetConstructors())
        {
            ModuleDefinition.ImportReference(item);
        }

        MethodReference attributeConstructor =
                ModuleDefinition.ImportReference(
                        typeof(ExceptionChannelAttribute).GetConstructor(
                                new[] { typeof(Type), typeof(System.String), typeof(System.String[]), typeof(System.String[]), typeof(System.Boolean) })
                        );


        Console.WriteLine("Import Resources");
    }

    private static TypeReference GetTypeReference(TypeDefinition newType, string fallback = null)
    {
        var typeName = "";
        if (!string.IsNullOrEmpty(fallback))
            typeName = fallback;
        else
            typeName = newType.FullName;

        var ns = GetTypeNamespace(typeName);
        var tn = GetTypeName(typeName);

        if (ns == "System")
        {
            switch (tn.ToLower())
            {
                case "none":
                case "void":
                    return newType.Module.TypeSystem.Void;
                case "int":
                    return newType.Module.TypeSystem.Int32;
                case "string":
                    return newType.Module.TypeSystem.String;
                case "float":
                    return newType.Module.TypeSystem.Double;
                case "bool":
                case "boolean":
                    return newType.Module.TypeSystem.Boolean;
                default:
                    return newType.Module.TypeSystem.Object;
            }
        }
        var typeRef = new TypeReference(ns, tn, newType.Module, newType.Module);
        typeRef.DeclaringType = newType.Module.Types.FirstOrDefault(t => t.FullName == typeName);
        typeRef.Scope = newType.Module;
        return typeRef;
    }

    private static string GetTypeName(string p)
    {
        if (p.Contains('.')) p = p.Split('.').LastOrDefault();
        var pl = p.ToLower();

        if (pl == "boolean")
            return "bool";
        if (pl == "none")
            return "void";

        if (pl == "float" || pl == "int" || pl == "bool" || pl == "string") return pl;

        return p;
    }

    private static string GetTypeNamespace(string p)
    {
        if (p.Contains('.')) p = p.Split('.').LastOrDefault();
        var pl = p.ToLower();
        /* have not added all possible types yet though.. might be a better way of doing it. */
        if (pl == "string" || pl == "int" || pl == "boolean" || pl == "bool" || pl == "none"
           || pl == "void" || pl == "float" || pl == "short" || pl == "char" || pl == "double"
           || pl == "int32" || pl == "integer32" || pl == "long" || pl == "uint")
        {
            return "System";
        }
        return "ECSFlow.Fody";
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="type"></param>
    void ProcessMethods(TypeDefinition type)
    {
        foreach (var method in type.Methods)
        {
            // Skip for abstract and delegates
            if (!method.HasBody || method.IsRuntimeSpecialName)
            {
                continue;
            }

            // ExceptionProcessor configuration
            var onExceptionProcessor = new OnExceptionProcessor
            {
                Method = method,
                ModuleWeaver = this
            };
            onExceptionProcessor.Process();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="type"></param>
    void ProcessMethods2(TypeDefinition type, List<TypeDefinition> types)
    {
        foreach (var innerType in types)
        {
            if (innerType.Module.GetTypes().Any())
            {
                ProcessMethods2(innerType, innerType.Module.GetTypes().ToList());
            }
            else
            {
                foreach (var method in type.Methods)
                {
                    // Skip for abstract and delegates
                    if (!method.HasBody)
                    {
                        continue;
                    }

                    // ExceptionProcessor configuration
                    var onExceptionProcessor = new OnExceptionProcessor
                    {
                        Method = method,
                        ModuleWeaver = this
                    };
                    onExceptionProcessor.Process();
                }
            }
        }
    }
}