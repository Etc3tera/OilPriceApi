using OilPriceApi.Helpers;
using OilPriceApi.Models;
using OilPriceApi.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Http;

namespace OilPriceApi.Controllers
{
    [RoutePrefix("api/price")]
    public class PriceController : ApiController
    {
        IOilPriceService service;

        public PriceController() : this(new OilPriceService()) { }
        public PriceController(IOilPriceService service)
        {
            this.service = service;
        }

        [Route("now/{sortKey?}")]
        [HttpGet]
        public IHttpActionResult CurrentPrice(string sortKey = null)
        {
            var results = service.GetCurrentPrice();
            if (results == null)
                return ResponseMessage(HttpHelper.GetResponse(HttpStatusCode.ServiceUnavailable, ErrorMessage.OilPriceServiceError));

            if (string.IsNullOrEmpty(sortKey))
                sortKey = "byname";

            if (sortKey.ToLower() == "byprice")
                results = results.OrderBy(p => p.Price).ToList();
            else if (sortKey.ToLower() == "byname")
                results = results.OrderBy(p => p.Name).ToList();
            else
                return BadRequest(ErrorMessage.UnsupportSortType);

            return Ok(results.Select(p => new
            {
                name = p.Name,
                current_price = p.Price
            }));
        }
    }
}