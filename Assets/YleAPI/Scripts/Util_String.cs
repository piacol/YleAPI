using System.Collections;

namespace YleAPI.Util
{
	public class Util_String
	{
		public static string ReplaceString(string str)
		{            
			return str.Replace("\\r\\n", "\r\n").Replace("\\\"", "\"");
		}

		public static string[] SplitTitle(string str)
		{
			string seperator = ":";
			char[] seperatorArray = seperator.ToCharArray();
			string[] result = str.Split(seperatorArray, 2);

			if(result.Length > 1 && 
				result[1] != null)
			{                
				result[1] = result[1].Trim();
			}

			return result;
		}	

		public static string ToDisplayStartTime(string str)
		{
			string seperator = "T";
			char[] seperatorArray = seperator.ToCharArray();
			string[] result = str.Split(seperatorArray, 2);

			return result [0];
		}

		public static string ToDisplayDuration(string str)
		{
			string result;
			string[] resultSplit;
			string seperator = "HMS";
			char[] seperatorArray = seperator.ToCharArray();
			string trimStr = str.Remove (0, 2);

			resultSplit = trimStr.Split(seperatorArray);

			if (trimStr.Contains("H") == true) 
			{
				result = string.Format ("{0}h {1}m", resultSplit [0], resultSplit [1]);
			}
			else if (trimStr.Contains("M") == true) 
			{
				result = string.Format ("{0}m", resultSplit [0]);
			}
			else
			{
				result = string.Format ("{0}s", resultSplit [0]);
			}

			return result;
		}
	}
}