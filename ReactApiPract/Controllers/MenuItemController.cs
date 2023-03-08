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
        private readonly IWebHostEnvironment _webHostEnvironment;
        public MenuItemController(AppDbContext context,ApiResponse response, IBlobService blobService, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _response = response;
            _blobservice = blobService;
            _webHostEnvironment = webHostEnvironment;
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
            if(id == 0)
            {
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.IsSuccess = false;
                return BadRequest(_response);
            }
            MenuItem menuitem = await _context.MenuItems.FirstOrDefaultAsync(x => x.Id == id);
            if(menuitem == null)
            {
                _response.StatusCode = HttpStatusCode.NotFound;
                _response.IsSuccess = false;
                return NotFound(_response);
            }
            _response.Result = menuitem;
            _response.StatusCode = HttpStatusCode.OK;
            return Ok(_response);
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
                        _response.StatusCode = HttpStatusCode.BadRequest;
                        _response.IsSuccess = false;
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
        [HttpPut("{id:int}", Name = "GetMenuItem")]
        public async Task<ActionResult<ApiResponse>> UpdateMenuItem(int id,[FromForm] MenuItemUpdateDTO menuitemupdate)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    if (menuitemupdate == null || id != menuitemupdate.Id)
                    {
                        _response.StatusCode = HttpStatusCode.BadRequest;
                        _response.IsSuccess = false;
                        return BadRequest();
                    }
                    MenuItem menuItem = await _context.MenuItems.FindAsync(id); 
                    if(menuItem == null)
                    {
                        _response.StatusCode = HttpStatusCode.BadRequest;
                        _response.IsSuccess = false;
                        return BadRequest();
                    }

                    menuItem.Id = menuitemupdate.Id;
                    menuItem.Name = menuitemupdate.Name;
                    menuItem.Price = menuitemupdate.Price;
                    menuItem.Category = menuitemupdate.Category;
                    menuItem.SpecialTag = menuitemupdate.SpecialTag;
                    if(menuitemupdate.File != null && menuitemupdate.File.Length > 0)
                    {
                        string filename = $"{Guid.NewGuid()}{Path.GetExtension(menuitemupdate.File.FileName)}";
                        await _blobservice.DeleteBlob(menuItem.Image.Split('/').Last(), SD.SD_Storage_Container);
                        menuItem.Image = await _blobservice.UploadBlob(filename, SD.SD_Storage_Container, menuitemupdate.File);
                    }

                   
                    _context.MenuItems.Update(menuItem);
                    _context.SaveChanges();
                    _response.StatusCode = HttpStatusCode.NoContent;
                    return Ok(_response);
                }
                else
                {
                    _response.IsSuccess = false;
                }
            }
            catch (Exception ex)
            {
                _response.IsSuccess = true;
                _response.ErrorMessages = new List<string> { ex.ToString() };
            }
            return _response;
        }
        [HttpDelete("{id:int}")]
        public async Task<ActionResult<ApiResponse>> DeleteMenuItem(int id)
        {
            try
            {              
                if(id == 0)
                {
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    _response.IsSuccess = false;
                    return BadRequest();
                }
                MenuItem menuItem = await _context.MenuItems.FindAsync(id);
                if (menuItem == null)
                {
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    _response.IsSuccess = false;
                    return BadRequest();
                }
                await _blobservice.DeleteBlob(menuItem.Image.Split('/').Last(), SD.SD_Storage_Container);
                Thread.Sleep(2000);

                _context.MenuItems.Remove(menuItem);
                _context.SaveChanges();
                _response.StatusCode = HttpStatusCode.NoContent;
                return Ok(_response);            
            }
            catch (Exception ex)
            {
                _response.IsSuccess = true;
                _response.ErrorMessages = new List<string> { ex.ToString() };
            }
            return _response;
        }


        // Create file to local folder

        //[NonAction]
        //public async Task<string> SaveImageMethod(IFormFile imageFile)
        //{
        //    string imageName = new string(Path.GetFileNameWithoutExtension(imageFile.FileName).Take(10).ToArray()).Replace(' ', '-');
        //    imageName = imageName + Path.GetExtension(imageFile.FileName);
        //    var imagePath = Path.Combine(_webHostEnvironment.ContentRootPath, "Images", imageName);
        //    using (var fileStream = new FileStream(imagePath, FileMode.Create))
        //    {
        //        await imageFile.CopyToAsync(fileStream);
        //    }
        //    return imageName;
        //}
        //[HttpPost("SaveImage")]
        //public async Task<IActionResult> SaveImage([FromBody]IFormFile imageFile)
        //{
        //    var result = SaveImage(imageFile);
        //    return Ok(result);
        //}
       
    }
}
