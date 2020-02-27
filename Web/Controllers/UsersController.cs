﻿using Data;
using Data.Entities;
using Data.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Web.Models.Shared;
using Web.Models.Users;

namespace Web.Controllers
{
    [Authorize]
    public class UsersController:Controller
    {
        private readonly UserManager<IdentityUser> userManager;
        private readonly SignInManager<IdentityUser> signInManager;
        private readonly FlightManagerDbContext _context;

        private int PageSize = 10;

        public UsersController(UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            this._context = new FlightManagerDbContext();
        }


        public async Task<IActionResult> Index(UsersIndexViewModel model)
        {
            model.Pager ??= new PagerViewModel();
            model.Pager.CurrentPage = model.Pager.CurrentPage <= 0 ? 1 : model.Pager.CurrentPage;

            ICollection<UsersViewModel> items = await _context.Users.Skip((model.Pager.CurrentPage - 1) * PageSize).Take(PageSize).Select(u => new UsersViewModel()
            {
                Adress = u.Adress,
                Email = u.Email,
                FirstName = u.FirstName,
                LastName = u.LastName,
                Password = u.Password,
                PhoneNumber = u.PhoneNumber,
                UniqueIdentificationNumber = u.UniqueIdentificationNumber,
                UserName = u.UserName
            }).ToListAsync();

            model.Users = items;
            model.Pager.PagesCount = (int)Math.Ceiling(await _context.Users.CountAsync() / (double)PageSize);

            return View(model);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public IActionResult Register()
        {
            RegisterViewModel model = new RegisterViewModel();
            return View(model);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var identityUser = new IdentityUser { UserName = model.UserName, Email = model.Email, PhoneNumber = model.PhoneNumber};

                var result = await  userManager.CreateAsync(identityUser, model.Password);

                if (result.Succeeded)
                {
                    User user = new User
                    {
                        FirstName = model.FirstName,
                        LastName = model.LastName,
                        Adress = model.Adress,
                        Email = model.Email,
                        PhoneNumber = model.PhoneNumber,
                        Role = UserRole.Employee,
                        Password = model.Password,
                        UniqueIdentificationNumber = model.UniqueIdentificationNumber,
                        UserName = model.UserName
                    };
                    await _context.Users.AddAsync(user);
                    identityUser.EmailConfirmed = true;
                    await userManager.AddToRoleAsync(identityUser, "Employee");

                    await _context.SaveChangesAsync();

                    return RedirectToAction("index", "home");
                }
            }
            return View(model);
        }

        public async Task<IActionResult> Logout()
        {
            await signInManager.SignOutAsync();
            return RedirectToAction("index", "home");
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login()
        {
            LoginViewModel model = new LoginViewModel();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await signInManager.PasswordSignInAsync(model.Username, model.Password, model.RememberMe, false);

                if (result.Succeeded)
                {
                    return RedirectToAction("index", "home");
                }

                ModelState.AddModelError(string.Empty, "Invalid Login Attempt");
            }
            return View(model);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete()
        {
            User user = await _context.Users.FindAsync(TempData["UserId"]);
            _context.Users.Remove(user);

            IdentityUser identityUser = await userManager.FindByNameAsync(user.UserName);

            await userManager.DeleteAsync(identityUser);

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}