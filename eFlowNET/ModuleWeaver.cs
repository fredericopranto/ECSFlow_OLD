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
        // Import .NET base assembly 
        var msCoreLibDefinition = AssemblyResolver.Resolve("mscorlib");
        ExceptionType = ModuleDefinition.ImportReference(msCoreLibDefinition.MainModule.Types.First(x => x.Name == "Exception"));

        // Process each module type
        foreach (var type in ModuleDefinition.GetTypes().Where(x => (x.BaseType != null) && !x.IsEnum && !x.IsInterface))
        {
            ProcessMethods(type, type.Module.GetTypes().ToList<TypeDefinition>);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="type"></param>
    void ProcessMethods(TypeDefinition type, IQueryable<TypeDefinition> types)
    {
        if(types.Count > 0)
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

            ProcessType(innerType, );
    }
}