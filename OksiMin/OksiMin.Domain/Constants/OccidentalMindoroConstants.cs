namespace OksiMin.Domain.Constants
{
    /// <summary>
    /// Municipality information for Occidental Mindoro
    /// </summary>
    public class MunicipalityInfo
    {
        public string Name { get; init; } = string.Empty;
        public string Code { get; init; } = string.Empty;
        public bool IsCapital { get; init; }
        public int? EstimatedPopulation { get; init; }
        public decimal? Latitude { get; init; }
        public decimal? Longitude { get; init; }
        public string? Description { get; init; }
    }

    /// <summary>
    /// Constants related to Occidental Mindoro geography and administration
    /// </summary>
    public static class OccidentalMindoroConstants
    {
        // Individual municipality name constants
        public const string AbraDeIlog = "Abra de Ilog";
        public const string Calintaan = "Calintaan";
        public const string Looc = "Looc";
        public const string Lubang = "Lubang";
        public const string Magsaysay = "Magsaysay";
        public const string Mamburao = "Mamburao";
        public const string Paluan = "Paluan";
        public const string Rizal = "Rizal";
        public const string Sablayan = "Sablayan";
        public const string SanJose = "San Jose";
        public const string SantaCruz = "Santa Cruz";

        /// <summary>
        /// All 11 municipalities in Occidental Mindoro province
        /// </summary>
        public static readonly string[] Municipalities = new[]
        {
            AbraDeIlog,
            Calintaan,
            Looc,
            Lubang,
            Magsaysay,
            Mamburao,
            Paluan,
            Rizal,
            Sablayan,
            SanJose,
            SantaCruz
        };

        /// <summary>
        /// Detailed municipality information including coordinates and metadata
        /// </summary>
        public static readonly Dictionary<string, MunicipalityInfo> MunicipalityDetails = new()
        {
            [AbraDeIlog] = new MunicipalityInfo
            {
                Name = AbraDeIlog,
                Code = "ADI",
                IsCapital = false,
                Latitude = 13.4500m,
                Longitude = 120.7333m,
                EstimatedPopulation = 30_000,
                Description = "A coastal municipality in the northern part of Occidental Mindoro"
            },
            [Calintaan] = new MunicipalityInfo
            {
                Name = Calintaan,
                Code = "CAL",
                IsCapital = false,
                Latitude = 12.5667m,
                Longitude = 120.9500m,
                EstimatedPopulation = 30_000,
                Description = "An inland municipality known for its agricultural products"
            },
            [Looc] = new MunicipalityInfo
            {
                Name = Looc,
                Code = "LOC",
                IsCapital = false,
                Latitude = 13.8000m,
                Longitude = 121.2333m,
                EstimatedPopulation = 10_000,
                Description = "An island municipality located off the northwestern coast"
            },
            [Lubang] = new MunicipalityInfo
            {
                Name = Lubang,
                Code = "LUB",
                IsCapital = false,
                Latitude = 13.8667m,
                Longitude = 120.1167m,
                EstimatedPopulation = 25_000,
                Description = "An island municipality northwest of mainland Mindoro"
            },
            [Magsaysay] = new MunicipalityInfo
            {
                Name = Magsaysay,
                Code = "MAG",
                IsCapital = false,
                Latitude = 12.3333m,
                Longitude = 121.1667m,
                EstimatedPopulation = 35_000,
                Description = "Located in the southeastern part of the province"
            },
            [Mamburao] = new MunicipalityInfo
            {
                Name = Mamburao,
                Code = "MAM",
                IsCapital = true,
                Latitude = 13.2122m,
                Longitude = 120.6022m,
                EstimatedPopulation = 45_000,
                Description = "The capital municipality and commercial center of Occidental Mindoro"
            },
            [Paluan] = new MunicipalityInfo
            {
                Name = Paluan,
                Code = "PAL",
                IsCapital = false,
                Latitude = 13.4167m,
                Longitude = 120.4500m,
                EstimatedPopulation = 18_000,
                Description = "A coastal municipality known for its beaches and dive sites"
            },
            [Rizal] = new MunicipalityInfo
            {
                Name = Rizal,
                Code = "RIZ",
                IsCapital = false,
                Latitude = 12.5000m,
                Longitude = 121.0833m,
                EstimatedPopulation = 40_000,
                Description = "An agricultural municipality in the interior"
            },
            [Sablayan] = new MunicipalityInfo
            {
                Name = Sablayan,
                Code = "SAB",
                IsCapital = false,
                Latitude = 12.8342m,
                Longitude = 120.7742m,
                EstimatedPopulation = 90_000,
                Description = "The largest municipality, gateway to Apo Reef Natural Park"
            },
            [SanJose] = new MunicipalityInfo
            {
                Name = SanJose,
                Code = "SJO",
                IsCapital = false,
                Latitude = 12.3500m,
                Longitude = 121.0667m,
                EstimatedPopulation = 150_000,
                Description = "The most populous municipality and economic hub of the province"
            },
            [SantaCruz] = new MunicipalityInfo
            {
                Name = SantaCruz,
                Code = "SCR",
                IsCapital = false,
                Latitude = 13.0000m,
                Longitude = 120.9500m,
                EstimatedPopulation = 35_000,
                Description = "Located in central Occidental Mindoro"
            }
        };

        /// <summary>
        /// Capital municipality
        /// </summary>
        public const string Capital = Mamburao;

        /// <summary>
        /// Province name
        /// </summary>
        public const string ProvinceName = "Occidental Mindoro";

        /// <summary>
        /// Region
        /// </summary>
        public const string Region = "MIMAROPA";

        /// <summary>
        /// Island
        /// </summary>
        public const string Island = "Mindoro";

        /// <summary>
        /// Number of municipalities
        /// </summary>
        public const int MunicipalityCount = 11;

        /// <summary>
        /// Approximate province coordinate bounds
        /// </summary>
        public static class CoordinateBounds
        {
            public const decimal LatitudeMin = 12.0m;
            public const decimal LatitudeMax = 14.0m;
            public const decimal LongitudeMin = 120.0m;
            public const decimal LongitudeMax = 121.5m;
        }

        /// <summary>
        /// Get municipality info by name (case-insensitive)
        /// </summary>
        public static MunicipalityInfo? GetMunicipalityInfo(string municipalityName)
        {
            var key = Municipalities.FirstOrDefault(m =>
                m.Equals(municipalityName, StringComparison.OrdinalIgnoreCase));

            return key != null && MunicipalityDetails.TryGetValue(key, out var info)
                ? info
                : null;
        }

        /// <summary>
        /// Check if a municipality name is valid (case-insensitive)
        /// </summary>
        public static bool IsValidMunicipality(string municipalityName)
        {
            return Municipalities.Any(m =>
                m.Equals(municipalityName, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Get the capital municipality info
        /// </summary>
        public static MunicipalityInfo GetCapitalInfo()
        {
            return MunicipalityDetails[Capital];
        }
    }
}