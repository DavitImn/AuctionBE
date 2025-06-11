﻿namespace AuctionService.Models.Requests
{
    public class RegisterRequestModel
    {
        public string UserName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public IFormFile UserImage { get; set; } // ✅ File in same model

    }
}
