using Microsoft.Extensions.Configuration;

namespace Calway.Configuration
{
    public class SqlServerTableConfigurationSource : IConfigurationSource
    {
        public IConfigurationProvider Build(IConfigurationBuilder builder)
        {
            return new SqlServerTableConfigurationProvider(this.options);
        }

        SqlTableConfigurationOptions options = null;

        public SqlServerTableConfigurationSource(SqlTableConfigurationOptions options)
        {
            this.options = options;
        }
    }

}
