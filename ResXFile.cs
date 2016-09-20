// <copyright file="ResXFile.cs" company="Florian Mücke">
// Copyright (c) Florian Mücke. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace fmdev.ResX
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.Design;
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
            IncludeComments = 1
        }

        public static List<ResXEntry> Read(string filename, Option options = Option.IncludeComments)
        {
            var result = new List<ResXEntry>();
            using (var resx = new ResXResourceReader(filename))
            {
                resx.UseResXDataNodes = true;
                var dict = resx.GetEnumerator();
                while (dict.MoveNext())
                {
                    var node = dict.Value as ResXDataNode;
                    var comment = options.HasFlag(Option.IncludeComments) ? node.Comment.Replace("\r", string.Empty) : string.Empty;
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
                    var node = new ResXDataNode(entry.Id, entry.Value.Replace("\n", Environment.NewLine));

                    if (options.HasFlag(Option.IncludeComments) && !string.IsNullOrWhiteSpace(entry.Comment))
                    {
                        node.Comment = entry.Comment.Replace("\n", Environment.NewLine);
                    }

                    resx.AddResource(node);
                }

                resx.Close();
            }
        }
    }
}