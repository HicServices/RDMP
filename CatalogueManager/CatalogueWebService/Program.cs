using System;
using System.ServiceModel;
using System.ServiceModel.Web;
using Nancy.Hosting.Wcf;

namespace CatalogueWebService
{
    class Program
    {
        static void Main(string[] args)
        {
            // localhost:8732/Design_Time_Addresses is a special namespace set up by NET3.5+ to allow WCF development without administrator access
            var service = new NancyWcfGenericService(new CustomBootstrapper());
            var host = new WebServiceHost(service, new Uri("http://localhost:8732/Design_Time_Addresses/CatalogueWS"));
            host.AddServiceEndpoint(typeof (NancyWcfGenericService), new WebHttpBinding(), "");
            host.Open();

            Console.WriteLine("Press <return> to end...");
            Console.ReadLine();
        }
    }
}
