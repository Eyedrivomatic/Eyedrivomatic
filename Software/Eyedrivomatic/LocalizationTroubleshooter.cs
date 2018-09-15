using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Eyedrivomatic.Logging;

namespace Eyedrivomatic
{
    public static class LocalizationTroubleshooter
    {
        public static void TestAndLogInstalledCultures()
        {
            try
            {
                AllCultures = CultureInfo.GetCultures(CultureTypes.AllCultures)
                    .Where(x => !IsInvariant(x))
                    .ToArray();
            }
            catch (Exception ex)
            {
                Log.Error(typeof(LocalizationTroubleshooter), $"Failed to initialize 'AllCultures' - [{ex}]");
                return;
            }

            try
            {
                CultureRegionMap =
                    AllCultures
                        .Where(x => !x.IsNeutralCulture)
                        .ToDictionary(
                            x => x,
                            x => new RegionInfo(x.Name),
                            CultureInfoComparer.ByName);
            }
            catch (Exception ex)
            {
                Log.Error(typeof(LocalizationTroubleshooter), $"Failed to initialize 'CultureRegionMap' - [{ex}]");
                return;
            }

            try
            {
                AllRegions = CultureRegionMap?.Values;
            }
            catch (Exception ex)
            {
                Log.Error(typeof(LocalizationTroubleshooter), $"Failed to initialize 'AllRegions' - [{ex}]");
                return;
            }


            try
            {
                TwoLetterISOLanguageNameCultureMap =
                    AllCultures.Where(x => x.Name == x.TwoLetterISOLanguageName)
                        .ToDictionary(
                            x => x.TwoLetterISOLanguageName,
                            x => x,
                            StringComparer.OrdinalIgnoreCase);
            }
            catch (Exception ex)
            {
                Log.Error(typeof(LocalizationTroubleshooter), $"Failed to initialize 'TwoLetterISOLanguageNameCultureMap' - [{ex}]");
                return;
            }


            try
            {
                NameCultureMap =
                    AllCultures.ToDictionary(
                        x => x.Name,
                        x => x,
                        StringComparer.OrdinalIgnoreCase);

            }
            catch (Exception ex)
            {
                Log.Error(typeof(LocalizationTroubleshooter), $"Failed to initialize 'NameCultureMap' - [{ex}]");
                return;
            }

            try
            {
                NameRegionMap =
                    AllRegions
                        .ToDictionary(
                            x => x.Name,
                            x => x,
                            StringComparer.OrdinalIgnoreCase);
            }
            catch (Exception ex)
            {
                Log.Error(typeof(LocalizationTroubleshooter), $"Failed to initialize 'NameRegionMap' - [{ex}]");
                return;
            }

            try
            {
                NeutralCultureRegionMap =
                    AllCultures
                        .Where(x => x.IsNeutralCulture)
                        .Select(x => NameCultureMap[CultureInfo.CreateSpecificCulture(x.Name).Name])
                        .Distinct(CultureInfoComparer.ByTwoLetterIsoLanguageName)
                        .ToDictionary(
                            x => x.Parent,
                            x => CultureRegionMap[x],
                            CultureInfoComparer.ByTwoLetterIsoLanguageName);
            }
            catch (Exception ex)
            {
                Log.Error(typeof(LocalizationTroubleshooter), $"Failed to initialize 'NeutralCultureRegionMap' - [{ex}]");

                try
                {
                    var missingNeuturalSpecificCultures = AllCultures
                        .Where(x => x.IsNeutralCulture &&
                                    !NameCultureMap.ContainsKey(CultureInfo.CreateSpecificCulture(x.Name).Name));

                    foreach (var missingNeuturalSpecificCulture in missingNeuturalSpecificCultures)
                    {
                        Log.Error(typeof(LocalizationTroubleshooter), $"Neutral Specific Culture missing for [{missingNeuturalSpecificCulture.Name}]");
                    }
                }
                catch (Exception ex1)
                {
                    Log.Error(typeof(LocalizationTroubleshooter),
                        $"Failed to find missing specific cultures for neutral cultures - [{ex1}]");
                    return;
                }

                try
                {
                    var specificCultures = AllCultures
                        .Where(x => x.IsNeutralCulture)
                        .Select(x => NameCultureMap[CultureInfo.CreateSpecificCulture(x.Name).Name])
                        .Distinct(CultureInfoComparer.ByTwoLetterIsoLanguageName);

                    var missingMaps = specificCultures.Where(x => !CultureRegionMap.ContainsKey(x));
                    foreach (var missingMap in missingMaps)
                    {
                        Log.Error(typeof(LocalizationTroubleshooter), $"Culture region missing for [{missingMap.Name}]");
                    }
                }
                catch (Exception ex1)
                {
                    Log.Error(typeof(LocalizationTroubleshooter),
                        $"Failed to find missing reguional maps for neutral cultures - [{ex1}]");
                    return;
                }


                return;
            }
        }


