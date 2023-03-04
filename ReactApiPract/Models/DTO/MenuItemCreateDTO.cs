﻿using System.ComponentModel.DataAnnotations;

namespace ReactApiPract.Models.DTO
{
    public class MenuItemCreateDTO
    {
       
        [Required]
        public string Name { get; set; }
        public string Description { get; set; }
        public IFormFile File { get; set; }
        public double Price { get; set; }
        [Range(1, int.MaxValue)]
        public string Category { get; set; }
        [Required]
        public string SpecialTag { get; set; }
    }
}
