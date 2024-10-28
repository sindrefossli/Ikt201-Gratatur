using System.ComponentModel;
using Microsoft.Build.Framework;
using NorMall.Models;

namespace NorMall.Models;

public class HomePageConfig
{
    public int Id { get; set; }
    
    [Required]
    public bool Active { get; set; }
    
    [Required]
    [DisplayName("Configuration name")]
    public string Name { get; set; }

    [Required]
    public List<Image> Images { get; set; } = new List<Image>();

    
    public int? BusinessId { get; set; }
    public Business? Business { get; set; }
}