// <copyright file="ResXFile.cs" company="Florian Mücke">
// Copyright (c) Florian Mücke. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace fmdev.ResX
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.Design;
    using System.IO;
    using System.Linq;
    using System.Resources;
    using System.Text;
    using System.Threading.Tasks;

    public static class ResXFile
    {
        [Flags]
        public enum Option
        {
            None = 0,
            SkipComments = 1
        }

        public static List<ResXEntry> Read(string filename, Option options = Option.None)
        {
            var result = new List<ResXEntry>();
            using (var resx = new ResXResourceReader(filename))
            {
                resx.UseResXDataNodes = true;
                var dict = resx.GetEnumerator();
                while (dict.MoveNext())
                {
                    var node = dict.Value as ResXDataNode;
                    var comment = options.HasFlag(Option.SkipComments) ? string.Empty : node.Comment.Replace("\r", string.Empty);
                    result.Add(new ResXEntry()
                    {
                        Id = dict.Key as string,
                        Value = (node.GetValue((ITypeResolutionService)null) as string).Replace("\r", string.Empty),
                        Comment = comment
                    });
                }

                resx.Close();
            }

            return result;
        }

        public static void Write(string filename, IEnumerable<ResXEntry> entries, Option options = Option.None)
        {
            using (var resx = new ResXResourceWriter(filename))
            {
                foreach (var entry in entries)
                {
                    var node = new ResXDataNode(entry.Id, entry.Value.Replace("\r", string.Empty).Replace("\n", Environment.NewLine));

                    if (!options.HasFlag(Option.SkipComments) && !string.IsNullOrWhiteSpace(entry.Comment))
                    {
                        node.Comment = entry.Comment.Replace("\r", string.Empty).Replace("\n", Environment.NewLine);
                    }

                    resx.AddResource(node);
                }

                resx.Close();
            }
        }

        // Returns false if at least one property couldn't be generated.
        public static bool GenerateInternalDesignerFile(string resXFile, string className, string namespaceName)
        {
            return GenerateDesignerFile(resXFile, className, namespaceName, true);
        }

        // Returns false if at least one property couldn't be generated.
        public static bool GenerateDesignerFile(string resXFile, string className, string namespaceName)
        {
            return GenerateDesignerFile(resXFile, className, namespaceName, false);
        }

        // Returns false if at least one property couldn't be generated.
        private static bool GenerateDesignerFile(string resXFile, string className, string namespaceName, bool isInternal)
        {
            if (!File.Exists(resXFile))
            {
                throw new FileNotFoundException($"The file '{resXFile}' could not be found");
            }

            if (string.IsNullOrEmpty(className))
            {
                throw new ArgumentException($"The class name must not be empty or null");
            }

            if (string.IsNullOrEmpty(namespaceName))
            {
                throw new ArgumentException($"The namespace name must not be empty or null");
            }

            string[] unmatchedElements;
            var codeProvider = new Microsoft.CSharp.CSharpCodeProvider();
            System.CodeDom.CodeCompileUnit code =
                System.Resources.Tools.StronglyTypedResourceBuilder.Create(
                    resXFile,
                    className,
                    namespaceName,
                    codeProvider,
                    isInternal,
                    out unmatchedElements);

            var designerFileName = Path.Combine(Path.GetDirectoryName(resXFile), $"{className}.Designer.cs");
            using (StreamWriter writer = new StreamWriter(designerFileName, false, System.Text.Encoding.UTF8))
            {
                codeProvider.GenerateCodeFromCompileUnit(code, writer, new System.CodeDom.Compiler.CodeGeneratorOptions());
            }

            return unmatchedElements.Length == 0;
        }
    }
}