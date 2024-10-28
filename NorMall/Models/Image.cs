using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using NorMall.Models;

namespace NorMall.Models;

public class Image
{
    public int Id { get; set; }
    
    [Required]
    [NotMapped]
    [Display(Name = "Image File")]
    public IFormFile ImageFile { get; set; }
    
    [Display(Name = "ImageData")]
    public byte[]? ImageData { get; set; }
    
    [Display(Name = "ImageMimeType")]
    public string? ImageMimeType { get; set; }
    
    public string? EntityType { get; set; } //To be set in the controller. The entity is either "HomePageConfig(Advert/Banner)", "Product", or "Logo"
                                           // based on where the image was uploaded, while creating/updating a HomePageConfig, a Product, or a logo
    public int? OrderIndex { get; set; }
    public int? ProductId { get; set; }
    public Product? Product { get; set; }
    
    public int? HomePageConfigId { get; set; }
    public HomePageConfig? HomePageConfig { get; set; }
    
    public int? BusinessId { get; set; }
    public Business? Business { get; set; }
}