namespace MusicGenMVC.Models
{
    public record SongItem(int Index, string Title, string Artist, string Album, string Genre, int Likes, bool IsSingle, string CoverUrl);

    public record SongDetail(
        int Index,
        string Title,
        string Artist,
        string Album,
        string Genre,
        int Likes,
        bool IsSingle,
        string CoverUrl,
        string AudioUrl,
        string ReviewText,
        List<LyricLine> Lyrics
    );

    public record LyricLine(double Time, string Text);

    public class GenerationParams
    {
        public string Lang { get; set; } = "en-US";
        public ulong Seed { get; set; } = 1UL;
        public double LikesAvg { get; set; } = 3.5;
    }
}

