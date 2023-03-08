using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReactApiPract.Data;
using ReactApiPract.Models;
using ReactApiPract.Services;
using System.Net;

namespace ReactApiPract.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly AppDbContext _context;
        private ApiResponse _response;
        public OrderController(AppDbContext context, ApiResponse response)
        { 
            _context = context;
            _response = response;
        }
        [HttpGet]
        public async Task<ActionResult<ApiResponse>> GetOrders(string userId)
        {
            try
            {
                var orderHeaders = _context.OrderHeaders.Include(q => q.OrderDetails)
                    .ThenInclude(q => q.MenuItem).OrderByDescending(q => q.OrderHeaderId);
                if(!string.IsNullOrEmpty(userId))
                {
                    _response.Result = orderHeaders.Where(q => q.ApplicationUserId == userId);
                }
                else
                {
                    _response.Result = orderHeaders;
                }
                _response.StatusCode = HttpStatusCode.OK;
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.ErrorMessages = new List<string> { ex.Message };
            }
            return _response;
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<ApiResponse>> GetOrder(int id)
        {
            try
            {
                if (id == 0)
                {
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    return BadRequest(_response);
                }
               
                var orderHeaders = _context.OrderHeaders.Include(q => q.OrderDetails)
                    .ThenInclude(q => q.MenuItem).Where(q => q.OrderHeaderId == id);
                if (orderHeaders == null)
                {
                    _response.StatusCode = HttpStatusCode.NotFound;
                    return NotFound(_response);
                }
                _response.Result = orderHeaders;    
                _response.StatusCode = HttpStatusCode.OK;
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.ErrorMessages = new List<string> { ex.Message };
            }
            return _response;
        }
    }
}
