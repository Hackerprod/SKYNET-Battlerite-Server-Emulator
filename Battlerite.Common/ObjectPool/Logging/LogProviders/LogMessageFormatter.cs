using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace CodeProject.ObjectPool.Logging.LogProviders
{
	internal static class LogMessageFormatter
	{
		private static readonly Regex Pattern = new Regex("(?<!{){@?(?<arg>[^ :{}]+)(?<format>:[^}]+)?}", RegexOptions.Compiled);

		public static Func<string> SimulateStructuredLogging(Func<string> messageBuilder, object[] formatParameters)
		{
			if (formatParameters == null || formatParameters.Length == 0)
			{
				return messageBuilder;
			}
			IEnumerable<string> patternMatches;
			return () => FormatStructuredMessage(messageBuilder(), formatParameters, out patternMatches);
		}

		private static string ReplaceFirst(string text, string search, string replace)
		{
			int num = text.IndexOf(search, StringComparison.Ordinal);
			if (num < 0)
			{
				return text;
			}
			return text.Substring(0, num) + replace + text.Substring(num + search.Length);
		}

		public static string FormatStructuredMessage(string targetMessage, object[] formatParameters, out IEnumerable<string> patternMatches)
		{
			if (formatParameters.Length != 0)
			{
				List<string> list = (List<string>)(patternMatches = new List<string>());
				foreach (Match item in Pattern.Matches(targetMessage))
				{
					string value = item.Groups["arg"].Value;
					if (!int.TryParse(value, out int _))
					{
						int num = list.IndexOf(value);
						if (num == -1)
						{
							num = list.Count;
							list.Add(value);
						}
						targetMessage = ReplaceFirst(targetMessage, item.Value, "{" + num + item.Groups["format"].Value + "}");
					}
				}
				try
				{
					return string.Format(CultureInfo.InvariantCulture, targetMessage, formatParameters);
				}
				catch (FormatException innerException)
				{
					throw new FormatException("The input string '" + targetMessage + "' could not be formatted using string.Format", innerException);
				}
			}
			patternMatches = Enumerable.Empty<string>();
			return targetMessage;
		}
	}
}
