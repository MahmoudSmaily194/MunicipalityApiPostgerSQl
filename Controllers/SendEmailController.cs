using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SawirahMunicipalityWeb.Models;
using SawirahMunicipalityWeb.Services.SendEmailServices;

namespace SawirahMunicipalityWeb.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SendEmailController : ControllerBase
    {
        private readonly ISendEmailService _sendEmailService;

        public SendEmailController(ISendEmailService sendEmailService)
        {
            _sendEmailService = sendEmailService;
        }

        [HttpPost]
        public async Task<IActionResult> SendEmail(SendEmailDto request)
        {
            await _sendEmailService.SendEmailAscync(request);
            return Ok(new { message = "Email sent successfully" });
        }
    }
}
