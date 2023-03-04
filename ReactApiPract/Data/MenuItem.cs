using System.ComponentModel.DataAnnotations;

namespace ReactApiPract.Data
{
    public class MenuItem
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        public string Description { get; set; }
        public string Image { get; set; }
        [Range(1,int.MaxValue)]
        public double Price { get; set; }
        public string Category { get; set; }
        [Required]
        public string SpecialTag { get; set; }
    }
}
