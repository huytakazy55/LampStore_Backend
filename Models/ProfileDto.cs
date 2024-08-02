using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using LampStoreProjects.Models;

namespace LampStoreProjects.Models
{
    public class ProfileDto
    {
        public string? UserName { get; set; }
        public string? Email { get; set; }
    }
}