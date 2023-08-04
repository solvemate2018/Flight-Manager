using Data;
using Data.Entities;
using Data.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
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
            SignInManager<IdentityUser> signInManager, IConfiguration configuration)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            this._context = new FlightManagerDbContext(configuration);
        }

        //Returns the index page for admin
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index(UsersIndexViewModel model, int pageSize, string sortOrder)
        {
            model.Pager ??= new PagerViewModel();
            model.Pager.CurrentPage = model.Pager.CurrentPage <= 0 ? 1 : model.Pager.CurrentPage;

            ICollection<UsersViewModel> items;

            items = OrderTheUsersForIndex(sortOrder, model.Pager).ToList();

            ResizeThePage(pageSize);

            model.Order = sortOrder;
            model.PageSize = PageSize;

            model.Users = items;
            model.Pager.PagesCount = (int)Math.Ceiling(await _context.Users.CountAsync() / (double)PageSize);

            return View(model);
        }

        //Returs ordered collection from users for index page
        private ICollection<UsersViewModel> OrderTheUsersForIndex(string sortOrder, PagerViewModel pager)
        {
            ViewData["UsernameSortParm"] = sortOrder == "username" ? "username_desc" : "username";
            ViewData["PasswordSortParm"] = sortOrder == "password" ? "password_desc" : "password";
            ViewData["FirstNameSortParm"] = sortOrder == "first_name" ? "first_name_desc" : "first_name";
            ViewData["LastNameSortParm"] = sortOrder == "last_name" ? "last_name_desc" : "last_name";
            ViewData["UINSortParm"] = sortOrder == "UIN" ? "UIN_desc" : "UIN";
            ViewData["PhoneNumberSortParm"] = sortOrder == "phone_number" ? "phone_number_desc" : "phone_number";
            ViewData["AdressSortParm"] = sortOrder == "adress" ? "adress_desc" : "adress";
            ViewData["EmailSortParm"] = sortOrder == "email" ? "email_desc" : "email";

            ICollection<UsersViewModel> items;

            switch (sortOrder)
            {
                case "email_desc":
                    items = _context.Users.Skip((pager.CurrentPage - 1) * PageSize).Take(PageSize).Select(u => new UsersViewModel()
                    {
                        Adress = u.Adress,
                        Email = u.Email,
                        FirstName = u.FirstName,
                        LastName = u.LastName,
                        Password = u.Password,
                        PhoneNumber = u.PhoneNumber,
                        UniqueIdentificationNumber = u.UniqueIdentificationNumber,
                        UserName = u.UserName
                    }).OrderByDescending(i => i.Email).ToList();
                    break;
                case "password":
                    items = _context.Users.Skip((pager.CurrentPage - 1) * PageSize).Take(PageSize).Select(u => new UsersViewModel()
                    {
                        Adress = u.Adress,
                        Email = u.Email,
                        FirstName = u.FirstName,
                        LastName = u.LastName,
                        Password = u.Password,
                        PhoneNumber = u.PhoneNumber,
                        UniqueIdentificationNumber = u.UniqueIdentificationNumber,
                        UserName = u.UserName
                    }).OrderBy(i => i.Password).ToList();
                    break;
                case "first_name":
                    items = _context.Users.Skip((pager.CurrentPage - 1) * PageSize).Take(PageSize).Select(u => new UsersViewModel()
                    {
                        Adress = u.Adress,
                        Email = u.Email,
                        FirstName = u.FirstName,
                        LastName = u.LastName,
                        Password = u.Password,
                        PhoneNumber = u.PhoneNumber,
                        UniqueIdentificationNumber = u.UniqueIdentificationNumber,
                        UserName = u.UserName
                    }).OrderBy(i => i.FirstName).ToList();
                    break;
                case "last_name":
                    items = _context.Users.Skip((pager.CurrentPage - 1) * PageSize).Take(PageSize).Select(u => new UsersViewModel()
                    {
                        Adress = u.Adress,
                        Email = u.Email,
                        FirstName = u.FirstName,
                        LastName = u.LastName,
                        Password = u.Password,
                        PhoneNumber = u.PhoneNumber,
                        UniqueIdentificationNumber = u.UniqueIdentificationNumber,
                        UserName = u.UserName
                    }).OrderBy(i => i.LastName).ToList();
                    break;
                case "UIN":
                    items = _context.Users.Skip((pager.CurrentPage - 1) * PageSize).Take(PageSize).Select(u => new UsersViewModel()
                    {
                        Adress = u.Adress,
                        Email = u.Email,
                        FirstName = u.FirstName,
                        LastName = u.LastName,
                        Password = u.Password,
                        PhoneNumber = u.PhoneNumber,
                        UniqueIdentificationNumber = u.UniqueIdentificationNumber,
                        UserName = u.UserName
                    }).OrderBy(i => i.UniqueIdentificationNumber).ToList();
                    break;
                case "phone_number":
                    items = _context.Users.Skip((pager.CurrentPage - 1) * PageSize).Take(PageSize).Select(u => new UsersViewModel()
                    {
                        Adress = u.Adress,
                        Email = u.Email,
                        FirstName = u.FirstName,
                        LastName = u.LastName,
                        Password = u.Password,
                        PhoneNumber = u.PhoneNumber,
                        UniqueIdentificationNumber = u.UniqueIdentificationNumber,
                        UserName = u.UserName
                    }).OrderBy(i => i.PhoneNumber).ToList();
                    break;
                case "adress":
                    items = _context.Users.Skip((pager.CurrentPage - 1) * PageSize).Take(PageSize).Select(u => new UsersViewModel()
                    {
                        Adress = u.Adress,
                        Email = u.Email,
                        FirstName = u.FirstName,
                        LastName = u.LastName,
                        Password = u.Password,
                        PhoneNumber = u.PhoneNumber,
                        UniqueIdentificationNumber = u.UniqueIdentificationNumber,
                        UserName = u.UserName
                    }).OrderBy(i => i.Adress).ToList();
                    break;
                case "email":
                    items = _context.Users.Skip((pager.CurrentPage - 1) * PageSize).Take(PageSize).Select(u => new UsersViewModel()
                    {
                        Adress = u.Adress,
                        Email = u.Email,
                        FirstName = u.FirstName,
                        LastName = u.LastName,
                        Password = u.Password,
                        PhoneNumber = u.PhoneNumber,
                        UniqueIdentificationNumber = u.UniqueIdentificationNumber,
                        UserName = u.UserName
                    }).OrderBy(i => i.Email).ToList();
                    break;
                case "username_desc":
                    items = _context.Users.Skip((pager.CurrentPage - 1) * PageSize).Take(PageSize).Select(u => new UsersViewModel()
                    {
                        Adress = u.Adress,
                        Email = u.Email,
                        FirstName = u.FirstName,
                        LastName = u.LastName,
                        Password = u.Password,
                        PhoneNumber = u.PhoneNumber,
                        UniqueIdentificationNumber = u.UniqueIdentificationNumber,
                        UserName = u.UserName
                    }).OrderByDescending(i => i.UserName).ToList();
                    break;
                case "password_desc":
                    items = _context.Users.Skip((pager.CurrentPage - 1) * PageSize).Take(PageSize).Select(u => new UsersViewModel()
                    {
                        Adress = u.Adress,
                        Email = u.Email,
                        FirstName = u.FirstName,
                        LastName = u.LastName,
                        Password = u.Password,
                        PhoneNumber = u.PhoneNumber,
                        UniqueIdentificationNumber = u.UniqueIdentificationNumber,
                        UserName = u.UserName
                    }).OrderByDescending(i => i.Password).ToList();
                    break;
                case "first_name_desc":
                    items = _context.Users.Skip((pager.CurrentPage - 1) * PageSize).Take(PageSize).Select(u => new UsersViewModel()
                    {
                        Adress = u.Adress,
                        Email = u.Email,
                        FirstName = u.FirstName,
                        LastName = u.LastName,
                        Password = u.Password,
                        PhoneNumber = u.PhoneNumber,
                        UniqueIdentificationNumber = u.UniqueIdentificationNumber,
                        UserName = u.UserName
                    }).OrderByDescending(i => i.FirstName).ToList();
                    break;
                case "last_name_desc":
                    items = _context.Users.Skip((pager.CurrentPage - 1) * PageSize).Take(PageSize).Select(u => new UsersViewModel()
                    {
                        Adress = u.Adress,
                        Email = u.Email,
                        FirstName = u.FirstName,
                        LastName = u.LastName,
                        Password = u.Password,
                        PhoneNumber = u.PhoneNumber,
                        UniqueIdentificationNumber = u.UniqueIdentificationNumber,
                        UserName = u.UserName
                    }).OrderByDescending(i => i.LastName).ToList();
                    break;
                case "UIN_desc":
                    items = _context.Users.Skip((pager.CurrentPage - 1) * PageSize).Take(PageSize).Select(u => new UsersViewModel()
                    {
                        Adress = u.Adress,
                        Email = u.Email,
                        FirstName = u.FirstName,
                        LastName = u.LastName,
                        Password = u.Password,
                        PhoneNumber = u.PhoneNumber,
                        UniqueIdentificationNumber = u.UniqueIdentificationNumber,
                        UserName = u.UserName
                    }).OrderByDescending(i => i.UniqueIdentificationNumber).ToList();
                    break;
                case "phone_number_desc":
                    items = _context.Users.Skip((pager.CurrentPage - 1) * PageSize).Take(PageSize).Select(u => new UsersViewModel()
                    {
                        Adress = u.Adress,
                        Email = u.Email,
                        FirstName = u.FirstName,
                        LastName = u.LastName,
                        Password = u.Password,
                        PhoneNumber = u.PhoneNumber,
                        UniqueIdentificationNumber = u.UniqueIdentificationNumber,
                        UserName = u.UserName
                    }).OrderByDescending(i => i.PhoneNumber).ToList();
                    break;
                case "adress_desc":
                    items = _context.Users.Skip((pager.CurrentPage - 1) * PageSize).Take(PageSize).Select(u => new UsersViewModel()
                    {
                        Adress = u.Adress,
                        Email = u.Email,
                        FirstName = u.FirstName,
                        LastName = u.LastName,
                        Password = u.Password,
                        PhoneNumber = u.PhoneNumber,
                        UniqueIdentificationNumber = u.UniqueIdentificationNumber,
                        UserName = u.UserName
                    }).OrderByDescending(i => i.Adress).ToList();
                    break;
                default:
                    items = _context.Users.Skip((pager.CurrentPage - 1) * PageSize).Take(PageSize).Select(u => new UsersViewModel()
                    {
                        Adress = u.Adress,
                        Email = u.Email,
                        FirstName = u.FirstName,
                        LastName = u.LastName,
                        Password = u.Password,
                        PhoneNumber = u.PhoneNumber,
                        UniqueIdentificationNumber = u.UniqueIdentificationNumber,
                        UserName = u.UserName
                    }).OrderBy(i => i.UserName).ToList();
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

        //Returns a page with form for registring
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public IActionResult Register()
        {
            RegisterViewModel model = new RegisterViewModel();
            return View(model);
        }

        //Register a new user, when the register form is filled
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

                    if (_context.Users.Select(u => u.UniqueIdentificationNumber).Count() != 0)
                    {
                        if (_context.Users.Select(u => u.UniqueIdentificationNumber).First() == user.UniqueIdentificationNumber)
                        {
                            return View(model);
                        }
                    }
                    await _context.SaveChangesAsync();

                    return RedirectToAction("index", "home");
                }
            }
            return View(model);
        }

        //Logging out of account
        public async Task<IActionResult> Logout()
        {
            await signInManager.SignOutAsync();
            return RedirectToAction("index", "home");
        }

        //Returns login page
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login()
        {
            LoginViewModel model = new LoginViewModel();
            return View(model);
        }

        //Login into existing account
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

        //Delete user account
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(string id)
        {
            User user = await _context.Users.FindAsync(id);
            _context.Users.Remove(user);

            IdentityUser identityUser = await userManager.FindByNameAsync(user.UserName);

            await userManager.DeleteAsync(identityUser);

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        //Returns a page with edit form
        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            User user = await _context.Users.FindAsync(id);
            UsersEditViewModel model = new UsersEditViewModel
            {
                Adress = user.Adress,
                FirstName = user.FirstName,
                Email = user.Email,
                LastName = user.Email,
                Password = user.Password,
                PhoneNumber = user.PhoneNumber,
                UniqueIdentificationNumber = user.UniqueIdentificationNumber
            };

            return View(model);
        }

        //Update user account with given info
        [HttpPost]
        public async Task<IActionResult> Edit(UsersEditViewModel model)
        {
            if (ModelState.IsValid)
           {
                User user = await _context.Users.FindAsync(model.UniqueIdentificationNumber);

                IdentityUser identityUser = await userManager.FindByNameAsync(user.UserName);

                await userManager.ChangePasswordAsync(identityUser, user.Password, model.Password);

                user.Adress = model.Adress;
                user.Email = model.Email;
                user.FirstName = model.FirstName;
                user.LastName = model.LastName;
                user.PhoneNumber = model.PhoneNumber;
                user.Password = model.Password;

                _context.Users.Update(user);

                identityUser.PhoneNumber = model.PhoneNumber;
                identityUser.Email = model.Email;

                await userManager.UpdateAsync(identityUser);

                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }
    }
}
