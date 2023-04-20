using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using ModuleCallBindingGenerator.Helpers;
using System.Collections.Generic;
using System.Linq;

namespace ModuleCallBindingGenerator
{
    public class ModuleSyntaxReceiver : ISyntaxReceiver
    {
        public List<ModuleRegisteration> moduleCalls = new List<ModuleRegisteration>();

        private void VisitMethod(ModuleRegisteration registeration, MethodDeclarationSyntax method)
        {
            if (method.Identifier.Text == "OnSystemLoaded")
            {
                registeration.containSystemLoadedCallback = true;
            }

            if (method.AttributeLists.Count == 0)
            {
                return;
            }

            if (method.AttributeLists.Any(attributeList => attributeList.Attributes.Any(
                attrib => attrib.Name.GetText().ToString() == "ModuleCall")))
            {
                registeration.methods.Add(method);
            }
        }

        public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
        {
            if (!(syntaxNode is ClassDeclarationSyntax classSyntax) || (classSyntax.AttributeLists.Count == 0))
            {
                return;
            }

            if (!classSyntax.AttributeLists.Any(attributeList => attributeList.Attributes.Any(
                attrib => attrib.Name.GetText().ToString() == "Module")))
            {
                return;
            }

            NamespaceDeclarationSyntax namespaceDeclarationSyntax = null;
            if (!SyntaxNodeHelper.TryGetParentSyntax(classSyntax, out namespaceDeclarationSyntax))
            {
                return;
            }

            string namespaceName = namespaceDeclarationSyntax.Name.ToString();
            string className = classSyntax.Identifier.ToString();

            ModuleRegisteration[] existingRegisterations = moduleCalls.Where(module => (module.className == className) && (module.namespaceName == namespaceName))
                .ToArray();

            ModuleRegisteration registeration;
            if (existingRegisterations != null && (existingRegisterations.Length >= 1))
            {
                registeration = existingRegisterations[0];
            } else
            {
                registeration = new ModuleRegisteration()
                {
                    className = className,
                    namespaceName = namespaceName
                };

                moduleCalls.Add(registeration);
            }

            foreach (MemberDeclarationSyntax memberDeclaration in classSyntax.Members)
            {
                if (memberDeclaration is MethodDeclarationSyntax methodDeclaration)
                {
                    VisitMethod(registeration, methodDeclaration);
                }
            }
        }
    }
}