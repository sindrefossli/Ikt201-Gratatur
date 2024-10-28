using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NorMall.Models;
public class ProductInCart
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public string Id { get; set; }
    public int Quantity { get; set; }
    
    
    public int? BusinessId { get; set; }
    public Business? Business { get; set; }
    public int? ProductId { get; set; }
    public Product? Product { get; set; }
    public string? ShoppingCartId { get; set; }
    public ShoppingCart? ShoppingCart { get; set; }
}