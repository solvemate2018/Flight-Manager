using Data;
using Data.Entities;
using Data.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Web.Models.Passager;
using Web.Models.Reservations;
using Web.Models.Shared;
using Web.Controllers;
using Microsoft.Extensions.Configuration;

namespace Web.Controllers
{
    [Authorize]
    public class ReservationsController : Controller
    {
        private readonly FlightManagerDbContext _context;
        private int PageSize = 10;
        private int PageSizeForDetails = 10;
        private MailsController mailsController;


        public ReservationsController(IConfiguration configuration)
        {
            _context = new FlightManagerDbContext(configuration);
            mailsController = new MailsController(configuration);
        }

        //Returns the index page for reservations
        [AllowAnonymous]
        public async Task<IActionResult> Index(ReservationsIndexViewModel model, int pageSize, string sortOrder)
        {
            model.Pager ??= new PagerViewModel();
            model.Pager.CurrentPage = model.Pager.CurrentPage <= 0 ? 1 : model.Pager.CurrentPage;

            ResizeThePage(pageSize);

            ICollection<ReservationsViewModel> items;

            items = await OrderTheReservationsForIndexAsync(sortOrder, model.Pager);

            model.PageSize = PageSize;
            model.Order = sortOrder;
            model.Items = items;
            model.Pager.PagesCount = (int)Math.Ceiling(await _context.Reservations.CountAsync() / (double)PageSize);

            return View(model);
        }

