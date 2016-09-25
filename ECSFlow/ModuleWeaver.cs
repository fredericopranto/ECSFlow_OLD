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

    private void ImportResources()
    {
        // Import .NET base assembly 
        var msCoreLibDefinition = AssemblyResolver.Resolve("mscorlib");
        ExceptionType = ModuleDefinition.ImportReference(msCoreLibDefinition.MainModule.Types.First(x => x.Name == "Exception"));

        Console.WriteLine("Import Resources");
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