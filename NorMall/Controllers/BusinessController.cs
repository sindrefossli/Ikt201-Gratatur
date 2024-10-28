using System.IO.Compression;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.V4.Pages.Account.Internal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using NorMall.Data;
using NorMall.Models;

namespace NorMall.Controllers;

/*
 * This controller controls all functionality specific to business owners
 * Only users with the role of a business owner is authorized for any of the acitons
 */
[Authorize(Roles = "BusinessOwner")]
public class BusinessController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public BusinessController(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    //Action retrieves the business details page for a business owner
    [HttpGet]
    public async Task<IActionResult> BusinessDetails()
    {
        //Display Business information as well as Buttons to go to InventoryManagement, EditHomePageConfig,
        var user = await _userManager.GetUserAsync(User);
        if (user != null)
        {
            var userWithBusiness = _db.Users
                .Include(u => u.Business)
                .ThenInclude(b => b.HomePageConfig)
                .ThenInclude(config => config.Images)
                .FirstOrDefault(u => u.Id == user.Id);
            return View(userWithBusiness);
        }

        return Forbid();
    }

    //Action sends user to a form for the user to create a new home page configuration
    [HttpGet]
    public IActionResult AddHomePageConfig()
    {
        return View();
    }

    //Action retrieves information from the form provided by the user, and handles the logic to add the home page configuration to the applications database
    [HttpPost]
    public async Task<IActionResult> AddHomePageConfig([Bind("Images, Name")] HomePageConfig homePageConfig)
    {
        // Retrieve the currently logged-in user
        var user = await _userManager.GetUserAsync(User);

        if (!ModelState.IsValid)
        {
            return View(homePageConfig);
        }

        if (user != null)
        {
            // Create a new HomePageConfig
            var newHomePageConfig = new HomePageConfig
            {
                BusinessId = user.BusinessId,
                Name = homePageConfig.Name,
                Active = true,
                Images = new List<Image>()
            };

            var userWithBusiness = _db.Users
                .Include(u => u.Business)
                .ThenInclude(b => b.HomePageConfig)
                .ThenInclude(config => config.Images)
                .FirstOrDefault(u => u.Id == user.Id);

            var business = userWithBusiness.Business;

            if (business.HomePageConfig.Count != 0)
            {
                var previousActiveConfig = business.HomePageConfig?.FirstOrDefault(config => config.Active);

                if (previousActiveConfig != null)
                {
                    previousActiveConfig.Active = false;
                    _db.HomePageConfigs.Update(previousActiveConfig);
                }
            }

            Image imageBanner = homePageConfig.Images[0];
            Image imageAdvert = homePageConfig.Images[1];

            _db.HomePageConfigs.Add(newHomePageConfig);

            using (var memoryStream = new MemoryStream())
            {
                await imageAdvert.ImageFile.CopyToAsync(memoryStream);
                imageAdvert.ImageData = memoryStream.ToArray();
                imageAdvert.ImageMimeType = imageAdvert.ImageFile.ContentType;
            }

            using (var memoryStream = new MemoryStream())
            {
                await imageBanner.ImageFile.CopyToAsync(memoryStream);
                imageBanner.ImageData = memoryStream.ToArray();
                imageBanner.ImageMimeType = imageBanner.ImageFile.ContentType;
            }

            imageAdvert.EntityType = "Advert";
            imageBanner.EntityType = "Banner";
            imageAdvert.Business = user.Business;
            imageBanner.Business = user.Business;
            imageAdvert.HomePageConfig = newHomePageConfig;
            imageBanner.HomePageConfig = newHomePageConfig;

            _db.Images.AddRange(imageAdvert, imageBanner);

            await _db.SaveChangesAsync();

            // Redirect to a success page or perform any other action
            return RedirectToAction("BusinessDetails");
        }

        // If ModelState is not valid or user is not found, return to the form with validation errors
        return View("AddHomePageConfig", homePageConfig);
    }

    //Action returns inventory management page where a list of products sold by the related business are displayed
    [HttpGet]
    public async Task<IActionResult> InventoryManagement()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound();
        }

        var products = _db.Products.Include(p => p.Images).Where(p => p.BusinessId == user.BusinessId).ToList();

        return View(products);
    }

    //Action to set an active home page based on the users selected active home page from a dropdown selector in business details
    [HttpPost]
    public async Task<IActionResult> SetActiveHomePageConfig(int id)
    {
        var newActiveConfig = _db.HomePageConfigs.FirstOrDefault(p => p.Id == id);
        var businessId = newActiveConfig.BusinessId;

        if (businessId != null)
        {
            var previousActiveConfig =
                _db.HomePageConfigs.FirstOrDefault(config => config.BusinessId == businessId && config.Active);
            if (previousActiveConfig != null)
            {
                previousActiveConfig.Active = false;
                _db.HomePageConfigs.Update(previousActiveConfig);

                newActiveConfig.Active = true;
                _db.HomePageConfigs.Update(newActiveConfig);
                await _db.SaveChangesAsync();
                return RedirectToAction("BusinessDetails");
            }
        }

        return RedirectToAction("BusinessDetails");
    }

    //Action returns the add product view with a form for the user to add a new product to their business
    [HttpGet]
    public IActionResult AddProduct()
    {
        var product = new Product();

        return View(product);
    }

    //Action retrieves information provided by the users input from the add product form, and creates the new product and stores it in the applications database
    [HttpPost]
    public async Task<IActionResult> AddProduct([Bind("Name, StockAmount, Price, Description")] Product product)
    {
        var user = await _userManager.GetUserAsync(User);

        if (ModelState.IsValid) //Bruk denne
        {
            var newProduct = new Product
            {
                BusinessId = user.BusinessId,
                Name = product.Name,
                StockAmount = product.StockAmount,
                Price = product.Price,
                Description = product.Description,
                Images = new List<Image>()
            };

            _db.Products.Add(newProduct);
            await _db.SaveChangesAsync();

            return RedirectToAction("InventoryManagement");
        }

        return View(product);
    }

    //Action returns the edit product page, by sending in a product and its related information into the razor page model
    //This is requested through the inventory management page
    [HttpGet]
    public async Task<IActionResult> EditProduct(int id)
    {
        var user = await _userManager.GetUserAsync(User);

        var product = _db.Products
            .Include(p => p.Images)
            .Include(p => p.Categories)
            .FirstOrDefault(p => p.Id == id);

        if (product == null)
        {
            return NotFound();
        }

        if (user.BusinessId != product.BusinessId)
        {
            return Forbid();
        }

        product.Images.OrderBy(i => i.OrderIndex);
        return View(product);
    }

    //Action retrieves user input in a form from the edit product page, overwriting previous information and saving it to the database
    //This is requested upon the form submission in the edit product page
    [HttpPost]
    public async Task<IActionResult> EditProduct(int id,
        [Bind("Id, Name, StockAmount, Price, Description, Images")]
        Product product)
    {
        var user = await _userManager.GetUserAsync(User);

        if (!ModelState.IsValid)
        {
            return View(product);
        }

        // Find the existing product in the database
        var existingProduct = _db.Products.Include(i => i.Images).FirstOrDefault(i => i.Id == id);

        if (existingProduct == null)
        {
            return NotFound();
        }

        // Update the properties of the existing product with the new values
        existingProduct.Name = product.Name;
        existingProduct.StockAmount = product.StockAmount;
        existingProduct.Price = product.Price;
        existingProduct.Description = product.Description;

        // Save the changes to the database
        _db.Products.Update(existingProduct);
        await _db.SaveChangesAsync();

        return RedirectToAction("InventoryManagement");
    }

    //Function returns a files mime type by reading the files name, which is used for extracting images from a zip file
    private string GetMimeType(string fileName)
    {
        string extension = Path.GetExtension(fileName).ToLowerInvariant();
        string mimeType = "application/octet-stream";

        if (extension == ".jpg" || extension == ".jpeg")
        {
            mimeType = "image/jpeg";
        }
        else if (extension == ".png")
        {
            mimeType = "image/png";
        }

        return mimeType;
    }

    //Action adds a given image to a product and saves it to the database. The products id and image file is passed through the actions parameters
    //The file sent in can also be a zip file. The action extracts all image files (jpg, jpeg, or png) and adds the to the database.
    //This is requested through the edit product page
    [HttpPost]
    public async Task<IActionResult> AddImage(int productId, IFormFile inputFile)
    {
        if (inputFile != null)
        {
            var product = _db.Products.Include(p => p.Images).FirstOrDefault(p => p.Id == productId);

            if (product != null)
            {
                if (Path.GetExtension(inputFile.FileName) == ".zip")
                {
                    var extractedImages = new List<Image>();

                    using (var archive = new ZipArchive(inputFile.OpenReadStream(), ZipArchiveMode.Read))
                    {
                        var count = product.Images.Count;
                        foreach (var entry in archive.Entries)
                        {
                            if (entry.FullName.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) ||
                                entry.FullName.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase) ||
                                entry.FullName.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
                            {
                                using (var stream = entry.Open())
                                {
                                    using (var memoryStream = new MemoryStream())
                                    {
                                        stream.CopyTo(memoryStream);

                                        var image = new Image
                                        {
                                            ImageData = memoryStream.ToArray(),
                                            ImageMimeType = GetMimeType(entry.Name),
                                            Product = product,
                                            EntityType = "Product",
                                            OrderIndex = count
                                            
                                        };

                                        _db.Images.Add(image);
                                        extractedImages.Add(image);
                                        count++;
                                    }
                                }
                            }
                        }

                        await _db.SaveChangesAsync();
                        
                        System.IO.File.Delete(inputFile.FileName);
                    }
                }
                else
                {
                    var image = new Image();
                    image.ImageFile = inputFile;

                    using (var memoryStream = new MemoryStream())
                    {
                        await image.ImageFile.CopyToAsync(memoryStream);
                        image.ImageData = memoryStream.ToArray();
                        image.ImageMimeType = image.ImageFile.ContentType;
                    }

                    _db.Products.Update(product);
                    await _db.SaveChangesAsync();

                    image.EntityType = "Product";
                    image.OrderIndex = product.Images.Count;
                    image.BusinessId = product.BusinessId;
                    image.ProductId = product.Id;


                    product.Images.Add(image);
                    _db.Images.Add(image);
                    _db.Products.Update(product);
                    await _db.SaveChangesAsync();
                }
            }
        }

        return RedirectToAction("EditProduct", new { id = productId });
    }

    //Action removes an existing image from a product and saves it to the database
    //This is requested through the edit product page
    [HttpPost]
    public async Task<IActionResult> RemoveImage(int productId, int imageId)
    {
        var product = await _db.Products.FirstOrDefaultAsync(p => p.Id == productId);
        var image = await _db.Images.FirstOrDefaultAsync(i => i.Id == imageId);

        if (product != null && image != null)
        {
            product.Images.Remove(image);
            _db.Images.Remove(image);
            _db.Products.Update(product);
            await _db.SaveChangesAsync();
        }

        return RedirectToAction("EditProduct", new { id = productId });
    }

    //Action updates the orderIndex of a products images, and saves it to the database
    //The image order is sent in as a key=value through the actions parameter
    //The order has been selected by the user in the edit product page
    [HttpPost]
    public async Task<IActionResult> UpdateImageOrder(int productId, Dictionary<int, int> userDefinedOrder)
    {
        var product = await _db.Products.Include(p => p.Images).FirstOrDefaultAsync(p => p.Id == productId);

        if (product != null)
        {
            var orderedImages = product.Images.OrderBy(image => userDefinedOrder[image.Id]).ToList();

            var index = 0;
            // Add the ordered images back to the product
            foreach (var image in orderedImages)
            {
                image.OrderIndex = index;
                _db.Images.Update(image);

                index++;
            }

            _db.Products.Update(product);
            await _db.SaveChangesAsync();
        }

        return RedirectToAction("EditProduct", new { id = productId });
    }

    //Action adds a category to a product, the category does not duplicate if it already exists,
    //but gets a new relation to the product it is being added to and saves it in the database
    //This is requested through the edit product page
    [HttpPost]
    public async Task<IActionResult> AddCategory(int productId, string newCategory)
    {
        if (newCategory != null)
        {
            newCategory = newCategory.ToLower();
            var product = _db.Products.Include(p => p.Categories).FirstOrDefault(p => p.Id == productId);

            if (product != null)
            {
                if (_db.Categories.Any(c => c.Name == newCategory))
                {
                    var existingCategory = await _db.Categories.FirstOrDefaultAsync(c => c.Name == newCategory);

                    if (!product.Categories.Any(c => c.Name == existingCategory.Name))
                    {
                        product.Categories.Add(existingCategory);
                        _db.Products.Update(product);
                        await _db.SaveChangesAsync();
                    }
                }
                else
                {
                    var category = new Category();
                    category.Name = newCategory;
                    category.Products.Add(product);


                    product.Categories.Add(category);
                    _db.Categories.Add(category);
                    _db.Products.Update(product);
                    await _db.SaveChangesAsync();
                }
            }
        }

        return RedirectToAction("EditProduct", new { id = productId });
    }

    //Action removes category from product, deleting it completely if the category
    //only has a relation to the product it is being deleted from
    //This is requested through the edit product page
    [HttpPost]
    public async Task<IActionResult> RemoveCategory(int productId, int categoryId)
    {
        var product = await _db.Products.FirstOrDefaultAsync(p => p.Id == productId);
        var category = await _db.Categories.FirstOrDefaultAsync(i => i.Id == categoryId);

        if (product != null && category != null)
        {
            product.Categories.Remove(category);

            var productsWithSameCategoryCount = _db.Products.Count(p =>
                p.Id != productId && p.Categories.Any(c => c.Name == category.Name));

            if (productsWithSameCategoryCount == 0)
            {
                _db.Categories.Remove(category);
            }

            _db.Products.Update(product);
            await _db.SaveChangesAsync();
        }

        return RedirectToAction("EditProduct", new { id = productId });
    }

    //Action deletes a product specified through the actions parameters
    //ensuring all related information to it is also deleted from the database
    //This is requested through the inventory management page
    [HttpPost]
    public async Task<IActionResult> DeleteProduct(int id)
    {
        var product = _db.Products
            .Include(p => p.Categories)
            .Include(p => p.Images)
            .FirstOrDefault(p => p.Id == id);

        if (product == null)
        {
            return NotFound();
        }

        var business = await _db.Businesses.Include(b => b.Product)
            .FirstOrDefaultAsync(b => b.Id == product.BusinessId);
        var images = product.Images;

        if (product.Categories.Count != 0)
        {
            foreach (var category in product.Categories)
            {
                var categoryFromDb = await _db.Categories.FirstOrDefaultAsync(c => c.Id == category.Id);

                if (categoryFromDb.Products.Count == 1)
                {
                    _db.Categories.Remove(category);
                }
                else
                {
                    categoryFromDb.Products.Remove(product);
                    _db.Categories.Update(categoryFromDb);
                }
            }

            await _db.SaveChangesAsync();
        }


        business.Product.Remove(product);

        if (images.Count != 0)
        {
            foreach (var image in images)
            {
                _db.Images.Remove(image);
            }
        }


        _db.Businesses.Update(business);
        await _db.SaveChangesAsync();

        return RedirectToAction("InventoryManagement");
    }

    //Action returns the PendingOrder view with a list of ordered products sold by the business, which is not yet marked as sent
    public async Task<IActionResult> PendingOrder()
    {
        var user = await _userManager.GetUserAsync(User);

        if (user == null || !User.IsInRole("BusinessOwner"))
        {
            return Forbid();
        }

        // Retrieve the pending orders for the business owner so he can see the unsent orders.
        var pendingOrder = await _db.ProductsOrdered
            .Include(po => po.Product)
            .Include(po =>
                po.Order) //filters the result to only include BusinessID of related product matches BusinessID to the user and the sendt property
            .Where(po => po.Product.BusinessId == user.BusinessId && !po.Sendt.HasValue)
            .OrderBy(po => po.Order.DateTime)
            .ToListAsync();


        return View(pendingOrder);
    }

    //Action marks products as sent when a business owner request so from the Pending orders view and saves it in the database
    [HttpPost]
    public async Task<IActionResult> MarkAsSent(List<string> id)
    {
        foreach (var orderId in id)
        {
            //fetches the product order for the spesific order ID
            var productsOrdered = await _db.ProductsOrdered
                .Where(po => po.OrderId == orderId)
                .ToListAsync();

            if (productsOrdered == null || !productsOrdered.Any())
            {
                return NotFound();
            }

            foreach (var productOrdered in productsOrdered)
            {
                productOrdered.Sendt = true; // Marks the product as sent
                _db.ProductsOrdered.Update(productOrdered); // update the product in db
            }
        }

        await _db.SaveChangesAsync();

        return RedirectToAction("PendingOrder");
    }
}