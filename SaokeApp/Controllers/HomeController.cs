using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SaokeApp.Data;
using SaokeApp.Models;
using System.Diagnostics;

namespace SaokeApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;
        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index([FromQuery(Name = "search")] string searchText, [FromQuery(Name = "page")] int pageNumber = 1, [FromQuery(Name = "itemsPerPage")] int itemsPerPage = 20)
        {
           
            if (string.IsNullOrEmpty(searchText))
            {
                return View(new IndexViewModel());
            }

            var queryable = _context.DonateTracks.Where(x => x.SearchVector.Matches(EF.Functions.PlainToTsQuery("vietnamese", searchText)));
            var totalCount = await queryable.CountAsync();
            var pageData = await queryable.OrderBy(x => x.Id).Skip((pageNumber-1)* itemsPerPage).Take(itemsPerPage).Select(x => new DonateTrackViewModel
            {
                Amount = x.Amount,
                CreatedAt = x.CreatedAt.ToDateTimeUtc(),
                Message = x.Message,
                TransactionId = x.TransactionId
            }).ToListAsync();
            return View(new IndexViewModel
            {
                DonateTracks = pageData,
                ItemsPerPage = itemsPerPage,
                PageNumber = pageNumber,
                TotalCount = totalCount,
                Search = searchText
            });
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
