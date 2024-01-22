using FanControl.Plugins;
using System;
using System.Collections.Generic;

namespace FanControl.LiquidCtl
{
	public class LiquidCtlPlugin : IPlugin2
	{
		public string Name => "liquidctl";

		private LiquidCtlExecutor liquidctl;
		private Dictionary<string, DeviceSensor> sensors;
		private readonly IPluginLogger _logger;

		public LiquidCtlPlugin(IPluginLogger logger)
		{
			_logger = logger;
		}

		public void Close()
		{
			return;
		}

		public void Initialize()
		{
			this.liquidctl = new LiquidCtlExecutor(this._logger);
			this.liquidctl.Init();
			var devices = this.liquidctl.Execute<List<DeviceInitResult>>("initialize all");
			this.sensors = new Dictionary<string, DeviceSensor>();
		}

		public void Load(IPluginSensorsContainer _container)
		{
			var detected_devices = this.liquidctl.Execute<List<DeviceStatus>>("status");
			var supported_units = new List<string> { "°C", "rpm", "%" };
			foreach (var device in detected_devices)
			{
				foreach (var channel in device.status)
				{
					if (!supported_units.Contains(channel.unit) || channel.value == null) { continue; }
					if (channel.unit == "%")
					{
						var sensor = new ControlSensor(device, channel, liquidctl);
						sensors[sensor.Id] = sensor;
						_container.ControlSensors.Add(sensor);
					}
					else
					{
						var sensor = new DeviceSensor(device, channel, liquidctl);
						sensors[sensor.Id] = sensor;
						if (channel.unit == "rpm") { _container.FanSensors.Add(sensor); }
						if (channel.unit == "°C") { _container.TempSensors.Add(sensor); }
					}
				}
			}
		}

		public void Update()
		{
			var detected_devices = this.liquidctl.Execute<List<DeviceStatus>>("status");
			foreach (var device in detected_devices)
			{
				foreach (var channel in device.status)
				{
					if (channel.value == null) { continue; }
					var sensor = new DeviceSensor(device, channel, liquidctl);
					if (!this.sensors.ContainsKey(sensor.Id)) { continue; }
					sensors[sensor.Id].Update(channel);
				}
			}
		}
	}
}
