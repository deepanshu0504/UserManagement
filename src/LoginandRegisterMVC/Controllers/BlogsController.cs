using LoginandRegisterMVC.Data;
using LoginandRegisterMVC.Models;
using LoginandRegisterMVC.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace LoginandRegisterMVC.Controllers;

public class BlogsController : Controller
{
    private readonly UserContext _context;
    private readonly IFileUploadService _fileUploadService;

    public BlogsController(UserContext context, IFileUploadService fileUploadService)
    {
        _context = context;
        _fileUploadService = fileUploadService;
    }

    // GET: Blogs (Public listing)
    public async Task<IActionResult> Index(int page = 1, int pageSize = 10)
    {
        var totalBlogs = await _context.Blogs
            .Where(b => b.Status == BlogStatus.Published)
            .CountAsync();

        var blogs = await _context.Blogs
            .Include(b => b.Author)
            .Where(b => b.Status == BlogStatus.Published)
            .OrderByDescending(b => b.PublishedDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        ViewBag.CurrentPage = page;
        ViewBag.TotalPages = (int)Math.Ceiling(totalBlogs / (double)pageSize);
        ViewBag.TotalBlogs = totalBlogs;

        return View(blogs);
    }

    // GET: Blogs/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var blog = await _context.Blogs
            .Include(b => b.Author)
            .FirstOrDefaultAsync(m => m.BlogId == id);

        if (blog == null)
        {
            return NotFound();
        }

        // Only show published blogs to non-authors/non-admins
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var isAdmin = User.IsInRole("Admin");
        
        if (blog.Status != BlogStatus.Published 
            && blog.AuthorId != currentUserId 
            && !isAdmin)
        {
            return NotFound();
        }

        // Increment view count (only for published blogs)
        if (blog.Status == BlogStatus.Published)
        {
            blog.ViewCount++;
            await _context.SaveChangesAsync();
        }

        return View(blog);
    }

    // GET: Blogs/Create
    [Authorize]
    public IActionResult Create()
    {
        return View(new Blog());
    }

