using Microsoft.AspNetCore.Identity;

namespace Task_Blog_Platform.Model
{
    public class Follow
    {
        public string FollowerId { get; set; }
        public ApplicationUser Follower { get; set; }

        public string FollowingId { get; set; }
        public ApplicationUser Following { get; set; }
    }
}
