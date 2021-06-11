#region Imports

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using CommandLine;
using Yagasoft.Libraries.Common;
using Yagasoft.Tools.CmdToolTemplate.Attributes;
using Yagasoft.Tools.CmdToolTemplate.Shell;
using Yagasoft.Tools.CmdToolTemplate.Tool;
using Yagasoft.Tools.Common.Exceptions;
using Yagasoft.Tools.Common.Helpers;

#endregion

namespace Yagasoft.Tools.CmdToolShell
{
	/// <summary>
	///     Author: Ahmed Elsawalhy (yagasoft.com)
	/// </summary>
	[Log]
	class Program
	{
		private static CrmLog log;

		private static bool isPauseOnExit = true;

		private static ShellArguments arguments;
		private static ToolArgumentsBase toolArgs;
		private static Type toolType;
		private static Type toolArgumentsType;

		[NoLog]
		static int Main(string[] args)
		{
			LogHelpers.SetDefaultLogTypeParser();

			log = GetLog("CmdToolShell");
			log.LogWarning("For shell help, add '--help' switch to the command line."
				+ " For tool help, either invoke a tool verb or use '-t' switch along with '--help'.");

			try
			{
				if (!ParseShellCmdArgs(args))
				{
					throw new ToolException("Failed to process shell command line arguments.");
				}

				if (toolType == null)
				{
					return 0;
				}

				if (!ParseToolCmdArgs(args))
				{
					throw new ToolException("Failed to process tool command line arguments.");
				}

				log.Log("Instantiating ...");

				var toolLog = GetLog(toolType.Name);

				try
				{
					try
					{
						var tool = Activator.CreateInstance(toolType);
						toolLog.Log("Executing ...");
						toolType.InvokeMember(nameof(ICmdTool<ToolArgumentsBase>.Initialise), BindingFlags.InvokeMethod, null, tool,
							new object[] { arguments, toolArgs, toolLog });
						toolType.InvokeMember(nameof(ICmdTool<ToolArgumentsBase>.Run), BindingFlags.InvokeMethod, null, tool, null);

					}
					catch (TargetInvocationException e) when (e.InnerException != null)
					{
						throw e.InnerException;
					}
				}
				catch (ToolException e)
				{
					toolLog.LogError(e.Message);
					toolLog.ExecutionFailed();
				}
				catch (Exception e)
				{
					toolLog.Log(e);
					toolLog.ExecutionFailed();
				}
				finally
				{
					toolLog.LogExecutionEnd();
				}

				return 0;
			}
			catch (ToolException e)
			{
				log.LogError(e.Message);
				log.ExecutionFailed();
				return 0;
			}
			catch (Exception e)
			{
				log.Log(e);
				log.ExecutionFailed();
				return 1;
			}
			finally
			{
				log.LogExecutionEnd();

				if (isPauseOnExit)
				{
					Console.WriteLine();
					Console.WriteLine("Press any key to exit ...");
					Console.ReadKey();
				}
			}
		}

		private static bool ParseShellCmdArgs(string[] args)
		{
			if (args.IsEmpty())
			{
				return false;
			}

			return
				new Parser(with =>
						   {
							   with.HelpWriter = Console.Out;
							   with.AutoHelp = true;
							   with.IgnoreUnknownArguments = true;
						   })
					.ParseArguments<ShellArguments>(args)
					.MapResult(
						o =>
						{
							arguments = o;

							if (arguments.IsListTools)
							{
								var tools = GetTools().ToArray();

								if (tools.Any())
								{
									log.Log($"\r\n{tools.Select(t => t.Verb).StringAggregate("\r\n")}");
								}
								else
								{
									log.LogWarning("No verb-based tools found. Use '-t' if you know the tool assembly.");
								}

								return true;
							}

							log.Log("Searching for tool ...");

							if (arguments.ToolAssembly.IsFilled())
							{
								var assemblyPath = arguments.ToolAssembly;

								log.Log($"Looking inside {assemblyPath} ...");

								if (!File.Exists(assemblyPath))
								{
									throw new ToolException("Tool assembly not found.");
								}

								var toolTypeInner = LoadToolType(assemblyPath);
								toolType = toolTypeInner ?? throw new ToolException("Tool class not found in assembly.");
								toolArgumentsType = GetToolArgumentsType(toolType);
							}
							else
							{
								var tool = SearchForTool(GetTools(), args);

								if (tool != null)
								{
									toolType = tool.Type;
									toolArgumentsType = tool.ArgumentType;
								}
							}

							if (toolType == null)
							{
								throw new ToolException("Either specify a tool assembly argument (-t) or a verb for an existing tool to invoke.");
							}

							log.Log($"Found: {toolType.Name}.");

							if (toolArgumentsType == null)
							{
								throw new ToolException("Tool arguments field not found. The field type must inherit from ToolArgumentsBase.");
							}

							isPauseOnExit = !o.IsNoPauseOnExit;

							return true;
						},
						_ => false);
		}

