using Microsoft.AspNetCore.Identity;
using SawirahMunicipalityWeb.Enums;

namespace SawirahMunicipalityWeb.Entities
{
    public class User:IdentityUser<Guid>
    {
        public string? ProfilePhoto { get; set; }
        public string? FirstName {  get; set; }

        public string? LastName { get; set; }    
       
        public Roles Role {  get; set; } = Roles.User;

        public string FullName => $"{FirstName ?? ""} {LastName ?? ""}".Trim();
        public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
      

    }
}