    // POST: Blogs/Create
    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Blog blog, string action)
    {
        // Remove ImageFile from ModelState as it's handled separately
        ModelState.Remove(nameof(blog.ImageFile));
        ModelState.Remove(nameof(blog.FeaturedImage));
        ModelState.Remove(nameof(blog.AuthorId));

        // Validate image upload
        if (blog.ImageFile == null)
        {
            ModelState.AddModelError(nameof(blog.ImageFile), "Please upload a featured image.");
        }
        else if (!_fileUploadService.ValidateImage(blog.ImageFile, out string errorMessage))
        {
            ModelState.AddModelError(nameof(blog.ImageFile), errorMessage);
        }

        if (!ModelState.IsValid)
        {
            return View(blog);
        }

        try
        {
            // Upload image
            blog.FeaturedImage = await _fileUploadService.UploadImageAsync(
                blog.ImageFile!, 
                "uploads/blogs"
            );

            // Set author
            blog.AuthorId = User.FindFirstValue(ClaimTypes.NameIdentifier) 
                ?? throw new InvalidOperationException("User not authenticated");

            // Set status based on button clicked
            blog.Status = action == "publish" ? BlogStatus.Published : BlogStatus.Draft;
            
            // Set dates
            blog.CreatedDate = DateTime.Now;
            blog.LastModifiedDate = DateTime.Now;
            
            if (blog.Status == BlogStatus.Published)
            {
                blog.PublishedDate = DateTime.Now;
            }

            _context.Blogs.Add(blog);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = blog.Status == BlogStatus.Published 
                ? "Blog published successfully!" 
                : "Blog saved as draft successfully!";

            return RedirectToAction(nameof(MyBlogs));
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", $"Error creating blog: {ex.Message}");
            return View(blog);
        }
    }

    // GET: Blogs/Edit/5
    [Authorize]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var blog = await _context.Blogs.FindAsync(id);
        
        if (blog == null)
        {
            return NotFound();
        }

        // Authorization check
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var isAdmin = User.IsInRole("Admin");
        
        if (blog.AuthorId != currentUserId && !isAdmin)
        {
            return Forbid();
        }

        return View(blog);
    }

    // POST: Blogs/Edit/5
    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Blog blog, string action)
    {
        if (id != blog.BlogId)
        {
            return NotFound();
        }

        // Get existing blog
        var existingBlog = await _context.Blogs.FindAsync(id);
        
        if (existingBlog == null)
        {
            return NotFound();
        }

        // Authorization check
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var isAdmin = User.IsInRole("Admin");
        
        if (existingBlog.AuthorId != currentUserId && !isAdmin)
        {
            return Forbid();
        }

        // Remove unnecessary ModelState entries
        ModelState.Remove(nameof(blog.ImageFile));
        ModelState.Remove(nameof(blog.FeaturedImage));
        ModelState.Remove(nameof(blog.AuthorId));
        ModelState.Remove(nameof(blog.Author));

        // Handle image upload if new image provided
        if (blog.ImageFile != null)
        {
            if (!_fileUploadService.ValidateImage(blog.ImageFile, out string errorMessage))
            {
                ModelState.AddModelError(nameof(blog.ImageFile), errorMessage);
            }
            else
            {
                // Delete old image
                _fileUploadService.DeleteImage(existingBlog.FeaturedImage);
                
                // Upload new image
                existingBlog.FeaturedImage = await _fileUploadService.UploadImageAsync(
                    blog.ImageFile, 
                    "uploads/blogs"
                );
            }
        }

        if (!ModelState.IsValid)
        {
            return View(blog);
        }

        try
        {
            // Update properties
            existingBlog.Title = blog.Title;
            existingBlog.ShortDescription = blog.ShortDescription;
            existingBlog.Content = blog.Content;
            existingBlog.MetaDescription = blog.MetaDescription;
            existingBlog.LastModifiedDate = DateTime.Now;

            // Update status if changed
            if (action == "publish" && existingBlog.Status != BlogStatus.Published)
            {
                existingBlog.Status = BlogStatus.Published;
                if (!existingBlog.PublishedDate.HasValue)
                {
                    existingBlog.PublishedDate = DateTime.Now;
                }
            }
            else if (action == "draft")
            {
                existingBlog.Status = BlogStatus.Draft;
            }

            _context.Update(existingBlog);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Blog updated successfully!";
            return RedirectToAction(nameof(MyBlogs));
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!BlogExists(blog.BlogId))
            {
                return NotFound();
            }
            throw;
        }
    }

    private bool BlogExists(int id)
    {
        return _context.Blogs.Any(e => e.BlogId == id);
    }

    // GET: Blogs/Delete/5
    [Authorize]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var blog = await _context.Blogs
            .Include(b => b.Author)
            .FirstOrDefaultAsync(m => m.BlogId == id);
            
        if (blog == null)
        {
            return NotFound();
        }

        // Authorization check
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var isAdmin = User.IsInRole("Admin");
        
        if (blog.AuthorId != currentUserId && !isAdmin)
        {
            return Forbid();
        }

        return View(blog);
    }

    // POST: Blogs/Delete/5
    [HttpPost, ActionName("Delete")]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var blog = await _context.Blogs.FindAsync(id);
        
        if (blog == null)
        {
            return NotFound();
        }

        // Authorization check
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var isAdmin = User.IsInRole("Admin");
        
        if (blog.AuthorId != currentUserId && !isAdmin)
        {
            return Forbid();
        }

        // Delete associated image file
        _fileUploadService.DeleteImage(blog.FeaturedImage);

        // Remove from database
        _context.Blogs.Remove(blog);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Blog deleted successfully!";
        return RedirectToAction(nameof(MyBlogs));
    }

    // POST: Blogs/ChangeStatus/5
    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangeStatus(int id, BlogStatus newStatus)
    {
        var blog = await _context.Blogs.FindAsync(id);
        
        if (blog == null)
        {
            return NotFound();
        }

        // Authorization check
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var isAdmin = User.IsInRole("Admin");
        
        if (blog.AuthorId != currentUserId && !isAdmin)
        {
            return Forbid();
        }

        blog.Status = newStatus;
        blog.LastModifiedDate = DateTime.Now;
        
        if (newStatus == BlogStatus.Published && !blog.PublishedDate.HasValue)
        {
            blog.PublishedDate = DateTime.Now;
        }

        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = $"Blog status changed to {newStatus} successfully!";
        return RedirectToAction(nameof(MyBlogs));
    }

    // GET: Blogs/MyBlogs
    [Authorize]
    public async Task<IActionResult> MyBlogs(string filter = "all")
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        
        var query = _context.Blogs
            .Where(b => b.AuthorId == currentUserId);

        // Apply filter
        query = filter.ToLower() switch
        {
            "published" => query.Where(b => b.Status == BlogStatus.Published),
            "drafts" => query.Where(b => b.Status == BlogStatus.Draft),
            _ => query
        };

        var blogs = await query
            .OrderByDescending(b => b.LastModifiedDate)
            .ToListAsync();

        ViewBag.CurrentFilter = filter;
        ViewBag.PublishedCount = await _context.Blogs
            .Where(b => b.AuthorId == currentUserId && b.Status == BlogStatus.Published)
            .CountAsync();
        ViewBag.DraftCount = await _context.Blogs
            .Where(b => b.AuthorId == currentUserId && b.Status == BlogStatus.Draft)
            .CountAsync();
        ViewBag.TotalCount = await _context.Blogs
            .Where(b => b.AuthorId == currentUserId)
            .CountAsync();

        return View(blogs);
    }
}

