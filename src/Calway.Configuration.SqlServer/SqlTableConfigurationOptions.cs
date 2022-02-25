using System;
using System.Collections.Generic;
using System.Text;

namespace Calway.Configuration
{
    public class SqlTableConfigurationOptions
    {
        /// <summary>
        /// The name of the connection string (configured in the ConnectionStrings section in appsettings.json)
        /// </summary>
        public string ConnectionStringName
        {
            get;
            set;
        } = "DefaultConnectionString";

        /// <summary>
        /// Indicates whether the configuration settings can be set/updated
        /// </summary>
        public bool UpdateAllowed
        {
            get;
            set;
        } = false;

        /// <summary>
        /// The connection string, when not specified the ConnectionStringName is used
        /// </summary>
        public string ConnectionString
        {
            get;
            set;
        } = "";

        /// <summary>
        /// When True the settings from the config take priority\overrules the settings from other locations (poviders)
        /// </summary>
        public bool OverrulesAll
        {
            get;
            set;
        } = false;

        /// <summary>
        /// The cache duration of a setting in seconds; when -1 the settings are cached infinitely
        /// </summary>
        public int CacheDuration
        {
            get;
            set;
        } = -1;

        /// <summary>
        /// The name of the table
        /// </summary>
        public string TableName
        {
            get;
            set;
        } = "CONFIG";

        public string ColumnNameSection
        {
            get;
            set;
        } = "SECTION";

        public string ColumnNameKey
        {
            get;
            set;
        } = "KEYNAME";

        public string ColumnNameValue
        {
            get;
            set;
        } = "VALUE";
    }
}
