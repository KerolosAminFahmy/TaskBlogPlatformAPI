using System.Text.Json.Serialization;

namespace Task_Blog_Platform.Model
{
    public class Comment
    {
        public int Id { get; set; }
        public string Content { get; set; }

        public string UserId { get; set; }

        public ApplicationUser User { get; set; }

        public int BlogId { get; set; }
        [JsonIgnore]
        public Blog Blog { get; set; }

    }
}
