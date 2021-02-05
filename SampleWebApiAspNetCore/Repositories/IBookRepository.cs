using System.Collections.Generic;
using System.Linq;
using SampleWebApiAspNetCore.Entities;
using SampleWebApiAspNetCore.Models;

namespace SampleWebApiAspNetCore.Repositories
{
    public interface IBookRepository
    {
        BookEntity GetSingle(int id);
        void Add(BookEntity item);
        void Delete(int id);
        BookEntity Update(int id, BookEntity item);
        IQueryable<BookEntity> GetAll(QueryParameters queryParameters);

        ICollection<BookEntity> GetRandomBook();
        int Count();

        bool Save();
    }
}
