using NUnit.Framework;

namespace UnitTests
{

    [TestFixture]
    public partial class TestStrings
    {

        [Test]
        public void Test_DoStringsMatch_ShouldPass()
        {
            string expected = "Hello";
            string actual = "Hello";
            NUnit.Framework.Assert.AreEqual(expected, actual, "Input strings don't match");
        }

        [Test]
        public void Test_DoStringsMatch_ShouldFail()
        {
            string expected = "Hello";
            string actual = "World";
            NUnit.Framework.Assert.AreEqual(expected, actual, "Input strings don't match");
        }


    }
}