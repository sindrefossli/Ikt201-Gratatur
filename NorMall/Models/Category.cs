using System.ComponentModel.DataAnnotations;

namespace NorMall.Models;

public class Category
{
    public int Id { get; set; }

    [MaxLength(20)]
    public string Name { get; set; }

    public List<Product>? Products { get; set; } = new List<Product>();
}