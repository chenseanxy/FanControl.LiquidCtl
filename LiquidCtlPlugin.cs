using FanControl.Plugins;
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
			if (detected_devices == null)
			{
				this._logger.Log("Liquidctl initialization failed, skipping");
				return;
			}
			foreach (var device in detected_devices)
			{
				// memorize pump / fan channels
				var interface_channels = new Dictionary<string, List<DeviceSensor>>();
				foreach (var channel in device.status)
				{
					if (!supported_units.Contains(channel.unit) || channel.value == null) { continue; }
					if (channel.unit == "%")
					{
						var sensor = new ControlSensor(device, channel, liquidctl);
						sensors[sensor.Id] = sensor;
						if (!interface_channels.ContainsKey(sensor.LiquidctlInterfaceName))
						{
							interface_channels[sensor.LiquidctlInterfaceName] = new List<DeviceSensor>();
						}
						interface_channels[sensor.LiquidctlInterfaceName].Add(sensor);
						_container.ControlSensors.Add(sensor);
						this._logger.Log($"Adding control sensor {sensor.Id}");
					}
					else
					{
						var sensor = new DeviceSensor(device, channel, liquidctl);
						sensors[sensor.Id] = sensor;
						if (channel.unit == "rpm") {
							if (!interface_channels.ContainsKey(sensor.LiquidctlInterfaceName))
							{
								interface_channels[sensor.LiquidctlInterfaceName] = new List<DeviceSensor>();
							}
							interface_channels[sensor.LiquidctlInterfaceName].Add(sensor);
							_container.FanSensors.Add(sensor);
							this._logger.Log($"Adding fan sensor {sensor.Id}");
						}
						if (channel.unit == "°C") {
							_container.TempSensors.Add(sensor);
							this._logger.Log($"Adding temp sensor {sensor.Id}");
						}
					}
				}

				// create additional ControlSensors if no ControlSensor is found for rpm channels
				foreach (var intf in interface_channels.Keys)
				{
					var channels = interface_channels[intf];
					if (channels.Count == 0 || channels.Exists(channel => channel is ControlSensor))
					{
						continue;
					}

					this._logger.Log(
						$"Interface {intf} has no control channel, using control only channel, " +
						$"real_channels=[{string.Join(",", channels.ConvertAll(chn=>chn.Id))}]"
					);

					var sensor = ControlOnlySensor.CopyFrom(channels[0]);
					if (sensors.ContainsKey(sensor.Id))
					{
						this._logger.Log($"Sensor {sensor.Id} key collision when trying to add control channel, skipped");
						continue;
					}

					sensors[sensor.Id] = sensor;
					_container.ControlSensors.Add(sensor);
					this._logger.Log($"Adding control only sensor {sensor.Id}");
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
