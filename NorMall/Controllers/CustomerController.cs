using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NorMall.Data;
using NorMall.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace NorMall.Controllers
{
    /*
     * This controller controls all actions related to a customer
     * Only users with the customer role is authorized to perform any of these actions
     */
    [Authorize(Roles = "Customer")]
    public class CustomerController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;


        public CustomerController(ApplicationDbContext db, UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager)
        {
            _db = db;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        //Action returns a users shopping cart with the products inside it
        public async Task<IActionResult> ShoppingCart()
        {
            var userId = _userManager.GetUserId(User); // Get the current user's ID

            var shoppingCart = await _db.ShoppingCarts
                .FirstOrDefaultAsync(sc => sc.ApplicationUserId == userId);

            var productsInCart = _db.ProductsInCart.Where(pic => pic.ShoppingCartId == shoppingCart.Id)
                .Include(pic => pic.Product).ToList();

            return View(productsInCart);
        }
        
        //Action returns account details page with a users related information and order history through a partial view
        public async Task<IActionResult> AccountDetails()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        //Action changes the quantity value for a productInCart for a users shopping cart
        //This is requested through the shopping cart page
        [HttpPost]
        public async Task<IActionResult> ChangeQuantity(string productInCartId, int newQuantity)
        {
            if (!ModelState.IsValid)
            {
                return NotFound();
            }

            var productInCart = await _db.ProductsInCart.FirstOrDefaultAsync(pic => pic.Id == productInCartId);

            if (productInCart != null)
            {
                if (newQuantity < 1)
                {
                    newQuantity = 1;
                }
                productInCart.Quantity = newQuantity;

                _db.ProductsInCart.Update(productInCart);
                await _db.SaveChangesAsync();
            }
            
            return RedirectToAction("ShoppingCart");
        }

        //Action returns the payment form page which is a placeholder for users to input card information to buy the products
        //This is requested through the shopping cart page
        [HttpGet]
        public async Task<IActionResult> PaymentForm()
        {
            var user = await _userManager.GetUserAsync(User);
            var cart = _db.ShoppingCarts.FirstOrDefault(c => c.ApplicationUserId == user.Id);
            var productsInCart = _db.ProductsInCart.Where(pic => pic.ShoppingCartId == cart.Id);

            if (!productsInCart.Any())
            {
                return Forbid();
            }
            
            return View();
        }

        //Action returns the order confirmation page, which gives a user the confirmation that their order has been received
        //The order is sent through the actions parameter, and used to find the list of products to display to the users
        //This is requested through the payment form page
        [HttpGet]
        public async Task<IActionResult> OrderConfirmation(string orderId)
        {
            var order = await _db.Orders.FirstOrDefaultAsync((o => o.Id == orderId));
            return View(order);
        }

        //Action adds a product to the users cart, the products id is passed through as the actions parameter
        //This is requested through a products page and stored in the database as a productInCart
        [HttpPost]
        public async Task<IActionResult> AddToCart(int productId)
        {
            var user = await _userManager.GetUserAsync(User);
            var product = await _db.Products.FirstOrDefaultAsync(p => p.Id == productId);
            var shoppingCart = await _db.ShoppingCarts.FirstOrDefaultAsync(sc => sc.ApplicationUserId == user.Id);
            var productsInCart = _db.ProductsInCart.Where(pic => pic.ShoppingCartId == shoppingCart.Id).ToList();

            if (shoppingCart != null)
            {
                if (productsInCart.Count > 0)
                {
                    if (!productsInCart.Any(pic => pic.ProductId == productId))
                    {
                        var newProductInCart = new ProductInCart()
                        {
                            Quantity = 1,
                            ProductId = productId,
                            BusinessId = product.BusinessId,
                            ShoppingCartId = shoppingCart.Id
                        };

                        _db.ProductsInCart.Add(newProductInCart);
                        _db.ShoppingCarts.Update(shoppingCart);
                        await _db.SaveChangesAsync();
                    }
                }
                else
                {
                    var newProductInCart = new ProductInCart()
                    {
                        Quantity = 1,
                        ProductId = productId,
                        BusinessId = product.BusinessId,
                        ShoppingCartId = shoppingCart.Id
                    };

                    _db.ProductsInCart.Add(newProductInCart);
                    _db.ShoppingCarts.Update(shoppingCart);
                    await _db.SaveChangesAsync();
                }
            }


            return RedirectToAction("Product", "Home", new { id = productId });
        }

        //Action removes an item from a users shopping cart based on the value passed through the actions parameter
        //This is requested from the shopping cart page
        [HttpPost]
        public async Task<IActionResult> RemoveFromCart(string productInCartId)
        {
            var productInCart = _db.ProductsInCart.FirstOrDefault(pic => pic.Id == productInCartId);
            if (productInCart != null)
            {
                var shoppingCart = _db.ShoppingCarts.FirstOrDefault(sc => sc.Id == productInCart.ShoppingCartId);
                shoppingCart.ProductInCart.Remove(productInCart);
                _db.ProductsInCart.Remove(productInCart);

                _db.ShoppingCarts.Update(shoppingCart);
                await _db.SaveChangesAsync();
            }

            return RedirectToAction("ShoppingCart");
        }

        //Action creates an order and saves it and its related products ordered based on the users current shopping cart
        //The action requires a value for the users address so that the order can know its destination address
        //The products will be cleared from the users shopping cart, and the information saved in the database
        //This is requested from the payment form page when the user confirms their order
        [HttpPost]
        public async Task<IActionResult> ConfirmOrder(string address)
        {
            var user = await _userManager.GetUserAsync(User);

            var shoppingCart = await _db.ShoppingCarts
                .FirstOrDefaultAsync(sc => sc.ApplicationUserId == user.Id);

            if (shoppingCart != null)
            {
                var productsInCart = _db.ProductsInCart.Where(pic => pic.ShoppingCartId == shoppingCart.Id)
                    .Include(pic => pic.Product).ToList();

                var total = 0;

                if (productsInCart.Count > 0)
                {
                    var newOrder = new Order()
                    {
                        ApplicationUserId = user.Id,
                        OrderInfo = "Some generic info",
                        ShippingAddress = address,
                        DateTime = DateTime.Now
                    };

                    _db.Orders.Add(newOrder);
                    await _db.SaveChangesAsync();

                    var productList = new List<ProductOrdered>();

                    foreach (var productInCart in productsInCart)
                    {
                        var productOrdered = new ProductOrdered()
                        {
                            Quantity = productInCart.Quantity,
                            BusinessId = productInCart.BusinessId,
                            ProductId = productInCart.ProductId,
                            OrderId = newOrder.Id
                        };

                        var product = await _db.Products.FirstOrDefaultAsync(p => p.Id == productInCart.ProductId);

                        var newStock = product.StockAmount - productInCart.Quantity;

                        if (newStock == null || newStock < 0)
                        {
                            //Add the already bought items back in the cart
                            foreach (var productDeletedFromCart in productList)
                            {
                                var newOldProductInCart = new ProductInCart()
                                {
                                    Quantity = productDeletedFromCart.Quantity,
                                    BusinessId = productDeletedFromCart.BusinessId,
                                    ProductId = productDeletedFromCart.ProductId,
                                    ShoppingCartId = shoppingCart.Id
                                };

                                _db.ProductsInCart.Add(newOldProductInCart);
                                _db.ProductsOrdered.Remove(productDeletedFromCart);
                                await _db.SaveChangesAsync();

                            }

                            _db.Orders.Remove(newOrder);
                            await _db.SaveChangesAsync();
                            return RedirectToAction("OutOfStockView", "Customer");
                        }

                        total = (total + ((product.Price * productInCart.Quantity)));
                        
                        product.StockAmount = (product.StockAmount - productInCart.Quantity);

                        _db.Products.Update(product);

                        productList.Add(productOrdered);
                        _db.ProductsOrdered.Add(productOrdered);
                        _db.ProductsInCart.Remove(productInCart);
                        await _db.SaveChangesAsync();
                    }

                    newOrder.ProductOrdered = productList;
                    newOrder.Total = total;
                    shoppingCart.ProductInCart.Clear();
                    
                    _db.Orders.Update(newOrder);
                    _db.ShoppingCarts.Update(shoppingCart);
                    await _db.SaveChangesAsync();
                    return RedirectToAction("OrderConfirmation",
                        new { orderId = newOrder.Id }); //Should return Confirmed Order Page
                }
            }
            
            return RedirectToAction("AccountDetails");
        }

        //Action returns the become business owner page where the user has a simple form requiring the name of the business they want to create
        //It sends in an empty business to the view
        //This is requested from the NorMall front page (Home/Index view)
        [HttpGet]
        public IActionResult BecomeBusinessOwner()
        {
            var business = new Business();
            return View(business);
        }

        //Action retrieves the input name of the business given by the user and stores the new business and its relation to the user
        //It also clears all the customers items from their cart, and removes their cart.
        //This is requested when the user sends in the form in th become business owner page
        [HttpPost]
        public async Task<IActionResult> BecomeBusinessOwner([Bind("Name")] Business business)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return Forbid();
            }
            else
            {

                if (_db.Businesses.Any(b => b.Name == business.Name))
                {
                    return View(business);
                }
                
                var newBusiness = new Business()
                {
                    Name = business.Name
                };

                _db.Businesses.Add(newBusiness);
                await _db.SaveChangesAsync();

                user.BusinessId = newBusiness.Id;
                _userManager.RemoveFromRoleAsync(user, "Customer").Wait();
                _userManager.AddToRoleAsync(user, "BusinessOwner").Wait();

                var usersCart = _db.ShoppingCarts.FirstOrDefault(sc => sc.ApplicationUserId == user.Id);
                if (usersCart != null)
                {
                    var productsInCart = _db.ProductsInCart.Where(pic => pic.ShoppingCartId == usersCart.Id);
                    var cartSize = productsInCart.Count();

                    if (cartSize > 0)
                    {
                        foreach (var productInCart in productsInCart)
                        {
                            _db.ProductsInCart.Remove(productInCart);
                        }
                    }


                    await _db.SaveChangesAsync();
                    _db.ShoppingCarts.Remove(usersCart);
                    await _db.SaveChangesAsync();
                }


                _db.Users.Update(user);
                await _db.SaveChangesAsync();
            }

            await _signInManager.SignOutAsync();
            await _signInManager.SignInAsync(user, isPersistent: false);

            return RedirectToAction("BusinessFrontPage", "Home");
        }

        //Action returns a view similar to Forbid() or NotFound(),
        //as it tells the user something went wrong with their attempt of buying,
        //usually because the user bought too much of a product as it became out of stock
        //This is requested from the confirmOrder action when a product becomes out of stock during the process of creating an order
        [HttpGet]
        public IActionResult OutOfStockView()
        {
            return View();
        }
    }
}