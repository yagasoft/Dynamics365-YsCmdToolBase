using Yagasoft.Libraries.Common;

namespace Yagasoft.Tools.Common.Config
{
	public class LogParams
	{
		public string FolderPath { get; set; }
		public LogLevel? Level { get; set; }
		public string FileDateFormat { get; set; }
		public SplitFrequency? FileSplitFrequency { get; set; }
		public SplitMode? FileSplitMode { get; set; }
		public bool? IsGroupFilesById { get; set; }
		public int? MaxFileSizeKb { get; set; }
	}
}
