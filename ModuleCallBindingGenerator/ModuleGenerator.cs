using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Text;

namespace ModuleCallBindingGenerator
{
    [Generator]
    public class ModuleGenerator : ISourceGenerator
    {
        private bool Is64BitIntegerType(string typename)
        {
            return (typename.Contains("ulong") || typename.Contains("UInt64") ||
                typename.Contains("long") || typename.Contains("Int64"));
        }

        private string GenerateRead32(ref int allocatedCount)
        {
            string resultString;

            if (allocatedCount < 16)
            {
                resultString = $"processor.Reg[Register.P{allocatedCount >> 2}]";
            }
            else
            {
                // Take it to the stack
                resultString = $"memory.ReadMemory32(processor.Reg[Register.SP] + {allocatedCount - 16})";
            }

            allocatedCount += 4;
            return resultString;
        }

        private string GenerateRead64(ref int allocatedCount)
        {
            string resultString;

            if (allocatedCount < 12)
            {
                resultString = $"processor.Reg[Register.P{(allocatedCount >> 2) + 1}] | ((UInt64)processor.Reg[Register.P{allocatedCount >> 2}] << 32)";
            }
            else if (allocatedCount == 12)
            {
                resultString = $"(UInt64)processor.Reg[Register.P3] << 32 | memory.ReadMemory32(processor.Reg[Register.SP])";
            }
            else
            {
                // Take it to the stack
                resultString = $"memory.ReadMemory64(processor.Reg[Register.SP] + {allocatedCount - 16})";
            }

            allocatedCount += 8;
            return resultString;
        }

        public string ReadValue(ParameterSyntax syntax, ref int allocatedCount)
        {
            // This works with the assumption that no argument is passed as raw struct
            // Not like that I have ever seen one in the API
            string typename = syntax.Type.ToString();

            if (typename.Contains("VMPtr") || typename.Contains("VMString"))
            {
                return $"new {syntax.Type}({GenerateRead32(ref allocatedCount)})";
            }
            else if (Is64BitIntegerType(typename))
            {
                return $"({typename}){GenerateRead64(ref allocatedCount)}";
            }
            else
            {
                return $"({typename}){GenerateRead32(ref allocatedCount)}";
            }
        }

        public void BuildMethod(StringBuilder builder, MethodDeclarationSyntax syntax)
        {
            builder.Append($"        private void {syntax.Identifier}_wrap(Processor processor, VMMemory memory) {{");

            int allocatedCount = 0;
            string accumulatedCall = "";

            foreach (var parameter in syntax.ParameterList.Parameters)
            {
                builder.Append(
$@"
            var {parameter.Identifier} = {ReadValue(parameter, ref allocatedCount)};"
);

                if (accumulatedCall.Length == 0)
                {
                    accumulatedCall = $"({parameter.Identifier}";
                }
                else
                {
                    accumulatedCall += $", {parameter.Identifier}";
                }
            }

            accumulatedCall += ")";

            string returnType = syntax.ReturnType.ToString();
            bool isVoid = (returnType == "void");

            string declareResutString = isVoid ? "" : "var result = ";

            // Make the call
            builder.Append(
$@"
            {declareResutString}{syntax.Identifier}{accumulatedCall};");

            if (!isVoid)
            {
                if (returnType.Contains("VMPtr"))
                {
                    builder.Append(
@"
            processor.Reg[Register.R0] = result.Value;");
                }
                else if (returnType.Contains("VMString"))
                {
                    builder.Append(
@"
            processor.Reg[Register.R0] = result.Address;");
                }
                else if (Is64BitIntegerType(returnType))
                {
                    builder.Append(
@"
            processor.Reg[Register.R0] = (uint)(result & 0xFFFFFFFF);
            processor.Reg[Register.R1] = (uint)(result >> 32);");
                }
                else
                {
                    builder.Append(
@"
            processor.Reg[Register.R0] = (uint)result;");
                }
            }

            builder.Append(
@"
        }
");
        }

        public string BuildPartialClassBind(ModuleRegisteration registeration)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(
$@"using Nofun.PIP2;
using Nofun.VM;
using System;

namespace {registeration.namespaceName} {{
    public partial class {registeration.className} : IModule {{
"
                );

            foreach (var method in registeration.methods)
            {
                BuildMethod(sb, method);
            }

            sb.Append(
@"
        void IModule.Register(VMCallMap callMap) {");

            foreach (var method in registeration.methods)
            {
                sb.Append(
$@"
            callMap.Add(""{method.Identifier}"", {method.Identifier}_wrap);"
                    );
            }

            sb.Append(
@"
        }
");

            sb.Append(
@"
    }
}"
                );

            return sb.ToString();
        }

        public void Execute(GeneratorExecutionContext context)
        {
            if (context.SyntaxReceiver == null)
            {
                return;
            }

            ModuleSyntaxReceiver receiver = (ModuleSyntaxReceiver)context.SyntaxReceiver;

            foreach (var module in receiver.moduleCalls)
            {
                string result = BuildPartialClassBind(module);
                context.AddSource($"{module.className}_wrap.g.cs", result);
            }

            // Create register file
            StringBuilder sb = new StringBuilder();
            sb.Append(
@"using System;

namespace Nofun.VM {
    public partial class VMSystem {
        public void RegisterModules() {");

            foreach (var module in receiver.moduleCalls)
            {
                sb.Append(
$@"
            Nofun.Module.IModule {module.className}_module = new {module.namespaceName + "." + module.className}(this);
            {module.className}_module.Register(callMap);
");
            }

            sb.Append(
@"
        }
    }
}");

            context.AddSource("VMSystem_RegisterModules.g.cs", sb.ToString());
        }

        public void Initialize(GeneratorInitializationContext context)
        {
            context.RegisterForSyntaxNotifications(() => new ModuleSyntaxReceiver());
        }
    }
}
