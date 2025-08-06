namespace SawirahMunicipalityWeb.Entities
{
    public class ServicesCategories
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public ICollection<Service> services { get; set; } = new List<Service>();


    }
}
