using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WelderTracker150722.Authorization;
using WelderTracker150722.Data;
using WelderTracker150722.Models;

namespace WelderTracker150722.Controllers
{
    public class CompletedItemModelsController : Controller
    {
        //dependency injection
        private readonly ApplicationDbContext _context;

        public CompletedItemModelsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: CompletedItemModels
        [AllowAnonymous]
        public async Task<IActionResult> Index(int searchInt, string sortOrder, DateTime? start, DateTime end)
        {


            ViewData["NameSortParm"] = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewData["WeldersSortParm"] = String.IsNullOrEmpty(sortOrder) ? "welders_desc" : "";
            ViewData["DateSortParm"] = sortOrder == "Date" ? "date_desc" : "Date";
            var welders = from c in _context.CompletedItemModel
                          select c;
            switch (sortOrder)
            {
                case "name_desc":
                    welders = welders.OrderByDescending(c => c.ItemName);
                    break;
                case "welders_desc":
                    welders = welders.OrderByDescending(c => c.WelderId);
                    break;
                case "Date":
                    welders = welders.OrderBy(c => c.DateofCompletition);
                    break;
                default:
                    welders = welders.OrderBy(c => c.DateofCompletition);
                    break;
            }
            if (searchInt != 0)
            {
                welders = welders.Where(c => c.WelderId == searchInt);
            }


            if (start != null && end!=null)
            {
               welders= welders.Where(x => x.DateofCompletition > start && x.DateofCompletition < end);
            }






            return _context.CompletedItemModel != null ?
                        View(await welders.ToListAsync()) :
                        Problem("Entity set 'ApplicationDbContext.CompletedItemModel'  is null.");
        }

        // GET: CompletedItemModels/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.CompletedItemModel == null)
            {
                return NotFound();
            }

            var completedItemModel = await _context.CompletedItemModel
                .FirstOrDefaultAsync(m => m.Id == id);
            if (completedItemModel == null)
            {
                return NotFound();
            }

            return View(completedItemModel);
        }

        // GET: CompletedItemModels/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: CompletedItemModels/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int? id, [Bind("Id,ItemName,amount,DateofCompletition,WelderId")] CompletedItemModel completedItemModel)
        {
            var isAdmin = User.IsInRole(Constants.AdminRole);
            if (isAdmin == false)
            {
                return Forbid();
            }
            if (ModelState.IsValid)
            {
                _context.Add(completedItemModel);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(completedItemModel);
        }

        // GET: CompletedItemModels/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.CompletedItemModel == null)
            {
                return NotFound();
            }

            var completedItemModel = await _context.CompletedItemModel.FindAsync(id);
            if (completedItemModel == null)
            {
                return NotFound();
            }
            return View(completedItemModel);
        }

        // POST: CompletedItemModels/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,ItemName,amount,DateofCompletition,WelderName")] CompletedItemModel completedItemModel)
        {
            if (id != completedItemModel.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(completedItemModel);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CompletedItemModelExists(completedItemModel.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(completedItemModel);
        }

        // GET: CompletedItemModels/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.CompletedItemModel == null)
            {
                return NotFound();
            }

            var completedItemModel = await _context.CompletedItemModel
                .FirstOrDefaultAsync(m => m.Id == id);
            if (completedItemModel == null)
            {
                return NotFound();
            }

            return View(completedItemModel);
        }

        // POST: CompletedItemModels/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.CompletedItemModel == null)
            {
                return Problem("Entity set 'ApplicationDbContext.CompletedItemModel'  is null.");
            }
            var completedItemModel = await _context.CompletedItemModel.FindAsync(id);
            if (completedItemModel != null)
            {
                _context.CompletedItemModel.Remove(completedItemModel);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CompletedItemModelExists(int id)
        {
            return (_context.CompletedItemModel?.Any(e => e.Id == id)).GetValueOrDefault();
        }

        public void Createnew(int id, string itemName, int amount, DateTime? toCompleteBy, int? welderid)
        {
            CompletedItemModel newItem = new CompletedItemModel { ItemName = itemName, Amount = amount, DateofCompletition = DateTime.Today, WelderId = welderid.Value };
            _context.Add(newItem);
            _context.SaveChanges();
           
        }
    }
}
