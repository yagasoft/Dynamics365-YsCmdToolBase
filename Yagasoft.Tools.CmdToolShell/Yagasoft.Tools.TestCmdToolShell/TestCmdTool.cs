#region Imports

using System;
using System.Management.Automation;
using Yagasoft.Libraries.Common;
using Yagasoft.Tools.CmdToolTemplate.Shell;
using Yagasoft.Tools.Common.Config;

#endregion

namespace Yagasoft.Tools.TestCmdletShell
{
	[Cmdlet("Test", "Run")]
	public class TestCmdTool : YsCmdletBase
	{
		protected override void ExecuteLogic()
		{
			Log.Log("Tool ran!");
			Log.Log(GetWorkingDirectory());
			throw new NotImplementedException();
		}

		protected override LogParams GetLogParams()
		{
			return
				new LogParams
				{
					FolderPath = "Logs",
					FileSplitMode = SplitMode.Both,
					Level = LogLevel.Debug,
					FileSplitFrequency = SplitFrequency.Daily,
					FileDateFormat = DateTime.Now.ToString("u")
				};
		}
	}
}
