using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Configuration;
using SFA.DAS.Payments.Core.Configuration;

namespace SFA.DAS.Payments.ScheduledJobs.V1.Configuration
{
    public class ConfigurationHelper : IConfigurationHelper
    {
        private readonly IConfiguration _configuration;

        public ConfigurationHelper(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string GetSetting(string key)
        {
            return _configuration[key];
        }

        public string GetSettingOrDefault(string key, string defaultValue)
        {
            return _configuration[key] ?? defaultValue;
        }

        public string GetConnectionString(string name)
        {
            return _configuration.GetConnectionString(name);
        }

        public bool HasSetting(string sectionName, string settingName)
        {
            return !string.IsNullOrEmpty(_configuration[settingName]);
        }

        public string GetSetting(string sectionName, string settingName)
        {
            return _configuration[settingName];
        }

    }
}
