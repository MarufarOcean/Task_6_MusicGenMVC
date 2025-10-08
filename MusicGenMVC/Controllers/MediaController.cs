using Microsoft.AspNetCore.Mvc;
using MusicGenMVC.Models;
using MusicGenMVC.Services;

namespace MusicGenMVC.Controllers
{
    public sealed class MediaController : Controller
    {
        private readonly SongGeneratorService _svc;
        public MediaController(SongGeneratorService svc) { _svc = svc; }


        [HttpGet("/media/cover")]
        public IActionResult Cover(string lang, ulong seed, int page, int index, string? title = null, string? artist = null)
        {
            var bytes = _svc.MakeCoverPng(new GenerationParams { Lang = lang, Seed = seed }, page, index, title ?? "", artist ?? "");
            return File(bytes, "image/png");
        }


        [HttpGet("/media/audio")]
        public IActionResult Audio(string lang, ulong seed, int page, int index)
        {
            var bytes = _svc.MakeAudioWav(new GenerationParams { Lang = lang, Seed = seed }, page, index);
            return File(bytes, "audio/wav");
        }

        [HttpGet("/media/proxy")]
        public async Task<IActionResult> Proxy(string url)
        {
            if (string.IsNullOrWhiteSpace(url)) return BadRequest();
            if (!Uri.TryCreate(url, UriKind.Absolute, out var u)) return BadRequest();
            if (!(u.Host.EndsWith("picsum.photos", StringComparison.OrdinalIgnoreCase) ||
                  u.Host.EndsWith("i.picsum.photos", StringComparison.OrdinalIgnoreCase) ||
                  u.Host.EndsWith("fastly.picsum.photos", StringComparison.OrdinalIgnoreCase)))
                return Forbid();

            using var http = new HttpClient();
            var resp = await http.GetAsync(u, HttpCompletionOption.ResponseHeadersRead);
            if (!resp.IsSuccessStatusCode) return StatusCode((int)resp.StatusCode);

            var contentType = resp.Content.Headers.ContentType?.ToString() ?? "image/jpeg";
            var bytes = await resp.Content.ReadAsByteArrayAsync();
            return File(bytes, contentType);
        }
    }
}
