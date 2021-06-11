#region Imports

using CommandLine;
using Yagasoft.Libraries.Common;
using Yagasoft.Tools.CmdToolTemplate.Attributes;
using Yagasoft.Tools.CmdToolTemplate.Shell;

#endregion

namespace Yagasoft.Tools.CmdToolTemplate.Tool
{
	/// <summary>
	///     The tool's contract for the shell to invoke.<br />
	///     The command line arguments class should implement either<br />
	///     -- <see cref="DefaultToolArgumentsBase" />: requires the use of the switch '-t' on the command line to work<br />
	///     -- <see cref="ToolArgumentsBase" />: requires passing a verb as the first argument on the command line; the verb is
	///     recognised by decorating the class with <see cref="VerbAttribute" /><br />
	///     In either case, properties of the class map to command line arguments, and those properties should be decorated
	///     with <see cref="OptionAttribute" /> to be recognised.<br />
	///     Pass <see cref="DefaultToolArgumentsBase" /> as type argument if no command line arguments are needed.
	/// </summary>
	/// <typeparam name="TArguments"></typeparam>
	public interface ICmdTool<in TArguments>
		where TArguments : ToolArgumentsBase
	{
		void Initialise(ShellArguments shellArguments, TArguments toolArguments, CrmLog crmLog);
		void Run();
	}
}
