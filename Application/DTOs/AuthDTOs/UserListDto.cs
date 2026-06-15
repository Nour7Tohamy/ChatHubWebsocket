using System;
using System.Collections.Generic;
using System.Text;

namespace Application.DTOs.AuthDTOs
{
    public class UserListDto
    {
        public string Id { get; set; } = "";
        public string UserName { get; set; } = "";
        public string Email { get; set; } = "";
        public string DisplayName { get; set; } = "";
        public bool IsOnline { get; set; }
    }
}
