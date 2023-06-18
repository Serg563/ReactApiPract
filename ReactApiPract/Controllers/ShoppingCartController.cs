using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReactApiPract.Data;
using ReactApiPract.Models;
using System.Net;

namespace ReactApiPract.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ShoppingCartController : ControllerBase
    {
        protected ApiResponse _response;
        private readonly AppDbContext _context;
        public ShoppingCartController(AppDbContext context)
        {
            _response = new();
            _context = context;
        }
        [HttpGet]
        public async Task<ActionResult<ApiResponse>> GetShoppingCart(string userId)
        {
            try
            {
                ShoppingCart shoppingCart;
                if (string.IsNullOrEmpty(userId))
                {
                    shoppingCart = new();
                }
                else
                {
                    shoppingCart = _context.ShoppingCarts.Include(q => q.CarItems)
                        .ThenInclude(q => q.MenuItem)
                        .FirstOrDefault(q=>q.UserId == userId);

                }
                if(shoppingCart.CarItems != null && shoppingCart.CarItems.Count > 0)
                {
                    shoppingCart.CartTotal = shoppingCart.CarItems.Sum(q => q.Quantity * q.MenuItem.Price);
                }
                _response.Result = shoppingCart;
                _response.StatusCode = HttpStatusCode.OK;
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.ErrorMessages
                     = new List<string>() { ex.ToString() };
                _response.StatusCode = HttpStatusCode.BadRequest;
            }
            return _response;
        }


        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse>> AddorUpdateItemInCart(string userId,int menuItemId,int updateQuantityBy)
        {
            ShoppingCart shoppingCart = _context.ShoppingCarts.Include(g=>g.CarItems)
                .FirstOrDefault(q => q.UserId == userId);
            MenuItem menuItem = _context.MenuItems.FirstOrDefault(q => q.Id == menuItemId);
            if (menuItem == null)
            {
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.IsSuccess = false;
                return BadRequest(_response);
            }
            if(shoppingCart == null && updateQuantityBy > 0)
            {
                ShoppingCart newCart = new ShoppingCart() { UserId = userId };
                _context.ShoppingCarts.Add(newCart);
                _context.SaveChanges();

                CartItem newCartItem = new CartItem()
                {
                    MenuItemId = menuItemId, 
                    Quantity = updateQuantityBy,
                    ShoppingCartId = newCart.Id,
                    MenuItem = null
                };
                _context.CartItems.Add(newCartItem);
                _context.SaveChanges();
            }
            else
            {
                //shopping cart exist
                CartItem cartItem = shoppingCart.CarItems
                    .FirstOrDefault(q => q.MenuItemId == menuItemId);
                if (cartItem == null)
                {
                    CartItem newCartItem = new CartItem()
                    {
                        MenuItemId = menuItemId,
                        Quantity = updateQuantityBy,
                        ShoppingCartId = shoppingCart.Id,
                        MenuItem = null
                    };
                    _context.CartItems.Add(newCartItem);
                    _context.SaveChanges();
                }
                else
                {
                    int addQuantity = cartItem.Quantity + updateQuantityBy;
                    if (addQuantity == 0 || addQuantity <= 0)
                    {
                        _context.CartItems.Remove(cartItem);
                        if (shoppingCart.CarItems.Count() == 0)
                        {
                            _context.ShoppingCarts.Remove(shoppingCart);
                        }
                        _context.SaveChanges();
                    }
                    else
                    {
                        cartItem.Quantity = cartItem.Quantity + updateQuantityBy;
                        _context.SaveChanges();
                    }
                }
            }
            return _response;
        }
    }
}
