using Microsoft.Extensions.DependencyInjection;
using Sitecore.DependencyInjection;
using Sitecore.Geek.Feature.Redirects.Repositories;
using System;

namespace Sitecore.Geek.Feature.Redirects.Pipelines.IoC
{
	public class RegisterRedirectsServices : IServicesConfigurator
	{
		public RegisterRedirectsServices()
		{
		}

		public void Configure(IServiceCollection serviceCollection)
		{
			serviceCollection.AddSingleton<IRedirectsRepository, RedirectsRepository>();
		}
	}
}