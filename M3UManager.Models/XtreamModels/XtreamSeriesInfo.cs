using System.Text.Json.Serialization;

namespace M3UManager.Models.XtreamModels
{
    public class XtreamSeriesInfo
    {
        [JsonPropertyName("info")]
        public SeriesDetails Info { get; set; } = new();

        [JsonPropertyName("episodes")]
        public Dictionary<string, List<XtreamEpisode>> Episodes { get; set; } = new();

        [JsonPropertyName("seasons")]
        public List<SeasonInfo> Seasons { get; set; } = new();
    }

    public class SeriesDetails
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("cover")]
        public string Cover { get; set; } = string.Empty;

        [JsonPropertyName("plot")]
        public string Plot { get; set; } = string.Empty;

        [JsonPropertyName("cast")]
        public string Cast { get; set; } = string.Empty;

        [JsonPropertyName("director")]
        public string Director { get; set; } = string.Empty;

        [JsonPropertyName("genre")]
        public string Genre { get; set; } = string.Empty;

        [JsonPropertyName("releaseDate")]
        public string ReleaseDate { get; set; } = string.Empty;

        [JsonPropertyName("rating")]
        public string Rating { get; set; } = string.Empty;

        [JsonPropertyName("youtube_trailer")]
        public string YoutubeTrailer { get; set; } = string.Empty;

        [JsonPropertyName("episode_run_time")]
        public string EpisodeRunTime { get; set; } = string.Empty;

        [JsonPropertyName("backdrop_path")]
        public List<string> BackdropPath { get; set; } = new();
    }

    public class XtreamEpisode
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("episode_num")]
        public int EpisodeNum { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;

        [JsonPropertyName("container_extension")]
        public string ContainerExtension { get; set; } = "mp4";

        [JsonPropertyName("info")]
        public EpisodeInfo Info { get; set; } = new();

        [JsonPropertyName("season")]
        public int Season { get; set; }

        [JsonPropertyName("direct_source")]
        public string DirectSource { get; set; } = string.Empty;
    }

    public class EpisodeInfo
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("plot")]
        public string Plot { get; set; } = string.Empty;

        [JsonPropertyName("duration")]
        public string Duration { get; set; } = string.Empty;

        [JsonPropertyName("rating")]
        public string Rating { get; set; } = string.Empty;

        [JsonPropertyName("releasedate")]
        public string ReleaseDate { get; set; } = string.Empty;

        [JsonPropertyName("movie_image")]
        public string MovieImage { get; set; } = string.Empty;
    }

    public class SeasonInfo
    {
        [JsonPropertyName("season_number")]
        public int SeasonNumber { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("overview")]
        public string Overview { get; set; } = string.Empty;

        [JsonPropertyName("air_date")]
        public string AirDate { get; set; } = string.Empty;

        [JsonPropertyName("cover")]
        public string Cover { get; set; } = string.Empty;

        [JsonPropertyName("episode_count")]
        public int EpisodeCount { get; set; }
    }
}
