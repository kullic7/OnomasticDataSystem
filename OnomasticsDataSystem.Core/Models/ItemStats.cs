using System;
using System.Collections.Generic;
using System.Text;

namespace OnomasticsDataSystem.Core.Models
{
	public class ItemStats
	{
		public string? Name { get; set; }
		public int Count { get; set; }

		public int Length => Name?.Length ?? 0;

		private int? _syllables;
		public int Syllables => _syllables ??= CountSyllables(Name ?? "");

		private static int CountSyllables(string word)
		{
			if (string.IsNullOrWhiteSpace(word))
				return 0;

			word = word.ToLower();

			string vowels = "aeiouyáéíóúäô";

			int count = 0;
			bool prevWasVowel = false;

			foreach (var c in word)
			{
				bool isVowel = vowels.Contains(c);

				if (isVowel && !prevWasVowel)
					count++;

				prevWasVowel = isVowel;
			}

			return count;
		}
	}

}
