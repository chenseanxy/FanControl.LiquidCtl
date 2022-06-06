using Newtonsoft.Json;
using System.Diagnostics;
using System.IO;
using Python.Runtime;
using FanControl.Plugins;

namespace FanControl.LiquidCtl
{
	class LiquidCtlExecutor
	{
		private readonly IPluginLogger _logger;
		dynamic liquidctl;

		public LiquidCtlExecutor(IPluginLogger logger)
		{
			this._logger = logger;
		}

		public void Init()
		{
			PythonEngine.Initialize();
			using (Py.GIL())
			{
				this.liquidctl = Py.Import("Plugins.FanControlLiquidCtl");
			}
			PythonEngine.BeginAllowThreads();
		}

		public T Execute<T>(string arguments)
		{
			// this._logger.Log($"exec {arguments}");
			string result;
			using (Py.GIL())
			{
				result = this.liquidctl.emulate_cli(arguments + " --json");
			}
			// this._logger.Log($"{arguments} => {result}");
			return JsonConvert.DeserializeObject<T>(result);
		}
	}
}
