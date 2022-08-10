using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WelderTracker150722.Data;
using WelderTracker150722.Models;

namespace WelderTracker150722.Controllers
{
    public class WeldersModelsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public WeldersModelsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: WeldersModels
        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            
              return _context.WeldersModel != null ? 
                          View(await _context.WeldersModel.ToListAsync()) :
                          Problem("Entity set 'ApplicationDbContext.WeldersModel'  is null.");
        }

        // GET: WeldersModels/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.WeldersModel == null)
            {
                return NotFound();
            }

            var weldersModel = await _context.WeldersModel
                .FirstOrDefaultAsync(m => m.Id == id);
            if (weldersModel == null)
            {
                return NotFound();
            }

            return View(weldersModel);
        }

        // GET: WeldersModels/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: WeldersModels/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public async Task<IActionResult> Create([Bind("Id,WelderId,WelderName")] WeldersModel weldersModel)
        {
            if (ModelState.IsValid)
            {
                _context.Add(weldersModel);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(weldersModel);
        }

        // GET: WeldersModels/Edit/5
        [AllowAnonymous]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.WeldersModel == null)
            {
                return NotFound();
            }

            var weldersModel = await _context.WeldersModel.FindAsync(id);
            if (weldersModel == null)
            {
                return NotFound();
            }
            return View(weldersModel);
        }

        // POST: WeldersModels/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,WelderId,WelderName")] WeldersModel weldersModel)
        {
            if (id != weldersModel.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(weldersModel);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!WeldersModelExists(weldersModel.Id))
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
            return View(weldersModel);
        }
        [AllowAnonymous]
        // GET: WeldersModels/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.WeldersModel == null)
            {
                return NotFound();
            }

            var weldersModel = await _context.WeldersModel
                .FirstOrDefaultAsync(m => m.Id == id);
            if (weldersModel == null)
            {
                return NotFound();
            }

            return View(weldersModel);
        }

        // POST: WeldersModels/Delete/5
        [AllowAnonymous]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.WeldersModel == null)
            {
                return Problem("Entity set 'ApplicationDbContext.WeldersModel'  is null.");
            }
            var weldersModel = await _context.WeldersModel.FindAsync(id);
            if (weldersModel != null)
            {
                _context.WeldersModel.Remove(weldersModel);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool WeldersModelExists(int id)
        {
          return (_context.WeldersModel?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
