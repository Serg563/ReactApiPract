using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReactApiPract.Data;
using ReactApiPract.Models;
using ReactApiPract.Services;
using Stripe;
using System.Net;

namespace ReactApiPract.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly AppDbContext _context;
        private ApiResponse _response;
        private readonly IConfiguration config;
        public PaymentController(AppDbContext context, IConfiguration config)
        {
            _context = context;
            _response = new();         
            this.config = config;
        }
        [HttpPost()]
        public async Task<ActionResult<ApiResponse>> MakePayment(string userId)
        {
            ShoppingCart shoppingCart = _context.ShoppingCarts.Include(q => q.CarItems)
                   .ThenInclude(q => q.MenuItem)
                   .FirstOrDefault(q => q.UserId == userId);
            if (shoppingCart == null || shoppingCart.CarItems== null || shoppingCart.CarItems.Count() == 0)
            {
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.IsSuccess = false;
                return BadRequest();
            }
            #region Create Payment Intent

            StripeConfiguration.ApiKey = config["StripeSettings:SecretKey"];
            shoppingCart.CartTotal = shoppingCart.CarItems.Sum(q => q.Quantity * q.MenuItem.Price);
            
            var options = new PaymentIntentCreateOptions
            {
                Amount = (int)(shoppingCart.CartTotal * 100),
                Currency = "usd",
                //AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions
                //{
                //    Enabled = true,
                //},
                PaymentMethodTypes = new List<string>
                {
                    "card",
                },
            };
            var service = new PaymentIntentService();
            PaymentIntent response = service.Create(options);
            shoppingCart.StripePaymentIntentId = response.Id;
            shoppingCart.ClientSecret = response.ClientSecret;
           

            #endregion

            _response.Result = shoppingCart;
            _response.StatusCode = HttpStatusCode.OK;
            _response.IsSuccess = true;
            return Ok(_response);
        }
    }
}
