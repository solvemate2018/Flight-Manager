using Data;
using Data.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Web.Models.Flights;
using Web.Models.Passager;
using Web.Models.Shared;

namespace Web.Controllers
{
    [Authorize]
    public class FlightsController:Controller
    {
        private readonly FlightManagerDbContext _context;
        private int PageSize = 10;

        public FlightsController()
        {
            _context = new FlightManagerDbContext();
        }

        [AllowAnonymous]
        public async Task<IActionResult> Index(FlightsIndexViewModel model)
        {

            model.Pager ??= new PagerViewModel();
            model.Pager.CurrentPage = model.Pager.CurrentPage <= 0 ? 1 : model.Pager.CurrentPage;

            ICollection<FlightsViewModel> items;

            RefreshFlights();

            if (User.IsInRole("Admin") || User.IsInRole("Employee"))
            {
                items = await _context.Flights.Skip((model.Pager.CurrentPage - 1) * PageSize).Take(PageSize).Select(f => new FlightsViewModel()
                {
                    IsCancelled = f.IsCanceled,
                    IsOld = f.IsOld,
                    LocationFrom = f.LocationFrom,
                    LocationTo = f.LocationTo,
                    TakeOffTime = f.TakeOffTime,
                    LandingTime = f.LandingTime,
                    Type = f.Type,
                    UniqueNumber = f.UniqueNumber,
                    PilotName = f.PilotName,
                    PassagerCapacity = f.PassagerCapacity,
                    BussinesClassCapacity = f.BussinesClassCapacity
                }).OrderByDescending(f => f.TakeOffTime).ToListAsync();
            }
            else
            {
                items = await _context.Flights.Where(f => f.IsOld == false).Skip((model.Pager.CurrentPage - 1) * PageSize).Take(PageSize).Select(f => new FlightsViewModel()
                {
                    IsCancelled = f.IsCanceled,
                    IsOld = f.IsOld,
                    LocationFrom = f.LocationFrom,
                    LocationTo = f.LocationTo,
                    TakeOffTime = f.TakeOffTime,
                    LandingTime = f.LandingTime,
                    Type = f.Type,
                    UniqueNumber = f.UniqueNumber,
                    PilotName = f.PilotName,
                    PassagerCapacity = f.PassagerCapacity,
                    BussinesClassCapacity = f.BussinesClassCapacity
                }).OrderByDescending(f => f.TakeOffTime).ToListAsync();
            }

            model.Items = items;
            model.Pager.PagesCount = (int)Math.Ceiling(await _context.Flights.CountAsync() / (double)PageSize);

            return View(model);
        }

        private void RefreshFlights()
        {
            List<Flight> flights = new List<Flight>();
            foreach (var item in _context.Flights)
            {
                flights.Add(item);
            }
            _context.UpdateRange(flights);
            _context.SaveChanges();
        }

        [AllowAnonymous]
        public async Task<IActionResult> Details(int? id)
        {
            Flight flight = _context.Flights.Find(id);
            flight.Reservations = await _context.Reservations.Where(r => r.FlightId == flight.UniqueNumber).ToListAsync();
            List<Passager> passagers = new List<Passager>();

            foreach (var reservation in flight.Reservations)
            {
                reservation.Passagers = await _context.Passagers.Where(p => p.ReservationId == reservation.Id).ToListAsync();
                passagers.AddRange(reservation.Passagers);
            }

            FlightsDetailsViewModel model = new FlightsDetailsViewModel();
            model.UniqueNumber = flight.UniqueNumber;
            model.LocationFrom = flight.LocationFrom;
            model.LocationTo = flight.LocationTo;
            model.TakeOffTime = flight.TakeOffTime;
            model.FlightDuration = flight.LandingTime.Subtract(flight.TakeOffTime);
            model.Type = flight.Type;
            model.PassagerCapacity = flight.PassagerCapacity;
            model.BussinesClassCapacity = flight.BussinesClassCapacity;
            model.Pager ??= new PagerViewModel();
            model.Pager.CurrentPage = model.Pager.CurrentPage <= 0 ? 1 : model.Pager.CurrentPage;

            List<PassagersViewModel> items = passagers.Skip((model.Pager.CurrentPage - 1) * PageSize).Take(PageSize).Select(r => new PassagersViewModel()
            {
                MiddleName = r.MiddleName,
                FirstName = r.FirstName,
                LastName = r.LastName,
                Nationality = r.Nationality,
                PhoneNumber = r.PhoneNumber,
                Type = r.Type
            }).OrderBy(f => f.FirstName).ToList();

            model.Passagers = items;
            model.Pager.PagesCount = (int)Math.Ceiling(passagers.Count() / (double)PageSize);

            return View(model);
        }
        
        [AllowAnonymous]
        public IActionResult CreateReservation(FlightsIndexViewModel model)
        {
            TempData["NumberOfPassagers"] = model.NumberOfPassagers;
            int id = (int)TempData["FlightId"];
            return RedirectToAction("Create", "Reservations", new { id });
        }

        [Authorize(Roles = "Employee, Admin")]
        public IActionResult CreateFlight()
        {
            FlightsCreateViewModel model = new FlightsCreateViewModel();

            return View(model);
        }

        [HttpPost, ActionName("CreateFlight")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles ="Employee, Admin")]
        public async Task<IActionResult> CreateFlight(FlightsCreateViewModel model)
        {
            if (ModelState.IsValid)
            {
                Flight flight = new Flight
                {
                    LocationFrom = model.LocationFrom,
                    LocationTo = model.LocationTo,
                    TakeOffTime = model.TakeOffTime,
                    LandingTime = model.LandingTime,
                    PilotName = model.PilotName,
                    Type = model.Type,
                    MaxBussinesCapacity = model.MaxBussinesCapacity,
                    MaxPassagerCapacity = model.MaxPassagerCapacity,
                };

                _context.Flights.Add(flight);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        public async Task<IActionResult> CancelFlight(int? id)
        {
            Flight flight = await _context.Flights.FindAsync(id);
            flight.IsCanceled = true;

            _context.Flights.Update(flight);
            await _context.SaveChangesAsync();

            TempData["FlightId"] = id;

            return RedirectToAction("CancelReservations", "Reservations", id);
        }

        public async Task<IActionResult> DeleteFlight(int? id)
        {
            Flight flight = await _context.Flights.FindAsync(id);
            if (flight.IsCanceled || flight.IsOld)
            {
                foreach (var reservation in _context.Reservations.Where(r => r.FlightId == flight.UniqueNumber))
                {
                    foreach (var passager in _context.Passagers.Where(p => p.ReservationId == reservation.Id))
                    {
                        _context.Passagers.Remove(passager);
                    }
                    _context.Reservations.Remove(reservation);
                }

                _context.Flights.Remove(flight);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
