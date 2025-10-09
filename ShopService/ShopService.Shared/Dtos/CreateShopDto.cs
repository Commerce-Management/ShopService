using System.ComponentModel.DataAnnotations;

namespace ShopService.Shared.Dtos;

public record CreateShopDto
(    
    [Required]
    string Name,
    
    [Required]
    string Description, 
    
    [Required]
    string ContactEmail,  
    
    [Required]
    string ContactPhone, 
    
    [Required]
    string Subdomain  
); 