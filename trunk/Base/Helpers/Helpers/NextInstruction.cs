using System;

namespace Natek.Helpers.Execution{
	[Flags]
	public enum NextInstruction {
		Break = 0,
		Continue = 1,
		Skip = 2 | Continue,
		Do = 4 | Continue,
		Abort = 8,
		Return = 16
	}
}
