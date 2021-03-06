using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;

namespace Calway.Configuration
{

    abstract public class SqlTableConfigurationProvider : ConfigurationProvider
    {
        protected string ConnectionString = "";

        public SqlTableConfigurationProvider(SqlTableConfigurationOptions options)
        {
            this.UpdateAllowed = options.UpdateAllowed;
            this.AutoCreateKey = options.AutoCreateKey;
            this.ConnectionString = options.ConnectionString;
            this.CacheDuration = options.CacheDuration;
            this.TableName = options.TableName;
            this.ColumnNameKey = options.ColumnNameKey;
            this.ColumnNameSection = options.ColumnNameSection;
            this.ColumnNameValue = options.ColumnNameValue;
        }

        /// <summary>
        /// When True a snapshot is queried once at the start up and not updated/refreshed any more.
        /// </summary>
        protected bool UseSnapshot
        {
            get
            {
                return (this.CacheDuration < 0);
            }
        }

        /// <summary>
        /// Indicates whether a configuration setting can be changed/updated 
        /// </summary>
        protected bool UpdateAllowed
        {
            get;
            set;
        } = false;


        /// <summary>
        /// When True the updated setting is automatically created/added when it does not exist
        /// </summary>
        protected bool AutoCreateKey
        {
            get;
            set;
        } = false;


        /// <summary>
        /// When True certain actions within the provider are made thread safe
        /// </summary>
        protected bool UseSyncRoot
        {
            get
            {
                return (!this.UseSnapshot || this.UpdateAllowed);
            }
        }

        /// <summary>
        /// The cache duration in seconds, when -1 a snapshot is made of the data and infinitely cached
        /// </summary>
        public int CacheDuration
        {
            get;
            set;
        } = -1;

        /// <summary>
        /// The abstract database factory
        /// </summary>
        abstract protected DbProviderFactory Factory
        {
            get;            
        }

        public override void Set(string key, string value)
        {
            if (!this.UpdateAllowed)
            {
                throw new Exception("Update not allowed");
            }

            string lowerKey = key.ToLower();

            (string section, string keyName) = GetSectionAndKeyName(lowerKey);

            if (this.updateValue(section, keyName, value))
            {
                lock (this.syncRoot)
                {
                    base.Set(lowerKey, value);
                    this.CacheData[lowerKey] = DateTime.Now;
                }
            } else
            {
                bool keyExists = false;

                lock (this.syncRoot)
                {
                    keyExists = this.Data.ContainsKey(lowerKey);
                }

                if ((!keyExists) && (this.AutoCreateKey))
                {
                    (section, keyName) = GetSectionAndKeyName(key);

                    if (this.createKey(section, keyName, value))
                    {
                        lock (this.syncRoot)
                        {
                            this.Data.Add(lowerKey, value);
                            this.CacheData.Add(lowerKey,DateTime.Now);
                        }                            
                    }
                }
            }
        }

        protected DbConnection CreateConnection()
        {
            var connection = this.Factory.CreateConnection();
            connection.ConnectionString = this.ConnectionString;

            return connection;
        }

        private bool tryQueryValue(string section, string keyName,out string value)
        {
            bool result = false;
            value = null;

            using (var connection = this.CreateConnection())
            {
                DbCommand command = connection.CreateCommand();
                command.CommandText = $"SELECT {ColumnNameValue} FROM {TableName} WHERE {ColumnNameSection}=@SECTION AND {ColumnNameKey}=@KEY";

                command.Parameters.Add(new SqlParameter("SECTION",section));
                command.Parameters.Add(new SqlParameter("KEY",keyName));

                connection.Open();

                object val = command.ExecuteScalar();

                if (val != DBNull.Value)
                {
                    value = ((string)val).Trim();
                }

                result = true;
            }

            return result;
        }

