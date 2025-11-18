using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LoginandRegisterMVC.Models;

public class Blog
{
    [Key]
    public int BlogId { get; set; }
    
    [Required(ErrorMessage = "Title is required")]
    [MaxLength(200, ErrorMessage = "Title cannot exceed 200 characters")]
    [Display(Name = "Blog Title")]
    public string Title { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Short description is required")]
    [MaxLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
    [Display(Name = "Short Description")]
    public string ShortDescription { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Content is required")]
    [Display(Name = "Blog Content")]
    public string Content { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Featured image is required")]
    [Display(Name = "Featured Image")]
    public string FeaturedImage { get; set; } = string.Empty;
    
    [MaxLength(160)]
    [Display(Name = "SEO Meta Description")]
    public string? MetaDescription { get; set; }
    
    [Required]
    public BlogStatus Status { get; set; } = BlogStatus.Draft;
    
    public DateTime CreatedDate { get; set; } = DateTime.Now;
    public DateTime LastModifiedDate { get; set; } = DateTime.Now;
    public DateTime? PublishedDate { get; set; }
    
    public int ViewCount { get; set; } = 0;
    
    // Foreign Keys
    [Required]
    public string AuthorId { get; set; } = string.Empty;
    
    [ForeignKey("AuthorId")]
    public User? Author { get; set; }
    
    // Not mapped property for file upload
    [NotMapped]
    [Display(Name = "Upload Image")]
    public IFormFile? ImageFile { get; set; }
}

public enum BlogStatus
{
    Draft,
    Published,
    Archived
}

