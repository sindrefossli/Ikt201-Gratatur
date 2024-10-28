using Microsoft.AspNetCore.Identity;
using NorMall.Models;
using NuGet.Packaging;

namespace NorMall.Data;

public class ApplicationDbInitializer
{
    public static void Initialize(ApplicationDbContext db, UserManager<ApplicationUser> um,
        RoleManager<IdentityRole> rm)
    {
        db.Database.EnsureDeleted();
        db.Database.EnsureCreated();

        var businessRole = new IdentityRole("BusinessOwner");
        rm.CreateAsync(businessRole).Wait();
        var customerRole = new IdentityRole("Customer");
        rm.CreateAsync(customerRole).Wait();

        var businessOwner = new ApplicationUser
            { UserName = "owner@example.com", Name = "Business Owner", EmailConfirmed = true };
        um.CreateAsync(businessOwner, "Password1.").Wait();
        um.AddToRoleAsync(businessOwner, "BusinessOwner").Wait();

        var customer = new ApplicationUser
            { UserName = "customer@example.com", Name = "Customer One", EmailConfirmed = true };
        um.CreateAsync(customer, "Password2.").Wait();
        um.AddToRoleAsync(customer, "Customer").Wait();

        var business = new Business
        {
            Name = "Farming",
        };
        db.Businesses.Add(business);

        businessOwner.Business = business;
        um.UpdateAsync(businessOwner).Wait();

        db.SaveChanges();

        string imagePath1 = "InitImages/Farming.png";
        string imagePath2 = "wwwroot/Images/defaultAdvertisement.jpg";
        string imagePath3 = "InitImages/Cooking.png";
        string imagePath4 = "wwwroot/Images/defaultAdvertisement.jpg";
        string imagePath5 = "InitImages/Ultra.png";
        string imagePath6 = "wwwroot/Images/norMallLogo.png";

        var pathList = new List<string>();

        pathList.Add(imagePath1);
        pathList.Add(imagePath2);
        pathList.Add(imagePath3);
        pathList.Add(imagePath4);
        pathList.Add(imagePath5);
        pathList.Add(imagePath6);

        byte[] imageData;

        var imageList = new List<Image>();

        var count = 1;

        foreach (var path in pathList)
        {
            using (FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                imageData = new byte[fileStream.Length];
                fileStream.Read(imageData, 0, (int)fileStream.Length);
            }

            if (count == 2 || count == 4)
            {
                var imageForList = new Image
                {
                    ImageData = imageData,
                    ImageMimeType = "image/jpg",
                };
                imageList.Add(imageForList);
            }
            else
            {
                var imageForList = new Image
                {
                    ImageData = imageData,
                    ImageMimeType = "image/png",
                };
                imageList.Add(imageForList);
            }

            count++;
        }

        var homePageConfig1 = new HomePageConfig()
        {
            Active = true,
            Name = "FirstConfig",
            Business = business,
            Images = new List<Image>()
        };

        var homePageConfig2 = new HomePageConfig()
        {
            Name = "SecondConfig",
            Business = business,
            Images = new List<Image>()
        };

        var homePageConfig3 = new HomePageConfig()
        {
            Name = "ThirdConfig",
            Business = business,
            Images = new List<Image>()
        };

        db.HomePageConfigs.AddRange(homePageConfig1, homePageConfig2, homePageConfig3);
        db.SaveChanges();

        imageList[0].Business = business;
        imageList[1].Business = business;
        imageList[0].EntityType = "Banner";
        imageList[1].EntityType = "Advert";
        imageList[0].HomePageConfig = homePageConfig1;
        imageList[1].HomePageConfig = homePageConfig1;
        imageList[2].Business = business;
        imageList[3].Business = business;
        imageList[2].EntityType = "Banner";
        imageList[3].EntityType = "Advert";
        imageList[2].HomePageConfig = homePageConfig2;
        imageList[3].HomePageConfig = homePageConfig2;
        imageList[4].Business = business;
        imageList[5].Business = business;
        imageList[4].EntityType = "Banner";
        imageList[5].EntityType = "Advert";
        imageList[4].HomePageConfig = homePageConfig3;
        imageList[5].HomePageConfig = homePageConfig3;

        db.AddRange(imageList[0], imageList[1], imageList[2], imageList[3], imageList[4], imageList[5]);
        db.SaveChanges();
        pathList.Clear();

        // Add products to the business
        var product1 = new Product
        {
            Name = "Iphone",
            Price = 10,
            StockAmount = 100,
            Description = "This is an iphone, cool right?",
            Business = business
        };
        var product2 = new Product
        {
            Name = "Television",
            Price = 20,
            StockAmount = 50,
            Description = "Television for far away viewing.",
            Business = business
        };
        var product3 = new Product
        {
            Name = "Empty product",
            Price = 15,
            StockAmount = 0,
            Description = "This is how a product with no images looks, it also just so happens to be out of stock aswell",
            Business = business
        };
        db.Products.AddRange(product1, product2, product3);
        db.SaveChanges();

        string productImagePath1 = "InitImages/Bilde1.jpg";
        string productImagePath2 = "InitImages/Bilde2.jpg";
        string productImagePath3 = "InitImages/Bilde3.jpg";
        string productImagePath4 = "InitImages/Bilde4.jpg";
        string productImagePath5 = "InitImages/Bilde8.jpg";
        string productImagePath6 = "InitImages/Bilde9.jpg";
        string productImagePath7 = "InitImages/Bilde10.jpg";

        pathList.Add(productImagePath1);
        pathList.Add(productImagePath2);
        pathList.Add(productImagePath3);
        pathList.Add(productImagePath4);
        pathList.Add(productImagePath5);
        pathList.Add(productImagePath6);
        pathList.Add(productImagePath7);

        count = 0;

        var product1ImageList = new List<Image>();
        var product2ImageList = new List<Image>();
        
        foreach (var path in pathList)
        {
            using (FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                imageData = new byte[fileStream.Length];
                fileStream.Read(imageData, 0, (int)fileStream.Length);
            }

            var imageForList = new Image
            {
                ImageData = imageData,
                ImageMimeType = "image/jpg",
            };
            if (count < 4)
            {
                product1ImageList.Add(imageForList);
            }
            else
            {
                product2ImageList.Add(imageForList);
            }
            count++;
        }

        count = 0;

        foreach (var imageFromList in product1ImageList)
        {
            imageFromList.Product = product1;
            imageFromList.EntityType = "Product";
            imageFromList.OrderIndex = count;
            imageFromList.Business = business;
            count++;
        }
        
        count = 0;

        foreach (var imageFromList in product1ImageList)
        {
            imageFromList.Product = product1;
            imageFromList.EntityType = "Product";
            imageFromList.OrderIndex = count;
            imageFromList.Business = business;
            count++;
            
            db.Images.Add(imageFromList);
            db.SaveChanges();
        }
        
        count = 0;

        foreach (var imageFromList in product2ImageList)
        {
            imageFromList.Product = product2;
            imageFromList.EntityType = "Product";
            imageFromList.OrderIndex = count;
            imageFromList.Business = business;
            count++;
            
            db.Images.Add(imageFromList);
            db.SaveChanges();
        }

        var categoryList = new List<Category>();

        var category1 = new Category()
        {
            Name = "phone",
            Products = new List<Product>()
        };
        
        var category2 = new Category()
        {
            Name = "tv",
            Products = new List<Product>()
        };
        
        var category3 = new Category()
        {
            Name = "electronics",
            Products = new List<Product>()
        };
        
        var category4 = new Category()
        {
            Name = "chicken",
            Products = new List<Product>()
        };
        
        var category5 = new Category()
        {
            Name = "fish",
            Products = new List<Product>()
        };
        var category6 = new Category()
        {
            Name = "mammal",
            Products = new List<Product>()
        };
        var category7 = new Category()
        {
            Name = "tractor",
            Products = new List<Product>()
        };
        var category8 = new Category()
        {
            Name = "sea",
            Products = new List<Product>()
        };
        var category9 = new Category()
        {
            Name = "ocean",
            Products = new List<Product>()
        };
        
        
        db.Categories.AddRange(category1, category2, category3, category4, category5, category6, category7, category8, category9);
        db.SaveChanges();
        
        categoryList.Add(category1);
        categoryList.Add(category2);
        categoryList.Add(category3);
        categoryList.Add(category4);
        categoryList.Add(category5);
        categoryList.Add(category6);
        categoryList.Add(category7);
        categoryList.Add(category8);
        categoryList.Add(category9);

        var shoppingCart = new ShoppingCart
        {
            ApplicationUserId = customer.Id
        };
        db.ShoppingCarts.Add(shoppingCart);
        db.SaveChanges();

        var productInCart = new ProductInCart
        {
            ProductId = product1.Id,
            BusinessId = product1.BusinessId,
            Quantity = 3,
            ShoppingCartId = shoppingCart.Id
        };
        db.ProductsInCart.Add(productInCart);

        shoppingCart.ProductInCart.Add(productInCart);

        productInCart = new ProductInCart
        {
            ProductId = product2.Id,
            BusinessId = product2.BusinessId,
            Quantity = 5,
            ShoppingCartId = shoppingCart.Id
        };
        db.ProductsInCart.Add(productInCart);

        shoppingCart.ProductInCart.Add(productInCart);
        db.ShoppingCarts.Update(shoppingCart);

        var businessOwner1 = new ApplicationUser
            { UserName = "owner1@example.com", Name = "Business Owner1", EmailConfirmed = true };
        var businessOwner2 = new ApplicationUser
            { UserName = "owner2@example.com", Name = "Business Owner2", EmailConfirmed = true };
        var businessOwner3 = new ApplicationUser
            { UserName = "owner3@example.com", Name = "Business Owner3", EmailConfirmed = true };
        var businessOwner4 = new ApplicationUser
            { UserName = "owner4@example.com", Name = "Business Owner1", EmailConfirmed = true };
        var businessOwner5 = new ApplicationUser
            { UserName = "owner5@example.com", Name = "Business Owner2", EmailConfirmed = true };
        var businessOwner6 = new ApplicationUser
            { UserName = "owner6@example.com", Name = "Business Owner3", EmailConfirmed = true };
        var businessOwner7 = new ApplicationUser
            { UserName = "owner7@example.com", Name = "Business Owner1", EmailConfirmed = true };
        var businessOwner8 = new ApplicationUser
            { UserName = "owner8@example.com", Name = "Business Owner2", EmailConfirmed = true };
        var businessOwner9 = new ApplicationUser
            { UserName = "owner9@example.com", Name = "Business Owner3", EmailConfirmed = true };
        var businessOwner10 = new ApplicationUser
            { UserName = "owner10@example.com", Name = "Business Owner1", EmailConfirmed = true };
        var businessOwner11 = new ApplicationUser
            { UserName = "owner11@example.com", Name = "Business Owner2", EmailConfirmed = true };
        var businessOwner12 = new ApplicationUser
            { UserName = "owner12@example.com", Name = "Business Owner3", EmailConfirmed = true };
        var businessOwner13 = new ApplicationUser
            { UserName = "owner13@example.com", Name = "Business Owner1", EmailConfirmed = true };
        var businessOwner14 = new ApplicationUser
            { UserName = "owner14@example.com", Name = "Business Owner2", EmailConfirmed = true };
        var businessOwner15 = new ApplicationUser
            { UserName = "owner15@example.com", Name = "Business Owner3", EmailConfirmed = true };
        var businessOwner16 = new ApplicationUser
            { UserName = "owner16@example.com", Name = "Business Owner1", EmailConfirmed = true };
        var businessOwner17 = new ApplicationUser
            { UserName = "owner17@example.com", Name = "Business Owner2", EmailConfirmed = true };
        var businessOwner18 = new ApplicationUser
            { UserName = "owner18@example.com", Name = "Business Owner3", EmailConfirmed = true };
        var businessOwner19 = new ApplicationUser
            { UserName = "owner19@example.com", Name = "Business Owner1", EmailConfirmed = true };

        List<ApplicationUser> businessOwners = new List<ApplicationUser>();

        businessOwners.Add(businessOwner1);
        businessOwners.Add(businessOwner2);
        businessOwners.Add(businessOwner3);
        businessOwners.Add(businessOwner4);
        businessOwners.Add(businessOwner5);
        businessOwners.Add(businessOwner6);
        businessOwners.Add(businessOwner7);
        businessOwners.Add(businessOwner8);
        businessOwners.Add(businessOwner9);
        businessOwners.Add(businessOwner10);
        businessOwners.Add(businessOwner11);
        businessOwners.Add(businessOwner12);
        businessOwners.Add(businessOwner13);
        businessOwners.Add(businessOwner14);
        businessOwners.Add(businessOwner15);
        businessOwners.Add(businessOwner16);
        businessOwners.Add(businessOwner17);
        businessOwners.Add(businessOwner18);
        businessOwners.Add(businessOwner19);

        foreach (var owner in businessOwners)
        {
            um.CreateAsync(owner, "Password1.").Wait();
            um.AddToRoleAsync(owner, "BusinessOwner").Wait();
        }

        var business1 = new Business { Name = "NotFarming" };
        var business2 = new Business { Name = "Incredible" };
        var business3 = new Business { Name = "Tech Solutions" };
        var business4 = new Business { Name = "Solstice" };
        var business5 = new Business { Name = "Ultra" };
        var business6 = new Business { Name = "Miscellaneous Shop" };
        var business7 = new Business { Name = "Everything" };
        var business8 = new Business { Name = "Future tech" };
        var business9 = new Business { Name = "Absolute" };
        var business10 = new Business { Name = "Solutions" };
        var business11 = new Business { Name = "Never Before" };
        var business12 = new Business { Name = "This thing" };
        var business13 = new Business { Name = "Goodness" };
        var business14 = new Business { Name = "Cooking" };
        var business15 = new Business { Name = "Homes" };
        var business16 = new Business { Name = "Appliances" };
        var business17 = new Business { Name = "Food" };
        var business18 = new Business { Name = "Friendly" };
        var business19 = new Business { Name = "Freedom" };

        List<Business> businesses = new List<Business>();

        businesses.Add(business1);
        businesses.Add(business2);
        businesses.Add(business3);
        businesses.Add(business4);
        businesses.Add(business5);
        businesses.Add(business6);
        businesses.Add(business7);
        businesses.Add(business8);
        businesses.Add(business9);
        businesses.Add(business10);
        businesses.Add(business11);
        businesses.Add(business12);
        businesses.Add(business13);
        businesses.Add(business14);
        businesses.Add(business15);
        businesses.Add(business16);
        businesses.Add(business17);
        businesses.Add(business18);
        businesses.Add(business19);

        var index = 0;

        foreach (var businessFromList in businesses)
        {
            db.Businesses.Add(businessFromList);

            businessOwners[index].Business = businessFromList;
            um.UpdateAsync(businessOwners[index]).Wait();

            db.SaveChanges();
            index++;
        }
        
        var product4 = new Product
        {
            Name = "Cheap Chicken",
            Price = 10,
            StockAmount = 101,
            Description = "Wow, an even cheaper chicken",
            Business = business1
        };
        
        var product5 = new Product
        {
            Name = "Tractor",
            Price = 10,
            StockAmount = 100,
            Description = "Incredible tractor",
            Business = business1
        };
        
        var product6 = new Product
        {
            Name = "Fish",
            Price = 10,
            StockAmount = 100,
            Description = "This creature lives in the sea",
            Business = business1
        };
        
        var product7 = new Product
        {
            Name = "Expensive Chicken",
            Price = 100,
            StockAmount = 100,
            Description = "Wow, a chicken",
            Business = business2
        };

        string newProductsPath1 = "InitImages/Chicken.png";
        string newProductsPath2 = "InitImages/JohnDeere.png";
        string newProductsPath3 = "InitImages/Fish.png";
        
        using (FileStream fileStream = new FileStream(newProductsPath1, FileMode.Open, FileAccess.Read))
        {
            imageData = new byte[fileStream.Length];
            fileStream.Read(imageData, 0, (int)fileStream.Length);
        }

        var chickenImage = new Image
        {
            ImageData = imageData,
            ImageMimeType = "image/png",
        };

       
        
        chickenImage.Product = product4;
        chickenImage.EntityType = "Product";
        chickenImage.Business = business1;
        chickenImage.OrderIndex = 0;
        
        db.Images.Add(chickenImage);
        db.SaveChanges();
        
        var secondChickenImage = new Image()
        {
            Product = product7,
            ImageData = chickenImage.ImageData,
            ImageMimeType = "image/png",
            EntityType = "Product",
            Business = business2,
            OrderIndex = 0
        };
        
        db.Images.Add(secondChickenImage);
        
        using (FileStream fileStream = new FileStream(newProductsPath2, FileMode.Open, FileAccess.Read))
        {
            imageData = new byte[fileStream.Length];
            fileStream.Read(imageData, 0, (int)fileStream.Length);
        }

        var newProductImage = new Image
        {
            ImageData = imageData,
            ImageMimeType = "image/png",
        };

        newProductImage.Product = product5;
        newProductImage.EntityType = "Product";
        newProductImage.Business = business1;
        newProductImage.OrderIndex = 0;

        db.Images.Add(newProductImage);
        
        using (FileStream fileStream = new FileStream(newProductsPath3, FileMode.Open, FileAccess.Read))
        {
            imageData = new byte[fileStream.Length];
            fileStream.Read(imageData, 0, (int)fileStream.Length);
        }

        newProductImage = new Image
        {
            ImageData = imageData,
            ImageMimeType = "image/png",
        };
        
        newProductImage.Product = product6;
        newProductImage.EntityType = "Product";
        newProductImage.Business = business1;
        newProductImage.OrderIndex = 0;
        
        db.Images.Add(newProductImage);
        
        
        category1.Products.Add(product1);
        category2.Products.Add(product2);
        category3.Products.Add(product1);
        category3.Products.Add(product2);
        category4.Products.Add(product4);
        category4.Products.Add(product7);
        category5.Products.Add(product6);
        category6.Products.Add(product4);
        category7.Products.Add(product5);
        category8.Products.Add(product6);
        category9.Products.Add(product6);

        db.Update(category1);
        db.Update(category2);
        db.Update(category3);
        db.Update(category4);
        db.Update(category5);
        db.Update(category6);
        db.Update(category7);
        db.Update(category8);
        db.Update(category9);
        

        db.SaveChanges();
    }
}