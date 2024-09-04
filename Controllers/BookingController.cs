using FernandoTan_Project.Data;
using FernandoTan_Project.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;
using OfficeOpenXml;
using OfficeOpenXml.Style;

namespace FernandoTan_Project.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class BookingController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public BookingController(ApplicationDbContext context)
        {
            _context = context;
        }

        [Authorize(Roles = UserRoles.Admin)]
        [HttpGet]
        public IActionResult GetAllBookings(string sortBy = "BookingDateFrom", bool ascending = true, string filterBy = null, string filterValue = null)
        {
            var bookings = _context.Bookings.AsQueryable();

            // Apply filtering
            if (!string.IsNullOrEmpty(filterBy) && !string.IsNullOrEmpty(filterValue))
            {
                switch (filterBy.ToLower())
                {
                    case "facilitydescription":
                        bookings = bookings.Where(b => b.FacilityDescription.Contains(filterValue));
                        break;
                    case "bookingby":
                        bookings = bookings.Where(b => b.BookingBy.Contains(filterValue));
                        break;
                    case "bookingstatus":
                        bookings = bookings.Where(b => b.BookingStatus.Contains(filterValue));
                        break;
                        // Add more filter cases as needed
                }
            }

            // Apply sorting
            switch (sortBy.ToLower())
            {
                case "facilitydescription":
                    bookings = ascending ? bookings.OrderBy(b => b.FacilityDescription) : bookings.OrderByDescending(b => b.FacilityDescription);
                    break;
                case "bookingdatefrom":
                    bookings = ascending ? bookings.OrderBy(b => b.BookingDateFrom) : bookings.OrderByDescending(b => b.BookingDateFrom);
                    break;
                case "bookingdateto":
                    bookings = ascending ? bookings.OrderBy(b => b.BookingDateTo) : bookings.OrderByDescending(b => b.BookingDateTo);
                    break;
                case "bookingby":
                    bookings = ascending ? bookings.OrderBy(b => b.BookingBy) : bookings.OrderByDescending(b => b.BookingBy);
                    break;
                case "bookingstatus":
                    bookings = ascending ? bookings.OrderBy(b => b.BookingStatus) : bookings.OrderByDescending(b => b.BookingStatus);
                    break;
                    // Add more sort cases as needed
            }

            return Ok(bookings.ToList());
        }

        [HttpGet("{id}")]
        public IActionResult GetById(int? id)
        {
            var booking = _context.Bookings.FirstOrDefault(x => x.BookingId == id);
            if (booking == null)
                return Problem(detail: "Booking with id " + id + " is not found.", statusCode: 404);

            return Ok(booking);
        }
        [HttpPost]
        public IActionResult Post(Booking booking)
        {
            _context.Bookings.Add(booking);
            _context.SaveChanges();

            return CreatedAtAction("GetAll", new { id = booking.BookingId }, booking);
        }
        [HttpPut("{id}")]
        public IActionResult Put(int id, [FromBody] Booking booking)
        {
            var entity = _context.Bookings.FirstOrDefault(x => x.BookingId == id);
            if (entity == null)
                return NotFound($"Booking with id {id} not found."); // Return 404 if the booking isn't found

            // Update the existing booking with the new data
            entity.FacilityDescription = booking.FacilityDescription;
            entity.BookingDateFrom = booking.BookingDateFrom;
            entity.BookingDateTo = booking.BookingDateTo;
            entity.BookingBy = booking.BookingBy;
            entity.BookingStatus = booking.BookingStatus;

            _context.SaveChanges(); // Save changes to the database

            return Ok(entity); // Return the updated booking as a response
        }
        [HttpDelete]
        public IActionResult Delete([FromBody] List<int> ids)
        {
            var bookingsToDelete = _context.Bookings.Where(x => ids.Contains(x.BookingId)).ToList();

            if (bookingsToDelete.Count == 0)
                return Problem(detail: "No bookings found for the provided IDs.", statusCode: 404);

            _context.Bookings.RemoveRange(bookingsToDelete);
            _context.SaveChanges();

            // Reset the identity column after deletion
            ResetBookingIdentity();

            return Ok(new { message = $"{bookingsToDelete.Count} bookings deleted." });
        }

        private void ResetBookingIdentity()
        {
            // Execute SQL command to reset the identity column
            _context.Database.ExecuteSqlRaw("DBCC CHECKIDENT ('Bookings', RESEED, 0)");
        }
        
        [HttpGet("history/{user}")]
        public IActionResult GetBookingHistory(string user)
        {
            var bookings = _context.Bookings
                .Where(b => b.BookingBy == user)
                .OrderByDescending(b => b.BookingDateFrom)
                .ToList();

            return Ok(bookings);
        }

    }
}
