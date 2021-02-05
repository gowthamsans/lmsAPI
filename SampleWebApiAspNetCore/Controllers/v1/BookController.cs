using System;
using System.Linq;
using AutoMapper;
using SampleWebApiAspNetCore.Dtos;
using Microsoft.AspNetCore.Mvc;
using SampleWebApiAspNetCore.Repositories;
using System.Collections.Generic;
using SampleWebApiAspNetCore.Entities;
using SampleWebApiAspNetCore.Models;
using SampleWebApiAspNetCore.Helpers;
using System.Text.Json;

namespace SampleWebApiAspNetCore.v1.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    //[Route("api/[controller]")]
    public class BookController : ControllerBase
    {
        private readonly IBookRepository _bookRepository;
        private readonly IUrlHelper _urlHelper;
        private readonly IMapper _mapper;

        public BookController(
            IUrlHelper urlHelper,
            IBookRepository bookRepository,
            IMapper mapper)
        {
            _bookRepository = bookRepository;
            _mapper = mapper;
            _urlHelper = urlHelper;
        }

        [HttpGet(Name = nameof(GetAllBooks))]
        public ActionResult GetAllBooks(ApiVersion version, [FromQuery] QueryParameters queryParameters)
        {
            List<BookEntity> bookItems = _bookRepository.GetAll(queryParameters).ToList();

            var allItemCount = _bookRepository.Count();

            var paginationMetadata = new
            {
                totalCount = allItemCount,
                pageSize = queryParameters.PageCount,
                currentPage = queryParameters.Page,
                totalPages = queryParameters.GetTotalPages(allItemCount)
            };

            Response.Headers.Add("X-Pagination", JsonSerializer.Serialize(paginationMetadata));

            var links = CreateLinksForCollection(queryParameters, allItemCount, version);

            var toReturn = bookItems.Select(x => ExpandSingleBookItem(x, version));

            return Ok(new
            {
                value = toReturn,
                links = links
            });
        }

        [HttpGet]
        [Route("{id:int}", Name = nameof(GetSingleBook))]
        public ActionResult GetSingleBook(ApiVersion version, int id)
        {
            BookEntity bookItem = _bookRepository.GetSingle(id);

            if (bookItem == null)
            {
                return NotFound();
            }

            return Ok(ExpandSingleBookItem(bookItem, version));
        }

        [HttpPost(Name = nameof(AddBook))]
        public ActionResult<BookDto> AddBook(ApiVersion version, [FromBody] BookCreateDto bookCreateDto)
        {
            if (bookCreateDto == null)
            {
                return BadRequest();
            }

            BookEntity toAdd = _mapper.Map<BookEntity>(bookCreateDto);

            _bookRepository.Add(toAdd);

            if (!_bookRepository.Save())
            {
                throw new Exception("Creating a bookitem failed on save.");
            }

            BookEntity newBookItem = _bookRepository.GetSingle(toAdd.Id);

            return CreatedAtRoute(nameof(GetSingleBook),
                new { version = version.ToString(), id = newBookItem.Id },
                _mapper.Map<BookDto>(newBookItem));
        }

        [HttpDelete]
        [Route("{id:int}", Name = nameof(RemoveBook))]
        public ActionResult RemoveBook(int id)
        {
            BookEntity bookItem = _bookRepository.GetSingle(id);

            if (bookItem == null)
            {
                return NotFound();
            }

            _bookRepository.Delete(id);

            if (!_bookRepository.Save())
            {
                throw new Exception("Deleting a bookitem failed on save.");
            }

            return NoContent();
        }

        [HttpPut]
        [Route("{id:int}", Name = nameof(UpdateBook))]
        public ActionResult<BookDto> UpdateBook(int id, [FromBody] BookUpdateDto bookUpdateDto)
        {
            if (bookUpdateDto == null)
            {
                return BadRequest();
            }

            var existingBookItem = _bookRepository.GetSingle(id);

            if (existingBookItem == null)
            {
                return NotFound();
            }

            _mapper.Map(bookUpdateDto, existingBookItem);

            _bookRepository.Update(id, existingBookItem);

            if (!_bookRepository.Save())
            {
                throw new Exception("Updating a bookitem failed on save.");
            }

            return Ok(_mapper.Map<BookDto>(existingBookItem));
        }

        [HttpGet("GetRandomBook", Name = nameof(GetRandomBook))]
        public ActionResult GetRandomBook()
        {
            ICollection<BookEntity> bookItems = _bookRepository.GetRandomBook();

            IEnumerable<BookDto> dtos = bookItems
                .Select(x => _mapper.Map<BookDto>(x));

            var links = new List<LinkDto>();

            // self 
            links.Add(new LinkDto(_urlHelper.Link(nameof(GetRandomBook), null), "self", "GET"));

            return Ok(new
            {
                value = dtos,
                links = links
            });
        }

        private List<LinkDto> CreateLinksForCollection(QueryParameters queryParameters, int totalCount, ApiVersion version)
        {
            var links = new List<LinkDto>();

            // self 
            links.Add(new LinkDto(_urlHelper.Link(nameof(GetAllBooks), new
            {
                pagecount = queryParameters.PageCount,
                page = queryParameters.Page,
                orderby = queryParameters.OrderBy
            }), "self", "GET"));

            links.Add(new LinkDto(_urlHelper.Link(nameof(GetAllBooks), new
            {
                pagecount = queryParameters.PageCount,
                page = 1,
                orderby = queryParameters.OrderBy
            }), "first", "GET"));

            links.Add(new LinkDto(_urlHelper.Link(nameof(GetAllBooks), new
            {
                pagecount = queryParameters.PageCount,
                page = queryParameters.GetTotalPages(totalCount),
                orderby = queryParameters.OrderBy
            }), "last", "GET"));

            if (queryParameters.HasNext(totalCount))
            {
                links.Add(new LinkDto(_urlHelper.Link(nameof(GetAllBooks), new
                {
                    pagecount = queryParameters.PageCount,
                    page = queryParameters.Page + 1,
                    orderby = queryParameters.OrderBy
                }), "next", "GET"));
            }

            if (queryParameters.HasPrevious())
            {
                links.Add(new LinkDto(_urlHelper.Link(nameof(GetAllBooks), new
                {
                    pagecount = queryParameters.PageCount,
                    page = queryParameters.Page - 1,
                    orderby = queryParameters.OrderBy
                }), "previous", "GET"));
            }

            var posturl = _urlHelper.Link(nameof(AddBook), new { version = version.ToString() });

            links.Add(
               new LinkDto(posturl,
               "create_book",
               "POST"));

            return links;
        }

        private dynamic ExpandSingleBookItem(BookEntity bookItem, ApiVersion version)
        {
            var links = GetLinks(bookItem.Id, version);
            BookDto item = _mapper.Map<BookDto>(bookItem);

            var resourceToReturn = item.ToDynamic() as IDictionary<string, object>;
            resourceToReturn.Add("links", links);

            return resourceToReturn;
        }

        private IEnumerable<LinkDto> GetLinks(int id, ApiVersion version)
        {
            var links = new List<LinkDto>();

            var getLink = _urlHelper.Link(nameof(GetSingleBook), new { version = version.ToString(), id = id });

            links.Add(
              new LinkDto(getLink, "self", "GET"));

            var deleteLink = _urlHelper.Link(nameof(RemoveBook), new { version = version.ToString(), id = id });

            links.Add(
              new LinkDto(deleteLink,
              "delete_book",
              "DELETE"));

            var createLink = _urlHelper.Link(nameof(AddBook), new { version = version.ToString() });

            links.Add(
              new LinkDto(createLink,
              "create_book",
              "POST"));

            var updateLink = _urlHelper.Link(nameof(UpdateBook), new { version = version.ToString(), id = id });

            links.Add(
               new LinkDto(updateLink,
               "update_book",
               "PUT"));

            return links;
        }
    }
}
