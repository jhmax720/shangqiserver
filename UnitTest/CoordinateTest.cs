using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shangqi.Logic;
using Shangqi.Logic.Model;

namespace UnitTest
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestInRange()
        {
            double longtitude_trigger = 22.3130538;
            double langtitude_trigger = 113.5376058;


            var coordinate = new Coordinate(longtitude_trigger, langtitude_trigger);
            var isInRanage = coordinate.IsInRange(longtitude_trigger, langtitude_trigger);

            Assert.IsTrue(isInRanage);


        }
    }
}
