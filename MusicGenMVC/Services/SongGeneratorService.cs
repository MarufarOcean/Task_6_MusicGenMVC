using Bogus;
using Microsoft.VisualBasic;
using MusicGenMVC.Models;
using NAudio.Wave;
using SkiaSharp;
namespace MusicGenMVC.Services
{
    public sealed class SongGeneratorService
    {
        private readonly LocaleStore _locale;
        private readonly SeededRandomFactory _rand;
        private readonly IWebHostEnvironment _env;
        public SongGeneratorService(LocaleStore locale, SeededRandomFactory rand, IWebHostEnvironment env)
        { _locale = locale; _rand = rand; _env = env; }

        public (IEnumerable<SongItem> items, int total) GeneratePage(GenerationParams p, int page, int pageSize)
        {
            var rng = _rand.Create(p.Seed, page);
            var faker = new Faker(locale: p.Lang == "de-DE" ? "de" : p.Lang == "uk-UA" ? "uk" : "en");
            var words = _locale.Get(p.Lang);

            int total = 24; //Song total
            int startIndex = (page - 1) * pageSize + 1;
            int endIndex = Math.Min(startIndex + pageSize - 1, total);
            var list = new List<SongItem>();
            for (int i = startIndex; i <= endIndex; i++)
            {
                var r = _rand.Create(p.Seed, page, i);
                bool isBand = r.NextDouble() < 0.6;
                string artist = isBand ? MakeBand(words, r) : MakePerson(words, r, faker);
                bool isSingle = r.NextDouble() < 0.3;
                string album = isSingle ? "Single" : MakeAlbum(words, r);
                string title = MakeTitle(words, r);
                string genre = words.Genres[r.Next(words.Genres.Count)];
                int likes = LikesFromAverage(p.LikesAvg, r);
                list.Add(new SongItem(i, title, artist, album, genre, likes, isSingle));
            }
            return (list, total);
        }

        private static int LikesFromAverage(double avg, Random r)
        {
            if (avg <= 0) return 0;
            if (avg >= 10) return 10;
            int floor = (int)Math.Floor(avg);
            double frac = avg - floor;
            return floor + (r.NextDouble() < frac ? 1 : 0);
        }

        private static string MakePerson(LocaleData d, Random r, Faker f)
            => $"{d.FirstNames[r.Next(d.FirstNames.Count)]} {d.LastNames[r.Next(d.LastNames.Count)]}";
        private static string MakeBand(LocaleData d, Random r)
            => $"{d.Adjectives[r.Next(d.Adjectives.Count)]} {d.Nouns[r.Next(d.Nouns.Count)]}";
        private static string MakeAlbum(LocaleData d, Random r)
            => $"{d.Nouns[r.Next(d.Nouns.Count)]}";
        private static string MakeTitle(LocaleData d, Random r)
            => $"{d.Adjectives[r.Next(d.Adjectives.Count)]} {d.Nouns[r.Next(d.Nouns.Count)]}";

        public SongDetail BuildDetail(GenerationParams p, int page, int index, SongItem item)
        {
            string coverUrl = $"/media/cover?lang={Uri.EscapeDataString(p.Lang)}&seed={p.Seed}&page={page}&index={index}";
            string audioUrl = $"/media/audio?lang={Uri.EscapeDataString(p.Lang)}&seed={p.Seed}&page={page}&index={index}";
            var review = string.Join(" ", _locale.Get(p.Lang).ReviewPhrases.Take(3));
            var lyrics = BuildLyrics(p, page, index, item);
            return new SongDetail(index, item.Title, item.Artist, item.Album, item.Genre, item.Likes, item.IsSingle, coverUrl, audioUrl, review, lyrics);
        }

        private List<LyricLine> BuildLyrics(GenerationParams p, int page, int index, SongItem item)
        {
            var r = _rand.Create(p.Seed, page, index);
            var lines = new List<LyricLine>();
            double t = 0;
            for (int i = 0; i < 12; i++)
            {
                t += 2.5 + r.NextDouble();
                lines.Add(new LyricLine(Math.Round(t, 2), $"{item.Title} {new string('♪', r.Next(1, 4))}"));
            }
            return lines;
        }

        // ---------- MEDIA GENERATION ----------
        public byte[] MakeCoverPng(GenerationParams p, int page, int index, string title, string artist)
        {
            var r = _rand.Create(p.Seed, page, index);
            int size = 600;
            using var surface = SKSurface.Create(new SKImageInfo(size, size));
            var canvas = surface.Canvas;
            canvas.Clear(new SKColor((byte)r.Next(40, 200), (byte)r.Next(40, 200), (byte)r.Next(40, 200)));

            using var paint = new SKPaint
            {
                Color = SKColors.White,
                IsAntialias = true,
                TextSize = 40,
                Typeface = SKTypeface.FromFamilyName("Arial", SKFontStyle.Bold)
            };
            var margin = 40f;
            canvas.DrawText(title, margin, size / 2f, paint);
            paint.TextSize = 28;
            canvas.DrawText("by " + artist, margin, size / 2f + 50, paint);

            using var img = surface.Snapshot();
            using var data = img.Encode(SKEncodedImageFormat.Png, 90);
            return data.ToArray();
        }

        public byte[] MakeAudioWav(GenerationParams p, int page, int index)
        {
            var r = _rand.Create(p.Seed, page, index);
            int sampleRate = 44100;
            int seconds = 12;
            var waveFormat = WaveFormat.CreateIeeeFloatWaveFormat(sampleRate, 1);
            using var ms = new MemoryStream();
            using (var writer = new WaveFileWriter(ms, waveFormat))
            {
                double[] scale = new[] { 261.63, 293.66, 329.63, 349.23, 392.00, 440.00, 493.88, 523.25 };
                double tempo = 100 + r.Next(0, 60);
                int samplesPerBeat = (int)(sampleRate * 60.0 / tempo);
                int totalSamples = sampleRate * seconds;
                int pos = 0;
                while (pos < totalSamples)
                {
                    double freq = scale[r.Next(scale.Length)] * (r.NextDouble() < 0.2 ? 0.5 : 1.0);
                    int noteLen = samplesPerBeat * r.Next(1, 3);
                    for (int i = 0; i < noteLen && pos < totalSamples; i++, pos++)
                    {
                        double t = pos / (double)sampleRate;
                        float sample = (float)(Math.Sin(2 * Math.PI * freq * t) * 0.2);
                        writer.WriteSample(sample);
                    }
                }
            }
            return ms.ToArray();
        }
    }
}