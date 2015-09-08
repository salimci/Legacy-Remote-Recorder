using CustomTools;

namespace Natek.Recorders.Remote {
	public class RecWrapper {
		public CustomBase.Rec rec;

		public RecWrapper() {
			rec = new CustomBase.Rec();
		}

		public long EventId {
			get { return rec.EventId; }
			set { rec.EventId = value; }
		}

		public int Recordnum {
			get { return rec.Recordnum; }
			set { rec.Recordnum = value; }
		}

		public string EventType {
			get { return rec.EventType; }
			set { rec.EventType = value; }
		}

		public string EventCategory {
			get { return rec.EventCategory; }
			set { rec.EventCategory = value; }
		}

		public string Datetime {
			get { return rec.Datetime; }
			set { rec.Datetime = value; }
		}

		public string Description {
			get { return rec.Description; }
			set { rec.Description = value; }
		}

		public string SourceName {
			get { return rec.SourceName; }
			set { rec.SourceName = value; }
		}

		public string ComputerName {
			get { return rec.ComputerName; }
			set { rec.ComputerName = value; }
		}

		public string UserName {
			get { return rec.UserName; }
			set { rec.UserName = value; }
		}

		public string LogName {
			get { return rec.LogName; }
			set { rec.LogName = value; }
		}

		public int CustomInt1 {
			get { return rec.CustomInt1; }
			set { rec.CustomInt1 = value; }
		}

		public int CustomInt2 {
			get { return rec.CustomInt2; }
			set { rec.CustomInt2 = value; }
		}

		public int CustomInt3 {
			get { return rec.CustomInt3; }
			set { rec.CustomInt3 = value; }
		}

		public int CustomInt4 {
			get { return rec.CustomInt4; }
			set { rec.CustomInt4 = value; }
		}

		public int CustomInt5 {
			get { return rec.CustomInt5; }
			set { rec.CustomInt5 = value; }
		}

		public long CustomInt6 {
			get { return rec.CustomInt6; }
			set { rec.CustomInt6 = value; }
		}

		public long CustomInt7 {
			get { return rec.CustomInt7; }
			set { rec.CustomInt7 = value; }
		}

		public long CustomInt8 {
			get { return rec.CustomInt8; }
			set { rec.CustomInt8 = value; }
		}

		public long CustomInt9 {
			get { return rec.CustomInt9; }
			set { rec.CustomInt9 = value; }
		}

		public long CustomInt10 {
			get { return rec.CustomInt10; }
			set { rec.CustomInt10 = value; }
		}

		public string CustomStr1 {
			get { return rec.CustomStr1; }
			set { rec.CustomStr1 = value; }
		}

		public string CustomStr2 {
			get { return rec.CustomStr2; }
			set { rec.CustomStr2 = value; }
		}

		public string CustomStr3 {
			get { return rec.CustomStr3; }
			set { rec.CustomStr3 = value; }
		}

		public string CustomStr4 {
			get { return rec.CustomStr4; }
			set { rec.CustomStr4 = value; }
		}

		public string CustomStr5 {
			get { return rec.CustomStr5; }
			set { rec.CustomStr5 = value; }
		}

		public string CustomStr6 {
			get { return rec.CustomStr6; }
			set { rec.CustomStr6 = value; }
		}

		public string CustomStr7 {
			get { return rec.CustomStr7; }
			set { rec.CustomStr7 = value; }
		}

		public string CustomStr8 {
			get { return rec.CustomStr8; }
			set { rec.CustomStr8 = value; }
		}

		public string CustomStr9 {
			get { return rec.CustomStr9; }
			set { rec.CustomStr9 = value; }
		}

		public string CustomStr10 {
			get { return rec.CustomStr10; }
			set { rec.CustomStr10 = value; }
		}
	}
}