        private bool createKey(string section, string keyName, string value)
        {
            bool result = false;

            using (var connection = this.CreateConnection())
            {
                DbCommand command = connection.CreateCommand();
                command.CommandText = $"INSERT {TableName}({ColumnNameSection},{ColumnNameKey},{ColumnNameValue}) VALUES(@SECTION,@KEY,@VALUE)";

                command.Parameters.Add(new SqlParameter("SECTION", section));
                command.Parameters.Add(new SqlParameter("KEY", keyName));
                command.Parameters.Add(new SqlParameter("VALUE", value));

                connection.Open();

                int affectedRows = command.ExecuteNonQuery();

                result = (affectedRows > 0);
            }

            return result;
        }


        private bool updateValue(string section, string keyName, string value)
        {
            bool result = false;
           
            using (var connection = this.CreateConnection())
            {
                DbCommand command = connection.CreateCommand();
                command.CommandText = $"UPDATE {TableName} SET {ColumnNameValue}=@VALUE WHERE {ColumnNameSection}=@SECTION AND {ColumnNameKey}=@KEY";

                command.Parameters.Add(new SqlParameter("SECTION", section));
                command.Parameters.Add(new SqlParameter("KEY", keyName));
                command.Parameters.Add(new SqlParameter("VALUE", (value == null ? (object)DBNull.Value : value)));

                connection.Open();

                int affectedRows = command.ExecuteNonQuery();

                result = (affectedRows > 0);
            }

            return result;
        }

        /// <summary>
        /// Splits the key into a section part and a key part when the key contais the keydelimiter ':'
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        protected (string,string) GetSectionAndKeyName(string key)
        {
            string keyName = key;
            string section = "";

            int index = key.IndexOf(ConfigurationPath.KeyDelimiter);

            if (index >= 0)
            {
                section = key.Substring(0, index);
                keyName = key.Substring(index + ConfigurationPath.KeyDelimiter.Length);
            }

            return (section, keyName);
        }

        protected Dictionary<string, DateTime> CacheData = new Dictionary<string, DateTime>();


        public override bool TryGet(string key, out string value)
        {
            key = key.ToLower();

            if (this.UseSnapshot)
            {
                return base.TryGet(key, out value);                
            }
            else
            {
                bool valid = false;

                lock (this.syncRoot)
                {
                    valid = base.TryGet(key, out value);                    
                }

                if (valid)
                {
                    DateTime expired;

                    lock (this.syncRoot)
                    {
                        expired = this.CacheData[key].Add(TimeSpan.FromSeconds(this.CacheDuration));
                    }

                    if (expired <= DateTime.Now)
                    {
                        (string section, string keyName) = GetSectionAndKeyName(key);
                                                
                        valid = this.tryQueryValue(section, keyName, out value);

                        if (valid)
                        {
                            lock (this.syncRoot)
                            {
                                this.CacheData[key] = DateTime.Now;
                                this.Data[key] = value;
                            }
                        }

                        return valid;
                    }
                    else
                    {
                        return true;
                    }                        
                } else
                    return false;
            }

        }

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

        private object syncRoot = new object();

        private void loadData()
        {
            this.Data.Clear();
            this.CacheData.Clear();

            using (var connection = this.CreateConnection())
            {
                DbCommand command = connection.CreateCommand();
                command.CommandText = $"SELECT {ColumnNameSection},{ColumnNameKey},{ColumnNameValue} FROM {TableName}";

                connection.Open();

                using (DbDataReader reader = command.ExecuteReader())
                {

                    while (reader.Read())
                    {
                        string section = reader[ColumnNameSection].ToString().Trim();
                        string key = reader[ColumnNameKey].ToString().Trim();

                        object value = reader[ColumnNameValue];

                        string str = null;
                        
                        if (value != DBNull.Value)
                        {
                            str = value.ToString().Trim();
                        }                        

                        string keyPath = (section + ConfigurationPath.KeyDelimiter + key).ToLower();

                        this.Data.Add(keyPath, str);
                        this.CacheData.Add(keyPath, DateTime.Now);
                    }
                }
            }
        }

        public override void Load()
        {
            if (this.UseSyncRoot)
            {
                lock (this.syncRoot)
                {
                    this.loadData();
                }
            }
            else
                this.loadData();
        }
    }

}
