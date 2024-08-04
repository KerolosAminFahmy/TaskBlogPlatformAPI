namespace Task_Blog_Platform.Model.DTO
{
    public class UserAccountDTO
    {
        public UserDto OwnInfo { get; set; } = new UserDto();
        public List<Comment> comments { get; set; } = new List<Comment>();
        public List<Blog> blogs { get; set; } = new List<Blog>();
        public List<UserDto> Followers { get; set; } = new List<UserDto>();
        public List<UserDto> Following { get; set; } = new List<UserDto>(); 
    }
}
