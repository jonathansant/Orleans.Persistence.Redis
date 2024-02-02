using Microsoft.Extensions.Hosting;
using Orleans.Configuration;
using System.Net;

namespace TestSilo
{
	internal class Program
	{
		public static async Task Main(string[] args)
		{
			Console.Title = "Silo 1";

			const int siloPort = 11111;
			const int gatewayPort = 30000;
			var siloAddress = IPAddress.Loopback;

			var builder = new HostBuilder().UseOrleans(siloBuilder =>
			{
				siloBuilder.Configure<ClusterOptions>(options =>
					{
						//options.SiloName = "TestCluster";
						options.ClusterId = "TestCluster";
						options.ServiceId = "123";
					})
					.UseDevelopmentClustering(
						options => options.PrimarySiloEndpoint = new IPEndPoint(siloAddress, siloPort))
					.ConfigureEndpoints(siloAddress, siloPort, gatewayPort)
					.AddRedisGrainStorage("Test")
					.Build(optionsBuilder =>
					{
						optionsBuilder.Configure(opts =>
						{
							opts.Servers = new[] { "localhost" };
						});
					})
					;
			});


			var host = builder.Build();
			await host.StartAsync();

			Console.ReadKey();

			await host.StopAsync();
		}
	}
}