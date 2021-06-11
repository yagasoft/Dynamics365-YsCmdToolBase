using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Yagasoft.Tools.Common.Connections;

namespace Yagasoft.Tools.Common.Helpers
{
    public static class ConnectionHelpers
    {
	    public static ConnectionStringConfig GetConnectionString(string connectionFile)
	    {
		    return ConfigHelpers.GetConfigurationParams<ConnectionStringConfig>(connectionFile);
	    }

	    public static string EscapePassword(string connectionString)
	    {
		    return Regex.Replace(connectionString.Trim(';') + ";", "password\\s*?=.+?;", "Password=******;",
			    RegexOptions.IgnoreCase);
	    }
    }
}
