using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System.Net;
using System.Reflection.Metadata;
using System.Security.Claims;
using Task_Blog_Platform.Data;
using Task_Blog_Platform.Model;
using Task_Blog_Platform.Model.DTO;
using static System.Reflection.Metadata.BlobBuilder;

namespace Task_Blog_Platform.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BlogController(ApplicationDbContext _db) : ControllerBase
    {
        APIResponse _APIResponse = new APIResponse();


        [HttpPost(nameof(CreateBlog))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public IActionResult CreateBlog(BlogDTO model)
        {
            if (string.IsNullOrEmpty(User.FindFirstValue(ClaimTypes.NameIdentifier)))
            {
                Log.Fatal("User try create blog without login at time {res}",DateTime.Now.ToShortDateString());
                _APIResponse.ErrorMessages.Add("You should login first please");
                _APIResponse.IsSuccess = false;
                _APIResponse.StatusCode = HttpStatusCode.Unauthorized;
                return Unauthorized(_APIResponse);
            }
            if (!ModelState.IsValid) {
                
                _APIResponse.StatusCode= HttpStatusCode.BadRequest;
                _APIResponse.IsSuccess= false;
                _APIResponse.Result= model;
                return BadRequest(_APIResponse);


            }
            var NewBlog= new Blog() { 
                CreatedDate = DateTime.Now,
                Title = model.Title,
                Content = model.Content,
                AuthorId= User.FindFirstValue(ClaimTypes.NameIdentifier)
            };
            _db.Blogs.Add(NewBlog);
            _db.SaveChanges();
            _APIResponse.IsSuccess = true;
            _APIResponse.StatusCode = HttpStatusCode.OK;
            _APIResponse.Result = model;
            return Ok(_APIResponse);
        }
        [HttpPost(nameof(AddComment))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public IActionResult AddComment(CommentDTO model)
        {
            if (string.IsNullOrEmpty(User.FindFirstValue(ClaimTypes.NameIdentifier)))
            {
                _APIResponse.ErrorMessages.Add("You should login first please");
                _APIResponse.IsSuccess = false;
                _APIResponse.StatusCode = HttpStatusCode.Unauthorized;
                return Unauthorized(_APIResponse);
            }
            if (!ModelState.IsValid)
            {

                _APIResponse.StatusCode = HttpStatusCode.BadRequest;
                _APIResponse.IsSuccess = false;
                _APIResponse.Result = model;
                return BadRequest(_APIResponse);


            }

            var NewComment = new Comment() {
                Content = model.Content,
                BlogId=model.BlogId,
                UserId= User.FindFirstValue(ClaimTypes.NameIdentifier),
            };
            _db.Comments.Add(NewComment);
            _db.SaveChanges();
            _APIResponse.IsSuccess = true;
            _APIResponse.StatusCode= HttpStatusCode.OK;
            _APIResponse.Result = NewComment;
            return Ok(_APIResponse);
        }
        [HttpGet(nameof(GetAllBlog))]
       
        [ProducesResponseType(StatusCodes.Status200OK)]

        public IActionResult GetAllBlog(string ?Title, string? AuthorName)
        {
            var query = _db.Blogs
         .Include(m => m.Comments)
         .ThenInclude(m => m.User)
         .Include(m => m.Author)
         .OrderBy(m => m.CreatedDate)
         .AsQueryable();

            if (!string.IsNullOrEmpty(Title))
            {
                query = query.Where(m => m.Title.Contains(Title));
            }

            if (!string.IsNullOrEmpty(AuthorName))
            {
                query = query.Where(m => m.Author.UserName.Contains(AuthorName));
            }

            var blogs = query.ToList();
            _APIResponse.IsSuccess = true;
            _APIResponse.StatusCode = HttpStatusCode.OK;
            _APIResponse.Result = blogs;
            return Ok(_APIResponse);
        }

            [HttpGet(nameof(GetBlog))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public IActionResult GetBlog(int Id)
        {
            if (string.IsNullOrEmpty(User.FindFirstValue(ClaimTypes.NameIdentifier)))
            {
                _APIResponse.ErrorMessages.Add("You should login first please");
                _APIResponse.IsSuccess = false;
                _APIResponse.StatusCode = HttpStatusCode.Unauthorized;
                return Unauthorized(_APIResponse);
            }
            var blog=_db.Blogs.Include(m=>m.Comments).ThenInclude(m=>m.User).Include(m=>m.Author).FirstOrDefault(m=>m.Id==Id);
            if (blog == null)
            {
                _APIResponse.ErrorMessages.Add("Blog Not Found");
                _APIResponse.StatusCode = HttpStatusCode.BadRequest;
                _APIResponse.IsSuccess = false;
                return BadRequest(_APIResponse);
            }
            _APIResponse.IsSuccess = true;
            _APIResponse.StatusCode = HttpStatusCode.OK;
            _APIResponse.Result = blog;
            return Ok(_APIResponse);

        }
        [HttpDelete(nameof(DeleteBlog))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public IActionResult DeleteBlog(int Id)
        {

            if (string.IsNullOrEmpty(User.FindFirstValue(ClaimTypes.NameIdentifier)))
            {
                _APIResponse.ErrorMessages.Add("You should login first please");
                _APIResponse.IsSuccess = false;
                _APIResponse.StatusCode = HttpStatusCode.Unauthorized;
                return Unauthorized(_APIResponse);
            }

            var blog = _db.Blogs.Include(m => m.Comments).ThenInclude(m => m.User).Include(m => m.Author).FirstOrDefault(m => m.Id == Id);
            
            if (blog == null)
            {
                _APIResponse.ErrorMessages.Add("Blog Not Found");
                _APIResponse.StatusCode = HttpStatusCode.BadRequest;
                _APIResponse.IsSuccess = false;
                return BadRequest(_APIResponse);
            }
            if(blog.AuthorId!= User.FindFirstValue(ClaimTypes.NameIdentifier))
            {
                _APIResponse.ErrorMessages.Add("Doesnot have access to this resources");
                _APIResponse.StatusCode = HttpStatusCode.Forbidden;
                _APIResponse.IsSuccess = false;
                return StatusCode(403, _APIResponse);
            }
            _db.Comments.RemoveRange(blog.Comments);
            _db.Blogs.Remove(blog);
            _db.SaveChanges();  
            _APIResponse.IsSuccess = true;
            _APIResponse.StatusCode = HttpStatusCode.OK;

            return Ok(_APIResponse);

        }
        [HttpPut(nameof(EditBlog))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public IActionResult EditBlog(BlogEditDTO model)
        {
            if (string.IsNullOrEmpty(User.FindFirstValue(ClaimTypes.NameIdentifier)))
            {
                _APIResponse.ErrorMessages.Add("You should login first please");
                _APIResponse.IsSuccess = false;
                _APIResponse.StatusCode = HttpStatusCode.Unauthorized;
                return Unauthorized(_APIResponse);
            }
            var OldBlog = _db.Blogs.SingleOrDefault(m=>m.Id==model.Id);
            if (OldBlog == null)
            {
                _APIResponse.ErrorMessages.Add("Blog Not Found");
                _APIResponse.StatusCode = HttpStatusCode.BadRequest;
                _APIResponse.IsSuccess = false;
                return BadRequest(_APIResponse);
            }
            if (OldBlog.AuthorId != User.FindFirstValue(ClaimTypes.NameIdentifier))
            {
                _APIResponse.ErrorMessages.Add("Doesnot have access to this resources");
                _APIResponse.StatusCode = HttpStatusCode.Forbidden;
                _APIResponse.IsSuccess = false;
                return StatusCode(403, _APIResponse);
            }
            OldBlog.Title = model.Title;
            OldBlog.Content = model.Content;
            _db.SaveChanges();
            _APIResponse.IsSuccess = true;
            _APIResponse.StatusCode = HttpStatusCode.OK;
            _APIResponse.Result = model;
            return Ok(_APIResponse);
        }
    }
}
