using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CodeLinkAdapterExt.Tests
{
    [TestClass]
    public sealed class CodeLinkParserTests
    {
        [TestMethod]
        [DataRow(@"[GridSession.cs (line 243)]\(C:/Users/home/source/repos/hyakushiki-kannon/hyakushiki-kannon.Core/GridSession.cs:243)")]
        [DataRow(@"[GridSession.cs (line 243)](C:/Users/home/source/repos/hyakushiki-kannon/hyakushiki-kannon.Core/GridSession.cs:243)")]
        [DataRow(@"[GridSession.cs (line 999)](C:/Users/home/source/repos/hyakushiki-kannon/hyakushiki-kannon.Core/GridSession.cs:243)")]
        public void TryParse_CopiedCodexLink_UsesDestinationPathAndLine(string input)
        {
            bool success = CodeLinkParser.TryParse(input, out CodeLinkTarget target);

            Assert.IsTrue(success);
            Assert.AreEqual(@"C:\Users\home\source\repos\hyakushiki-kannon\hyakushiki-kannon.Core\GridSession.cs", target.FilePath);
            Assert.AreEqual(243, target.Line);
            Assert.AreEqual(1, target.Column);
        }

        [TestMethod]
        [DataRow("C:/repo/File.cs:12", @"C:\repo\File.cs", 12, 1)]
        [DataRow(@"C:\repo\File.cs:12:8", @"C:\repo\File.cs", 12, 8)]
        [DataRow("C:/repo/File.cs#L12C8", @"C:\repo\File.cs", 12, 8)]
        [DataRow("C:/repo/File.cs#L12", @"C:\repo\File.cs", 12, 1)]
        [DataRow("[File.cs](/C:/repo/File.cs:12)", @"C:\repo\File.cs", 12, 1)]
        [DataRow("[File[1].cs](C:/repo/File[1].cs:12)", @"C:\repo\File[1].cs", 12, 1)]
        [DataRow("  `C:/repo/File.cs:12`\r\n", @"C:\repo\File.cs", 12, 1)]
        [DataRow("\"C:/repo/File.cs:12:8\"", @"C:\repo\File.cs", 12, 8)]
        [DataRow("[File.cs](<C:/my repo/File.cs:12>)", @"C:\my repo\File.cs", 12, 1)]
        [DataRow("[File.cs](C:/repo (old)/File.cs:12)", @"C:\repo (old)\File.cs", 12, 1)]
        [DataRow(@"[File.cs](C:\repo\(old)\File.cs:12)", @"C:\repo\(old)\File.cs", 12, 1)]
        [DataRow(@"[File.cs]\(C:/repo/File.cs:12\)", @"C:\repo\File.cs", 12, 1)]
        [DataRow("[ファイル.cs](C:/開発/ファイル.cs:2:3)", @"C:\開発\ファイル.cs", 2, 3)]
        [DataRow("C:/repo/File%20Name.cs:12", @"C:\repo\File%20Name.cs", 12, 1)]
        [DataRow("file:///C:/my%20repo/File.cs:12:8", @"C:\my repo\File.cs", 12, 8)]
        [DataRow("file:///C:/repo/File.cs#L12", @"C:\repo\File.cs", 12, 1)]
        [DataRow(@"\\server\share\File.cs:12", @"\\server\share\File.cs", 12, 1)]
        [DataRow("file://server/share/File.cs#L12", @"\\server\share\File.cs", 12, 1)]
        [DataRow("C:/repo/../source/File.cs:1", @"C:\source\File.cs", 1, 1)]
        [DataRow("C:/repo/File.cs:2147483647:2147483647", @"C:\repo\File.cs", int.MaxValue, int.MaxValue)]
        public void TryParse_AbsoluteFileLocation_ReturnsOneBasedPosition(string input, string expectedPath, int expectedLine, int expectedColumn)
        {
            bool success = CodeLinkParser.TryParse(input, out CodeLinkTarget target);

            Assert.IsTrue(success, input);
            Assert.AreEqual(expectedPath, target.FilePath);
            Assert.AreEqual(expectedLine, target.Line);
            Assert.AreEqual(expectedColumn, target.Column);
        }

        [TestMethod]
        [DataRow(null)]
        [DataRow("")]
        [DataRow("  ")]
        [DataRow("GridSession.cs (line 243)")]
        [DataRow("C:/repo/File.cs")]
        [DataRow("C:/repo/File.cs:0")]
        [DataRow("C:/repo/File.cs:-1")]
        [DataRow("C:/repo/File.cs:1:0")]
        [DataRow("C:/repo/File.cs:1:-2")]
        [DataRow("C:/repo/File.cs:2147483648")]
        [DataRow("C:/repo/File.cs:1:2147483648")]
        [DataRow("C:/repo/File.cs#L0")]
        [DataRow("C:/repo/File.cs#L1C0")]
        [DataRow("C:/repo/File.cs:abc")]
        [DataRow("C:/repo/File.cs:1:2:3")]
        [DataRow("repo/File.cs:1")]
        [DataRow("C:repo/File.cs:1")]
        [DataRow("/home/source/File.cs:1")]
        [DataRow("https://example.com/File.cs:1")]
        [DataRow("file:///C:/repo/File.cs?query:1")]
        [DataRow("C:/repo/:1")]
        [DataRow("C:/repo/F*le.cs:1")]
        [DataRow("C:/repo/File.cs:stream:1")]
        [DataRow("[File.cs](C:/repo/File.cs:1")]
        [DataRow("[File.cs](C:/repo/File.cs:1) trailing text")]
        [DataRow("C:/repo/File.cs:1\nC:/repo/Other.cs:2")]
        [DataRow("[File.cs](C:/repo/File.cs:1) [Other.cs](C:/repo/Other.cs:2)")]
        public void TryParse_InvalidOrAmbiguousInput_ReturnsFalse(string input)
        {
            bool success = CodeLinkParser.TryParse(input, out CodeLinkTarget target);

            Assert.IsFalse(success, input);
            Assert.IsNull(target);
        }
    }
}
