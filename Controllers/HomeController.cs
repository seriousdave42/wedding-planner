using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WeddingPlanner.Models;

namespace WeddingPlanner.Controllers
{
    public class HomeController : Controller
    {
        private PlannerContext _context;

        public HomeController(PlannerContext context)
        {
            _context = context;
        }

        [HttpGet("")]
        public IActionResult Index()
        {
            List<Wedding> allWeddings = _context.Weddings.ToList();
            foreach (Wedding w in allWeddings)
            {
                if (w.Date < DateTime.Now)
                {
                    _context.Weddings.Remove(w);
                    _context.SaveChanges();
                }
            }
            return View();
        }

        [HttpPost("register")]
        public IActionResult Register(User newUser)
        {
            if(ModelState.IsValid)
            {
                if(_context.Users.Any(u => u.Email == newUser.Email))
                {
                    ModelState.AddModelError("Email", "Email already registered!");
                    return View("Index");
                }
                else
                {
                    PasswordHasher<User> hasher = new PasswordHasher<User>();
                    newUser.Password = hasher.HashPassword(newUser, newUser.Password);
                    _context.Users.Add(newUser);
                    _context.SaveChanges();
                    User justMade = _context.Users.FirstOrDefault(u => u.Email == newUser.Email);
                    HttpContext.Session.SetInt32("LoggedId", justMade.UserId);
                    return RedirectToAction("Dashboard");
                }
            }
            else
            {
                return View ("Index");
            }
        }

        [HttpPost("login")]
        public IActionResult Login(UserLogin userLogin)
        {
            if (ModelState.IsValid)
            {
                User userInDb = _context.Users.FirstOrDefault(u => u.Email == userLogin.Email);
                if (userInDb == null)
                {
                    ModelState.AddModelError("Email", "Invalid Email/Password");
                    return View("Index");
                }
                else
                {
                    PasswordHasher<UserLogin> hasher = new PasswordHasher<UserLogin>();
                    PasswordVerificationResult check = hasher.VerifyHashedPassword(userLogin, userInDb.Password, userLogin.Password);
                    if (check == 0)
                    {
                        ModelState.AddModelError("Email", "Invalid Email/Password");
                        return View("Index");
                    }
                    else
                    {
                        HttpContext.Session.SetInt32("LoggedId", userInDb.UserId);
                        return RedirectToAction("Dashboard");
                    }
                }
            }
            else
            {
                return View("Index");
            }
        }

        [HttpGet("wedding")]
        public IActionResult Wedding()
        {
            return View();
        }

        [HttpPost("wedding")]
        public IActionResult NewWedding(Wedding newWedding)
        {
            if (ModelState.IsValid)
            {
                int? userId = HttpContext.Session.GetInt32("LoggedId");
                newWedding.UserId = (int)userId;
                _context.Weddings.Add(newWedding);
                _context.SaveChanges();
                Wedding createdWedding = _context.Weddings.FirstOrDefault(w => 
                                                        w.UserId == (int)userId &&
                                                        w.Wedder1 == newWedding.Wedder1 &&
                                                        w.Wedder2 == newWedding.Wedder2 &&
                                                        w.Date == newWedding.Date &&
                                                        w.Address == newWedding.Address);
                return RedirectToAction("SingleWedding", new {weddingId = createdWedding.WeddingId});
            }
            else
            {
                return View("Wedding");
            }
        }

        [HttpGet("weddings/{weddingId}")]
        public IActionResult SingleWedding(int weddingId)
        {
            Wedding wedding = _context.Weddings
                                      .Include(w => w.Attendees)
                                      .ThenInclude(g => g.Attendee)
                                      .SingleOrDefault(w => w.WeddingId == weddingId);
            return View(wedding);
        }

        [HttpGet("dashboard")]
        public IActionResult Dashboard()
        {
            int? userId = HttpContext.Session.GetInt32("LoggedId");
            if (userId == null)
            {
                return RedirectToAction("Index");
            }
            else
            {
                ViewBag.AllWeddings = _context.Weddings
                                              .Include(w => w.Attendees)
                                              .ToList();
                User user = _context.Users
                                    .Include(u => u.RSVPs)
                                    .ThenInclude(g => g.RSVP)
                                    .Include(u => u.WeddingsPlanned)
                                    .FirstOrDefault(u => u.UserId == (int)userId);
                return View(user);
            }
        }

        [HttpGet("rsvp/{userId}/{weddingId}")]
        public IActionResult RSVP(int userId, int weddingId)
        {
            Guest newGuest = new Guest()
            {
                UserId = userId,
                WeddingId = weddingId
            };
            _context.Guests.Add(newGuest);
            _context.SaveChanges();
            return RedirectToAction("Dashboard");
        }

        [HttpGet("unrsvp/{userId}/{weddingId}")]
        public IActionResult UnRSVP(int userId, int weddingId)
        {
            Guest cancelGuest = _context.Guests.FirstOrDefault(g => g.UserId == userId && g.WeddingId == weddingId);
            _context.Guests.Remove(cancelGuest);
            _context.SaveChanges();
            return RedirectToAction("Dashboard");
        }

        [HttpGet("cancel/{weddingId}")]
        public IActionResult Cancel(int weddingId)
        {
            Wedding cancelled = _context.Weddings.FirstOrDefault(w => w.WeddingId == weddingId);
            _context.Weddings.Remove(cancelled);
            _context.SaveChanges();
            return RedirectToAction("Dashboard");
        }

        [HttpGet("logout")]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index");
        }


        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