        //Returns ordered collection for index page
        private async Task<ICollection<ReservationsViewModel>> OrderTheReservationsForIndexAsync(string sortOrder, PagerViewModel pager)
        {
            ViewData["IdSortParm"] = sortOrder == "id" ? "id_desc" : "id";
            ViewData["IsConfirmedSortParm"] = sortOrder == "is_confirmed" ? "is_confirmed_desc" : "is_confirmed";
            ViewData["NumberOfPassagersSortParm"] = sortOrder == "number_of_passagers" ? "number_of_passagers_desc" : "number_of_passagers";
            ViewData["EmailSortParm"] = sortOrder == "email" ? "email_desc" : "email";

            ICollection<ReservationsViewModel> items;

            switch (sortOrder)
            {
                case "id":
                    items = await _context.Reservations.Skip((pager.CurrentPage - 1) * PageSize).Take(PageSize).Select(r => new ReservationsViewModel()
                    {
                        Id = r.Id,
                        IsConfirmed = r.IsConfirmed,
                        Email = r.Email,
                        TotalNumberOfPassagers = r.Passagers.Count
                    }).OrderBy(r => r.Id).ToListAsync();
                    break;
                case "is_confirmed":
                    items = await _context.Reservations.Skip((pager.CurrentPage - 1) * PageSize).Take(PageSize).Select(r => new ReservationsViewModel()
                    {
                        Id = r.Id,
                        IsConfirmed = r.IsConfirmed,
                        Email = r.Email,
                        TotalNumberOfPassagers = r.Passagers.Count
                    }).OrderBy(r => r.IsConfirmed).ToListAsync();
                    break;
                case "number_of_passagers":
                    items = await _context.Reservations.Skip((pager.CurrentPage - 1) * PageSize).Take(PageSize).Select(r => new ReservationsViewModel()
                    {
                        Id = r.Id,
                        IsConfirmed = r.IsConfirmed,
                        Email = r.Email,
                        TotalNumberOfPassagers = r.Passagers.Count
                    }).OrderBy(r => r.TotalNumberOfPassagers).ToListAsync();
                    break;
                case "email":
                    items = await _context.Reservations.Skip((pager.CurrentPage - 1) * PageSize).Take(PageSize).Select(r => new ReservationsViewModel()
                    {
                        Id = r.Id,
                        IsConfirmed = r.IsConfirmed,
                        Email = r.Email,
                        TotalNumberOfPassagers = r.Passagers.Count
                    }).OrderBy(r => r.Email).ToListAsync();
                    break;
                case "id_desc":
                    items = await _context.Reservations.Skip((pager.CurrentPage - 1) * PageSize).Take(PageSize).Select(r => new ReservationsViewModel()
                    {
                        Id = r.Id,
                        IsConfirmed = r.IsConfirmed,
                        Email = r.Email,
                        TotalNumberOfPassagers = r.Passagers.Count
                    }).OrderByDescending(r => r.Id).ToListAsync();
                    break;
                case "is_confirmed_desc":
                    items = await _context.Reservations.Skip((pager.CurrentPage - 1) * PageSize).Take(PageSize).Select(r => new ReservationsViewModel()
                    {
                        Id = r.Id,
                        IsConfirmed = r.IsConfirmed,
                        Email = r.Email,
                        TotalNumberOfPassagers = r.Passagers.Count
                    }).OrderByDescending(r => r.IsConfirmed).ToListAsync();
                    break;
                case "number_of_passagers_desc":
                    items = await _context.Reservations.Skip((pager.CurrentPage - 1) * PageSize).Take(PageSize).Select(r => new ReservationsViewModel()
                    {
                        Id = r.Id,
                        IsConfirmed = r.IsConfirmed,
                        Email = r.Email,
                        TotalNumberOfPassagers = r.Passagers.Count
                    }).OrderByDescending(r => r.TotalNumberOfPassagers).ToListAsync();
                    break;
                default:
                    items = await _context.Reservations.Skip((pager.CurrentPage - 1) * PageSize).Take(PageSize).Select(r => new ReservationsViewModel()
                    {
                        Id = r.Id,
                        IsConfirmed = r.IsConfirmed,
                        Email = r.Email,
                        TotalNumberOfPassagers = r.Passagers.Count
                    }).OrderByDescending(r => r.Email).ToListAsync();
                    break;
            }
            return items;
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

        //Returns a page with form for creating
        [AllowAnonymous]
        public IActionResult Create()
        {
            ReservationsCreateViewModel model = new ReservationsCreateViewModel();

            return View(model);
        }

        //Create new entity with given model
        [HttpPost, ActionName("Create")]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public async Task<IActionResult> Create(ReservationsCreateViewModel model, int? id)
        {
            if (ModelState.IsValid)
            {
                Reservation reservation = new Reservation
                {
                    Email = model.Email,
                    FlightId = (int)id,
                    Flight = _context.Flights.Find(id)
                };

                Flight flight = _context.Flights.Find(reservation.FlightId);

                for (int i = 0; i < (int)TempData["NumberOfPassagers"]; i++)
                {
                    Passager passager = new Passager
                    {
                        FirstName = model.Passagers[i].FirstName,
                        MiddleName = model.Passagers[i].MiddleName,
                        LastName = model.Passagers[i].LastName,
                        UniqueIdentificationNumber = model.Passagers[i].UniqueIdentificationNumber,
                        PhoneNumber = model.Passagers[i].PhoneNumber,
                        Nationality = model.Passagers[i].Nationality,
                        Type = model.Passagers[i].Type,
                        Reservation = reservation
                    };

                    _context.Passagers.Add(passager);
                }

                _context.Reservations.Add(reservation);

                flight.AddReservation(reservation);
                if (reservation.IsConfirmed)
                {
                    mailsController.SendReservationConfirmation(reservation.Email, reservation.Passagers);
                }

                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        //Returns the details page
        [AllowAnonymous]
        public async Task<IActionResult> Details(ReservationsDetailsViewModel model, int? id, int pageSize, string sortOrder)
        {
            model.Pager ??= new PagerViewModel();
            model.Pager.CurrentPage = model.Pager.CurrentPage <= 0 ? 1 : model.Pager.CurrentPage;

            ResizeThePage(pageSize);

            Reservation reservation = _context.Reservations.FindAsync(id).Result;
            Flight flight = _context.Flights.FindAsync(reservation.FlightId).Result;

            reservation.Flight = flight;
            reservation.Passagers = await _context.Passagers.Where(p => p.ReservationId == reservation.Id).ToListAsync();

            model.Id = reservation.Id;
            model.IsConfirmed = reservation.IsConfirmed;
            model.NumberOfBussinesPassagers = reservation.NumberOfBussinesPassagers;
            model.NumberOfRegularPassagers = reservation.NumberOfRegularPassagers;
            model.LocationFrom = flight.LocationFrom;
            model.Email = reservation.Email;
            model.LocationTo = flight.LocationTo;
            model.TakeOffTime = flight.TakeOffTime;
            model.LandingTime = flight.LandingTime;
            model.PageSize = PageSize;
            model.Order = sortOrder;

            List<PassagersViewModel> items = OrderThePassagersForDetails(sortOrder, model.Pager, reservation.Passagers).ToList();

            model.Passagers = items;
            model.Pager.PagesCount = (int)Math.Ceiling(await _context.Passagers.Where(p => p.ReservationId == reservation.Id).CountAsync() / (double)PageSize);

            return View(model);
        }

        //Delete reservation, if it is not confiirmed
        [Authorize(Roles = "Employee,Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            Reservation reservation = _context.Reservations.FindAsync(id).Result;

            if (!reservation.IsConfirmed)
            {
                _context.Passagers.RemoveRange(_context.Passagers.Where(p => p.ReservationId == reservation.Id));
                _context.Reservations.Remove(reservation);

                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            return RedirectToAction(nameof(Index));
        }

        //Send emails to each reservation host, when a flight is cancelled
        public async Task<IActionResult> CancelReservations()
        {
            int id = (int)TempData["FlightId"];
            foreach (Reservation reservation in _context.Reservations.Where(r => r.FlightId == id && r.IsConfirmed == true))
            {
                reservation.IsCancelled = true;
                _context.Reservations.Update(reservation);
                reservation.Flight = await _context.Flights.FindAsync(id);
            }

            mailsController.SendReservationCancellation(await _context.Reservations.Where(r => r.FlightId == id && r.IsConfirmed == true).ToListAsync());

            await _context.SaveChangesAsync();

            return RedirectToAction("Index", "Flights");
        }

        //Returns ordered collection with passagers from given reservation
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
    }
}
