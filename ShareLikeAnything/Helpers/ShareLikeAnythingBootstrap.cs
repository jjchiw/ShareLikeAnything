using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Nancy.TinyIoc;
using Nancy;
using Raven.Client.Document;
using Raven.Client;
using Raven.Client.Extensions;
using Nancy.Conventions;
using Raven.Abstractions.Data;
using System.Web.Configuration;

namespace ShareLikeAnything.Helpers
{
	public class ShareLikeAnythingBootstrap : Nancy.DefaultNancyBootstrapper
	{
		protected override void ConfigureRequestContainer(TinyIoCContainer container, NancyContext context)
		{
			base.ConfigureRequestContainer(container, context);

			var parser = ConnectionStringParser<RavenConnectionStringOptions>.FromConnectionStringName("RavenHQ");
			parser.Parse();

			var documentStore = new DocumentStore
			{
				ApiKey = parser.ConnectionStringOptions.ApiKey,
				Url = parser.ConnectionStringOptions.Url
			};

			documentStore.Initialize();
			documentStore.DatabaseCommands.EnsureDatabaseExists("ShareLikeAnything");


			container.Register<IDocumentStore>(documentStore);
			container.Register<IDocumentSession>(documentStore.OpenSession("ShareLikeAnything"));

			var apiKey = WebConfigurationManager.AppSettings["dropboxApiKey"].ToString();
			var apiSecret = WebConfigurationManager.AppSettings["dropboxApiSecret"].ToString();
			var userToken = WebConfigurationManager.AppSettings["dropboxApiUserToken"].ToString();
			var userSecret = WebConfigurationManager.AppSettings["dropboxApiUserSecret"].ToString();

			var dropboxCredentials = new DropboxCredentials(apiKey, apiSecret, userToken, userSecret);

			container.Register<DropboxCredentials>(dropboxCredentials);

		}

		protected override void RequestStartup(TinyIoCContainer container, Nancy.Bootstrapper.IPipelines pipelines, NancyContext context)
		{
			base.RequestStartup(container, pipelines, context);

			pipelines.AfterRequest.AddItemToEndOfPipeline(ctx =>
			{
				var documentStore = container.Resolve<IDocumentStore>();
				var documentSession = container.Resolve<IDocumentSession>();
				if (ctx.Response.StatusCode != HttpStatusCode.InternalServerError)
				{
					documentSession.SaveChanges();
				}
				documentSession.Dispose();
				documentStore.Dispose();
			});
		}

		protected override void ConfigureConventions(NancyConventions conventions)
		{
			base.ConfigureConventions(conventions);

			conventions.StaticContentsConventions.Add(
				StaticContentConventionBuilder.AddDirectory("css", @"Content/css")
			);

			conventions.StaticContentsConventions.Add(
				StaticContentConventionBuilder.AddDirectory("js", @"Content/scripts")
			);

			conventions.StaticContentsConventions.Add(
				StaticContentConventionBuilder.AddDirectory("data", @"Content/data")
			);
		}
	}
}