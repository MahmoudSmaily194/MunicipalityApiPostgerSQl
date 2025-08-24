using System.ComponentModel.DataAnnotations;

namespace SawirahMunicipalityWeb.Entities
{
    public class ServicesCategories
    {
        [Key]
        public Guid Id { get; set; }

        public string Name { get; set; }
        public bool  IsDeleted { get; set; }=false; 
        public ICollection<Service> services { get; set; } = new List<Service>();


    }
}
