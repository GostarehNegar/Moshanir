using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace GN.Library.Helpers
{
	public class KeyValueString
	{
		private string source;
		private List<KeyValuePair<string, string>> items;
		public KeyValueString() : this("")
		{

		}
		public KeyValueString(string source, string seperators = ",;", string equalSigns = "=")
		{
			Parse(source, seperators, equalSigns);
		}
		public KeyValueString Parse(string source, string seperators = ",;", string equalSigns = "=")
		{
			this.items = DoParse(source, seperators, equalSigns);
			return this;
		}
		public IEnumerable<KeyValuePair<string, string>> Items => this.items;
		public string GetValue(string key, bool ignoreCase = true)
		{
			var item = this.Items.FirstOrDefault(x => string.Compare(x.Key, key, ignoreCase) == 0);
			return item.Value;
		}
		public void SetValue(string key, string value, bool ignoreCase =true)
		{
			this.items = this.items.Where(x => string.Compare(x.Key, key, ignoreCase) != 0).ToList();
			this.items.Add(new KeyValuePair<string, string>(key,value));
		}
		public string ToString(char delimeter, bool skipEmptyValues=false)
		{
			var result = "";
			this.Items.ToList().ForEach(x =>
			{
				if (!skipEmptyValues || x.Value != null)
				{
					result +=
						(string.IsNullOrWhiteSpace(result) ? "" : $"{delimeter}") +
						x.Key +
						(string.IsNullOrWhiteSpace(x.Value) ? "" : "=" + x.Value);
				}
			});
			return result;
		}
		public override string ToString()
		{
			var result = "";
			this.Items.ToList().ForEach(x =>
			{
				result +=
					(string.IsNullOrWhiteSpace(result) ? "" : ",") +
					x.Key +
					(string.IsNullOrWhiteSpace(x.Value) ? "" : "=" + x.Value);
			});
			return result;
		}



		private static List<KeyValuePair<string, string>> DoParse(string source, string seperators, string equalSigns)
		{
			bool inQuotes = false;
			source = source ?? "";
			seperators = string.IsNullOrWhiteSpace(seperators) ? "," : seperators;
			equalSigns = string.IsNullOrWhiteSpace(equalSigns)
				? "="
				: equalSigns;
			var result = new List<KeyValuePair<string, string>>();
			Split(source, c =>
			{
				if (c == '\"' || c == '\'')
					inQuotes = !inQuotes;
				return !inQuotes && seperators.Contains(c);
			}).ToList()
			.ForEach(x =>
			{
				var split = x.Split(equalSigns.ToArray(),2);
				var left = split.Length > 0 ? split[0] : "";
				var right = split.Length > 1 ? TrimMatchingQuotes(TrimMatchingQuotes(split[1].Trim(), '\''), '\"') : null;

				if (!string.IsNullOrWhiteSpace(left))
					result.Add(new KeyValuePair<string, string>(left.Trim(), right));
			});
			return result;
		}
		public static string TrimMatchingQuotes(string input, char quote)
		{
			if (string.IsNullOrWhiteSpace(input))
				return input;
			if ((input.Length >= 2) &&
				(input[0] == quote) && (input[input.Length - 1] == quote))
				return input.Substring(1, input.Length - 2);

			return input;
		}

		private static IEnumerable<string> Split(string str,
											Func<char, bool> controller)
		{
			int nextPiece = 0;
			for (int c = 0; c < str.Length; c++)
			{
				if (controller(str[c]))
				{
					yield return str.Substring(nextPiece, c - nextPiece);
					nextPiece = c + 1;
				}
			}
			yield return str.Substring(nextPiece);
		}
	}
}
