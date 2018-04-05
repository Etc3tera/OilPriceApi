using Microsoft.QualityTools.Testing.Fakes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OilPriceApi.Services;
using System.Net.Fakes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using OilPriceApi.Models;

namespace OilPriceApi.Test
{
    [TestClass]
    public class OilPriceService_Test
    {
        private Exception lastException;

        [TestMethod]
        public void should_return_null_if_has_exception()
        {
            var testService = new OilPriceService(testLog);

            using (ShimsContext.Create())
            {
                ShimWebClient.AllInstances.DownloadStringString = (client, url) =>
                {
                    throw new WebException();
                };
                var result = testService.GetCurrentPrice();

                Assert.IsNull(result);
                Assert.IsNotNull(lastException);
                Assert.IsInstanceOfType(lastException, typeof(WebException));
            }
        }

        [TestMethod]
        public void should_return_null_if_cannot_parse()
        {
            var testService = new OilPriceService(testLog);

            using (ShimsContext.Create())
            {
                ShimWebClient.AllInstances.DownloadStringString = (client, url) =>
                {
                    return "aaa";
                };
                var result = testService.GetCurrentPrice();

                Assert.IsNull(result);
                Assert.IsNotNull(lastException);
                Assert.IsInstanceOfType(lastException, typeof(XmlException));
            }
        }

        [TestMethod]
        [DataRow(@"<something></something>")]
        [DataRow(@"<?xml version='1.0' encoding='UTF-8'?>
    <header>
    <item>
        <name>Gasohol 91</name>
        <today>31.51</today>
    </item>
    </header>")]
        [DataRow(@"<?xml version='1.0' encoding='UTF-8'?>
    <header>
    <item>
        <type>Gasohol 91</type>
        <price>31.51</price>
    </item>
    </header>")]
        public void should_return_null_if_invalid_schema(string testCase)
        {
            var testService = new OilPriceService(testLog);

            using (ShimsContext.Create())
            {
                ShimWebClient.AllInstances.DownloadStringString = (client, url) =>
                {
                    return testCase;
                };
                var result = testService.GetCurrentPrice();

                Assert.IsNull(result);
                Assert.AreEqual(ErrorMessage.MismatchXmlSchema, lastException.Message);
            }
        }

        [TestMethod]
        public void should_return_blank_list_if_no_data()
        {
            var testService = new OilPriceService(testLog);

            using (ShimsContext.Create())
            {
                ShimWebClient.AllInstances.DownloadStringString = (client, url) =>
                {
                    return @"<?xml version='1.0' encoding='UTF-8'?>
    <header>
    </header>";
                };
                var result = testService.GetCurrentPrice();

                Assert.IsNotNull(result);
                Assert.AreEqual(0, result.Count);
            }
        }        

        [TestMethod]
        public void should_return_parse_data()
        {
            var testService = new OilPriceService(testLog);

            using (ShimsContext.Create())
            {
                ShimWebClient.AllInstances.DownloadStringString = (client, url) =>
                {
                    return @"<?xml version='1.0' encoding='UTF-8'?>
    <header>
    <item>
        <type>Gasohol 91</type>
        <today>31.51</today>
        <unit_en>Baht/Liter</unit_en>
    </item>
    <item>
        <type>Gasohol 95</type>
        <today>27.64</today>
    </item>
    </header>";
                };
                var result = testService.GetCurrentPrice();

                Assert.IsNotNull(result);
                Assert.AreEqual(2, result.Count);
                Assert.AreEqual("Gasohol 91", result[0].Name);
                Assert.AreEqual(31.51m, result[0].Price);
                Assert.AreEqual("Gasohol 95", result[1].Name);
                Assert.AreEqual(27.64m, result[1].Price);
            }
        }

        private void testLog(Exception e)
        {
            lastException = e;
        }
    }
}
