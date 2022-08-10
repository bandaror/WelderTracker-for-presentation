using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Data;
//using Microsoft.AspNetCore.Identity;
//using Microsoft.AspNet.Identity;
using WelderTracker150722.Authorization;
using WelderTracker150722.Data;
using WelderTracker150722.Models;

namespace WelderTracker150722.Controllers


{
    public class OrderItemsController : Controller
    {
        //Dependency injection
        private readonly ApplicationDbContext _context;

        public OrderItemsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: OrderItems


        [AllowAnonymous]
        public async Task<IActionResult> Index(string searchString, string sortOrder)
        {
           //sorting the orders
            ViewData["NameSortParm"] = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewData["DateSortParm"] = sortOrder == "Date" ? "date_desc" : "Date";
            var orders = from o in _context.Items
                         select o;
            switch (sortOrder)
            {
                case "name_desc":
                    orders = orders.OrderByDescending(o => o.ItemName);
                    break;
                case "Date":
                    orders = orders.OrderBy(o => o.ToCompleteBy);
                    break;
                default:
                    orders = orders.OrderBy(o => o.ToCompleteBy);
                    break;
            }
            //searching field
            if (!String.IsNullOrEmpty(searchString))
            {
                orders = orders.Where(o => o.ItemName!.Contains(searchString));
            }
            return _context.Items != null ?
                        View(await orders.ToListAsync()) :
                        Problem("Entity set 'ApplicationDbContext.Items'  is null.");
        }

        // GET: OrderItems/Details/5
        [AllowAnonymous]
        public async Task<IActionResult> Details(int? id)
        {

            if (id == null || _context.Items == null)
            {
                return NotFound();
            }

            var orderItems = await _context.Items.Include(c => c.Welders).AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == id);
            if (orderItems == null)
            {
                return NotFound();
            }


            return View(orderItems);
        }

        // GET: OrderItems/Create
        public IActionResult Create()
        {
            PopulateWeldersDropDownList();
            return View();
        }

        // POST: OrderItems/Create
      
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create( OrderItemsModel orderItems)
        {
            var isAdmin = User.IsInRole(Constants.AdminRole);
            if (isAdmin == false)
            {
                return Forbid();
            }
            if (ModelState.IsValid)
            {
                var order = new OrderItemsModel();
                order.Welders = new List<WeldersModel>();
                _context.Add(orderItems);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            PopulateWeldersDropDownList(orderItems.WeldersId);

            return View(orderItems);
        }
        [AllowAnonymous]
        // GET: OrderItems/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Items == null)
            {
                return NotFound();
            }

            var orderItems = await _context.Items.AsNoTracking().
                FirstOrDefaultAsync(m => m.Id == id);
            if (orderItems == null)
            {
                return NotFound();
            }
            PopulateWeldersDropDownList(orderItems.WeldersId);

            return View(orderItems);
        }

        [AllowAnonymous]
        [HttpPost, ActionName("Edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPost(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var orderItemToUpdate = await _context.Items
                .FirstOrDefaultAsync(c => c.Id == id);

            if (await TryUpdateModelAsync<OrderItemsModel>(orderItemToUpdate,
                "",
                c => c.FirNumber, c => c.WeldersId, c => c.ItemName, c => c.Status, c => c.Amount, c => c.CompletedAmount))
            {
                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateException /* ex */)
                {
                    //Log the error (uncomment ex variable name and write a log.)
                    ModelState.AddModelError("", "Unable to save changes. " +
                        "Try again, and if the problem persists, " +
                        "see your system administrator.");
                }
                //update the completed amount to the total amount once the order is marked as complete or the completedamount is equal to the total amount
                if (orderItemToUpdate.Status == OrderStatus.Completed || orderItemToUpdate.Amount == orderItemToUpdate.CompletedAmount)
                {
                    var completeitem = new CompletedItemModelsController(_context);
                    orderItemToUpdate.CompletedAmount = orderItemToUpdate.Amount;


                    if (orderItemToUpdate.Status != OrderStatus.Completed)
                    {
                        orderItemToUpdate.Status = OrderStatus.Completed;
                    }
                    completeitem.Createnew(orderItemToUpdate.Id, orderItemToUpdate.ItemName, orderItemToUpdate.Amount,
                        orderItemToUpdate.ToCompleteBy, orderItemToUpdate.WeldersId);

                   
                }
                //mark the order as started as soon as the completed item is bigger than 0 but less than the total amount OR the welderId is not null
                if (orderItemToUpdate.CompletedAmount >= 0 && orderItemToUpdate.CompletedAmount < orderItemToUpdate.Amount &&
                    orderItemToUpdate.WeldersId != null)
                {
                    var completeitem = new CompletedItemModelsController(_context);
                    orderItemToUpdate.Status = OrderStatus.Started;


                    completeitem.Createnew(orderItemToUpdate.Id, orderItemToUpdate.ItemName, orderItemToUpdate.Amount,
                        orderItemToUpdate.ToCompleteBy, orderItemToUpdate.WeldersId);


                }

                return RedirectToAction(nameof(Index));
            }
            PopulateWeldersDropDownList(orderItemToUpdate.WeldersId);

            return View(orderItemToUpdate);
        }
        //using viewbag to hold the welders so the user can choose one from the index view
        private void PopulateWeldersDropDownList(object selectedWelder = null)
        {
            var weldersQuery = from d in _context.WeldersModel
                               orderby d.WelderId
                               select d;
            ViewBag.WelderId = new SelectList(weldersQuery.AsNoTracking(), "WelderId", "WelderName", selectedWelder);
        }

        // GET: OrderItems/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            var isAdmin = User.IsInRole(Constants.AdminRole);
            if (isAdmin == false)
            {
                return Forbid();
            }
            if (id == null || _context.Items == null)
            {
                return NotFound();
            }

            var orderItems = await _context.Items.Include(c => c.Welders).AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == id);
            if (orderItems == null)
            {
                return NotFound();
            }

            return View(orderItems);
        }

        // POST: OrderItems/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Items == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Items'  is null.");
            }
            var orderItems = await _context.Items.FindAsync(id);
            if (orderItems != null)
            {
                _context.Items.Remove(orderItems);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

       /* private bool OrderItemsExists(int id)
        {
            return (_context.Items?.Any(e => e.Id == id)).GetValueOrDefault();
        }
       */


    }


}




