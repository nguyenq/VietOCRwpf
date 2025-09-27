using Microsoft.VisualStudio.TestTools.UnitTesting;
using VietOCR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VietOCR.Tests
{
    [TestClass()]
    public class ConsoleAppTests
    {
        [TestMethod()]
        public void MainTest()
        {
            string[] args = {"vietsample.tif", "out",  "-l", "vie", "pdf_textonly" };
            ConsoleApp.Main(args);
            //Assert.Fail();
        }
    }
}