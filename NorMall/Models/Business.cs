using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace NorMall.Models;

public class Business
{
    public int Id { get; set; }
    
    [DisplayName("Business Name")] 
    [Required]
    [MaxLength(30)]

    public string Name { get; set; }
    
    public List<HomePageConfig>? HomePageConfig { get; set; } = new List<HomePageConfig>();
    public List<Product>? Product { get; set; } = new List<Product>();
    
}