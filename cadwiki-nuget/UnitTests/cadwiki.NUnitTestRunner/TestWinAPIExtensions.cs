using cadwiki.NUnitTestRunner.Creators;
using cadwiki.NUnitTestRunner.WinAPI;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace UnitTests
{
    [TestClass()]
    public class TestWinAPIExtensions
    {
        [TestMethod()]
        public void Test_CloseWindow_ShouldPass()
        {
            var thread = new Thread(() =>
            {
                var form = new System.Windows.Forms.Form
                {
                    Text = "software test window",
                    Width = 300,
                    Height = 200
                };

                System.Windows.Forms.Application.Run(form);
            });

            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();

            // give the window time to show
            Thread.Sleep(500);

            bool closed = cadwiki.NUnitTestRunner.WinAPI.ExtensionMethods.CloseWindowByTitle("software");
            Assert.IsTrue(closed);
        }
    }
}
