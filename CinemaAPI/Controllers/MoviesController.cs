using CinemaAPI.Data;
using CinemaAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CinemaAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MoviesController : ControllerBase
    {
        private CinemaDbContext _dbContext;
        public MoviesController(CinemaDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [Authorize]
        [HttpGet("[action]")]
        public IActionResult GetAllMovies(string sort, int? pageNumber, int? pageSize)
        {

            var currentPageNumber = pageNumber ?? 1;
            var currenPageSize = pageSize ?? 5;
            var movies = from movie in _dbContext.Movies
                         select new
                         {
                             Id = movie.Id,
                             Name = movie.Name,
                             Duration = movie.Duration,
                             Language = movie.Language,
                             Rating = movie.Rating,
                             Genre = movie.Genre,
                             ImageUrl = movie.ImageUrl
                         };

            switch (sort)
            {
                case "desc":
                    return Ok(movies.Skip((currentPageNumber - 1) * currenPageSize).Take(currenPageSize).OrderByDescending(m => m.Rating));
                case "asc":
                    return Ok(movies.Skip((currentPageNumber - 1) * currenPageSize).Take(currenPageSize).OrderBy(m => m.Rating));
                default:
                    return Ok(movies.Skip((currentPageNumber - 1) * currenPageSize).Take(currenPageSize));

            }
        }
        // api/movies/moviedetail/1
        [Authorize]
        [HttpGet("[action]/{id}")]
        public IActionResult MovieDetail(int id)
        {
            var movie = _dbContext.Movies.Find(id);
            if (movie == null)
            {
                return NotFound();
            }
            return Ok(movie);

        }

        [Authorize]
        [HttpGet("[action]")]
        //api/movies/findmovies?movieName=Mission Impossible Fallout
        public IActionResult FindMovies(string movieName)
        {
            var movies = from movie in _dbContext.Movies
                         where movie.Name.StartsWith(movieName) // Search Functionality - This line will check whether the movie starts with a keyword that we will pass in this method or not
                         select new
                         {
                             Id = movie.Id,
                             Name = movie.Name,
                             ImageUrl = movie.ImageUrl
                         };
            return Ok(movies);
        }


        [Authorize(Roles = "Admin")]
        [HttpPost]
        public IActionResult Post([FromForm]Movie movieObj)
        {
            var guid = Guid.NewGuid();
            var filePath = Path.Combine("wwwroot", guid + ".jpg");
            if (movieObj.Image != null)
            {
                var fileStream = new FileStream(filePath, FileMode.Create);
                movieObj.Image.CopyTo(fileStream);
            }
            movieObj.ImageUrl = filePath.Remove(0, 7);
            _dbContext.Movies.Add(movieObj);
            _dbContext.SaveChanges();
            return StatusCode(StatusCodes.Status201Created); 
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public IActionResult Put(int id, [FromForm]Movie movieObj)
        {
            var movie = _dbContext.Movies.Find(id);
            if(movie == null)
            {
                return NotFound("No record found against this Id");
            }
            else
            {
                var guid = Guid.NewGuid();
                var filePath = Path.Combine("wwwroot", guid + ".jpg");
                if (movieObj.Image != null)
                {
                    var fileStream = new FileStream(filePath, FileMode.Create);
                    movieObj.Image.CopyTo(fileStream);
                    movieObj.ImageUrl = filePath.Remove(0, 7);
                }
                movie.Name = movieObj.Name;
                movie.Description = movieObj.Description;
                movie.Language = movieObj.Language;
                movie.Duration = movieObj.Duration;
                movie.PlayingDate = movieObj.PlayingDate;
                movie.PlayingTime = movieObj.PlayingTime;
                movie.TicketPrice = movieObj.TicketPrice;
                movie.Rating = movieObj.Rating;
                movie.Genre = movieObj.Genre;
                movie.TrailorUrl = movieObj.TrailorUrl;
                _dbContext.SaveChanges();
                return Ok("Record updated successfully");
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var movie = _dbContext.Movies.Find(id);
            if(movie == null)
            {
                return NotFound("No record found against this Id");
            }
            else
            {
                _dbContext.Movies.Remove(movie);
                _dbContext.SaveChanges();
                return Ok("Record Deleted");
            }
        }

    }
}
