using Orleans.Configuration;
using Orleans.Runtime;
using System.Net;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TestGrains;

namespace TestClient
{
	class Program
	{
		private static async Task Main(string[] args)
		{
			Console.Title = "Client";

			var clientTask = StartClientWithRetries();

			var clusterClient = await clientTask;

			var grainId = "PLAYER-5a98c80e-26b8-4d1c-a5da-cb64237f2392";
			var testGrain = clusterClient.GetGrain<ITestGrain>(grainId);
			var testGrain2 = clusterClient.GetGrain<ITestGrain2>(grainId);

			var result = await testGrain.GetThePhrase();
			await testGrain2.Save();
			await testGrain2.Save();

			await testGrain2.Delete();
			await testGrain2.Save();

			Console.BackgroundColor = ConsoleColor.DarkMagenta;
			Console.WriteLine(result);

			await Task.Delay(2000000);
		}

		private static async Task<IClusterClient> StartClientWithRetries(int initializeAttemptsBeforeFailing = 7)
		{
			var attempt = 0;
			IClusterClient client;
			while (true)
			{
				try
				{
					var siloAddress = IPAddress.Loopback;
					var gatewayPort = 30000;

					var host = new HostBuilder()
						.UseOrleansClient(clientBuilder =>
							{
								clientBuilder.Configure<ClusterOptions>(options =>
								{
									options.ClusterId = "TestCluster";
									options.ServiceId = "123";
								});
								clientBuilder.UseStaticClustering(options =>
									options.Gateways.Add(( new IPEndPoint(siloAddress, gatewayPort) ).ToGatewayUri()));
							})
							.Build();
					client = host.Services.GetRequiredService<IClusterClient>();
					await host.StartAsync();

					Console.WriteLine("Client successfully connect to silo host");
					break;
				}
				catch (Exception)
				{
					attempt++;
					Console.WriteLine(
						$"Attempt {attempt} of {initializeAttemptsBeforeFailing} failed to initialize the Orleans client.");
					if (attempt > initializeAttemptsBeforeFailing)
					{
						throw;
					}
					Thread.Sleep(TimeSpan.FromSeconds(3));
				}
			}

			return client;
		}
	}
}