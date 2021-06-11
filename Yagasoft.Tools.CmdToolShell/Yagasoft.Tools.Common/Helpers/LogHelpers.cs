using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Yagasoft.Tools.Common.Helpers
{
    public static class LogHelpers
    {
	    public static void SetDefaultLogTypeParser()
	    {
		    Libraries.Common.Helpers.DefaultEvaluator =
			    o =>
				{
					try
					{
						if (o == null)
						{
							return "null";
						}

						var ser = 
							JsonConvert.SerializeObject(o,
								new JsonSerializerSettings
								{
									ContractResolver = new CamelCasePropertyNamesContractResolver(),
									Formatting = Formatting.Indented,
									ReferenceLoopHandling = ReferenceLoopHandling.Ignore
								});
						return ser;
					}
					catch
					{
						return o.ToString();
					}
				};

		    Libraries.Common.Helpers.IsAlwaysUseDefaultEvaluator = true;
	    }
    }
}
