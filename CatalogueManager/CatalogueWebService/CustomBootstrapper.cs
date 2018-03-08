using CatalogueLibrary.Repositories;
using CatalogueWebService.Cache;
using CatalogueWebService.Modules.Data;
using Nancy;
using Nancy.Bootstrapper;
using Nancy.TinyIoc;
using RDMPStartup;

namespace CatalogueWebService
{
    internal class CustomBootstrapper : DefaultNancyBootstrapper
    {
        protected override void RequestStartup(TinyIoCContainer container, IPipelines pipelines, NancyContext context)
        {
            //CORS Enable
            pipelines.AfterRequest.AddItemToEndOfPipeline(ctx => 
                ctx.Response.WithHeader("Access-Control-Allow-Origin", "*")
                    .WithHeader("Access-Control-Allow-Methods", "POST,GET")
                    .WithHeader("Access-Control-Allow-Headers", "Accept, Origin, Content-type"));

            base.RequestStartup(container, pipelines, context);
        }

        protected override void ConfigureApplicationContainer(TinyIoCContainer container)
        {
            base.ConfigureApplicationContainer(container);

            container.Register<ICacheProvider<ChartData>, KeyValueCache<ChartData>>();
            container.Register<IRDMPPlatformRepositoryServiceLocator, UserSettingsRepositoryFinder>();
        }
    }
}