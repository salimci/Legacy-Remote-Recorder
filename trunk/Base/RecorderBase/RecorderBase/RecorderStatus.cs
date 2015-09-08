using System;

namespace Natek.Recorders.Remote {
	[Flags]
	public enum RecorderStatus {
		None = 0,
		Initialized = 1,
		Initializing = 2
	}
}