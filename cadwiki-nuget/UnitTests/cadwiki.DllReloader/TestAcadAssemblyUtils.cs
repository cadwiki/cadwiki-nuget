using System.Reflection;
using cadwiki.DllReloader.AutoCAD;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests
{

    [TestClass()]
    public class TestAcadAssemblyUtils
    {

        [TestMethod()]
        public void Test_GetAppAssemblySafeley_WithNothing_ShouldReturnNothing()
        {
            object expected = null;
            Assembly actual = (Assembly)AcadAssemblyUtils.GetAppObjectSafely(null);
            Assert.AreEqual(expected, actual);
        }

    }
}