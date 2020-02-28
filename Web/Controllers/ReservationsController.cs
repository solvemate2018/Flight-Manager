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

namespace Web.Controllers
{
    [Authorize]
    public class ReservationsController : Controller
    {
        private readonly FlightManagerDbContext _context;
        private int PageSize = 10;
        private int PageSizeForDetails = 10;
        private MailsController mailsController;

        public ReservationsController()
        {
            _context = new FlightManagerDbContext();
            mailsController = new MailsController();
        }

        [AllowAnonymous]
        public async Task<IActionResult> Index(ReservationsIndexViewModel model)
        {
            model.Pager ??= new PagerViewModel();
            model.Pager.CurrentPage = model.Pager.CurrentPage <= 0 ? 1 : model.Pager.CurrentPage;

            List<ReservationsViewModel> items = await _context.Reservations.Skip((model.Pager.CurrentPage - 1) * PageSize).Take(PageSize).Select(r => new ReservationsViewModel()
            {
                Id = r.Id,
                IsConfirmed = r.IsConfirmed,
                Email = r.Email,
                TotalNumberOfPassagers = r.Passagers.Count
            }).ToListAsync();

            model.Items = items;
            model.Pager.PagesCount = (int)Math.Ceiling(await _context.Reservations.CountAsync() / (double)PageSize);

            return View(model);
        }

        [AllowAnonymous]
        public IActionResult Create()
        {
            ReservationsCreateViewModel model = new ReservationsCreateViewModel();

            return View(model);
        }

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

        [AllowAnonymous]
        public async Task<IActionResult> Details(ReservationsDetailsViewModel model, int? id)
        {
            model.Pager ??= new PagerViewModel();
            model.Pager.CurrentPage = model.Pager.CurrentPage <= 0 ? 1 : model.Pager.CurrentPage;

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

            List<PassagersViewModel> items = await _context.Passagers.Skip((model.Pager.CurrentPage - 1) * PageSize).Take(PageSize).Where(p => p.ReservationId == reservation.Id).Select(r => new PassagersViewModel()
            {
                MiddleName = r.MiddleName,
                FirstName = r.FirstName,
                LastName = r.LastName,
                Nationality = r.Nationality,
                PhoneNumber = r.PhoneNumber,
                Type = r.Type
            }).ToListAsync();

            model.Passagers = items;
            model.Pager.PagesCount = (int)Math.Ceiling(await _context.Passagers.Where(p => p.ReservationId == reservation.Id).CountAsync() / (double)PageSize);

            return View(model);
        }

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
    }
}
