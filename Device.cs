using FanControl.Plugins;
using System;
using System.Collections.Generic;

namespace FanControl.LiquidCtl
{
	public class DeviceInitResult { }
	public class StatusValue
	{
		public string key { get; set; }
		public double? value { get; set; }
		public string unit { get; set; }
	}



	public class DeviceStatus
	{
		public string bus { get; set; }
		public string address { get; set; }
		public string description { get; set; }
		public List<StatusValue> status { get; set; }
	}

	public class DeviceSensor : IPluginSensor
	{
		public string Id => $"{this.Device.description}_{this.Channel.key}";
		public string Name => $"{this.Device.description}: {this.Channel.key}";
		public float? Value
		{
			get
			{
				return (float)this.Channel.value;
			}
		}
		public string LiquidctlInterfaceName => this.Channel.key.Split(' ')[0].ToLower();
		public void Update() { }
		public void Update(StatusValue status)
		{
			this.Channel = status;
		}

		internal DeviceStatus Device { get; }
		internal StatusValue Channel { get; set; }
		internal LiquidCtlExecutor Executor { get; }
		internal DeviceSensor(DeviceStatus device, StatusValue channel, LiquidCtlExecutor executor)
		{
			this.Device = device;
			this.Channel = channel;
			this.Executor = executor;
		}
	}

	public class ControlSensor : DeviceSensor, IPluginControlSensor
	{
		internal float? Initial { get; }
		internal ControlSensor(DeviceStatus device, StatusValue channel, LiquidCtlExecutor executor) :
			base(device, channel, executor)
		{
			// initial value protection: don't reset speeds back to 0
			this.Initial = this.Value.GetValueOrDefault()==0 ? this.Value : 50;
		}

		public void Reset()
		{
			if (this.Initial != null)
			{
				this.Set(Initial.GetValueOrDefault());
			}
		}

		public void Set(float val)
		{
			int target = (int)Math.Round(val);
			var _ = this.Executor.Execute<List<DeviceStatus>>($"set {this.LiquidctlInterfaceName} speed {target}");
		}
	}

	public class ControlOnlySensor : ControlSensor
	{
		internal ControlOnlySensor(DeviceStatus device, StatusValue channel, LiquidCtlExecutor executor) :
		base(device, channel, executor)
		{ }

		public static ControlOnlySensor CopyFrom(DeviceSensor sensor)
		{
			var channel = new StatusValue();
			channel.key = $"{sensor.LiquidctlInterfaceName} duty";
			channel.unit = "%";
			channel.value = 0;

			return new ControlOnlySensor(sensor.Device, channel, sensor.Executor);
		}
	}
}


