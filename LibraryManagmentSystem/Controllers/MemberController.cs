﻿using LibraryManagmentSystem.Authorization;
using LibraryManagmentSystem.Data;
using LibraryManagmentSystem.Data.Interfaces;
using LibraryManagmentSystem.Data.Model;
using LibraryManagmentSystem.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace LibraryManagmentSystem.Controllers
{
    public class MemberController : Controller
      
    {
        protected readonly LibraryDbContext _context;
        protected readonly IAuthorizationService _authorizationService;
        protected readonly UserManager<IdentityUser> _userManager;
        public MemberController(LibraryDbContext context,
            IAuthorizationService authorizationService,
            UserManager<IdentityUser> userManager)
        {
            _context = context;
            _authorizationService = authorizationService;
            _userManager = userManager;

        }

        [Route("Member")]
        public IActionResult Index()
        {
                var memberVM = new List<ViewModel.MemberViewModel>();
                var members = _context.Members.ToList();

                if (members.Count() == 0)
                {
                    return View("Empty");
                }

                foreach (var member in members)
                {
                    memberVM.Add(new ViewModel.MemberViewModel
                    {
                        Member = member,
                        BookCount = _context.IssueTransactions.Count(m => m.MemberID == member.MemberID && m.Status == false)
                    });

                }

                return View(memberVM);
        }

        public IActionResult Delete(int id)
        {
            var member = _context.Members.Where( m => m.MemberID == id).First();
            _context.Members.Remove(member);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(Member member)
        {
            _context.Members.Add(member);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }

        public IActionResult Update(int id)
        {
            var member = _context.Members.Where(m => m.MemberID == id).First();
            return View(member);
        }

        [HttpPost]
        public async Task<IActionResult> Update(Member member, AccountStatus status)
        {
            if(status == null)
            {
                var isManager = User.IsInRole(Constants.ManagerRole);
                var isAdmin = User.IsInRole(Constants.AdminRole);

                if (isManager == false|| isAdmin == false)
                    return Forbid();

                member.Status = status;
            }
            _context.Members.Update(member);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }
    }
}
