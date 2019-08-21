using CMSSampleApplication.Models.Data;
using CMSSampleApplication.Models.ViewModels.Shop;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using PagedList;

namespace CMSSampleApplication.Areas.Admin.Controllers
{
    public class ShopController : Controller
    {
        // GET: Admin/Shop/Categories
        public ActionResult Categories()
        {
            // Declare a list of Model
            List<CategoryVM> categoryVMList;

            using (Db db = new Db())
            {
                categoryVMList = db.Categories
                    .ToArray()
                    .OrderBy(x => x.Sorting)
                    .Select(x => new CategoryVM(x))
                    .ToList();
            }
            //Init the list

            //return view with list
            return View(categoryVMList);
        }

        // POST: Admin/Shop/AddNewCategory
        [HttpPost]
        public string AddNewCategory(string catName)
        {
            //Declare ID
            string id;

            using (Db db = new Db())
            {
                //Check that the Category  name is unique
                if (db.Categories.Any(x => x.Name == catName))
                {
                    return "titletaken";
                }
                //Init DTO
                CategoryDTO dto = new CategoryDTO();
                //Add to DTO
                dto.Name = catName;
                dto.Slug = catName.Replace(" ", "-").ToLower();
                dto.Sorting = 100;
                // Save DTO
                db.Categories.Add(dto);
                db.SaveChanges();
                // Get the Id
                id = dto.Id.ToString();
            }

            //Retutn Id
            return id;
        }

        //POST: Admin/shop/ReorderCategories
        [HttpPost]
        public void ReorderCategories(int[] id)
        {
            using (Db db = new Db())
            {
                //Set initial count
                int count = 1;
                //Declare PageDTO
                CategoryDTO dto;
                //set sorting for each category
                foreach (var catId in id)
                {
                    dto = db.Categories.Find(catId);
                    dto.Sorting = count;
                    db.SaveChanges();
                    count++;
                }
                //
            }
        }

        //GET: Admin/shop/DeleteCategory/id
        public ActionResult DeleteCategory(int id)
        {

            using (Db db = new Db())
            {
                //Get the category
                CategoryDTO dto = db.Categories.Find(id);

                //Remove the page
                db.Categories.Remove(dto);
                //Save
                db.SaveChanges();
            }
            //Redirect

            return RedirectToAction("Categories");
        }

