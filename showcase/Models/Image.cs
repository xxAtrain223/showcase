﻿using System.ComponentModel.DataAnnotations;


namespace showcase.Models
{
    public class Image
    {
        public int Id { get; set; }

        public string Name { get; set; }
        
        public string Path { get; set; }

        public string AltText { get; set; }
    }
}
