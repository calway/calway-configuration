using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;

namespace Calway.Configuration
{
    public class SqlServerTableConfigurationProvider : SqlTableConfigurationProvider
    {
        protected override DbProviderFactory Factory => SqlClientFactory.Instance;
        
        public SqlServerTableConfigurationProvider(SqlTableConfigurationOptions options) : base(options)
            {
            }
    }
}
