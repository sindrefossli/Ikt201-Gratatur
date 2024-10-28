using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using NorMall.Models;

namespace NorMall.Models;

public class Product
{
    public int Id { get; set; }
    
    [Required]
    public string Name { get; set; }

    [Required] 
    [DisplayName("Price")] 
    public int Price { get; set; }

    [Required]
    [DisplayName("Amount in stock")] 
    public int? StockAmount { get; set; }

    [Required]
    [DisplayName("Description")]
    [MaxLength(400)]
    public string? Description { get; set; }

    [NotMapped]
    public List<ProductOrdered>? ProductsOrdered { get; set; } = new List<ProductOrdered>();
    [NotMapped]
    public List<ProductInCart>? ProductsInCarts { get; set; } = new List<ProductInCart>();
    public List<Image>? Images { get; set; } = new List<Image>();
    public List<Category> Categories { get; set; } = new List<Category>();

    public int? BusinessId { get; set; }
    public Business? Business { get; set; }
}