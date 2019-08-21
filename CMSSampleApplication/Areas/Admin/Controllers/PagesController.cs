using CMSSampleApplication.Models.Data;
using CMSSampleApplication.Models.ViewModels.Pages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CMSSampleApplication.Areas.Admin.Controllers
{
    public class PagesController : Controller
    {
        // GET: Admin/Pages
        public ActionResult Index()
        {
            List<PageVM> pagesList;

            using (Db db = new Db())
            {
                pagesList = db.Pages.ToArray().OrderBy(x => x.Sorting).Select(x => new PageVM(x)).ToList();
            }
            return View(pagesList);
        }
        // GET: Admin/Pages/AddPages
        [HttpGet]
        public ActionResult AddPage()
        {
            return View();
        }

        // POST: Admin/Pages/AddPage
        [HttpPost]
        public ActionResult AddPage(PageVM model)
        {
            //check Model State
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            using (Db db = new Db())
            {
                //declare slug
                string slug;

                //Init PageDTO
                PageDTO dto = new PageDTO();
                //DTO Title
                dto.Title = model.Title;

                //check for and set slug if nedd be
                if (string.IsNullOrWhiteSpace(model.Slug))
                {
                    slug = model.Title.Replace(" ", "-").ToLower();
                }
                else
                {
                    slug = model.Slug.Replace(" ", "-").ToLower();
                }

                //make sure slug and title be unique

                if (db.Pages.Any(x => x.Title == model.Title) || db.Pages.Any(x => x.Slug == slug))
                {
                    ModelState.AddModelError("", "That title aor slug already exists");
                    return View(model);
                }

                //DTO the rest
                dto.Slug = slug;
                dto.Body = model.Body;
                dto.HasSideBar = model.HasSideBar;
                dto.Sorting = 100;


                //Save DTO
                db.Pages.Add(dto);
                db.SaveChanges();
            }


            //set temp Data message

            TempData["SM"] = "You have added a new page";

            //Redirect
            return RedirectToAction("AddPage");
        }

        //GET: Admin/Pages/EditPage/id
        [HttpGet]
        public ActionResult EditPage(int id)
        {
            PageVM model;
            using (Db db = new Db())
            {
                PageDTO dto = db.Pages.Find(id);
                if (dto == null)
                {
                    return Content("The page does not exists");
                }
                model = new PageVM(dto);
            }
            return View(model);
        }
        //POST: Admin/Pages/EditPage/id
        [HttpPost]
        public ActionResult EditPage(PageVM model)
        {
            //check model state
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            using (Db db = new Db())
            {

                //Get Page Id
                int id = model.Id;

                //Declare Slug

                string slug = null;
                //Get the page
                PageDTO dto = db.Pages.Find(id);

                //DTO the title
                dto.Title = model.Title;
                //check for slug if need be
                if (model.Slug != "home")
                {
                    if (string.IsNullOrWhiteSpace(model.Slug))
                    {
                        slug = model.Title.Replace(" ", "-").ToLower();
                    }
                    else
                    {
                        slug = model.Slug.Replace(" ", "-").ToLower();
                    }
                }
                //make sure title and slug are unique
                if (db.Pages.Where(x => x.Id != id).Any(x => x.Title == model.Title) ||
                    db.Pages.Where(x => x.Id != id).Any(x => x.Slug == model.Slug))
                {
                    ModelState.AddModelError("", "That title or slug already exists");
                    return View(model);
                }

              //DTO the rest
                dto.Slug = slug;
                dto.Body = model.Body;
                dto.HasSideBar = model.HasSideBar;
                //Save the DTO

                db.SaveChanges();
            }

            //Set Tempdata message
            TempData["SM"] = "You have edited the page";
            //redirect
            return RedirectToAction("EditPage");
        }

        public ActionResult PageDetails(int id)
        {
            //Declare Page VM
            PageVM model;

            using (Db db = new Db())
            {
                //Get the Page
                PageDTO dto = db.Pages.Find(id);

                //Confirm page exixts
                if (dto == null)
                {
                    return Content("The page does not exists");
                }
                //init PageVM
                model = new PageVM(dto);
            }
            //return View with model
            return View(model);
        }
        //GET: Admin/Pages/DeletePage/id
        public ActionResult DeletePage(int id)
        {

            using (Db db = new Db())
            {
                //Get the page
                PageDTO dto = db.Pages.Find(id);

                //Remove the page
                db.Pages.Remove(dto);
                //Save
                db.SaveChanges();
            }
            //Redirect

            return RedirectToAction("Index");
        }

        //POST: Admin/Pages/ReorderPages
        [HttpPost]
        public void ReorderPages(int[] id)
        {
            using (Db db = new Db())
            {
                //Set initial count
                int count = 1;
                //Declare PageDTO
                PageDTO dto;
                //set sorting for each page
                foreach (var pageId in id)
                {
                    dto = db.Pages.Find(pageId);
                    dto.Sorting = count;
                    db.SaveChanges();
                    count++;
                }
            //
            }
        }

        [HttpGet]
        //GET: Admin/Pages/EditSidebar
        public ActionResult EditSidebar()
        {
            //Declare Model

            SidebarVM model;
            using (Db db = new Db())
            {
                //Get the DTO
                SidebarDTO dto = db.Sidebar.Find(1);

                //Init Model
                model = new SidebarVM(dto);

            //
            }
            //Return View with model
            return View(model);
        }

        //GET: Admin/Pages/EditSidebar
        [HttpPost]
        public ActionResult EditSidebar(SidebarVM model)
        {

            using (Db db = new Db())
            {
                //Get the DTO
                SidebarDTO dto = db.Sidebar.Find(1);

                //DTO the body
                dto.Body = model.Body;

                //Save
                db.SaveChanges();
            }

            //set Tempdata message
            TempData["SM"] = "You have edited the Sidebar";
            //Redirect

            return RedirectToAction("EditSidebar");
        }
    }
}