using Newtonsoft.Json;
using System.Diagnostics;
using System.IO;

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
