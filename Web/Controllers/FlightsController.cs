using Data;
using Data.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
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

        public FlightsController(IConfiguration configuration)
        {
            _context = new FlightManagerDbContext(configuration);
        }

        //The index page for flights
        [AllowAnonymous]
        public async Task<IActionResult> Index(FlightsIndexViewModel model, string sortOrder, int pageSize)
        {
            model.Pager ??= new PagerViewModel();
            model.Pager.CurrentPage = model.Pager.CurrentPage <= 0 ? 1 : model.Pager.CurrentPage;

            ICollection<FlightsViewModel> items;

            items = await OrderTheFlightsForIndexAsync(sortOrder, model.Pager);

            ResizeThePage(pageSize);

            RefreshFlights();

            model.Order = sortOrder;
            model.PageSize = PageSize;

            model.Items = items;
            model.Pager.PagesCount = (int)Math.Ceiling(await _context.Flights.CountAsync() / (double)PageSize);

            return View(model);
        }

        //The details page for flights
        [AllowAnonymous]
        public async Task<IActionResult> Details(int? id, string sortOrder, int pageSize)
        {

            ResizeThePage(pageSize);


            Flight flight = await FindFlight(id);

            List<Passager> passagers = new List<Passager>();

            foreach (Reservation reservation in flight.Reservations)
            {
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
            model.Order = sortOrder;
            model.PageSize = PageSize;

            List<PassagersViewModel> items = OrderThePassagersForDetails(sortOrder, model.Pager, passagers).ToList();

            model.Passagers = items;
            model.Pager.PagesCount = (int)Math.Ceiling(passagers.Count() / (double)PageSize);

            return View(model);
        }

        //The redirection for creating reservation
        [AllowAnonymous]
        public IActionResult CreateReservation(FlightsIndexViewModel model)
        {
            TempData["NumberOfPassagers"] = model.NumberOfPassagers;
            int id = (int)TempData["FlightId"];
            return RedirectToAction("Create", "Reservations", new { id });
        }

        //The page for creating flights
        [Authorize(Roles = "Employee, Admin")]
        public IActionResult CreateFlight()
        {
            FlightsCreateViewModel model = new FlightsCreateViewModel();

            return View(model);
        }

        //The functionality for creating flight with given data
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

        //The functionality for cancelling flight
        public async Task<IActionResult> CancelFlight(int? id)
        {
            Flight flight = await _context.Flights.FindAsync(id);
            flight.IsCanceled = true;

            _context.Flights.Update(flight);
            await _context.SaveChangesAsync();

            TempData["FlightId"] = id;

            return RedirectToAction("CancelReservations", "Reservations", id);
        }

        //The functionality for deleting flight
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

        //The page for editing flight
        [HttpGet]
        public async Task<IActionResult> EditFlight(int? id)
        {
            Flight flight = await _context.Flights.FindAsync(id);
            FlightsEditViewModel model = new FlightsEditViewModel
            {
                LandingTime = flight.LandingTime,
                TakeOffTime = flight.TakeOffTime,
                LocationFrom = flight.LocationFrom,
                LocationTo = flight.LocationTo,
                PilotName = flight.PilotName,
                Type = flight.Type,
                UniqueNumber = flight.UniqueNumber
            };

            return View(model);
        }

        //The functionality for editing flight with given data
        [HttpPost]
        public async Task<IActionResult> EditFlight(FlightsEditViewModel model)
        {
            if (ModelState.IsValid)
            {
                Flight flight = await _context.Flights.FindAsync(model.UniqueNumber);

                flight.Type = model.Type;
                flight.LocationFrom = model.LocationFrom;
                flight.LocationTo = model.LocationTo;
                flight.TakeOffTime = model.TakeOffTime;
                flight.LandingTime = model.LandingTime;
                flight.PilotName = model.PilotName;

                _context.Flights.Update(flight);

                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        //Returns flight with implemented reservations
        private async Task<Flight> FindFlight(int? id)
        {
            Flight flight = _context.Flights.Find(id);
            flight.Reservations = await _context.Reservations.Where(r => r.FlightId == flight.UniqueNumber).ToListAsync();
            List<Passager> passagers = new List<Passager>();

            foreach (var reservation in flight.Reservations)
            {
                reservation.Passagers = await _context.Passagers.Where(p => p.ReservationId == reservation.Id).ToListAsync();
                passagers.AddRange(reservation.Passagers);
            }

            return flight;
        }

        //Resize the number of rows with given size
        private void ResizeThePage(int pageSize)
        {
            ViewData["PageSizeParm10"] = pageSize == 10 ? 10 : 10;
            ViewData["PageSizeParm25"] = pageSize == 25 ? 10 : 25;
            ViewData["PageSizeParm50"] = pageSize == 50 ? 10 : 50;

            if (pageSize != 0)
            {
                PageSize = pageSize;
            }
        }

        //Returns ordered collection for the user(Admin, Employee or Guest)
        private async Task<ICollection<FlightsViewModel>> OrderTheFlightsForIndexAsync(string sortOrder, PagerViewModel pager)
        {
            ViewData["LocationFromSortParm"] = sortOrder == "location_from" ? "location_from_desc" : "location_from";
            ViewData["LocationToSortParm"] = sortOrder == "location_to" ? "location_to_desc" : "location_to";
            ViewData["TakeOffTimeSortParm"] = sortOrder == "TakeOff" ? "take_off_desc" : "TakeOff";
            ViewData["LandingTimeSortParm"] = sortOrder == "Landing" ? "landing_desc" : "Landing";

            ICollection<FlightsViewModel> items;

            if (User.IsInRole("Admin") || User.IsInRole("Employee"))
            {
                items = await OrderFlightsForAdminAsync(sortOrder, pager);
            }
            else
            {
                items = await OrderFlightsForUserAsync(sortOrder, pager);
            }

            return items;
        }

        //Returns ordered collection for the admin
        private async Task<ICollection<FlightsViewModel>> OrderFlightsForAdminAsync(string sortOrder, PagerViewModel Pager)
        {
            ICollection<FlightsViewModel> items;

            switch (sortOrder)
            {
                case "location_from":
                    items = await _context.Flights.Skip((Pager.CurrentPage - 1) * PageSize).Take(PageSize).Select(f => new FlightsViewModel()
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
                    }).OrderBy(f => f.LocationFrom).ToListAsync();
                    break;
                case "location_to":
                    items = await _context.Flights.Skip((Pager.CurrentPage - 1) * PageSize).Take(PageSize).Select(f => new FlightsViewModel()
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
                    }).OrderBy(f => f.LocationTo).ToListAsync();
                    break;
                case "location_from_desc":
                    items = await _context.Flights.Skip((Pager.CurrentPage - 1) * PageSize).Take(PageSize).Select(f => new FlightsViewModel()
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
                    }).OrderByDescending(f => f.LocationFrom).ToListAsync();
                    break;
                case "location_to_desc":
                    items = await _context.Flights.Skip((Pager.CurrentPage - 1) * PageSize).Take(PageSize).Select(f => new FlightsViewModel()
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
                    }).OrderByDescending(f => f.LocationTo).ToListAsync();
                    break;
                case "TakeOff":
                    items = await _context.Flights.Skip((Pager.CurrentPage - 1) * PageSize).Take(PageSize).Select(f => new FlightsViewModel()
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
                    }).OrderBy(f => f.TakeOffTime).ToListAsync();
                    break;
                case "Landing":
                    items = await _context.Flights.Skip((Pager.CurrentPage - 1) * PageSize).Take(PageSize).Select(f => new FlightsViewModel()
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
                    }).OrderBy(f => f.LandingTime).ToListAsync();
                    break;
                case "landing_desc":
                    items = await _context.Flights.Skip((Pager.CurrentPage - 1) * PageSize).Take(PageSize).Select(f => new FlightsViewModel()
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
                    }).OrderByDescending(f => f.LandingTime).ToListAsync();
                    break;
                default:
                    items = await _context.Flights.Skip((Pager.CurrentPage - 1) * PageSize).Take(PageSize).Select(f => new FlightsViewModel()
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
                    break;
            }
            return items;

        }

        //Returns ordered collection for guests
        private async Task<ICollection<FlightsViewModel>> OrderFlightsForUserAsync(string sortOrder, PagerViewModel Pager)
        {
            ICollection<FlightsViewModel> items;

            switch (sortOrder)
            {
                case "location_from":
                    items = await _context.Flights.Where(f => f.IsOld == false).Skip((Pager.CurrentPage - 1) * PageSize).Take(PageSize).Select(f => new FlightsViewModel()
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
                    }).OrderBy(f => f.LocationFrom).ToListAsync();
                    break;
                case "location_to":
                    items = await _context.Flights.Where(f => f.IsOld == false).Skip((Pager.CurrentPage - 1) * PageSize).Take(PageSize).Select(f => new FlightsViewModel()
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
                    }).OrderBy(f => f.LocationTo).ToListAsync();
                    break;
                case "location_from_desc":
                    items = await _context.Flights.Where(f => f.IsOld == false).Skip((Pager.CurrentPage - 1) * PageSize).Take(PageSize).Select(f => new FlightsViewModel()
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
                    }).OrderByDescending(f => f.LocationFrom).ToListAsync();
                    break;
                case "location_to_desc":
                    items = await _context.Flights.Where(f => f.IsOld == false).Skip((Pager.CurrentPage - 1) * PageSize).Take(PageSize).Select(f => new FlightsViewModel()
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
                    }).OrderByDescending(f => f.LocationTo).ToListAsync();
                    break;
                case "TakeOff":
                    items = await _context.Flights.Where(f => f.IsOld == false).Skip((Pager.CurrentPage - 1) * PageSize).Take(PageSize).Select(f => new FlightsViewModel()
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
                    }).OrderBy(f => f.TakeOffTime).ToListAsync();
                    break;
                case "Landing":
                    items = await _context.Flights.Where(f => f.IsOld == false).Skip((Pager.CurrentPage - 1) * PageSize).Take(PageSize).Select(f => new FlightsViewModel()
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
                    }).OrderBy(f => f.LandingTime).ToListAsync();
                    break;
                case "landing_desc":
                    items = await _context.Flights.Where(f => f.IsOld == false).Skip((Pager.CurrentPage - 1) * PageSize).Take(PageSize).Select(f => new FlightsViewModel()
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
                    }).OrderByDescending(f => f.LandingTime).ToListAsync();
                    break;
                default:
                    items = await _context.Flights.Where(f => f.IsOld == false).Skip((Pager.CurrentPage - 1) * PageSize).Take(PageSize).Select(f => new FlightsViewModel()
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
                    break;
            }
            return items;

        }

        //Returns ordered collection with passagers from given flight
        private ICollection<PassagersViewModel> OrderThePassagersForDetails(string sortOrder, PagerViewModel pager, ICollection<Passager> passagers)
        {
            ViewData["FirstNameSortParm"] = sortOrder == "first_name" ? "first_name_desc" : "first_name";
            ViewData["MiddleNameSortParm"] = sortOrder == "middle_name" ? "middle_name_desc" : "middle_name";
            ViewData["LastNameSortParm"] = sortOrder == "last_name" ? "last_name_desc" : "last_name";
            ViewData["NationalitySortParm"] = sortOrder == "nationality" ? "nationality_desc" : "nationality";
            ViewData["PhoneNumberSortParm"] = sortOrder == "phone_number" ? "phone_number_desc" : "phone_number";
            ViewData["TypeSortParm"] = sortOrder == "type" ? "type_desc" : "type";

            ICollection<PassagersViewModel> items;

            switch (sortOrder)
            {
                case "first_name":
                    items = passagers.Skip((pager.CurrentPage - 1) * PageSize).Take(PageSize).Select(r => new PassagersViewModel()
                    {
                        MiddleName = r.MiddleName,
                        FirstName = r.FirstName,
                        LastName = r.LastName,
                        Nationality = r.Nationality,
                        PhoneNumber = r.PhoneNumber,
                        Type = r.Type
                    }).OrderBy(f => f.FirstName).ToList();
                    break;
                case "middle_name":
                    items = passagers.Skip((pager.CurrentPage - 1) * PageSize).Take(PageSize).Select(r => new PassagersViewModel()
                    {
                        MiddleName = r.MiddleName,
                        FirstName = r.FirstName,
                        LastName = r.LastName,
                        Nationality = r.Nationality,
                        PhoneNumber = r.PhoneNumber,
                        Type = r.Type
                    }).OrderBy(f => f.MiddleName).ToList();
                    break;
                case "last_name":
                    items = passagers.Skip((pager.CurrentPage - 1) * PageSize).Take(PageSize).Select(r => new PassagersViewModel()
                    {
                        MiddleName = r.MiddleName,
                        FirstName = r.FirstName,
                        LastName = r.LastName,
                        Nationality = r.Nationality,
                        PhoneNumber = r.PhoneNumber,
                        Type = r.Type
                    }).OrderBy(f => f.LastName).ToList();
                    break;
                case "nationality":
                    items = passagers.Skip((pager.CurrentPage - 1) * PageSize).Take(PageSize).Select(r => new PassagersViewModel()
                    {
                        MiddleName = r.MiddleName,
                        FirstName = r.FirstName,
                        LastName = r.LastName,
                        Nationality = r.Nationality,
                        PhoneNumber = r.PhoneNumber,
                        Type = r.Type
                    }).OrderBy(f => f.Nationality).ToList();
                    break;
                case "phone_number":
                    items = passagers.Skip((pager.CurrentPage - 1) * PageSize).Take(PageSize).Select(r => new PassagersViewModel()
                    {
                        MiddleName = r.MiddleName,
                        FirstName = r.FirstName,
                        LastName = r.LastName,
                        Nationality = r.Nationality,
                        PhoneNumber = r.PhoneNumber,
                        Type = r.Type
                    }).OrderBy(f => f.PhoneNumber).ToList();
                    break;
                case "type":
                    items = passagers.Skip((pager.CurrentPage - 1) * PageSize).Take(PageSize).Select(r => new PassagersViewModel()
                    {
                        MiddleName = r.MiddleName,
                        FirstName = r.FirstName,
                        LastName = r.LastName,
                        Nationality = r.Nationality,
                        PhoneNumber = r.PhoneNumber,
                        Type = r.Type
                    }).OrderBy(f => f.Type).ToList();
                    break;
                case "first_name_desc":
                    items = passagers.Skip((pager.CurrentPage - 1) * PageSize).Take(PageSize).Select(r => new PassagersViewModel()
                    {
                        MiddleName = r.MiddleName,
                        FirstName = r.FirstName,
                        LastName = r.LastName,
                        Nationality = r.Nationality,
                        PhoneNumber = r.PhoneNumber,
                        Type = r.Type
                    }).OrderByDescending(f => f.FirstName).ToList();
                    break;
                case "middle_name_desc":
                    items = passagers.Skip((pager.CurrentPage - 1) * PageSize).Take(PageSize).Select(r => new PassagersViewModel()
                    {
                        MiddleName = r.MiddleName,
                        FirstName = r.FirstName,
                        LastName = r.LastName,
                        Nationality = r.Nationality,
                        PhoneNumber = r.PhoneNumber,
                        Type = r.Type
                    }).OrderByDescending(f => f.MiddleName).ToList();
                    break;
                case "last_name_desc":
                    items = passagers.Skip((pager.CurrentPage - 1) * PageSize).Take(PageSize).Select(r => new PassagersViewModel()
                    {
                        MiddleName = r.MiddleName,
                        FirstName = r.FirstName,
                        LastName = r.LastName,
                        Nationality = r.Nationality,
                        PhoneNumber = r.PhoneNumber,
                        Type = r.Type
                    }).OrderByDescending(f => f.LastName).ToList();
                    break;
                case "nationality_desc":
                    items = passagers.Skip((pager.CurrentPage - 1) * PageSize).Take(PageSize).Select(r => new PassagersViewModel()
                    {
                        MiddleName = r.MiddleName,
                        FirstName = r.FirstName,
                        LastName = r.LastName,
                        Nationality = r.Nationality,
                        PhoneNumber = r.PhoneNumber,
                        Type = r.Type
                    }).OrderByDescending(f => f.Nationality).ToList();
                    break;
                case "phone_number_desc":
                    items = passagers.Skip((pager.CurrentPage - 1) * PageSize).Take(PageSize).Select(r => new PassagersViewModel()
                    {
                        MiddleName = r.MiddleName,
                        FirstName = r.FirstName,
                        LastName = r.LastName,
                        Nationality = r.Nationality,
                        PhoneNumber = r.PhoneNumber,
                        Type = r.Type
                    }).OrderByDescending(f => f.PhoneNumber).ToList();
                    break;
                case "type_desc":
                    items = passagers.Skip((pager.CurrentPage - 1) * PageSize).Take(PageSize).Select(r => new PassagersViewModel()
                    {
                        MiddleName = r.MiddleName,
                        FirstName = r.FirstName,
                        LastName = r.LastName,
                        Nationality = r.Nationality,
                        PhoneNumber = r.PhoneNumber,
                        Type = r.Type
                    }).OrderByDescending(f => f.Type).ToList();
                    break;
                default:
                    items = passagers.Skip((pager.CurrentPage - 1) * PageSize).Take(PageSize).Select(r => new PassagersViewModel()
                    {
                        MiddleName = r.MiddleName,
                        FirstName = r.FirstName,
                        LastName = r.LastName,
                        Nationality = r.Nationality,
                        PhoneNumber = r.PhoneNumber,
                        Type = r.Type
                    }).OrderByDescending(f => f.FirstName).ToList();
                    break;
            }
            return items;
        }

        //Refresh the flights to check if they are old
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
    }
}
