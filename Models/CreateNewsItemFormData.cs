using SawirahMunicipalityWeb.Enums;

namespace SawirahMunicipalityWeb.Models
{
    public class CreateNewsItemFormData
    {
        public IFormFile? Image { get; set; }
        public string Title { get; set; }
        public string Description { get; set; } = string.Empty;
        public Visibility Visibility { get; set; }
    }
}
