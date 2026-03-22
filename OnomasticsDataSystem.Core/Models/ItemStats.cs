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
		// 🔥 CACHE (zdieľaná pre všetky objekty)
		private static readonly Dictionary<string, int> _syllableCache = new();
		private static readonly object _lock = new();
		public int Syllables
		{
			get
			{
				if (_syllables.HasValue)
					return _syllables.Value;

				var key = Name ?? "";

				lock (_lock)
				{
					if (_syllableCache.TryGetValue(key, out var value))
					{
						_syllables = value;
						return value;
					}

					value = CountSyllables(key);
					_syllableCache[key] = value;

					_syllables = value;
					return value;
				}
			}
		}

		// 🔥 RÝCHLEJŠIE LOOKUPY
		private static readonly HashSet<char> Vowels = new("aeiouyáéíóúäô");
		private static readonly HashSet<char> Syllabic = new("rl");
		private static int CountSyllables(string word)
		{
			if (string.IsNullOrWhiteSpace(word))
				return 0;

			word = word.ToLower();

			int count = 0;

			for (int i = 0; i < word.Length; i++)
			{
				char c = word[i];

				// 1. samohlásky = vždy slabika
				if (Vowels.Contains(c))
				{
					count++;
				}
				// 2. slabikotvorné r/l
				else if (Syllabic.Contains(c))
				{
					bool prevIsVowel = i > 0 && Vowels.Contains(word[i - 1]);
					bool nextIsVowel = i + 1 < word.Length && Vowels.Contains(word[i + 1]);

					if (!prevIsVowel && !nextIsVowel)
						count++;
				}
			}

			return count;
		}
	}

}
