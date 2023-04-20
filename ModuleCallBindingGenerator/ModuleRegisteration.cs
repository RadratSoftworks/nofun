using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;

namespace ModuleCallBindingGenerator
{
    public class ModuleRegisteration
    {
        public string className;
        public string namespaceName;
        public bool containSystemLoadedCallback = false;

        public List<MethodDeclarationSyntax> methods = new List<MethodDeclarationSyntax>();
    }
}
