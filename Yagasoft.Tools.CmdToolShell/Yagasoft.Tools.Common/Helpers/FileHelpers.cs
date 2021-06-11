using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yagasoft.Tools.Common.Helpers
{
    public static class FileHelpers
    {
	    public static void EnsureFolderExists(string path)
	    {
		    if (path != null && !Directory.Exists(path))
		    {
			    Directory.CreateDirectory(path);
		    }
	    }
    }
}
