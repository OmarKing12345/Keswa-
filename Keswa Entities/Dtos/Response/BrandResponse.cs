using System.ComponentModel.DataAnnotations;

namespace Keswa_Entities.Dtos.Response
{
    public class BrandResponse
    {
        public int Id { get; set; }
        [Required]
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool Status { get; set; }

    }
}
