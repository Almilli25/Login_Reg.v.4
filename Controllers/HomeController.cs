using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;
using login_register.Models;



namespace login_register.Controllers
{
    public class HomeController : Controller
    {
        private MyContext _context {get;set;}
        public HomeController(MyContext context)
        {
            _context = context;
        }




        [HttpGet("")]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost("register")]
        public IActionResult Register(User newUser)
        {
            if(ModelState.IsValid)
            {
                if(_context.Users.Any(u => u.Email == newUser.Email))
                {
                    ModelState.AddModelError("Email", "Email already in use!");
                    return View("Index");
                }
                else
                {
                    PasswordHasher<User> Hasher = new PasswordHasher<User>();
                    newUser.Password = Hasher.HashPassword(newUser, newUser.Password);
                    _context.Users.Add(newUser);
                    _context.SaveChanges();
                    return RedirectToAction("Log");
                }
            }
            else
            {
                return View("Index");
            }
        }

        [HttpGet("log")]
        public IActionResult Log()
        {
            return View("Login");
        }

        [HttpPost("login")]
        public IActionResult Login(LoginUser userSubmission)
        {
            if(ModelState.IsValid)
            {
                var userInDb = _context.Users.FirstOrDefault(u => u.Email == userSubmission.Email);
                if(userInDb == null)
                {
                    ModelState.AddModelError("Email", "Invalid Email/Password");
                    return View("Login");
                }
                var hasher = new PasswordHasher<LoginUser>();
                var result = hasher.VerifyHashedPassword(userSubmission, userInDb.Password, userSubmission.Password);
                if(result == 0)
                {
                    ModelState.AddModelError("Password", "Invalid Email/Password");
                    return View("Login");
                }
                else
                {
                    HttpContext.Session.SetInt32("userId", userInDb.UserId);
                    return RedirectToAction("Success");
                }
            }
            else
            {
                return View("Login");
            }
        }

        [HttpGet("success")]
        public IActionResult Success()
        {
            var userInDb = _context.Users.FirstOrDefault(u => u.UserId == HttpContext.Session.GetInt32("userId"));
            if(userInDb == null)
            {
                return RedirectToAction("Logout");
            }
            return View();
        }

        [HttpGet("logout")]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index");
        }

























        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
