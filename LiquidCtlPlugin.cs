﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;

using FanControl.Plugins;

namespace FanControl.LiquidCtl
{
	public class LiquidCtlPlugin : IPlugin2
	{
		public string Name => "liquidctl";

		private LiquidCtlExecutor liquidctl;
		private Dictionary<string, DeviceSensor> sensors;

		private readonly object _lock = new object();

		public void Close()
		{
			return;
		}

		public void Initialize()
		{
			this.liquidctl = new LiquidCtlExecutor();
			Console.WriteLine("Initializing");
			var devices = this.liquidctl.Execute<List<DeviceInitResult>>("initialize");
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
					if (!supported_units.Contains(channel.unit)) { continue; }
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
					var sensor = new DeviceSensor(device, channel, liquidctl);
					if (!this.sensors.ContainsKey(sensor.Id)) { continue; }
					sensors[sensor.Id].Update(channel);
				}
			}
		}
	}
}