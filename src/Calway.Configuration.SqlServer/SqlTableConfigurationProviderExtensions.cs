using Microsoft.Extensions.Configuration;
using System;

namespace Calway.Configuration
{

    public static class SqlServerTableConfigurationProviderExtensions
    {
        static public void AddSqlServerTableConfiguration(this IConfigurationBuilder builder, string connectionStringName = "DefaultConnectionString")
        {            
            SqlTableConfigurationOptions options = new SqlTableConfigurationOptions();
            options.ConnectionStringName = connectionStringName;

            SqlTableConfigurationProviderUtils.InitializeOptions(builder, options);

            builder.AddSqlServerTableConfiguration(options);
        }

        static public void AddSqlServerTableConfiguration(this IConfigurationBuilder builder, Action<SqlTableConfigurationOptions> optionsDelegate)
        {
            SqlTableConfigurationOptions options = new SqlTableConfigurationOptions();
            optionsDelegate.Invoke(options);

            builder.AddSqlServerTableConfiguration(options);
        }

        static public void AddSqlServerTableConfiguration(this IConfigurationBuilder builder, SqlTableConfigurationOptions options)
        {
            SqlTableConfigurationProviderUtils.InitializeOptions(builder, options);

            var configSource = new SqlServerTableConfigurationSource(options);

            SqlTableConfigurationProviderUtils.AddConfigSource(builder, configSource,options);
        }
    }

}