        internal static IReadOnlyList<CultureInfo> AllCultures;

        internal static IReadOnlyDictionary<CultureInfo, RegionInfo> CultureRegionMap;

        internal static IEnumerable<RegionInfo> AllRegions;

        private static Dictionary<string, CultureInfo> TwoLetterISOLanguageNameCultureMap;

        private static Dictionary<string, CultureInfo> NameCultureMap;

        private static Dictionary<string, RegionInfo> NameRegionMap;

        private static Dictionary<CultureInfo, RegionInfo> NeutralCultureRegionMap;

        internal static bool TryGet(string name, out CultureInfo culture)
        {
            if (name == null)
            {
                culture = null;
                return false;
            }

            return NameCultureMap.TryGetValue(name, out culture) ||
                   TwoLetterISOLanguageNameCultureMap.TryGetValue(name, out culture);
        }

        internal static bool TryGetRegion(CultureInfo culture, out RegionInfo region)
        {
            if (culture == null)
            {
                region = null;
                return false;
            }

            if (culture.IsNeutralCulture)
            {
                return NeutralCultureRegionMap.TryGetValue(culture, out region);
            }

            return NameRegionMap.TryGetValue(culture.Name, out region);
        }

        internal static bool NameEquals(CultureInfo first, CultureInfo other)
        {
            return CultureInfoComparer.ByName.Equals(first, other);
        }

        internal static bool TwoLetterIsoLanguageNameEquals(CultureInfo first, CultureInfo other)
        {
            return CultureInfoComparer.ByTwoLetterIsoLanguageName.Equals(first, other);
        }

        internal static bool IsInvariant(this CultureInfo culture)
        {
            return NameEquals(culture, CultureInfo.InvariantCulture);
        }

    }

    /// <summary>A comparer for <see cref="CultureInfo"/> </summary>
    internal class CultureInfoComparer : IEqualityComparer<CultureInfo>, IComparer<CultureInfo>
    {
        /// <summary> Gets a comparer that compares by <see cref="CultureInfo.TwoLetterISOLanguageName"/> </summary>
        internal static readonly CultureInfoComparer ByTwoLetterIsoLanguageName = new CultureInfoComparer(x => x?.TwoLetterISOLanguageName);

        /// <summary> Gets a comparer that compares by <see cref="CultureInfo.Name"/> </summary>
        internal static readonly CultureInfoComparer ByName = new CultureInfoComparer(x => x?.Name);

        private static readonly StringComparer StringComparer = StringComparer.OrdinalIgnoreCase;

        private readonly Func<CultureInfo, string> nameGetter;

        private CultureInfoComparer(Func<CultureInfo, string> nameGetter)
        {
            this.nameGetter = nameGetter;
        }

        /// <inheritdoc />
        public bool Equals(CultureInfo x, CultureInfo y)
        {
            if (x == null && y == null)
            {
                return true;
            }

            if (x == null || y == null)
            {
                return false;
            }

            return StringComparer.Equals(this.nameGetter(x), this.nameGetter(y));
        }

        /// <inheritdoc />
        public int GetHashCode(CultureInfo obj)
        {
            if (obj == null)
            {
                return 0;
            }

            return StringComparer.GetHashCode(this.nameGetter(obj));
        }

        public int Compare(CultureInfo x, CultureInfo y)
        {
            return string.Compare(this.nameGetter(x), this.nameGetter(y), StringComparison.Ordinal);
        }
    }
}