        //GET: Admin/shop/RenameCategory
        public string RenameCategory(string newCatName, int id)
        {
            using (Db db = new Db())
            {
                //Check category name is unique
                if (db.Categories.Any(x => x.Name == newCatName))
                {
                    return "titletaken";
                }

                //Get DTO
                CategoryDTO dto = db.Categories.Find(id);
                //Edit DTO
                dto.Name = newCatName;
                dto.Slug = newCatName.Replace(" ", "-").ToLower();
                //save
                db.SaveChanges();


            }

            //return
            return "ok";
        }
        [HttpGet]
        //GET: Admin/shop/AddProduct
        public ActionResult AddProduct()
        {

            //init model
            ProductVM model = new ProductVM();
            //add select list of categories to model
            using(Db db = new Db())
            {
                model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name"); 
            }
            //return view with model
            return View(model);
        }
        [HttpPost]
        //POST: Admin/shop/AddProduct
        public ActionResult AddProduct(ProductVM model, HttpPostedFileBase file)
        {

            //check model state
            if (! ModelState.IsValid)
            {
                using (Db db = new Db())
                {
                    model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");
                    return View(model);
                }
            }

            using (Db db = new Db())
            {
                if (db.Products.Any(x => x.Name == model.Name))
                {
                    model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");
                    ModelState.AddModelError("", "That product name is taken!");
                    return View(model);
                }
            }

            //product name is unique
            int id;
            //Decalre Product ID
            using (Db db = new Db())
            {
                ProductDTO product = new ProductDTO();
                product.Name = model.Name;
                product.Slug = model.Name.Replace(" ", "-").ToLower();
                product.Description = model.Description;
                product.Price = model.Price;
                product.CategoryId = model.CategoryId;

                CategoryDTO catDTo = db.Categories.FirstOrDefault(x => x.Id == model.CategoryId);
                product.CategoryName = catDTo.Name;

                db.Products.Add(product);
                db.SaveChanges();

                //Get Id

                id = product.Id;
            }


            //Set Tempdata message

            TempData["SM"] = "You Have added a product";

            #region Upload Image
            // Create necessary direcories
            var originalDirectory = new DirectoryInfo(string.Format("{0}Images\\Uploads", Server.MapPath(@"\")));

            
            var pathString1 = Path.Combine(originalDirectory.ToString(), "Products");
            var pathString2 = Path.Combine(originalDirectory.ToString(), "Products\\"+ id.ToString());
            var pathString3 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString() + "\\Thumbs");
            var pathString4 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString() + "\\Gallery");
            var pathString5 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString() + "\\Gallery\\Thumbs");

            if (!Directory.Exists(pathString1))
                Directory.CreateDirectory(pathString1);

            if (!Directory.Exists(pathString2))
                Directory.CreateDirectory(pathString2);

            if (!Directory.Exists(pathString3))
                Directory.CreateDirectory(pathString3);

            if (!Directory.Exists(pathString4))
                Directory.CreateDirectory(pathString4);

            if (!Directory.Exists(pathString5))
                Directory.CreateDirectory(pathString5);


            // Check if a file was uploaded

            if (file != null && file.ContentLength > 0)
            {
                //Get file extension
                string ext = file.ContentType.ToLower();

                //Verify extension
                if (ext != "image/jpg" &&
                    ext != "image/jpeg" &&
                    ext != "image/pjpeg" &&
                    ext != "image/gif" &&
                    ext != "image/x-png" &&
                    ext != "image/png" 
                    )
                {
                    using (Db db = new Db())
                    {
                        model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");
                        ModelState.AddModelError("", "That image was not uploaded - wrong image extension");
                        return View(model);
                    }
                }
                //Init image name
                string imageName = file.FileName;
                //Save image name to DTO
                using (Db db = new Db())
                {
                    ProductDTO dto = db.Products.Find(id);
                    dto.ImageName = imageName;
                    db.SaveChanges();
                }
                //Set original and thumb image path
                var path = string.Format("{0}\\{1}", pathString2, imageName);
                var path2 = string.Format("{0}\\{1}", pathString3, imageName);
                //Save original
                file.SaveAs(path);

                //create and save thumb 
                WebImage img = new WebImage(file.InputStream);
                img.Resize(200, 200);
                img.Save(path2);
            }
            #endregion


            return RedirectToAction("AddProduct");
        }
        //GET: Admin/shop/Products
        public ActionResult Products(int? page, int? catId)
        {
            //Declare list of   ProductVM
            List<ProductVM> listOfProductVM;
            //Set page number
            var pageNumber = page ?? 1;
            using (Db db = new Db())
            {
                //Init the list
                listOfProductVM = db.Products.ToArray()
                    .Where(x => catId == null || catId == 0 || x.CategoryId == catId)
                    .Select(x => new ProductVM(x))
                    .ToList();
                //Populate catogeries select list
                ViewBag.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");
                //Set selectes Category
                ViewBag.SelectedCat = catId.ToString();           
        }

            //Set Pagination
            var onePageOfProducts = listOfProductVM.ToPagedList(pageNumber, 3);
            ViewBag.OnePageOfProducts = onePageOfProducts;
            //Return with the list
            return View(listOfProductVM);
        }
        //GET: Admin/shop/EditProduct/id
        [HttpGet]
        public ActionResult EditProduct(int id)
        {
            //Declare ProductVM
            ProductVM model;
            //Get the Product
            using (Db db = new Db())
            {
                ProductDTO dto = db.Products.Find(id);
                //Make sure product exists
                if (dto == null)
                {
                    return Content("This Product does not exist");
                }
          
                 //init model
                model = new ProductVM(dto);
                //make a select list
                model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");

                //Get all gallery image
                model.GalleryImages = Directory.EnumerateFiles(Server.MapPath("~/Images/Uploads/Products/" + id + "/Gallery/Thumbs"))
                    .Select(fn => Path.GetFileName(fn));
            }

            //retur view with model
            return View(model);
        }
        //POST: Admin/shop/EditProduct/id
        [HttpPost]
        public ActionResult EditProduct(ProductVM model, HttpPostedFileBase file)
        {
            //Get Product Id
            int id = model.Id;
            //Populate categories select list and gallery images
            using (Db db = new Db())
            {
                model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");
            }
            model.GalleryImages = Directory.EnumerateFiles(Server.MapPath("~/Images/Uploads/Products/" + id + "/Gallery/Thumbs"))
                .Select(fn => Path.GetFileName(fn));

            //check Model satate
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            //Make sure product name is unique
            using (Db db = new Db())
            {
                if (db.Products.Where(x => x.Id != id).Any(x => x.Name == model.Name))
                {
                    ModelState.AddModelError("", "The Product name is taken");
                    return View(model);
                }                        
            }
            //Update product
            using (Db db = new Db())
            {
                ProductDTO dto = db.Products.Find(id);
                dto.Name = model.Name;
                dto.Slug = model.Name.Replace(" ", "-").ToLower();
                dto.Description = model.Description;
                dto.Price = model.Price;
                dto.CategoryId = model.CategoryId;
                dto.ImageName = model.ImageName;

                CategoryDTO catDTO = db.Categories.FirstOrDefault(x => x.Id == model.CategoryId);
                dto.CategoryName = catDTO.Name;
                db.SaveChanges();

            }
            //Set Tempdata message
            TempData["SM"] = "You have edited the product";
            #region image upload
            //Check for file upload
            if (file != null && file.ContentLength > 0)
            {
                //Get extension
                string ext = file.ContentType.ToLower();
                //Verify extension
                if (ext != "image/jpg" &&
                  ext != "image/jpeg" &&
                  ext != "image/pjpeg" &&
                  ext != "image/gif" &&
                  ext != "image/x-png" &&
                  ext != "image/png"
                  )
                {
                    using (Db db = new Db())
                    {
                        ModelState.AddModelError("", "The image was not uploaded - wrong image extension");
                        return View(model);
                    }
                }
                //Set Uload directory path
                var originalDirectory = new DirectoryInfo(string.Format("{0}Images\\Uploads", Server.MapPath(@"\")));
                
                var pathString1 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString());
                var pathString2 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString() + "\\Thumbs");

                //delete file from directory
                DirectoryInfo di1 = new DirectoryInfo(pathString1);
                DirectoryInfo di2 = new DirectoryInfo(pathString2);
                foreach (FileInfo file2 in di1.GetFiles())
                {
                    file2.Delete();
                }
                foreach (FileInfo file3 in di2.GetFiles())
                {
                    file3.Delete();
                }
                //save image name
                string imageName = file.FileName;
                using (Db db = new Db())
                {
                    ProductDTO dto = db.Products.Find(id);
                    dto.ImageName = imageName;
                    db.SaveChanges();
                }
                //Save original and thumb image
                var path = string.Format("{0}\\{1}", pathString1, imageName);
                var path2 = string.Format("{0}\\{1}", pathString2, imageName);
                
                file.SaveAs(path);
               
                WebImage img = new WebImage(file.InputStream);
                img.Resize(200, 200);
                img.Save(path2);
            }

            #endregion

            //Redirect
            return RedirectToAction("EditProduct");
        }
        //GET: Admin/shop/DeleteProduct/id
        public ActionResult DeleteProduct(int id)
        {
            //Delete product form Database
            using (Db db = new Db())
            {
                ProductDTO dto = db.Products.Find(id);
                db.Products.Remove(dto);
                db.SaveChanges();
            }
            //Delete product folder

            var originalDirectory = new DirectoryInfo(string.Format("{0}Images\\Uploads", Server.MapPath(@"\")));
            string pathString = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString());
            if (Directory.Exists(pathString))
            {
                Directory.Delete(pathString,true);
            }

            return RedirectToAction("Products");
        }
        //POST: Admin/shop/SaveGalleryImages
        [HttpPost]
        public void SaveGalleryImages(int id)
        {
            //loop through the file
            foreach (string filename in Request.Files)
            {
                //init the file
                HttpPostedFileBase file = Request.Files[filename];

                //Check it is not null
                if (file != null && file.ContentLength > 0)
                {

                    //Set directory path
                    var originalDirectory = new DirectoryInfo(string.Format("{0}Images\\Uploads", Server.MapPath(@"\")));

                    string pathString1 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString() + "\\Gallery");
                    string pathString2 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString() + "\\Gallery\\Thumbs");

                    //set Images path
                    var path = string.Format("{0}\\{1}", pathString1, file.FileName);
                    var path2 = string.Format("{0}\\{1}", pathString2, file.FileName);
                    //Save original and thumb
                    file.SaveAs(path);

                    WebImage img = new WebImage(file.InputStream);
                    img.Resize(200, 200);
                    img.Save(path2);

                }
            }

          
        }
        //POST: Admin/shop/DeleteImage
        [HttpPost]
        public void DeleteImage(int id, string imageName)
        {
            string fullPath1 = Request.MapPath("~/Images/Uploads/Products" + id.ToString() + "/Gallery/" + imageName);
            string fullPath2 = Request.MapPath("~/Images/Uploads/Products" + id.ToString() + "/Gallery/Thumbs/" + imageName);

            if (System.IO.File.Exists(fullPath1))
                System.IO.File.Delete(fullPath1);

            if (System.IO.File.Exists(fullPath2))
                System.IO.File.Delete(fullPath2);
        }
    }
}