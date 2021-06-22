#region Imports

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Reflection;
using Yagasoft.Libraries.Common;
using Yagasoft.Tools.Common.Config;
using Yagasoft.Tools.Common.Exceptions;
using Yagasoft.Tools.Common.Helpers;

#endregion

namespace Yagasoft.Tools.CmdToolTemplate.Shell
{
	/// <summary>
	/// Must add attribute: <see cref="Cmdlet"/>.
	/// </summary>
	public abstract class YsCmdletBase : PSCmdlet, IDisposable
	{
		protected CrmLog Log;

		private LogParams logParams;
		private CrmLog log;
		private IDictionary<string, string> assembliesToBind;

		private bool disposed;

		protected sealed override void ProcessRecord()
		{
			logParams = GetLogParams();
			logParams.Require(nameof(logParams));

			log = GetLog($"shell");

			try
			{
				assembliesToBind = GetAssembliesToBind();

				AppDomain.CurrentDomain.AssemblyResolve +=
					(sender, e) =>
					{
						var requestedName = new AssemblyName(e.Name).Name;

						switch (requestedName)
						{
							case "Newtonsoft.Json":
								log.Log($"Loading assembly: {requestedName} ...");
								return Assembly.LoadFrom("Newtonsoft.Json.dll");

							default:
								if (assembliesToBind != null && assembliesToBind.TryGetValue(requestedName, out var assemblyFile)
									&& assemblyFile.IsFilled())
								{
									log.Log($"Loading assembly: {requestedName} ...");
									return Assembly.LoadFrom(assemblyFile);
								}
								else
								{
									return null;
								}
						}
					};

				log.Log($"Initialising cmdlet log ...");
				Log = GetLog($"cmdlet-{MyInvocation.InvocationName}");

				try
				{
					log.Log($"Executing cmdlet logic ...");
					ExecuteLogic();
					log.Log($"Logic ran successfully.");
				}
				catch (ToolException e)
				{
					Log.LogError(e.Message);
					Log.ExecutionFailed();
					log.LogError($"Cmdlet failed => {e.Message}.");
					throw;
				}
				catch (Exception e)
				{
					Log.Log(e);
					Log.ExecutionFailed();
					log.Log($"Cmdlet failed.");
					throw;
				}
			}
			catch (Exception e)
			{
				log.Log(e);
				log.ExecutionFailed();
				throw;
			}
		}

		protected string GetWorkingDirectory()
		{
			return SessionState.Path.CurrentFileSystemLocation.Path;
		}

		protected abstract LogParams GetLogParams();

		/// <summary>
		/// Binds the list of assemblies at runtime. The list should be assembly simple names mapped to a file.
		/// </summary>
		protected virtual IDictionary<string, string> GetAssembliesToBind()
		{
			return null;
		}

		protected abstract void ExecuteLogic();

		private CrmLog GetLog(string id)
		{
			var level = logParams.Level;
			var dateFormat = logParams.FileDateFormat;

			var frequency = logParams.FileSplitFrequency;
			var splitMode = logParams.FileSplitMode ?? SplitMode.Size;
			var isGroupById = logParams.IsGroupFilesById ?? true;
			var maxFileSizeKb = logParams.MaxFileSizeKb ?? int.MaxValue;

			var folder = logParams.FolderPath;
			folder = (folder == "." || folder.IsEmpty()) ? SessionState.Path.CurrentFileSystemLocation.Path : folder;

			if (isGroupById)
			{
				folder = Path.Combine(folder, id);
			}

			var newLog = new CrmLog(true, level);

			newLog.InitOfflineLog(Path.Combine(folder, $"log-{id}.csv"), false,
				new FileConfiguration
				{
					FileSplitMode = splitMode,
					MaxFileSizeKb = maxFileSizeKb < 1 ? int.MaxValue : maxFileSizeKb,
					FileSplitFrequency = frequency,
					FileDateFormat = DateTime.Now.ToString(dateFormat)
				});

			return newLog;
		}

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
	        if (disposed)
	        {
		        return;
	        }

	        if(disposing)
	        {
				log.Log("Finalising cmdlet log ...");
		        Log.LogExecutionEnd();
		        log.Log("Finalising shell log ...");
		        log.LogExecutionEnd();
	        }

	        disposed = true;
        }

        ~YsCmdletBase()
        {
            Dispose(false);
        }
	}
}
