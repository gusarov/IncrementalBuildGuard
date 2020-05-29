using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IncrementalBuildGuard
{
	public class IncrementalBuildGuardLogger : Logger
	{
		private string _flagFile;
		
		public override void Initialize(IEventSource eventSource)
		{
			if (null == Parameters)
			{
				throw new LoggerException("Flag file was not set.");
			}

			Console.WriteLine("Parameters: " + Parameters);
			var parameters = Parameters.Split(';');
			_flagFile = parameters[0];
			if (string.IsNullOrEmpty(_flagFile))
			{
				throw new LoggerException("Log file was not set.");
			}

			if (parameters.Length > 1)
			{
				throw new LoggerException("Too many parameters passed.");
			}

			// eventSource.ProjectStarted += eventSource_ProjectStarted;
			eventSource.TargetStarted += TargetStarted;
			eventSource.TaskStarted += TaskStarted;
			// eventSource.ProjectFinished += new ProjectFinishedEventHandler(eventSource_ProjectFinished);
		}

		private void TaskStarted(object sender, TaskStartedEventArgs e)
		{
			if (string.Equals(e.TaskName, "csc", StringComparison.OrdinalIgnoreCase))
			{
				var fk = Console.ForegroundColor;
				Console.ForegroundColor = ConsoleColor.Red;
				var det = JsonConvert.SerializeObject(e, Formatting.Indented);
				Console.WriteLine(det);
				Console.WriteLine("Parameters: " + Parameters);
				Console.WriteLine("_flagFile: " + _flagFile);
				Console.ForegroundColor = fk;
				File.AppendAllText(_flagFile, det);
			}
		}

		private void TargetStarted(object sender, TargetStartedEventArgs e)
		{
			// Console.WriteLine(JsonConvert.SerializeObject(e, Formatting.Indented));
		}

	}
}
