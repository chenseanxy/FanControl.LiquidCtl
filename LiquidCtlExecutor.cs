using System;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace FanControl.LiquidCtl
{
	class LiquidCtlExecutor
	{
		public T Execute<T>(string arguments)
		{
			ProcessStartInfo start = new ProcessStartInfo();
			start.FileName = "liquidctl";
			start.Arguments = arguments + " --json";
			start.UseShellExecute = false;
			start.RedirectStandardOutput = true;
			start.CreateNoWindow = true;

			using (Process process = Process.Start(start))
			{
				using (StreamReader reader = process.StandardOutput)
				{
					string result = reader.ReadToEnd();
					return JsonConvert.DeserializeObject<T>(result);
				}
			}
		}
	}
}
