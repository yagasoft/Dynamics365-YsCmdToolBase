#region Imports

using CommandLine;

#endregion

namespace Yagasoft.Tools.CmdToolTemplate.Shell
{
	public class ShellArguments
	{
		[Option('l', "list", HelpText = "Lists all tools that have a triggering verb.")]
		public bool IsListTools { get; set; }

		[Option('t', "tool", HelpText = "The tool assembly name; e.g. Company.Tools.TestTool.dll."
			+ " Alternatively, specify the tool's verb as the first argument of the shell.")]
		public string ToolAssembly { get; set; }

		[Option('P', "no-pause", Required = false, HelpText = "Prevent the tool from pausing on exit.")]
		public bool IsNoPauseOnExit { get; set; }
	}
}
