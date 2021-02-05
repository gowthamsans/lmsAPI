using SampleWebApiAspNetCore.Entities;
using SampleWebApiAspNetCore.Helpers;
using SampleWebApiAspNetCore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;

namespace SampleWebApiAspNetCore.Repositories
{
    public class BookSqlRepository : IBookRepository
    {
        private readonly BookDbContext _bookDbContext;

        public BookSqlRepository(BookDbContext bookDbContext)
        {
            _bookDbContext = bookDbContext;
        }

        public BookEntity GetSingle(int id)
        {
            return _bookDbContext.BookItems.FirstOrDefault(x => x.Id == id);
        }

        public void Add(BookEntity item)
        {
            _bookDbContext.BookItems.Add(item);
        }

        public void Delete(int id)
        {
            BookEntity bookItem = GetSingle(id);
            _bookDbContext.BookItems.Remove(bookItem);
        }

        public BookEntity Update(int id, BookEntity item)
        {
            _bookDbContext.BookItems.Update(item);
            return item;
        }

        public IQueryable<BookEntity> GetAll(QueryParameters queryParameters)
        {
            IQueryable<BookEntity> _allItems = _bookDbContext.BookItems.OrderBy(queryParameters.OrderBy,
              queryParameters.IsDescending());

            if (queryParameters.HasQuery())
            {
                _allItems = _allItems
                    .Where(x => x.AvailableOrRented.ToString().Contains(queryParameters.Query.ToLowerInvariant())
                    || x.Name.ToLowerInvariant().Contains(queryParameters.Query.ToLowerInvariant()));
            }

            return _allItems
                .Skip(queryParameters.PageCount * (queryParameters.Page - 1))
                .Take(queryParameters.PageCount);
        }

        public int Count()
        {
            return _bookDbContext.BookItems.Count();
        }

        public bool Save()
        {
            return (_bookDbContext.SaveChanges() >= 0);
        }

        public ICollection<BookEntity> GetRandomBook()
        {
            List<BookEntity> toReturn = new List<BookEntity>();

            toReturn.Add(GetRandomItem("Available"));
            toReturn.Add(GetRandomItem("Available"));
            toReturn.Add(GetRandomItem("Rented"));

            return toReturn;
        }

        private BookEntity GetRandomItem(string type)
        {
            return _bookDbContext.BookItems
                .Where(x => x.Author == type)
                .OrderBy(o => Guid.NewGuid())
                .FirstOrDefault();
        }
    }
}
