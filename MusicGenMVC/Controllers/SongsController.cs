using Microsoft.AspNetCore.Mvc;
using MusicGenMVC.Models;
using MusicGenMVC.Services;

namespace MusicGenMVC.Controllers
{
    public sealed class SongsController : Controller
    {
        private readonly SongGeneratorService _svc;
        public SongsController(SongGeneratorService svc) { _svc = svc; }


        public IActionResult Index(string lang = "en-US", ulong? seed = null, double likes = 3.5)
        {
            var p = new GenerationParams { Lang = lang, Seed = seed ?? SeededRandomFactory.RandomSeed(), LikesAvg = likes };
            ViewBag.Params = p;
            return View();
        }


        [HttpGet]
        public IActionResult Table(string lang, ulong seed, double likes, int page = 1, int pageSize = 20)
        {
            var p = new GenerationParams { Lang = lang, Seed = seed, LikesAvg = likes };
            var (items, total) = _svc.GeneratePage(p, page, pageSize);
            ViewBag.Total = total; 
            ViewBag.Page = page; 
            ViewBag.PageSize = pageSize; 
            ViewBag.Params = p;
            return PartialView("_Table", items);
        }


        [HttpGet]
        public IActionResult Gallery(string lang, ulong seed, double likes, int page = 1, int pageSize = 100)
        {
            var p = new GenerationParams { Lang = lang, Seed = seed, LikesAvg = likes };
            var (items, total) = _svc.GeneratePage(p, page, pageSize);
            ViewBag.Params = p; 
            ViewBag.Page = page;
            ViewBag.PageSize = pageSize;
            ViewBag.Total = total;
            return PartialView("_Gallery", items);
        }


        [HttpGet]
        public IActionResult Detail(string lang, ulong seed, double likes, int page, int index)
        {
            var p = new GenerationParams { Lang = lang, Seed = seed, LikesAvg = likes };
            var item = _svc.GenerateItem(p, page, index);
            var detail = _svc.BuildDetail(p, page, index, item);
            return PartialView("_RowDetail", detail);
        }

    }
}
