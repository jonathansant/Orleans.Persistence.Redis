using Microsoft.Extensions.Options;
using Orleans.Hosting;
using Orleans.Persistence.Redis.Serialization;
using System;
using System.Collections.Generic;

namespace Orleans.Persistence.Redis.Config
{
	public class RedisStorageOptions
	{
		public IEnumerable<string> Servers { get; set; }
		public int Database { get; set; }
		public bool ThrowExceptionOnInconsistentETag { get; set; } = true;
		public bool HumanReadableSerialization { get; set; }
		public TimeSpan MaxRetryElapsedTimeAllowedMilliseconds { get; set; } = TimeSpan.FromSeconds(3);
		public TimeSpan CommandTimeout { get; set; } = TimeSpan.FromSeconds(5);
		public int ConnectRetry { get; set; } = 25;
		public string Password { get; set; }
		public string KeyPrefix { get; set; } = string.Empty;
		public string ClientName { get; set; }
		public bool UseSsl { get; set; }
		public string SslHost { get; set; }
		public float FieldSizeWarningThresholdInMb { get; set; } = 50;
		/// <summary>
		/// Segment Size in bytes, if not supplied, segments will not be used
		/// </summary>
		public int? SegmentSize { get; set; }
		/// <summary>
		/// Deletes old segments (default is true)
		/// </summary>
		public bool DeleteOldSegments { get; set; }

		internal Func<string, string> KeyBuilder { get; set; }

		/// <summary>
		/// Gets or sets the threshold warning timespan in order to log as warning.
		/// </summary>
		public TimeSpan ExecutionDurationWarnThreshold { get; set; } = TimeSpan.FromMilliseconds(400);

		/// <summary>
		/// Configures how the key for redis will be combined.
		/// </summary>
		/// <param name="keyBuilder">Function to configure. Should return the Key combined. NOTE: This will omit the KeyPrefix.</param>
		/// <returns></returns>
		public RedisStorageOptions WithKeyBuilder(Func<string, string> keyBuilder)
		{
			KeyBuilder = keyBuilder;
			return this;
		}
	}

	public class RedisStorageSiloHostBuilderOptionsBuilder
	{
		private readonly ISiloHostBuilder _builder;
		private bool _humanSerializerAdded;
		private bool _serializerAdded;
		private readonly string _name;

		public RedisStorageSiloHostBuilderOptionsBuilder(ISiloHostBuilder builder, string name)
		{
			_builder = builder;
			_name = name;
		}

		public RedisStorageSiloHostBuilderOptionsBuilder AddRedisSerializer<TSerializer>(params object[] settings)
			where TSerializer : ISerializer
		{
			_builder.AddRedisSerializer<TSerializer>(_name, settings);
			_serializerAdded = true;
			return this;
		}

		public RedisStorageSiloHostBuilderOptionsBuilder AddRedisHumanReadableSerializer<TSerializer>(params object[] settings)
			where TSerializer : IHumanReadableSerializer
		{
			_builder.AddRedisHumanReadableSerializer<TSerializer>(_name, settings);
			_humanSerializerAdded = true;
			return this;
		}

		public RedisStorageSiloHostBuilderOptionsBuilder AddRedisHumanReadableSerializer<TSerializer>(Func<IServiceProvider, object[]> cfg)
			where TSerializer : IHumanReadableSerializer
		{
			_builder.AddRedisHumanReadableSerializer<TSerializer>(_name, cfg);
			_humanSerializerAdded = true;
			return this;
		}

		public RedisStorageSiloHostBuilderOptionsBuilder AddDefaultRedisSerializer()
		{
			_builder.AddRedisDefaultSerializer(_name);
			_serializerAdded = true;
			return this;
		}

		public RedisStorageSiloHostBuilderOptionsBuilder AddRedisDefaultHumanReadableSerializer()
		{
			_builder.AddRedisDefaultHumanReadableSerializer(_name);
			_humanSerializerAdded = true;
			return this;
		}

		public ISiloHostBuilder Build(Action<OptionsBuilder<RedisStorageOptions>> configureOptions)
		{
			if (!_serializerAdded)
				_builder.AddRedisDefaultSerializer(_name);

			if (!_humanSerializerAdded)
				_builder.AddRedisDefaultHumanReadableSerializer(_name);

			return _builder
				.ConfigureServices(services => services.AddRedisGrainStorage(_name, configureOptions));
		}
	}

	public class RedisStorageOptionsBuilder
	{
		private readonly ISiloBuilder _builder;
		private bool _humanSerializerAdded;
		private bool _serializerAdded;
		private readonly string _name;

		public RedisStorageOptionsBuilder(ISiloBuilder builder, string name)
		{
			_builder = builder;
			_name = name;
		}

		public RedisStorageOptionsBuilder AddRedisSerializer<TSerializer>(params object[] settings)
			where TSerializer : ISerializer
		{
			_builder.AddRedisSerializer<TSerializer>(_name, settings);
			_serializerAdded = true;
			return this;
		}

		public RedisStorageOptionsBuilder AddRedisSerializer<TSerializer>(Func<IServiceProvider, object[]> cfg)
			where TSerializer : ISerializer
		{
			_builder.AddRedisSerializer<TSerializer>(_name, cfg);
			_serializerAdded = true;
			return this;
		}

		public RedisStorageOptionsBuilder AddRedisHumanReadableSerializer<TSerializer>(params object[] settings)
			where TSerializer : IHumanReadableSerializer
		{
			_builder.AddRedisHumanReadableSerializer<TSerializer>(_name, settings);
			_humanSerializerAdded = true;
			return this;
		}

		public RedisStorageOptionsBuilder AddRedisHumanReadableSerializer<TSerializer>(Func<IServiceProvider, object[]> cfg)
			where TSerializer : IHumanReadableSerializer
		{
			_builder.AddRedisHumanReadableSerializer<TSerializer>(_name, cfg);
			_humanSerializerAdded = true;
			return this;
		}

		public RedisStorageOptionsBuilder AddDefaultRedisSerializer()
		{
			_builder.AddRedisDefaultSerializer(_name);
			_serializerAdded = true;
			return this;
		}

		public RedisStorageOptionsBuilder AddRedisDefaultHumanReadableSerializer()
		{
			_builder.AddRedisDefaultHumanReadableSerializer(_name);
			_humanSerializerAdded = true;
			return this;
		}

		public ISiloBuilder Build(Action<OptionsBuilder<RedisStorageOptions>> configureOptions)
		{
			if (!_serializerAdded)
				_builder.AddRedisDefaultSerializer(_name);

			if (!_humanSerializerAdded)
				_builder.AddRedisDefaultHumanReadableSerializer(_name);

			return _builder
				.ConfigureServices(services => services.AddRedisGrainStorage(_name, configureOptions));
		}
	}
}
