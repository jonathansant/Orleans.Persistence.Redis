#nullable enable
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Orleans.Configuration;
using Orleans.Persistence.Redis.Compression;
using Orleans.Persistence.Redis.Config;
using Orleans.Persistence.Redis.Core;
using Orleans.Persistence.Redis.Serialization;
using Orleans.Runtime.Hosting;
using Orleans.Serialization;
using Orleans.Storage;
using JsonSerializer = Orleans.Persistence.Redis.Serialization.JsonSerializer;
using OrleansSerializer = Orleans.Persistence.Redis.Serialization.OrleansSerializer;

// ReSharper disable once CheckNamespace
namespace Orleans.Hosting;

public static class RedisSiloBuilderExtensions
{
	public static RedisStorageOptionsBuilder AddRedisGrainStorage(this ISiloBuilder builder, string name)
		=> new(builder, name);

	public static RedisStorageOptionsBuilder AddRedisGrainStorageAsDefault(
		this ISiloBuilder builder
	) => builder.AddRedisGrainStorage("Default");

	internal static IServiceCollection AddRedisGrainStorage(
		this IServiceCollection services,
		string name,
		Action<OptionsBuilder<RedisStorageOptions>>? configureOptions = null
	)
	{
		configureOptions?.Invoke(services.AddOptions<RedisStorageOptions>(name));
		services.ConfigureNamedOptionForLogging<RedisStorageOptions>(name);

		// services.AddTransient<IConfigurationValidator>(sp => new DynamoDBGrainStorageOptionsValidator(sp.GetService<IOptionsSnapshot<RedisStorageOptions>>().Get(name), name));
		return services
				.AddKeyedSingleton<IGrainStateStore>(name, (sp, k) =>
					{
						var key = (string)k;
						var connection = sp.GetRequiredKeyedService<DbConnection>(key);
						var serializer = sp.GetRequiredKeyedService<ISerializer>(key);
						var humanReadableSerializer = sp.GetKeyedService<IHumanReadableSerializer>(key);
						var options = sp.GetRequiredService<IOptionsSnapshot<RedisStorageOptions>>();
						var logger = sp.GetRequiredService<ILogger<GrainStateStore>>();

						return ActivatorUtilities.CreateInstance<GrainStateStore>(
							sp,
							key,
							connection,
							options.Get(key),
							serializer,
							humanReadableSerializer,
							logger,
							sp
						);
					}
			)
			.AddKeyedSingleton(name, (sp, k) =>
				{
					var key = (string)k;
					var optionsSnapshot = sp.GetRequiredService<IOptionsSnapshot<RedisStorageOptions>>();
					var logger = sp.GetRequiredService<ILogger<DbConnection>>();
					return ActivatorUtilities.CreateInstance<DbConnection>(sp, optionsSnapshot.Get(key), logger);
				})
			.AddKeyedSingleton<IGrainStorage>(name, (sp, k) =>
				{
					var key = (string)k;
					var store = sp.GetRequiredKeyedService<IGrainStateStore>(key);
					var connection = sp.GetRequiredKeyedService<DbConnection>(key);
					return ActivatorUtilities.CreateInstance<RedisGrainStorage>(sp, key, store, connection);
				}
			)
			.AddGrainStorage(name, (sp, key) =>
				{
					var store = sp.GetRequiredKeyedService<IGrainStateStore>(key);
					var connection = sp.GetRequiredKeyedService<DbConnection>(key);
					return ActivatorUtilities.CreateInstance<RedisGrainStorage>(sp, key, store, connection);
				})
			;
	}

	internal static ISiloBuilder AddRedisDefaultSerializer(this ISiloBuilder builder, string name)
		=> builder.AddRedisSerializer<OrleansSerializer>(name);

	internal static ISiloBuilder AddRedisDefaultBrotliSerializer(this ISiloBuilder builder, string name)
		=> builder.AddRedisSerializer<BrotliSerializer>(name);

	internal static ISiloBuilder AddRedisDefaultHumanReadableSerializer(this ISiloBuilder builder, string name)
		=> builder.AddRedisHumanReadableSerializer<JsonSerializer>(
			name,
			provider => new object[] { RedisDefaultJsonSerializerSettings.Get(provider) }
		);

	internal static ISiloBuilder AddCompression<TCompression>(this ISiloBuilder builder, string name)
		where TCompression : ICompression
		=> builder.ConfigureServices(services =>
			services.AddKeyedSingleton<ICompression>(name, (provider, n)
				=> ActivatorUtilities.CreateInstance<TCompression>(provider)));

	internal static ISiloBuilder AddRedisSerializer<TSerializer>(
		this ISiloBuilder builder,
		string name,
		params object[] settings
	)
		where TSerializer : ISerializer
		=> builder.ConfigureServices(services =>
			services.AddKeyedSingleton<ISerializer>(name, (provider, n)
				=> ActivatorUtilities.CreateInstance<TSerializer>(provider, settings))
		);

	internal static ISiloBuilder AddRedisHumanReadableSerializer<TSerializer>(
		this ISiloBuilder builder,
		string name,
		params object[] settings
	)
		where TSerializer : IHumanReadableSerializer
		=> builder.ConfigureServices(services =>
			services.AddKeyedSingleton<IHumanReadableSerializer>(name, (provider, n)
				=> ActivatorUtilities.CreateInstance<TSerializer>(provider, settings))
		);

	internal static ISiloBuilder AddRedisHumanReadableSerializer<TSerializer>(
		this ISiloBuilder builder,
		string name,
		Func<IServiceProvider, object[]> cfg
	) where TSerializer : IHumanReadableSerializer
		=> builder.ConfigureServices(services =>
			services.AddKeyedSingleton<IHumanReadableSerializer>(name, (provider, n)
				=> ActivatorUtilities.CreateInstance<TSerializer>(provider, cfg?.Invoke(provider)))
		);
}

public static class RedisDefaultJsonSerializerSettings
{
	public static JsonSerializerSettings Get(IServiceProvider provider)
	{
		var settings = OrleansJsonSerializerSettings.GetDefaultSerializerSettings(provider);

		settings.ContractResolver = new DefaultContractResolver
		{
			NamingStrategy = new DefaultNamingStrategy
			{
				ProcessDictionaryKeys = false
			}
		};

		return settings;
	}
}