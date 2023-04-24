/*
 * (C) 2023 Radrat Softworks
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
 
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