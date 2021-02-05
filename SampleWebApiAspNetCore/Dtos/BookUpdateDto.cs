using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SampleWebApiAspNetCore.Dtos
{
    public class BookUpdateDto
    {
        public string Name { get; set; }
        public string AvailableOrRented { get; set; }
        public string Author { get; set; }
        public DateTime Created { get; set; }
    }
}
