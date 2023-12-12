﻿using BookShop.Abstract;
using BookShop.Entities;
using BookShop.Models.BookModels;
using BookShop.Models.BookVersionModels;
using BookShop.Models.RequestModels;
using BookShop.Services;
using LinqKit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq.Expressions;

namespace BookShop.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class BookController : ControllerBase
    {
        private readonly IRepository<Book> _bookRepository;
        public BookController(IRepository<Book> bookRepository)
        {
            _bookRepository = bookRepository;
        }


        [HttpPost]
        [Route("[action]")]
        public async Task<IActionResult> GetBooks(BookRequest model)
        {
            try
            {
                //Where.
                Expression<Func<Book, bool>> filter = i => true;

                //Date(Filter).
                if (model.StartDate != null)
                    filter = filter.And(i => i.CreateDate.Date >= model.StartDate.Value.Date);

                if (model.EndDate != null)
                    filter = filter.And(i => i.CreateDate.Date <= model.EndDate.Value.Date);

                //Search.
                if (!string.IsNullOrEmpty(model.Search))
                    filter = filter.And(i => i.Title.Contains(model.Search));

                //Sort.
                Expression<Func<Book, object>> Order = model.Order switch
                {
                    "id" => i => i.Id,
                    "title" => i => i.Title,
                    _ => i => i.Id,
                };

                //OrderBy.
                IOrderedQueryable<Book> orderBy(IQueryable<Book> i)
                   => model.SortDir == "ascend"
                   ? i.OrderBy(Order)
                   : i.OrderByDescending(Order);

                //Select
                static IQueryable<BookRModel> select(IQueryable<Book> query) => query.Select(entity => new BookRModel
                {
                    Id = entity.Id,
                    Title = entity.Title,
                    Description = entity.Description,
                    PublishedDate = entity.PublishedDate,
                    Cover = entity.Cover,
                    CreateDate = entity.CreateDate,
                    //Categories = entity.Categories,
                    BookAuthors = entity.BookAuthors.Select(i => new AuthorInBookModel
                    {
                        AuthorId = i.AuthorId,
                        AuhorRatio = i.AuhorRatio,
                    }).ToList(),

                    BookVersions = entity.BookVersions.Select(i =>
                    new BookVersionRModel
                    {
                        Id = i.Id,
                        BookCount = i.BookCount,
                        Number = i.Number,
                        CostPrice = i.CostPrice,
                        TotalCostPrice = i.CostPrice * i.BookCount,
                        SellPrice = i.SellPrice,
                        TotalSellPrice = i.SellPrice * i.BookCount,
                        LibraryRatio = i.LibraryRatio,
                    }).ToList(),

                });

                var (total, data) = await _bookRepository.GetListAndTotalAsync(select, filter, null, orderBy, skip: model.Skip, take: model.Take);

                return Ok(new { data, total });
            }
            catch (OzelException ex)
            {
                return BadRequest(ex.Errors);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet]
        [Route("[action]/{id}")]
        public async Task<IActionResult> GetBook(int id)
        {
            try
            {
                //Where
                Expression<Func<Book, bool>> filter = i => i.Id == id;

                //Select
                static IQueryable<BookRModel> select(IQueryable<Book> query) => query.Select(entity => new BookRModel
                {
                    Id = entity.Id,
                    Title = entity.Title,
                    Description = entity.Description,
                    PublishedDate = entity.PublishedDate,
                    Cover = entity.Cover,
                    CreateDate = entity.CreateDate,
                    //Categories = entity.Categories,
                    BookAuthors = entity.BookAuthors.Select(i => new AuthorInBookModel
                    {
                        AuthorId = i.AuthorId,
                        AuhorRatio = i.AuhorRatio,
                    }).ToList(),

                    BookVersions = entity.BookVersions.Select(i =>
                    new BookVersionRModel
                    {
                        Id = i.Id,
                        BookCount = i.BookCount,
                        Number = i.Number,
                        CostPrice = i.CostPrice,
                        TotalCostPrice = i.CostPrice * i.BookCount,
                        SellPrice = i.SellPrice,
                        TotalSellPrice = i.SellPrice * i.BookCount,
                        LibraryRatio = i.LibraryRatio,
                    }).ToList(),

                });

                var book = await _bookRepository.FindAsync(select, filter);

                return Ok(book);
            }
            catch (OzelException ex)
            {
                return BadRequest(ex.Errors);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        [Route("[action]")]
        public async Task<IActionResult> CreateBook(BookCModel model)
        {
            try
            {
                var entity = new Book
                {
                    Title = model.Title,
                    Description = model.Description,
                    PublishedDate = model.PublishedDate,
                    Cover = model.Cover,
                    CreateDate = DateTime.Now,
                    //Categories = model.CategoriesId.Select(i => new Category
                    //{
                    //    Id = i
                    //}).ToList(),
                    BookAuthors = model.BookAuthors.Select(i => new BookAuthor
                    {
                        AuthorId = i.AuthorId,
                        AuhorRatio = i.AuhorRatio,
                    }).ToList()
                };

                await _bookRepository.AddAsync(entity);

                return Ok();
            }
            catch (OzelException ex)
            {
                return BadRequest(ex.Errors);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        [Route("[action]")]
        public async Task<IActionResult> UpdateBook(BookUModel model)
        {

            try
            {
                if (model.Id < 0 || model.Id == null)
                    throw new Exception("Reauested Book Not Found!.");

                //Where
                Expression<Func<Book, bool>> filter = i => i.Id == model.Id;

                var entity = await _bookRepository.FindAsync(filter);

                entity!.Title = model.Title;
                entity.Description = model.Description;
                entity.PublishedDate = model.PublishedDate;
                entity.Cover = model.Cover;
                //entity.Categories = model.CategoriesId.Select(i => new Category
                //{
                //    Id = i
                //}).ToList();
                entity.BookAuthors = model.BookAuthors.Select(i => new BookAuthor
                {
                    AuthorId = i.AuthorId,
                    AuhorRatio = i.AuhorRatio,
                }).ToList();

                return Ok();
            }
            catch (OzelException ex)
            {
                return BadRequest(ex.Errors);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete]
        [Route("[action]")]
        public async Task<IActionResult> DeleteBook(int id)
        {
            try
            {
                //Where
                Expression<Func<Book, bool>> filter = i => i.Id == id;

                await _bookRepository.DeleteAsync(filter);

                return Ok();
            }
            catch (OzelException ex)
            {
                return BadRequest(ex.Errors);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
