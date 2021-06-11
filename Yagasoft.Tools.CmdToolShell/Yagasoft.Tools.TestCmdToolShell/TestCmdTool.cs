#region Imports

using System;
using CommandLine;
using Yagasoft.Libraries.Common;
using Yagasoft.Tools.CmdToolTemplate.Attributes;
using Yagasoft.Tools.CmdToolTemplate.Shell;
using Yagasoft.Tools.CmdToolTemplate.Tool;

#endregion

namespace Yagasoft.Tools.TestCmdToolShell
{
	public class TestCmdTool : ICmdTool<Arguments>
	{
		private ShellArguments shellArgs;
		private Arguments args;
		private CrmLog log;

		public void Initialise(ShellArguments shellArguments, Arguments toolArguments, CrmLog crmLog)
		{
			shellArgs = shellArguments;
			args = toolArguments;
			log = crmLog;
		}

		public void Run()
		{
			log.Log("Tool ran!");
			log.Log($"Arg: {args.Test}");
			throw new NotImplementedException();
		}
	}

	[Verb("test", true, HelpText = "Run the tool.")]
	public class Arguments : ToolArgumentsBase
	{
		[Option('x', "xtest", Required = true, HelpText = "Tool arg test.")]
		public string Test { get; set; }
	}
}
