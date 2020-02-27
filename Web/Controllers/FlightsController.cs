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

            ICollection<FlightsViewModel> items = await _context.Flights.Skip((model.Pager.CurrentPage - 1) * PageSize).Take(PageSize).Select(f => new FlightsViewModel()
            {
                IsCancelled = f.IsCanceled,
                LocationFrom = f.LocationFrom,
                LocationTo = f.LocationTo,
                TakeOffTime = f.TakeOffTime,
                LandingTime = f.LandingTime,
                Type = f.Type,
                UniqueNumber = f.UniqueNumber,
                PilotName = f.PilotName,
                PassagerCapacity = f.PassagerCapacity,
                BussinesClassCapacity = f.BussinesClassCapacity
            }).ToListAsync();

            model.Items = items;
            model.Pager.PagesCount = (int)Math.Ceiling(await _context.Flights.CountAsync() / (double)PageSize);

            return View(model);
        }

        [AllowAnonymous]
        public async Task<IActionResult> Details(int? id)
        {
            Flight flight = _context.Flights.Find(id);

            foreach (var item in _context.Reservations)
            {
                Flight fl = _context.Flights.FindAsync(item.FlightId).Result;
                fl.Reservations.Add(item);
            }

            await _context.SaveChangesAsync();

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

            List<Passager> passagers = new List<Passager>();

            foreach (Reservation reservation in _context.Reservations.Where(r => r.FlightId == flight.UniqueNumber))
            {
                foreach (Passager passager in _context.Passagers.Where(p => p.ReservationId == reservation.Id))
                {
                    passagers.Add(passager);
                }
            }

            List<PassagersViewModel> items = passagers.Skip((model.Pager.CurrentPage - 1) * PageSize).Take(PageSize).Select(r => new PassagersViewModel()
            {
                MiddleName = r.MiddleName,
                FirstName = r.FirstName,
                LastName = r.LastName,
                Nationality = r.Nationality,
                PhoneNumber = r.PhoneNumber,
                Type = r.Type
            }).ToList();

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
            if (flight.IsCanceled)
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
