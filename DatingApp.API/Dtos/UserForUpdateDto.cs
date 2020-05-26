namespace DatingApp.API.Dtos
{
    public class UserForUpdateDto
    {
         public string Gender { get; set; }
        public string KnownAs { get; set; }
        public string DateOfBirth { get; set; }
        public string Email { get; set; }
        public bool EmailConfirmed { get; set; }
        public string Introduction { get; set; }
        public string LookingFor { get; set; }
        public string Interests { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
    }
}