using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OMNI
{
    internal class AppConfig
    {
        public static IConfigurationRoot Get()
        {
            IConfigurationBuilder builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
            IConfigurationRoot Configuration = builder.Build();

            return Configuration;
        }

    }
}
