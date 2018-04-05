using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using OilPriceApi.Services;
using OilPriceApi.Controllers;
using OilPriceApi.Models;
using System.Collections.Generic;
using System.Web.Http.Results;
using System.Net;
using Newtonsoft.Json.Linq;

namespace OilPriceApi.Test
{
    [TestClass]
    public class PriceController_Test
    {
        Mock<IOilPriceService> mockService;
        PriceController testController;

        [TestInitialize]
        public void TestInitialize()
        {
            mockService = new Mock<IOilPriceService>();
            testController = new PriceController(mockService.Object);
        }

        [TestMethod]
        public void should_return_error_if_service_return_null()
        {
            mockService.Setup(p => p.GetCurrentPrice()).Returns(default(List<OilPrice>));

            var results = testController.CurrentPrice() as ResponseMessageResult;

            Assert.IsNotNull(results);
            Assert.AreEqual(HttpStatusCode.ServiceUnavailable, results.Response.StatusCode);
        }

        [TestMethod]
        public void should_return_blank_json_array_if_service_return_blank_array()
        {
            mockService.Setup(p => p.GetCurrentPrice()).Returns(new List<OilPrice>());

            dynamic results = testController.CurrentPrice();
            JArray json = JArray.FromObject(results.Content);

            Assert.IsNotNull(json);
            Assert.AreEqual(0, json.Count);
        }

        [TestMethod]
        [DataRow(null)]
        [DataRow("byname")]
        public void should_return_sorted_by_name_result(string sortType)
        {
            mockService.Setup(p => p.GetCurrentPrice()).Returns(new List<OilPrice>()
            {
                new OilPrice(){ Name = "95", Price = 10m },
                new OilPrice(){ Name = "91", Price = 8m },
            });

            dynamic results = testController.CurrentPrice(sortType);
            JArray json = JArray.FromObject(results.Content);

            Assert.IsNotNull(json);
            Assert.AreEqual(2, json.Count);
            Assert.AreEqual("91", json[0]["name"].ToObject<string>());
            Assert.AreEqual(8m, json[0]["current_price"].ToObject<decimal>());
            Assert.AreEqual("95", json[1]["name"].ToObject<string>());
            Assert.AreEqual(10m, json[1]["current_price"].ToObject<decimal>());
        }

        [TestMethod]
        public void should_return_sorted_by_price_result()
        {
            mockService.Setup(p => p.GetCurrentPrice()).Returns(new List<OilPrice>()
            {
                new OilPrice(){ Name = "A00", Price = 20m },
                new OilPrice(){ Name = "A91", Price = 10m },
                new OilPrice(){ Name = "A95", Price = 8m },
            });

            dynamic results = testController.CurrentPrice("byprice");
            JArray json = JArray.FromObject(results.Content);

            Assert.IsNotNull(json);
            Assert.AreEqual(3, json.Count);
            Assert.AreEqual("A95", json[0]["name"].ToObject<string>());
            Assert.AreEqual(8m, json[0]["current_price"].ToObject<decimal>());
            Assert.AreEqual("A91", json[1]["name"].ToObject<string>());
            Assert.AreEqual(10m, json[1]["current_price"].ToObject<decimal>());
            Assert.AreEqual("A00", json[2]["name"].ToObject<string>());
            Assert.AreEqual(20m, json[2]["current_price"].ToObject<decimal>());
        }

        [TestMethod]
        public void should_return_bad_request_if_invalid_sort_type()
        {
            mockService.Setup(p => p.GetCurrentPrice()).Returns(new List<OilPrice>()
            {
                new OilPrice(){ Name = "95", Price = 10m },
                new OilPrice(){ Name = "91", Price = 8m },
            });

            var result = testController.CurrentPrice("bynamedesc") as BadRequestErrorMessageResult;

            Assert.IsNotNull(result);
            Assert.AreEqual(ErrorMessage.UnsupportSortType, result.Message);
        }
    }
}
