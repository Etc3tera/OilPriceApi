using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OilPriceApi.Models
{
    public class ErrorMessage
    {
        public const string MismatchXmlSchema = "Mismatch XML Schema";
        public const string OilPriceServiceError = "something was wrong, please try again.";
        public const string UnsupportSortType = "Invalid sort type";
    }
}