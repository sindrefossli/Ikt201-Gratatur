using Microsoft.AspNetCore.Mvc;
using NorMall.Data;
using System.Linq;
using System.Runtime.InteropServices.JavaScript;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace NorMall.Controllers
{
    /*
     * This controller serves the purpose of handling actions any user of the website should be able to utilize
     */
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _db;

        // Dependency inject ApplicationDbContext
        public HomeController(ApplicationDbContext db)
        {
            _db = db;
        }

        
        //Action retrieves the image specified by the id sent in as a parameter, and returns the image as a file in a url
        public IActionResult GetImage(int id)
        {
            var image = _db.Images.FirstOrDefault(i => i.Id == id);

            if (image != null)
            {
                return File(image.ImageData, image.ImageMimeType);
            }

            return NotFound();
        }

        //Action updates the page value stored in the client-server session based on the parameter value for currentPage
        [HttpPost]
        public IActionResult UpdateCurrentPage(int currentPage)
        {
            HttpContext.Session.SetInt32("currentPage", currentPage);

            return Ok();
        }

        //Function stores categories selected in client-server session by extracting the string sent in as parameter which is in json format
        public void SetSelectedCategoriesInSession(string selectedCategories)
        {
            var categoriesList = JsonSerializer.Deserialize<List<string>>(selectedCategories);
            var categoriesString = string.Join(",", categoriesList);

            HttpContext.Session.SetString("selectedCategories", categoriesString);
        }

        //Function stores users search string in client-server session based on the string passed through the parameter
        public void SetSearchString(string inputSearchString)
        {
            if (inputSearchString == null)
            {
                inputSearchString = "";
            }

            HttpContext.Session.SetString("searchString", inputSearchString);

            var searchedString = HttpContext.Session.GetString("searchString");
        }

        //Action retrieves a list of businesses to display for the user in the NorMall front page
        public IActionResult Index()
        {
            var pageSize = 15;
            var currentPage = HttpContext.Session.GetInt32("currentPage") ?? 1;

            ViewBag.CurrentPage = currentPage;

            var selectedCategories = HttpContext.Session.GetString("selectedCategories")?.Split(',');
            var searchString = HttpContext.Session.GetString("searchString");

            if (selectedCategories != null && !(selectedCategories[0].Length > 1))
            {
                selectedCategories = null;
            }

            var businesses = _db.Businesses
                .Include(b => b.Product)
                .ThenInclude(p => p.Categories)
                .Include(b => b.HomePageConfig)
                .ThenInclude(hpc => hpc.Images).ToList();

            businesses = businesses.Where(b =>
                    (string.IsNullOrEmpty(searchString) ||
                     b.Product.Any(p =>
                         p.Name.ToLower().Contains(searchString.ToLower()) || // Search in product names
                         p.Categories.Any(c =>
                             c.Name.ToLower().Contains(searchString.ToLower())))) && // Search in categories
                    (selectedCategories == null ||
                     selectedCategories.Length == 0 || // Check for selected categories
                     b.Product.Any(p =>
                         p.Categories.Any(c =>
                             selectedCategories.Contains(c.Name.ToLower())))))
                .ToList();
            var maxPage = (int)Math.Ceiling((double)businesses.Count() / pageSize);
            ViewBag.MaxPage = maxPage;

            if (currentPage > maxPage)
            {
                currentPage = maxPage;
                UpdateCurrentPage(currentPage);
                ViewBag.CurrentPage = currentPage;
            }
            else if (currentPage < 1)
            {
                currentPage = 1;
                UpdateCurrentPage(currentPage);
                ViewBag.CurrentPage = currentPage;
            }


            if (searchString != null)
            {
                ViewBag.SearchString = searchString;
            }

            if (selectedCategories != null)
            {
                ViewBag.SelectedCategories = selectedCategories;
            }

            businesses = businesses
                .Skip((currentPage - 1) * pageSize)
                .Take(pageSize).ToList();

            return View(businesses);
        }

        //Action retrieves a businesses products as a list for a specified business passed through the parameter of the action
        public IActionResult BusinessFrontPage(int businessId)
        {
            var business = _db.Businesses
                .Include(b => b.HomePageConfig)
                .ThenInclude(config => config.Images)
                .FirstOrDefault(b => b.Id == businessId);

            if (business == null)
            {
                return RedirectToAction("Index");
            }

            var pageSize = 16;
            var currentPage = HttpContext.Session.GetInt32("currentPage") ?? 1;

            ViewBag.CurrentPage = currentPage;

            string searchString = HttpContext.Session.GetString("searchString");


            var selectedCategories = HttpContext.Session.GetString("selectedCategories")?.Split(',');
            if (selectedCategories != null && !(selectedCategories[0].Length > 1))
            {
                selectedCategories = null;
            }

            var productQuery = _db.Products.Include(p => p.Images)
                .Where(p => p.BusinessId == businessId &&
                            (string.IsNullOrEmpty(searchString) ||
                             p.Name.ToLower().Contains(searchString.ToLower()) ||
                             p.Categories.Any(c => c.Name.ToLower().Contains(searchString.ToLower()))) &&
                            (selectedCategories == null || selectedCategories.Length == 0 ||
                             p.Categories.Any(c => selectedCategories.Contains(c.Name.ToLower())))).OrderByDescending(p => p.Id);

            var maxPage = (int)Math.Ceiling((double)productQuery.Count() / pageSize);
            
            ViewBag.MaxPage = maxPage;
            
            if (currentPage > maxPage)
            {
                currentPage = maxPage;
                UpdateCurrentPage(currentPage);
                ViewBag.CurrentPage = currentPage;
            }
            else if (currentPage < 1)
            {
                currentPage = 1;
                UpdateCurrentPage(currentPage);
                ViewBag.CurrentPage = currentPage;
            }


            business.Product = productQuery
                .Skip((currentPage - 1) * pageSize)
                .Take(pageSize)
                .ToList();


            if (searchString != null)
            {
                ViewBag.SearchString = searchString;
            }

            if (selectedCategories != null)
            {
                ViewBag.SelectedCategories = selectedCategories;
            }

            return View(business);
        }

        // Action for the Product page
        // This action displays a product based on the provided ID
        public IActionResult Product(int id)
        {
            // Fetch the product details directly from the database
            var product = _db.Products.Include(p => p.Images).FirstOrDefault(p => p.Id == id);

            if (product == null)
            {
                return NotFound(); // Return a 404 Not Found response if the product is not found
            }

            return View(product); // Return the product details to the view
        }


        // Action for the 'About Us' page
        public IActionResult AboutUs()
        {
            return View();
        }

        //Placeholder for informational pages
        public IActionResult OurOffer()
        {
            return View();
        }
        
        //Placeholder for informational pages
        public IActionResult WhatWeDo()
        {
            return View();
        }
    }
}