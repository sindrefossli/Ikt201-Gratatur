using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;
using NorMall.Models;

namespace NorMall.Models;

public class Order
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public string Id { get; set; }
    
    public string? OrderInfo { get; set; }
    
    public DateTime DateTime { get; set; }
    public string ShippingAddress { get; set; }
    public int? Total { get; set; }

    public List<ProductOrdered>? ProductOrdered { get; set; } = new List<ProductOrdered>();
    
    public string? ApplicationUserId { get; set; }
    public ApplicationUser? ApplicationUser { get; set; }
}