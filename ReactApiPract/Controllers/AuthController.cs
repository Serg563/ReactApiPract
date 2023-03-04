using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ReactApiPract.Data;
using ReactApiPract.Models;
using ReactApiPract.Services;

namespace ReactApiPract.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private ApiResponse _response; 
        public AuthController(AppDbContext context, ApiResponse response)
        {
            _context = context;
            _response = response;       
        }
    }
}
