using System;
using Natek.Recorders.Remote.Mapping;

namespace Natek.Recorders.Remote.Helpers.Mapping {
	public static class MappedDataHelper {
		public static void ClearRecord(RecWrapper rec) {
			foreach (var pInfo in typeof(RecWrapper).GetProperties()) {
				pInfo.SetValue(rec, pInfo.PropertyType.IsValueType ? Activator.CreateInstance(pInfo.PropertyType) : null, null);
			}
		}

		public static bool ProcessLine(DataMappingInfo headerInfo, string line, RecWrapper rec, object data, out string[] fields, ref Exception error, char[] separators) {
			try {
				ClearRecord(rec);
				fields = line.Split(separators);
				foreach (var info in headerInfo.Mappings) {
					var i = 0;
					foreach (var index in info.SourceIndex) {
						info.SourceValues[i++] = index != -1
													 ? (fields.Length > index ? fields[index] : string.Empty)
													 : null;
					}
					info.MappedField.SetValue(rec, info.MethodInfo != null
													   ? info.MethodInfo(rec, info.Original[0][0], info.SourceValues, data)
													   : info.SourceValues[0], null);
				}
				return true;
			} catch (Exception e) {
				fields = null;
				error = e;
			}
			return false;
		}
	}
}
