using SawirahMunicipalityWeb.Models;

namespace SawirahMunicipalityWeb.Services.SendEmailServices
{
    public interface ISendEmailService
    {
        public Task SendEmailAscync(SendEmailDto sendEmailDto);
    }
}
