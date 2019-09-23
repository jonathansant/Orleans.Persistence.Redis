# Orleans.Persistence.Redis
Redis persistence for Microsoft Orleans with configurable serialization.

[![NuGet version](https://badge.fury.io/nu/Sucrose.Orleans.Persistence.Redis.svg)](https://badge.fury.io/nu/Sucrose.Orleans.Persistence.Redis)

## Installation
To start working with the `Orleans.Persistence.Redis` make sure you do the following steps:

1. Install Redis on a machine (or cluster) which you have access to.
3. Install the `Sucrose.Orleans.Persistence.Redis` nuget from the nuget repository.
4. Add to the Silo configuration the new persistence provider with the necessary parameters and the optional ones (if you wish). You can see what is configurable in `RedisGrainStorage` under [Configurable Values](#configurableValues).

Example RedisGrainStorage configuration:
```CSharp
public class SiloBuilderConfigurator : ISiloBuilderConfigurator
{
  public void Configure(ISiloHostBuilder hostBuilder)
    => hostBuilder
      .AddRedisGrainStorage("TestingProvider")
      .Build(builder => builder.Configure(opts =>
        {
          opts.Servers = new List<string> { "localhost" };
          opts.ClientName = "testing";
          opts.KeyPrefix = "My-Key-Prefix";
        })
      )
}
```

## <a name="configurableValues"></a>Configurable Values
These are the configurable values that the `Orleans.Persistence.Redis`:

- **Servers**: The list of servers the client will connect with.
- **HumanReadableSerialization** Specifies which type of serializer to use (Non-Human readable serialization is more performant). *Default value: false*
- **ThrowExceptionOnInconsistentETag** Specifies whether the state is validated. *Default value: true*
- **Database** Specifies which Redis database to use. *Default value: 0*
- **ConnectRetry** The number of times the client will attempt to connect to the Redis server. *Default value: 25*
- **Password** Password to authenticate with the server.
- **KeyPrefix** a prefix that gets appended with every key. *Default value: ""*
- **ClientName** The client's name.
