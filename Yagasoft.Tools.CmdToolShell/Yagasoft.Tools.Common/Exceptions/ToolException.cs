#region Imports

using System;

#endregion

namespace Yagasoft.Tools.Common.Exceptions
{
	public class ToolException : Exception
	{
		public ToolException(string message) : base(message)
		{ }
	}
}
