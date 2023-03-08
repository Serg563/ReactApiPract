using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReactApiPract.Data;
using ReactApiPract.Models;
using ReactApiPract.Models.DTO;
using ReactApiPract.Services;
using ReactApiPract.Utility;
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
        public async Task<ActionResult<ApiResponse>> GetOrders(string? userId)
        {
            try
            {
                var orderHeaders = _context.OrderHeaders.Include(q => q.OrderDetails)
                    .ThenInclude(q => q.MenuItem).OrderByDescending(q => q.OrderHeaderId);
                if (!string.IsNullOrEmpty(userId))
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

        [HttpPost]
        public async Task<ActionResult<ApiResponse>> CreateOrder([FromBody] OrderHeaderCreateDTO createHeader)
        {
            try
            {
                OrderHeader order = new OrderHeader()
                {
                    ApplicationUserId = createHeader.ApplicationUserId,
                    PickupEmail = createHeader.PickupEmail,
                    PickupName = createHeader.PickupName,
                    PickupPhoneNumber = createHeader.PickupPhoneNumber,
                    OrderTotal = createHeader.OrderTotal,
                    OrderDate = DateTime.Now,
                    StripePaymentIntentID = createHeader.StripePaymentIntentID,
                    TotalItems = createHeader.TotalItems,
                    Status = string.IsNullOrEmpty(createHeader.Status) ? SD.status_pending : createHeader.Status,
                };
                if (ModelState.IsValid)
                {
                    _context.OrderHeaders.Add(order);
                    _context.SaveChanges();
                    foreach (var orderDetailDTO in createHeader.OrderDetailsDTO)
                    {
                        OrderDetails orderDetails = new OrderDetails()
                        {
                            OrderHeaderId = order.OrderHeaderId,
                            ItemName = orderDetailDTO.ItemName,
                            MenuItemId = orderDetailDTO.MenuItemId,
                            Price = orderDetailDTO.Price,
                            Quantity = orderDetailDTO.Quantity
                        };
                        _context.OrderDetails.Add(orderDetails);
                    }
                    _context.SaveChanges();
                    _response.Result = order;
                    order.OrderDetails = null;
                    _response.StatusCode = HttpStatusCode.Created;
                    return Ok(_response);

                }

            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.ErrorMessages = new List<string> { ex.ToString() };
            }
            return _response;
        }
        [HttpPut]
        public async Task<ActionResult<ApiResponse>> UpdateOrder(int id,[FromBody] OrderHeaderUpdateDTO updateHeader)
        {
            try
            {
                if(updateHeader == null || id != updateHeader.OrderHeaderId)
                {
                    _response.IsSuccess = false;
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    return BadRequest();
                }
                OrderHeader orderHeader = _context.OrderHeaders
                    .FirstOrDefault(q => q.OrderHeaderId == id);
                if(orderHeader == null)
                {
                    _response.IsSuccess = false;
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    return BadRequest();
                }
                if (!string.IsNullOrEmpty(updateHeader.PickupName))
                {
                    orderHeader.PickupName = updateHeader.PickupName;
                }
                if (!string.IsNullOrEmpty(updateHeader.PickupPhoneNumber))
                {
                    orderHeader.PickupPhoneNumber = updateHeader.PickupPhoneNumber;
                }
                if (!string.IsNullOrEmpty(updateHeader.PickupEmail))
                {
                    orderHeader.PickupEmail = updateHeader.PickupEmail;
                }
                if (!string.IsNullOrEmpty(updateHeader.Status))
                {
                    orderHeader.Status = updateHeader.Status;
                }
                if (!string.IsNullOrEmpty(updateHeader.StripePaymentIntentID))
                {
                    orderHeader.StripePaymentIntentID = updateHeader.StripePaymentIntentID;
                }
                _context.SaveChanges();
                _response.IsSuccess = true;
                _response.StatusCode = HttpStatusCode.NoContent;
                return Ok(_response);

            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.ErrorMessages = new List<string> { ex.ToString() };
            }
            return _response;
        }
    }
}
