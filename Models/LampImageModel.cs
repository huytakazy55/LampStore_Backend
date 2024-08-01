using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using LampStoreProjects.Models;

namespace LampStoreProjects.Models
{
    public class LampImageModel
    {
        public int Id { get; set; }

        [Required]
        public string? ImagePath { get; set; }

        public int LampModelId { get; set; }

        public LampModel? LampModel { get; set; }
    }
}