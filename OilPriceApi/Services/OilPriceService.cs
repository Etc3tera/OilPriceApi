using OilPriceApi.Helpers;
using OilPriceApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Xml;

namespace OilPriceApi.Services
{
    public interface IOilPriceService
    {
        List<OilPrice> GetCurrentPrice();
    }

    public class OilPriceService : IOilPriceService
    {
        LogFunction logFunction;

        public OilPriceService() : this(LogHelper.LogError) { }
        public OilPriceService(LogFunction logFunction)
        {
            this.logFunction = logFunction;
        }

        public List<OilPrice> GetCurrentPrice()
        {
            try
            {
                using (WebClient client = new WebClient())
                {
                    string responseXmlString = client.DownloadString("https://crmmobile.bangchak.co.th/webservice/oil_price.aspx");

                    XmlDocument xDoc = new XmlDocument();
                    xDoc.LoadXml(responseXmlString);

                    List<OilPrice> results = new List<OilPrice>();
                    var headerNode = xDoc.SelectNodes("//header");
                    if (headerNode.Count == 0)
                        throw new Exception(ErrorMessage.MismatchXmlSchema);

                    var nodeList = xDoc.SelectNodes("//header//item");
                    foreach (XmlNode node in nodeList)
                    {
                        var typeNode = node.SelectNodes("type");
                        var todayPriceNode = node.SelectNodes("today");
                        if (typeNode.Count == 0 || todayPriceNode.Count == 0)
                            throw new Exception(ErrorMessage.MismatchXmlSchema);

                        results.Add(new OilPrice() { Name = typeNode[0].InnerText, Price = Convert.ToDecimal(todayPriceNode[0].InnerText) });
                    }

                    return results;
                }
            }
            catch(Exception e)
            {
                logFunction(e);
                return null;
            }
        }
    }
}