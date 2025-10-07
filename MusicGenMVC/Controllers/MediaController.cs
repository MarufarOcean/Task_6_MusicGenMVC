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
    }
}
