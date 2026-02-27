using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;


namespace Shopify
{
    public static class Soundex
    {
        public static string For(string word)
        {
            const int MaxSoundexCodeLength = 4;

            var soundexCode = new StringBuilder();
            var previousWasHOrW = false;

            word = Regex.Replace(
                word == null ? string.Empty : word.ToUpper(),
                @"[^\w\s]",
                string.Empty);

            if (string.IsNullOrEmpty(word))
                return string.Empty.PadRight(MaxSoundexCodeLength, '0');

            soundexCode.Append(word.First());

            for (var i = 1; i < word.Length; i++)
            {
                var numberCharForCurrentLetter =
                    GetCharNumberForLetter(word[i]);

                if (i == 1 &&
                    numberCharForCurrentLetter ==
                    GetCharNumberForLetter(soundexCode[0]))
                    continue;

                if (soundexCode.Length > 2 && previousWasHOrW &&
                    numberCharForCurrentLetter ==
                    soundexCode[soundexCode.Length - 2])
                    continue;

                if (soundexCode.Length > 0 &&
                    numberCharForCurrentLetter ==
                    soundexCode[soundexCode.Length - 1])
                    continue;

                soundexCode.Append(numberCharForCurrentLetter);

                previousWasHOrW = "HW".Contains(word[i]);
            }

            return soundexCode
                .Replace("0", string.Empty)
                .ToString()
                .PadRight(MaxSoundexCodeLength, '0')
                .Substring(0, MaxSoundexCodeLength);
        }

        private static char GetCharNumberForLetter(char letter)
        {
            if ("BFPV".Contains(letter)) return '1';
            if ("CGJKQSXZ".Contains(letter)) return '2';
            if ("DT".Contains(letter)) return '3';
            if ('L' == letter) return '4';
            if ("MN".Contains(letter)) return '5';
            if ('R' == letter) return '6';

            return '0';
        }


        //================================================================

        public const string Empty = "0000";

        private static readonly Regex Sanitiser = new Regex(@"[^A-Z]", RegexOptions.Compiled);
        private static readonly Regex CollapseRepeatedNumbers = new Regex(@"(\d)?\1*[WH]*\1*", RegexOptions.Compiled);
        private static readonly Regex RemoveVowelSounds = new Regex(@"[AEIOUY]", RegexOptions.Compiled);

        public static string Generate(string Phrase)
        {
            // Remove non-alphas
            Phrase = Sanitiser.Replace((Phrase ?? string.Empty).ToUpper(), string.Empty);

            // Nothing to soundex, return empty
            if (string.IsNullOrEmpty(Phrase))
                return Empty;

            // Convert consonants to numerical representation
            var Numified = Numify(Phrase);

            // Remove repeated numberics (characters of the same sound class), even if separated by H or W
            Numified = CollapseRepeatedNumbers.Replace(Numified, @"$1");

            if (Numified.Length > 0 && Numified[0] == Numify(Phrase[0]))
            {
                // Remove first numeric as first letter in same class as subsequent letters
                Numified = Numified.Substring(1);
            }

            // Remove vowels
            Numified = RemoveVowelSounds.Replace(Numified, string.Empty);

            // Concatenate, pad and trim to ensure X### format.
            return string.Format("{0}{1}", Phrase[0], Numified).PadRight(4, '0').Substring(0, 4);
        }

        private static string Numify(string Phrase)
        {
            return new string(Phrase.ToCharArray().Select(Numify).ToArray());
        }

        private static char Numify(char Character)
        {
            switch (Character)
            {
                case 'B':
                case 'F':
                case 'P':
                case 'V':
                    return '1';
                case 'C':
                case 'G':
                case 'J':
                case 'K':
                case 'Q':
                case 'S':
                case 'X':
                case 'Z':
                    return '2';
                case 'D':
                case 'T':
                    return '3';
                case 'L':
                    return '4';
                case 'M':
                case 'N':
                    return '5';
                case 'R':
                    return '6';
                default:
                    return Character;
            }
        }
    }
}