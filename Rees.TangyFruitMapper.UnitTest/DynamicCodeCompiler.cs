using System;
using System.CodeDom.Compiler;
using Microsoft.CSharp;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Rees.TangyFruitMapper.UnitTest
{
    internal class DynamicCodeCompiler
    {
        public IDtoMapper<TDto, TModel> CompileMapperCode<TDto, TModel>(string code, ITestOutputHelper output)
        {
            var provider = new CSharpCodeProvider();
            var parameters = new CompilerParameters();
            parameters.ReferencedAssemblies.Add("Rees.TangyFruitMapper.dll");
            parameters.ReferencedAssemblies.Add("System.dll");
            parameters.ReferencedAssemblies.Add("System.Core.dll");
            parameters.ReferencedAssemblies.Add("System.Runtime.dll");
            parameters.ReferencedAssemblies.Add("System.Linq.dll");
            parameters.ReferencedAssemblies.Add("Rees.TangyFruitMapper.UnitTest.dll");
            // True - memory generation, false - external file generation
            parameters.GenerateInMemory = true;
            // True - exe file generation, false - dll file generation
            parameters.GenerateExecutable = false;
            var results = provider.CompileAssemblyFromSource(parameters, code);
            if (results.Errors.HasErrors)
            {
                foreach (CompilerError error in results.Errors)
                {
                    output.WriteLine($"Error ({error.ErrorNumber}): {error.ErrorText}");
                }
                throw new XunitException("Error compiling the generated mapper code.");
            }
            var assembly = results.CompiledAssembly;
            var mapperType = assembly.GetType($"GeneratedCode.Mapper_{typeof(TDto).Name}_{typeof(TModel).Name}");
            return (IDtoMapper<TDto, TModel>)mapperType.GetConstructor(new Type[] { }).Invoke(new object[] { });
        }
    }
}
