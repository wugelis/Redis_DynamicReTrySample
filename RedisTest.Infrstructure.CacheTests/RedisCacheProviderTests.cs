using Microsoft.VisualStudio.TestTools.UnitTesting;
using RedisTest.Infrstructure.Cache;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedisTest.Infrstructure.Cache.Tests
{
    [TestClass()]
    public class RedisCacheProviderTests
    {
        [TestMethod()]
        public void Test_RedisCacheProvider_Put()
        {
            //Arrange.
            RedisCacheProvider cacheObj = new RedisCacheProvider();

            //Act.
            int inputData = 1000;
            string key = "__RedisCacheUnitTest1_TestData";
            cacheObj.Put(key, inputData);

            //Assert.
            Assert.IsTrue(true);
        }

        [TestMethod()]
        public void Test_RedisCacheProvider_Put_Livetime()
        {
            //Arrange.
            RedisCacheProvider cacheObj = new RedisCacheProvider();

            //Act.
            int inputData = 1000;
            string key = "__RedisCacheUnitTest1_TestData_liveTime";
            cacheObj.Put(key, inputData, DateTime.Now.AddMinutes(3) - DateTime.Now);

            //Assert.
            Assert.IsTrue(true);
        }

        [TestMethod()]
        public void Test_RedisCacheProvider_Get()
        {
            //Arrange.
            RedisCacheProvider cacheObj = new RedisCacheProvider();

            //Act.
            int expected = 1000;
            int actual = 0;
            string key = "__RedisCacheUnitTest1_TestData_liveTime";

            actual = Convert.ToInt32(cacheObj.Get(key));

            //Assert.
            Assert.AreEqual(expected, actual);
        }
    }
}