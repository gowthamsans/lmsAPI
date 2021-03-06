﻿using System;

namespace SampleWebApiAspNetCore.Dtos
{
    public class BookDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Author { get; set; }
        public string AvailableOrRented { get; set; }
        public DateTime Created { get; set; }
    }
}
