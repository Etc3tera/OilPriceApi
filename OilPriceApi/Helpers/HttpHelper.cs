using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;

namespace OilPriceApi.Helpers
{
    public static class HttpHelper
    {
        public static HttpResponseMessage GetResponse(HttpStatusCode statusCode, string message)
        {
            var response = new HttpResponseMessage();
            response.StatusCode = statusCode;
            response.Content = new StringContent(message);
            response.Content.Headers.ContentType = new MediaTypeHeaderValue("text/html");
            return response;
        }
    }
}