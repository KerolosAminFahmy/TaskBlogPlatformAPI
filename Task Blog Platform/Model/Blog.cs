using System.Text.Json.Serialization;

namespace Task_Blog_Platform.Model
{
    public class Blog
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public DateTime CreatedDate { get; set; }
        public string AuthorId {  get; set; }
        public ApplicationUser Author { get; set; }
        
        public ICollection<Comment> Comments { get; set; } = new List<Comment>();   
    }
}
