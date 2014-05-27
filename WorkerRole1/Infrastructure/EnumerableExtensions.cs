using System.Linq;
using System.Collections.Generic;

namespace Mandro.Blog.Worker.Infrastructure
{
    public static class EnumerableExtensions
    {
        /// <summary>
        /// Finds the position of pattern in source.
        /// Uses Boyer-Moore algorithm.
        /// </summary>
        /// <typeparam name="T">T must correctly implement Equals and GetHashCode methods.</typeparam>
        /// <param name="source">Source array</param>
        /// <param name="pattern">Pattern array</param>
        /// <param name="startIndex">Starting index of search</param>
        /// <returns>0 based index of first occurance of pattern in source, -1 otherwise.</returns>
        public static int IndexOf<T>(this IEnumerable<T> source, T[] pattern, int startIndex = 0)
        {
            var patternLength = pattern.Length;
            var offset = startIndex;

            var Z = BuildZ(pattern);
            var L = BuildL(pattern, Z);

            while (offset + patternLength - 1 < source.Count())
            {
                bool found = true;
                int k = patternLength - 1;
                while (k >= 0)
                {
                    if (!source.ElementAt(offset + k).Equals(pattern[k]))
                    {
                        found = false;
                        break;
                    }

                    --k;
                }

                if (found) return offset;

                int shift = 1;

                // Bad Character Rule
                // t_text[offset + k] - comparison failed here
                // pattern[k] - comparison failed here
                // find char in P, so that position < k AND P[postion] == pattern[k]               
                int badCharRuleShift = 0;

                var charachter = source.ElementAt(offset + k);
                int[] found_value;
                int found_position = -1;
                if (Z.TryGetValue(charachter, out found_value))
                {
                    found_position = found_value[k];
                }

                if (found_position != -1) badCharRuleShift = k - found_position;
                else badCharRuleShift = k + 1;

                if (badCharRuleShift > shift) shift = badCharRuleShift;

                // Good Suffix Rule
                int goodSuffixRuleShift = 0;
                if (k < patternLength - 1) // at least one char was matched
                {
                    // t_text[offset + k + 1]  - last matching char
                    // pattern[k + 1] - the same as above
                    int candidate_found_index = L[patternLength - (k + 1) - 1];
                    if (candidate_found_index != -1)
                    {
                        goodSuffixRuleShift = k + 1 - candidate_found_index;
                    }
                }

                if (goodSuffixRuleShift > shift) shift = goodSuffixRuleShift;

                offset += shift;
            }

            return -1;
        }

        private static int[] BuildL<T>(T[] pattern, Dictionary<T, int[]> Z)
        {
            int patternLength = pattern.Length;
            var L = new int[patternLength - 1];

            for (int i = 0; i < patternLength - 1; ++i)
            {
                // pattern[n_pattern_length - i - 1] - beggining of suffix

                var suffix_start = patternLength - i - 1;
                var suffix_first = pattern[suffix_start];

                int search_i = Z.ContainsKey(suffix_first) ? Z[suffix_first][suffix_start] : -1;

                while (search_i != -1)
                {
                    // i + 1 - length of candidate
                    // search_i - beginning of candidate in P
                    var candidate_found = true;
                    if (search_i == 0 || (!pattern[search_i - 1].Equals(pattern[suffix_start - 1])))
                    {
                        for (int j = 0; j < i + 1; ++j)
                        {
                            if (!pattern[search_i + j].Equals(pattern[suffix_start + j]))
                            {
                                candidate_found = false;
                                break;
                            }
                        }
                    }
                    else
                    {
                        candidate_found = false;
                    }

                    if (!candidate_found)
                    {
                        search_i = Z.ContainsKey(suffix_first) ? Z[suffix_first][search_i] : -1;
                    }
                    else
                    {
                        break;
                    }
                }

                L[i] = search_i;
            }

            return L;
        }

        private static Dictionary<T, int[]> BuildZ<T>(T[] pattern)
        {
            var zDictionary = new Dictionary<T, int[]>();
            var nPatternLength = pattern.Length;

            for (int i = 0; i < nPatternLength - 1; ++i)
            {
                for (int j = i + 1; j < nPatternLength; ++j)
                {
                    if (!zDictionary.ContainsKey(pattern[i]))
                    {
                        var ints = new int[nPatternLength];

                        for (int l = 0; l < nPatternLength; ++l)
                        {
                            ints[l] = -1;
                        }

                        zDictionary[pattern[i]] = ints;
                    }

                    zDictionary[pattern[i]][j] = i;
                }
            }

            return zDictionary;
        }
    }
}