#region Imports

using CommandLine;

#endregion

namespace Yagasoft.Tools.CmdToolTemplate.Attributes
{
	/// <summary>
	///     Must be implemented in each tool.<br />
	///     Must be decorated with:
	///     <code><![CDATA[[Verb("<verb-to-invoke-tool>", false, HelpText = "<tool-description>")]]]></code>
	/// </summary>
	public abstract class ToolArgumentsBase
	{ }

	[Verb("run", true, HelpText = "Run the tool.")]
	public class DefaultToolArgumentsBase : ToolArgumentsBase
	{ }
}
