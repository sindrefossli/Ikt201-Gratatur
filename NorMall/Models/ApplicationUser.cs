using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace NorMall.Models;

public class ApplicationUser : IdentityUser
{
    [DisplayName("Full name")]
    [MaxLength(30)]
    public string Name { get; set; }
    
    public int? BusinessId { get; set; }
    public Business? Business { get; set; }
}