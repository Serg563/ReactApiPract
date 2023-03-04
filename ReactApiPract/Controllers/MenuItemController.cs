using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReactApiPract.Data;
using ReactApiPract.Models;
using ReactApiPract.Models.DTO;
using ReactApiPract.Services;
using ReactApiPract.Utility;
using System;
using System.Net;

namespace ReactApiPract.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MenuItemController : ControllerBase
    {
        private readonly AppDbContext _context;
        private ApiResponse _response;
        IBlobService _blobservice;
        public MenuItemController(AppDbContext context,ApiResponse response,IBlobService blobService)
        {
            _context = context;
            _response = response;
            _blobservice = blobService;
        }
        [HttpGet]
        [ProducesResponseType(200)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<ApiResponse>> GetMenuItems() 
        {
            _response.Result = _context.MenuItems;
            _response.StatusCode = HttpStatusCode.OK;
            return Ok(_response);
        }
        [HttpGet("{id:int}",Name ="GetMenuItem")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetMenuItem(int id)
        {
            var res = _context.MenuItems.FirstOrDefault(x => x.Id == id);
            if (res == null) { return NotFound(); }
            return Ok(res);
        }
        [HttpPost]
        public async Task<ActionResult<ApiResponse>> CreateMenuItem([FromForm]MenuItemCreateDTO menuitem)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    if(menuitem.File == null || menuitem.File.Length == 0)
                    {
                        return BadRequest();
                    }
                    string filename = $"{Guid.NewGuid()}{Path.GetExtension(menuitem.File.FileName)}";
                    MenuItem menuitemcreate = new()
                    {
                        Name = menuitem.Name,
                        Description = menuitem.Description,
                        Image = await _blobservice.UploadBlob(filename,SD.SD_Storage_Container,menuitem.File),
                        Price = menuitem.Price,
                        Category = menuitem.Category,
                        SpecialTag = menuitem.SpecialTag
                    };
                    _context.MenuItems.Add(menuitemcreate);
                    _context.SaveChanges();
                    _response.Result = menuitemcreate;
                    _response.StatusCode = HttpStatusCode.Created;
                    return CreatedAtRoute("GetMenuItem", new { id = menuitemcreate.Id }, _response);
                }
                else
                {
                    _response.IsSuccess = false;
                }
            }
            catch(Exception ex)
            {
                _response.IsSuccess = true;
                _response.ErrorMessages = new List<string> { ex.ToString() };
            }
            return _response;
        } 
    }
}
