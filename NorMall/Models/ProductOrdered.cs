using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace NorMall.Models;


public class ProductOrdered
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public string Id { get; set; }
    
    public int Quantity { get; set; }
    

    public int? BusinessId { get; set; }
    public Business? Business { get; set; }
    public int? ProductId { get; set; }
    public Product? Product { get; set; }
    
    public string? OrderId { get; set; }
    public Order? Order { get; set; }
    
    public bool? Sendt { get; set; }
}