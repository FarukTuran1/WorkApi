﻿// Models/Dtos/UserDto.cs
namespace WorkApi.Models.Dtos
{
    public class UserDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        // PasswordHash yok!
        // Orders veya AdminLogs listeleri yok!
    }
}