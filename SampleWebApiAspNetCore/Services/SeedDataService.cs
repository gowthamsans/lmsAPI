using SampleWebApiAspNetCore.Entities;
using SampleWebApiAspNetCore.Repositories;
using System;
using System.Threading.Tasks;

namespace SampleWebApiAspNetCore.Services
{
    public class SeedDataService : ISeedDataService
    {
        public async Task Initialize(BookDbContext context)
        {
            context.BookItems.Add(new BookEntity() { AvailableOrRented = "Available", Author = "Singh", Name = "PoliticsofOpportunism", Created = DateTime.Now });
            context.BookItems.Add(new BookEntity() { AvailableOrRented = "Rented", Author = "Atwood", Name = "Testaments", Created = DateTime.Now });
            context.BookItems.Add(new BookEntity() { AvailableOrRented = "Available", Author = "Alharthi", Name = "CelestialBodies", Created = DateTime.Now });
            context.BookItems.Add(new BookEntity() { AvailableOrRented = "Rented", Author = "Mohi", Name = "Chequebook", Created = DateTime.Now });

            await context.SaveChangesAsync();
        }
    }
}
