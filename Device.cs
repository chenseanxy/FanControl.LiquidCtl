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
				switch (this.Channel.unit)
				{
					case "%":
						return (float)this.Channel.value;
					default:
						return (float)this.Channel.value;
				}
			}
		}
		public void Update() { }
		internal void Update(StatusValue status)
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
			this.Initial = this.Value;
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
			var channel = this.Channel.key.Split(' ')[0].ToLower();
			var _ = this.Executor.Execute<List<DeviceStatus>>(
				// Filter to current device
				// Assume there's one type of device per install
				$"-m \"{this.Device.description}\" set {channel} speed {target}");
		}
	}
}


