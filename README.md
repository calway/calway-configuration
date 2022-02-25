# Calway.Configuration

This library adds an SqlServer table as Configuration provider

## Usage

Configure the Sql Provider in StartUp.

```csharp
public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.ConfigureAppConfiguration(configurationBuilder =>
                        configurationBuilder.AddSqlServerTableConfiguration()                   
                    );

```

Then use configuration as usual

```csharp
config.GetValue<int>("calendar:searchstepsize");
config.GetValue<bool>("agentmood:useagentmood");
```

By default the `DefaultConnectionString` is used to connect to the database, but this can be configured.

```csharp
webBuilder.ConfigureAppConfiguration(configurationBuilder =>
	configurationBuilder.AddSqlServerTableConfiguration(new SqlTableConfigurationOptions() 
	{ 
		ConnectionStringName = "DefaultConnectionString", 
		CacheDuration = 60, 
		UpdateAllowed = true 
	})
);
```