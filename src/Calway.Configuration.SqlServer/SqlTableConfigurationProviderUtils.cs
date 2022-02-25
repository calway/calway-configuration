using Microsoft.Extensions.Configuration;

namespace Calway.Configuration
{
    public static class SqlTableConfigurationProviderUtils
    {
        public static void InitializeOptions(IConfigurationBuilder builder, SqlTableConfigurationOptions options)
        {
            if (string.IsNullOrEmpty(options.ConnectionString))
            {
                var tempConfig = builder.Build();
                options.ConnectionString = tempConfig.GetConnectionString(options.ConnectionStringName);
            }
        }

        public static void AddConfigSource(IConfigurationBuilder builder,IConfigurationSource configSource, SqlTableConfigurationOptions options)
        {

            if (options.OverrulesAll)
            {
                builder.Add(configSource);
            }
            else
            {
                builder.Sources.Insert(0, configSource);
            }
        }
    }

}
