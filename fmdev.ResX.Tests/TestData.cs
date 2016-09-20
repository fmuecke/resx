using fmdev.ResX;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fmdev.ResX.Tests
{
    internal class TestData
    {
        public static List<ResXEntry> SampleEntries()
        {
            return new List<ResXEntry>()
                {
                    new ResXEntry() { Id = "a", Value = "1st item", Comment = "1st comment" },
                    new ResXEntry() { Id = "b", Value = "2nd item", Comment = "2nd comment" },
                    new ResXEntry() { Id = "c", Value = "3rd item", Comment = "3rd comment" }
                };
        }

        public static List<ResXEntry> SampleEntriesWithWindowsLineEndings()
        {
            return new List<ResXEntry>()
                {
                    new ResXEntry() { Id = "a", Value = "1st item\r\nsecond line", Comment = "1st comment\r\n\r\n3rd line" },
                    new ResXEntry() { Id = "b", Value = "\r\n2nd item\r\nsecond line\r\n", Comment = "\r\n2nd comment\r\n\r\n3rd line\r\n" },
                    new ResXEntry() { Id = "c", Value = "3rd item\r\nsecond line\r\n", Comment = "3rd comment\r\n\r\n3rd line\r\n" }
                };
        }

        public static List<ResXEntry> SampleEntriesWithUnixLineEndings()
        {
            return new List<ResXEntry>()
                {
                    new ResXEntry() { Id = "a", Value = "1st item\nsecond line", Comment = "1st comment\n\n3rd line" },
                    new ResXEntry() { Id = "b", Value = "\n2nd item\nsecond line\n", Comment = "\n2nd comment\n\n3rd line\n" },
                    new ResXEntry() { Id = "c", Value = "3rd item\nsecond line\n", Comment = "3rd comment\n\n3rd line\n" }
                };
        }
    }
}
