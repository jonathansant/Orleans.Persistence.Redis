using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Orleans.Configuration;
using Orleans.Hosting;
using Orleans.Persistence.Redis.Core;
using Orleans.Persistence.Redis.Serialization;
using Orleans.Providers;
using Orleans.Runtime;
using Orleans.Storage;
using System;
using JsonSerializer = Orleans.Persistence.Redis.Serialization.JsonSerializer;

namespace Orleans.Persistence.Redis.Config
{
	public static class RedisSiloHostBuilderExtensions
	{
		public static ISiloHostBuilder AddRedisGrainStorage(
			this ISiloHostBuilder builder,
			string name,
			Action<OptionsBuilder<RedisStorageOptions>> configureOptions = null
		)
			=> builder.ConfigureServices(services => services.AddRedisGrainStorage(name, configureOptions));

		public static ISiloHostBuilder AddRedisGrainStorageAsDefault(
			this ISiloHostBuilder builder,
			Action<OptionsBuilder<RedisStorageOptions>> configureOptions = null
		)
			=> builder.AddRedisGrainStorage("Default", configureOptions);

		public static IServiceCollection AddRedisGrainStorage(
			this IServiceCollection services,
			string name,
			Action<OptionsBuilder<RedisStorageOptions>> configureOptions = null
		)
		{
			configureOptions?.Invoke(services.AddOptions<RedisStorageOptions>(name));
			// services.AddTransient<IConfigurationValidator>(sp => new DynamoDBGrainStorageOptionsValidator(sp.GetService<IOptionsSnapshot<RedisStorageOptions>>().Get(name), name));
			services.AddSingletonNamedService(name, CreateStateStore);
			services.ConfigureNamedOptionForLogging<RedisStorageOptions>(name);
			services.TryAddSingleton(sp => sp.GetServiceByName<IGrainStorage>(ProviderConstants.DEFAULT_STORAGE_PROVIDER_NAME));

			return services
				.AddSingletonNamedService(name, CreateDbConnection)
				.AddSingletonNamedService(name, CreateRedisStorage)
				.AddSingletonNamedService(name, (provider, n)
					=> (ILifecycleParticipant<ISiloLifecycle>)provider.GetRequiredServiceByName<IGrainStorage>(n));
		}

		public static ISiloHostBuilder AddRedisDefaultSerializer(this ISiloHostBuilder builder, string name, params object[] settings)
			=> builder.AddRedisSerializer<OrleansSerializer>(name, settings);

		public static ISiloHostBuilder AddRedisDefaultHumanReadableSerializer(this ISiloHostBuilder builder, string name)
			=> builder.AddRedisHumanReadableSerializer<JsonSerializer>(
				name,
				new JsonSerializerSettings
				{
					TypeNameHandling = TypeNameHandling.All,
					PreserveReferencesHandling = PreserveReferencesHandling.Objects,
					DateFormatHandling = DateFormatHandling.IsoDateFormat,
					DefaultValueHandling = DefaultValueHandling.Ignore,
					MissingMemberHandling = MissingMemberHandling.Ignore,
					NullValueHandling = NullValueHandling.Ignore,
					ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor,
				});

		public static ISiloHostBuilder AddRedisSerializer<TSerializer>(this ISiloHostBuilder builder, string name, params object[] settings)
			where TSerializer : ISerializer
			=> builder.ConfigureServices(services =>
					services.AddSingletonNamedService<ISerializer>(name, (provider, n)
						=> ActivatorUtilities.CreateInstance<TSerializer>(provider, settings))
			);

		public static ISiloHostBuilder AddRedisHumanReadableSerializer<TSerializer>(this ISiloHostBuilder builder, string name, params object[] settings)
			where TSerializer : IHumanReadableSerializer
			=> builder.ConfigureServices(services =>
				services.AddSingletonNamedService<IHumanReadableSerializer>(name, (provider, n)
					=> ActivatorUtilities.CreateInstance<TSerializer>(provider, settings))
			);

		private static IGrainStorage CreateRedisStorage(IServiceProvider services, string name)
		{
			var store = services.GetRequiredServiceByName<IGrainStateStore>(name);
			var connection = services.GetRequiredServiceByName<DbConnection>(name);
			return ActivatorUtilities.CreateInstance<RedisGrainStorage>(services, name, store, connection);
		}

		private static IGrainStateStore CreateStateStore(IServiceProvider provider, string name)
		{
			var connection = provider.GetRequiredServiceByName<DbConnection>(name);
			var serializer = provider.GetRequiredServiceByName<ISerializer>(name);
			var humanReadableSerializer = provider.GetServiceByName<IHumanReadableSerializer>(name);
			var options = provider.GetRequiredService<IOptionsSnapshot<RedisStorageOptions>>();
			return ActivatorUtilities.CreateInstance<GrainStateStore>(
				provider,
				connection,
				options.Get(name),
				serializer,
				humanReadableSerializer
			);
		}

		private static DbConnection CreateDbConnection(IServiceProvider provider, string name)
		{
			var optionsSnapshot = provider.GetRequiredService<IOptionsSnapshot<RedisStorageOptions>>();
			var logger = provider.GetRequiredService<ILogger<DbConnection>>();
			return ActivatorUtilities.CreateInstance<DbConnection>(provider, optionsSnapshot.Get(name), logger);
		}
	}
}