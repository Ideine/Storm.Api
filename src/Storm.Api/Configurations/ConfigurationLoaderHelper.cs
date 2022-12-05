using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace Storm.Api.Configurations
{
	public static class ConfigurationLoaderHelper
	{
		public static void LoadConfiguration(this IConfigurationBuilder configurationBuilder, IWebHostEnvironment hostingEnvironment)
		{
			EnvironmentHelper.SetFromEnvironment(hostingEnvironment.EnvironmentName);

			configurationBuilder
				.AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
				.AddJsonFile("projectsettings.json", optional: true, reloadOnChange: false)
				.AddJsonFile($"appsettings.{hostingEnvironment.EnvironmentName}.json", optional: true, reloadOnChange: false)
				.AddJsonFile($"projectsettings.{hostingEnvironment.EnvironmentName}.json", optional: true, reloadOnChange: false)
				.AddEnvironmentVariables();
		}
	}
}