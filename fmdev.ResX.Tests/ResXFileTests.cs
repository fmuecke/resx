using Microsoft.VisualStudio.TestTools.UnitTesting;
using fmdev.ResX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace fmdev.ResX.Tests
{
    [TestClass]
    public class ResXFileTests
    {
        [TestMethod]
        public void WriteTest()
        {
            var tempFile = Path.GetTempFileName();
            try
            {
                var testData = TestData.SampleEntries();
                ResXFile.Write(tempFile, testData);
                var content = File.ReadAllText(tempFile, Encoding.UTF8);
                foreach (var entry in testData)
                {
                    Assert.IsTrue(content.Contains($"<data name=\"{entry.Id}\""), $"file must contain entry '{entry.Id}'");
                    Assert.IsTrue(content.Contains($"<value>{entry.Value}</value>"), $"file must contain value for entry '{entry.Id}'");
                    Assert.IsTrue(content.Contains($"<comment>{entry.Comment}</comment>"), $"file must contain comment for entry '{entry.Id}'");
                }
            }
            finally
            {
                File.Delete(tempFile);
            }
        }

        [TestMethod]
        public void WriteSkipCommentsTest()
        {
            var tempFile = Path.GetTempFileName();
            try
            {
                var testData = TestData.SampleEntries();
                ResXFile.Write(tempFile, testData, ResXFile.Option.SkipComments);
                var content = File.ReadAllText(tempFile, Encoding.UTF8);
                foreach (var entry in testData)
                {
                    Assert.IsTrue(content.Contains($"<data name=\"{entry.Id}\""), $"file must contain entry '{entry.Id}'");
                    Assert.IsTrue(content.Contains($"<value>{entry.Value}</value>"), $"file must contain expected value for entry '{entry.Id}'");
                    Assert.IsFalse(content.Contains(entry.Comment), $"file must not comment for entry '{entry.Id}'");
                }
            }
            finally
            {
                File.Delete(tempFile);
            }
        }

        [TestMethod]
        public void WriteUnixEolTest()
        {
            var tempFile = Path.GetTempFileName();
            try
            {
                var testData = TestData.SampleEntriesWithUnixLineEndings();
                ResXFile.Write(tempFile, testData);
                var content = File.ReadAllText(tempFile, Encoding.UTF8).Replace("\r", string.Empty);
                foreach (var entry in testData)
                {
                    Assert.IsTrue(content.Contains($"<data name=\"{entry.Id}\""), $"file must contain entry '{entry.Id}'");
                    Assert.IsTrue(content.Contains($"<value>{entry.Value}</value>"), $"file must contain expected value for entry '{entry.Id}'");
                    Assert.IsTrue(content.Contains(entry.Comment), $"file must contain comment for entry '{entry.Id}'");
                }
            }
            finally
            {
                File.Delete(tempFile);
            }
        }

        [TestMethod]
        public void WriteWindowsEolTest()
        {
            var tempFile = Path.GetTempFileName();
            try
            {
                var testData = TestData.SampleEntriesWithWindowsLineEndings();
                ResXFile.Write(tempFile, testData);
                var content = File.ReadAllText(tempFile, Encoding.UTF8);
                Assert.IsTrue(content.Contains("\r\n"), "file content is expected to have windows line endings");

                foreach (var entry in testData)
                {
                    Assert.IsTrue(content.Contains($"<data name=\"{entry.Id}\""), $"file must contain entry '{entry.Id}'");
                    Assert.IsTrue(content.Contains($"<value>{entry.Value}</value>"), $"file must contain expected value for entry '{entry.Id}'");
                    Assert.IsTrue(content.Contains(entry.Comment), $"file must contain comment for entry '{entry.Id}'");
                }
            }
            finally
            {
                File.Delete(tempFile);
            }
        }

        [TestMethod]
        public void ReadTest()
        {
            var tempFile = Path.GetTempFileName();
            try
            {
                var testData = TestData.SampleEntries();
                Assert.IsTrue(testData.Count > 0, "test data must not be empty");
                ResXFile.Write(tempFile, testData);
                var entries = ResXFile.Read(tempFile);

                foreach (var writtenEntry in testData)
                {
                    Assert.IsTrue(entries.Exists(e => e.Id == writtenEntry.Id && e.Value == writtenEntry.Value && e.Comment == writtenEntry.Comment),
                        $"item '{writtenEntry.Id}' must be in read entries");
                }

                // skip comments test
                entries = ResXFile.Read(tempFile, ResXFile.Option.SkipComments);
                Assert.IsTrue(entries.All(e => string.IsNullOrEmpty(e.Comment)), "comments must be empty");

                foreach (var writtenEntry in testData)
                {
                    Assert.IsTrue(entries.Exists(e => e.Id == writtenEntry.Id && e.Value == writtenEntry.Value),
                        $"item '{writtenEntry.Id}' must be in read entries");
                }

            }
            finally
            {
                File.Delete(tempFile);
            }
        }

        [TestMethod]
        public void GenerateDesignerFileTest()
        {
            var tempFile = Path.GetTempFileName();
            try
            {
                var testData = TestData.SampleEntries();
                ResXFile.Write(tempFile, testData);
                var className = "TestClassName";
                var namespaceName = "fmdev.ResX.Tests";
                Assert.IsTrue(ResXFile.GenerateDesignerFile(tempFile, className, namespaceName), "Designer file generation must return true");
                var expectedDesignerFile = Path.Combine(Path.GetDirectoryName(tempFile), $"{className}.Designer.cs");
                Assert.IsTrue(File.Exists(expectedDesignerFile), "Generated designer file must be written");
                Assert.IsTrue(File.ReadAllText(expectedDesignerFile).Contains($"public class {className} "), "generated public designer class must be public");
                File.Delete(expectedDesignerFile);

                Assert.IsTrue(ResXFile.GenerateInternalDesignerFile(tempFile, className, namespaceName), "Designer file generation must return true");
                Assert.IsTrue(File.Exists(expectedDesignerFile), "Generated designer file must be written");
                Assert.IsTrue(File.ReadAllText(expectedDesignerFile).Contains($"internal class {className} "), "generated internal designer class must be internal");
                File.Delete(expectedDesignerFile);
            }
            finally
            {
                File.Delete(tempFile);
            }
        }

        [TestMethod]
        public void InvalidGenerateDesignerFileTest()
        {
            Assert.ThrowsException<FileNotFoundException>(() => ResXFile.GenerateDesignerFile("ThisResXFileDoesNotExist.rex", "someclass", "some.namespace"), "Nonexistent resX file must lead to FileNotFoundException");
            var tempFile = Path.GetTempFileName();
            try
            {
                Assert.ThrowsException<ArgumentException>(() => ResXFile.GenerateDesignerFile(tempFile, null, null), "Empty class names should not be allowed and result in an ArgumentException");
                Assert.ThrowsException<ArgumentException>(() => ResXFile.GenerateDesignerFile(tempFile, "myClass", null), "Empty namespace names should not be allowed and result in an ArgumentException");
            }
            finally
            {
                File.Delete(tempFile);
            }
        }
    }
}
