using System.Text.RegularExpressions;

namespace Tollrech.Common
{
	// Взято из https://github.com/Humanizr/Humanizer/blob/master/src/Humanizer/InflectorExtensions.cs

	/// <summary>
	/// Inflector extensions
	/// </summary>
	public static class InflectorExtensions
	{
		/// <summary>
		/// By default, pascalize converts strings to UpperCamelCase also removing underscores
		/// </summary>
		/// <param name = "input"> </param>
		/// <returns> </returns>
		public static string Pascalize(this string input) => Regex.Replace(input, "(?:^|_| +)(.)", match => match.Groups[1].Value.ToUpper());

		/// <summary>
		/// Same as Pascalize except that the first character is lower case
		/// </summary>
		/// <param name = "input"> </param>
		/// <returns> </returns>
		public static string Camelize(this string input)
		{
			var word = input.Pascalize();

			return word.Length > 0 ? $"{word.Substring(0, 1).ToLower()}{word.Substring(1)}" : word;
		}

		/// <summary>
		/// Separates the input words with underscore
		/// </summary>
		/// <param name = "input"> The string to be underscored </param>
		/// <returns> </returns>
		public static string Underscore(this string input) =>
			Regex.Replace(
				Regex.Replace(
					Regex.Replace(input, @"([\p{Lu}]+)([\p{Lu}][\p{Ll}])", "$1_$2"), @"([\p{Ll}\d])([\p{Lu}])", "$1_$2"), @"[-\s]", "_").ToLower();

		/// <summary>
		/// Separates the input words with hyphens and all the words are converted to lowercase
		/// </summary>
		/// <param name = "input"> </param>
		/// <returns> </returns>
		public static string Kebaberize(this string input) => Underscore(input).Dasherize();

		/// <summary>
		/// Replaces underscores with dashes in the string
		/// </summary>
		/// <param name = "underscoredWord"> </param>
		/// <returns> </returns>
		private static string Dasherize(this string underscoredWord) => underscoredWord.Replace('_', '-');
	}
}