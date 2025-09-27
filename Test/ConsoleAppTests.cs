using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Security.AccessControl;
using VietOCR;

namespace UnitTest
{
    [TestClass]
    public sealed class ConsoleAppTests
    {
        [TestMethod]
        public void MainTest()
        {
            string[] args = { "samples/vietsample.tif", "out", "-l", "vie", "pdf_textonly" };
            ConsoleApp.Main(args);
            Assert.IsTrue(File.Exists("out.pdf"));
        }
    }
}