		private static IEnumerable<ToolInfo> GetTools()
		{
			var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

			if (path.IsEmpty())
			{
				throw new ToolException("Could not find the execution directory.");
			}

			var assemblies = Directory.GetFiles(path, "*.dll")
				.Where(f => new[] { "Microsoft", "Giacomo", "Newton", "Dmitry" }
					.All(e => !FileVersionInfo.GetVersionInfo(f).LegalCopyright.Contains(e)));

			return
				assemblies.Select(
					a =>
					{
						log.Log($"Looking inside {Path.GetFileName(a)} ...");

						try
						{
							var toolTypeInner = LoadToolType(a);

							if (toolTypeInner != null)
							{
								var argsTypeInner = GetToolArgumentsType(toolTypeInner);
								var verb = argsTypeInner?.GetCustomAttributes<VerbAttribute>().FirstOrDefault()?.Name;

								if (argsTypeInner != null && verb.IsFilled())
								{
									return new ToolInfo(verb, toolTypeInner, argsTypeInner);
								}
							}
						}
						catch
						{
							// ignored
						}

						return null;
					})
					.Where(t => t != null && t.Verb != "run")
					.ToArray();
		}

		private static ToolInfo SearchForTool(IEnumerable<ToolInfo> tools, string[] args)
		{
			return tools.FirstOrDefault(t => t.Verb == args.FirstOrDefault());
		}

		private static Type LoadToolType(string assemblyPath)
		{
			var assembly = Assembly.LoadFrom(assemblyPath);
			return assembly.GetTypes().FirstOrDefault(t => t.GetInterfaces()
				.Any(i => i.IsGenericType && typeof(ICmdTool<>).IsAssignableFrom(i.GetGenericTypeDefinition())));
		}

		private static Type GetToolArgumentsType(Type toolTypeParam)
		{
			return toolTypeParam
				.GetInterfaces().FirstOrDefault(i => typeof(ICmdTool<>).IsAssignableFrom(i.GetGenericTypeDefinition()))?
				.GetGenericArguments().FirstOrDefault(a => typeof(ToolArgumentsBase).IsAssignableFrom(a));
		}

		private static bool ParseToolCmdArgs(string[] args)
		{
			log.Log("Parsing tool command line arguments ...");
			return
				new Parser(with =>
						   {
							   with.HelpWriter = Console.Out;
							   with.AutoHelp = true;
							   with.IgnoreUnknownArguments = true;
						   })
					.ParseArguments(args, toolArgumentsType)
					.MapResult(
						o =>
						{
							toolArgs = o as ToolArgumentsBase;
							return true;
						},
						_ => false);
		}

		[NoLog]
		private static CrmLog GetLog(string id)
		{
			var folder = ConfigHelpers.Get("LogFolderPath");
			folder = folder == "." ? AppDomain.CurrentDomain.BaseDirectory : folder;

			var level = ConfigHelpers.Get("LogLevel");
			var dateFormat = ConfigHelpers.Get("LogFileDateFormat");

			int? frequency = null;

			if (int.TryParse(ConfigHelpers.Get("LogFileSplitFrequency"), out var frequencyParse))
			{
				frequency = frequencyParse;
			}

			if (!int.TryParse(ConfigHelpers.Get("LogFileSplitMode"), out var splitMode))
			{
				splitMode = (int)SplitMode.Size;
			}

			if (!bool.TryParse(ConfigHelpers.Get("LogFileGroupById"), out var groupById))
			{
				groupById = true;
			}

			if (!int.TryParse(ConfigHelpers.Get("LogFileMaxSizeKb"), out var maxSizeKb))
			{
				maxSizeKb = int.MaxValue;
			}

			if (groupById)
			{
				folder = Path.Combine(folder, id);
			}

			var log = new CrmLog(true, (LogLevel)int.Parse(level));

			log.InitOfflineLog(Path.Combine(folder, $"log-{id}.csv"), false,
				new FileConfiguration
				{
					FileSplitMode = (SplitMode)splitMode,
					MaxFileSizeKb = maxSizeKb < 1 ? int.MaxValue : maxSizeKb,
					FileSplitFrequency = (SplitFrequency?)frequency,
					FileDateFormat = DateTime.Now.ToString(dateFormat)
				});

			return log;
		}
	}

	internal class ToolInfo : Exception
	{
		public string Verb { get; }
		public Type Type { get; }
		public Type ArgumentType { get; }

		public ToolInfo(string verb, Type type, Type argumentType)
		{
			Verb = verb;
			Type = type;
			ArgumentType = argumentType;
		}
	}
}
