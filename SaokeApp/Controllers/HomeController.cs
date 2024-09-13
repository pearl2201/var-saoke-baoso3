using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using SaokeApp.Data;
using SaokeApp.Models;
using System.Diagnostics;

namespace SaokeApp.Controllers
{
    public class HomeController : Controller
    {
        public const string ANALYTIC_CACHE = nameof(ANALYTIC_CACHE);

        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;
        private readonly IMemoryCache _memoryCache;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context, IMemoryCache memoryCache)
        {
            _logger = logger;
            _context = context;
            _memoryCache = memoryCache;
        }

        public async Task<IActionResult> Index([FromQuery(Name = "search")] string searchText, [FromQuery(Name = "min")] int? minAmount, [FromQuery(Name = "max")] int? maxAmount, [FromQuery(Name = "page")] int pageNumber = 1, [FromQuery(Name = "itemsPerPage")] int itemsPerPage = 20)
        {

            if (string.IsNullOrEmpty(searchText))
            {
                return View(new IndexViewModel());
            }

            var queryable = _context.DonateTracks.Where(x => x.SearchVector.Matches(EF.Functions.PlainToTsQuery("vietnamese", searchText)));
            if (minAmount.HasValue)
            {
                queryable = queryable.Where(x => x.Amount >= minAmount.Value);
            }
            if (maxAmount.HasValue)
            {
                queryable = queryable.Where(x => x.Amount <= maxAmount.Value);
            }
            var totalCount = await queryable.CountAsync();
            var pageData = await queryable.OrderBy(x => x.Id).Skip((pageNumber - 1) * itemsPerPage).Take(itemsPerPage).Select(x => new DonateTrackViewModel
            {
                Amount = x.Amount,
                CreatedAt = x.CreatedAt.ToDateTimeUtc(),
                Message = x.Message,
                TransactionId = x.TransactionId
            }).ToListAsync();
            return View(new IndexViewModel
            {
                Min = minAmount,
                Max = maxAmount,
                DonateTracks = pageData,
                ItemsPerPage = itemsPerPage,
                PageNumber = pageNumber,
                TotalCount = totalCount,
                Search = searchText
            });
        }

        public async Task<IActionResult> Analytic()
        {
            if (!_memoryCache.TryGetValue(ANALYTIC_CACHE, out AnalyticViewModel cacheValue))
            {
                long maxAmount = await _context.DonateTracks.Select(x => x.Amount).MaxAsync();
                long totalAmount = await _context.DonateTracks.Select(x => x.Amount).SumAsync();
                int totalPerson = await _context.DonateTracks.CountAsync();
                var distributed = await _context.DonateTracks.Select(x => new
                {
                    Range_0_50000 = _context.DonateTracks.Where(x => x.Amount < 50000).Count(),
                    Range_50000_100000 = _context.DonateTracks.Where(x => x.Amount > 50000 && x.Amount < 100000).Count(),
                    Range_100000_200000 = _context.DonateTracks.Where(x => x.Amount > 100000 && x.Amount < 200000).Count(),
                    Range_200000_500000 = _context.DonateTracks.Where(x => x.Amount >= 100000 && x.Amount < 500000).Count(),
                    Range_500000_1000000 = _context.DonateTracks.Where(x => x.Amount >= 500000 && x.Amount < 1000000).Count(),
                    Range_1000000_5000000 = _context.DonateTracks.Where(x => x.Amount >= 1000000 && x.Amount < 5000000).Count(),
                    Range_5000000_10000000 = _context.DonateTracks.Where(x => x.Amount >= 5000000 && x.Amount < 10000000).Count(),
                    Range_10000000_50000000 = _context.DonateTracks.Where(x => x.Amount >= 10000000 && x.Amount < 50000000).Count(),
                    Range_50000000_100000000 = _context.DonateTracks.Where(x => x.Amount >= 50000000 && x.Amount < 100000000).Count(),
                    Range_100000000_500000000 = _context.DonateTracks.Where(x => x.Amount >= 100000000 && x.Amount < 500000000).Count(),
                    Range_500000000_up = _context.DonateTracks.Where(x => x.Amount >= 500000000).Count(),
                }).FirstOrDefaultAsync();
                var donateTimeSeries = await _context.DonateTracks.GroupBy(x => x.CreatedAt.ToDateTimeUtc().Date).Select(x => new { Key = x.Key, Value = x.Select(y => y.Amount).Sum() }).ToListAsync();

                cacheValue = new AnalyticViewModel
                {
                    MaxAmount = maxAmount,
                    TotalDonateAmount = totalAmount,
                    TotalPersonCount = totalPerson,
                    DistributedAmount = new ApexChartTreeMapDataSet
                    {
                        Data = new List<ApexChartTreeMapDataPoint>
                        {
                            new ApexChartTreeMapDataPoint{X = "Dưới 50k",Value = distributed.Range_0_50000},
                            new ApexChartTreeMapDataPoint{X = "50k tới 100k",Value = distributed.Range_50000_100000},
                            new ApexChartTreeMapDataPoint{X = "100k tới 200k",Value = distributed.Range_100000_200000},
                            new ApexChartTreeMapDataPoint{X = "200k tới 500k",Value = distributed.Range_200000_500000},
                            new ApexChartTreeMapDataPoint{X = "500k tới 1 triệu",Value = distributed.Range_500000_1000000},
                            new ApexChartTreeMapDataPoint{X = "1 triệu tới 5 triệu",Value = distributed.Range_1000000_5000000},
                            new ApexChartTreeMapDataPoint{X = "5 triệu tới 10 triệu",Value = distributed.Range_5000000_10000000},
                            new ApexChartTreeMapDataPoint{X = "10 triêu tới 50 triệu",Value = distributed.Range_10000000_50000000},
                            new ApexChartTreeMapDataPoint{X = "50 triệu tới 100 triệu",Value = distributed.Range_50000000_100000000},
                            new ApexChartTreeMapDataPoint{X = "100 triệu tới 500 triệu",Value = distributed.Range_100000000_500000000},
                            new ApexChartTreeMapDataPoint{X = "Trên 500 triệu",Value = distributed.Range_500000000_up},

                        }
                    },
                    DonateTimeSeries = new ApexChartViewModel<DateTime, long>
                    {
                        Labels = donateTimeSeries.OrderBy(x => x.Key).Select(x => x.Key).ToList(),
                        Series = new List<ApexChartDataSet<long>>
                        {
                            new ApexChartDataSet<long>
                            {
                                Name = "Donate",
                                Data = donateTimeSeries.OrderBy(x => x.Key).Select(x => x.Value).ToList()
                            }
                        }

                    }
                };

                var cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromDays(7));

                _memoryCache.Set(ANALYTIC_CACHE, cacheValue, cacheEntryOptions);
            }


            return View(cacheValue);


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
