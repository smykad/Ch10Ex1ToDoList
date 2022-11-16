using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ToDoList.Models;

namespace ToDoList.Controllers
{
    public class HomeController : Controller
    {
        private ToDoContext context;
        public HomeController(ToDoContext ctx) => context = ctx;

        public IActionResult Index(string id)
        {
            // TODO: create a ToDoViewModel object
            ToDoViewModel model = new ToDoViewModel();
            // load current filters and data needed for filter drop downs in ViewBag
            var filters = new Filters(id);

            // TODO: modify code that stores data in the ViewBag so it stores that data in the view model object
            model.Filters = filters;
            model.Categories = context.Categories.ToList();
            model.Statuses = context.Statuses.ToList();
            model.DueFilters = Filters.DueFilterValues;

            // get ToDo objects from database based on current filters
            IQueryable<ToDo> query = context.ToDos
                .Include(t => t.Category).Include(t => t.Status);
            if (filters.HasCategory) {
                query = query.Where(t => t.CategoryId == filters.CategoryId);
            }
            if (filters.HasStatus) {
                query = query.Where(t => t.StatusId == filters.StatusId);
            }
            if (filters.HasDue) {
                var today = DateTime.Today;
                if (filters.IsPast)
                    query = query.Where(t => t.DueDate < today);
                else if (filters.IsFuture)
                    query = query.Where(t => t.DueDate > today);
                else if (filters.IsToday)
                    query = query.Where(t => t.DueDate == today);
            }
            var tasks = query.OrderBy(t => t.DueDate).ToList();
            
            // TODO: Modify the return statement so it passes the view model to the view 
            model.Tasks = tasks;
            return View(model);
        }

        public IActionResult Add()
        {
            ViewBag.Categories = context.Categories.ToList();
            ViewBag.Statuses = context.Statuses.ToList();
            return View();
        }

        [HttpPost]
        public IActionResult Add(ToDo task)
        {
            if (ModelState.IsValid)
            {
                context.ToDos.Add(task);
                context.SaveChanges();
                return RedirectToAction("Index");
            }
            else
            {
                ViewBag.Categories = context.Categories.ToList();
                ViewBag.Statuses = context.Statuses.ToList();
                return View(task);
            }
        }

        [HttpPost]
        public IActionResult Filter(string[] filter)
        {
            string id = string.Join('-', filter);
            return RedirectToAction("Index", new { ID = id });
        }

        [HttpPost]
        public IActionResult Edit([FromRoute]string id, ToDo selected)
        {
            if (selected.StatusId == null) {
                context.ToDos.Remove(selected);
            }
            else {
                string newStatusId = selected.StatusId;
                selected = context.ToDos.Find(selected.Id);
                selected.StatusId = newStatusId;
                context.ToDos.Update(selected);
            }
            context.SaveChanges();

            return RedirectToAction("Index", new { ID = id });
        }
    }
}