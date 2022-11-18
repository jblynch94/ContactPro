using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using ContactPro.Data;
using ContactPro.Models;
using ContactPro.Enums;
using ContactPro.Services;
using ContactPro.Services.Interfaces;
using ContactPro.Models.ViewModels;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace ContactPro.Controllers
{
    public class ContactsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly IImageService _imageService;
        private readonly IAddressBookService _addressBookService;
        private readonly IEmailSender _emailService;

        public ContactsController(ApplicationDbContext context,
                                  UserManager<AppUser> userManager,
                                  IImageService imageService,
                                  IAddressBookService addressBookService,
                                  IEmailSender emailService)
        {
            _context = context;
            _userManager = userManager;
            _imageService = imageService;
            _addressBookService = addressBookService;
            _emailService = emailService;
        }

        // GET: Contacts
        [Authorize]
        public async Task<IActionResult> Index(int categoryId, string swalMessage = null!)
        {
            ViewData["SwalMessage"] = swalMessage;

            string appUserId = _userManager.GetUserId(User);

            List<Contact> contacts = new List<Contact>();

            AppUser? appUser = await _context.Users
                                                  .Include(u => u.Contacts)
                                                  .ThenInclude(c => c.Categories)
                                                  .FirstOrDefaultAsync(u => u.Id == appUserId);
            if (categoryId == 0)
            {
                contacts = appUser!.Contacts
                                  .OrderBy(c => c.LastName)
                                  .ThenBy(c => c.FirstName)
                                  .ToList();

            }
            else
            {
                contacts = appUser!.Categories.FirstOrDefault(c => c.Id == categoryId)!
                                    .Contacts
                                    .OrderBy(c => c.LastName)
                                    .ThenBy(c => c.FirstName)
                                    .ToList();
            }

            ViewData["CategoryId"] = new SelectList(appUser.Categories, "Id", "Name", categoryId);

            return View(contacts);
        }

        [Authorize]
        public async Task<IActionResult> Search(string searchString)
        {
            string appUserId = _userManager.GetUserId(User);

            List<Contact> contacts = new List<Contact>();

            AppUser? appUser = await _context.Users
                                                 .Include(u => u.Contacts)
                                                 .ThenInclude(c => c.Categories)
                                                 .FirstOrDefaultAsync(u => u.Id == appUserId);

            if (string.IsNullOrEmpty(searchString))
            {
                contacts = appUser!.Contacts
                                     .OrderBy(c => c.LastName)
                                     .ThenBy(c => c.FirstName)
                                    .ToList();
            }
            else
            {
                contacts = appUser!.Contacts
                                    .Where(c => c.FullName!.ToLower().Contains(searchString.ToLower()))
                                   .OrderBy(c => c.LastName)
                                   .ThenBy(c => c.FirstName)
                                   .ToList();
            }

            ViewData["CategoryId"] = new SelectList(appUser.Categories, "Id", "Name");

            return View(nameof(Index), contacts);
        }


        // GET: Contacts/Create
        [Authorize]
        public async Task<IActionResult> Create()
        {
            string appUserId = _userManager.GetUserId(User);
            ViewData["StatesList"] = new SelectList(Enum.GetValues(typeof(States)).Cast<States>().ToList());
            ViewData["CategoryList"] = new SelectList(await _addressBookService.GetUserCategoriesAsync(appUserId), "Id", "Name");

            return View();
        }

        // POST: Contacts/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,AppUserId,FirstName,LastName,BirthDate,Address1,Address2,City,State,ZipCode,Email,PhoneNumber,Created,ImageFile")] Contact contact, List<int> CategoryList)
        {
            ModelState.Remove("AppUserId");

            if (ModelState.IsValid)
            {
                contact.AppUserId = _userManager.GetUserId(User);
                contact.Created = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc);

                if (contact.BirthDate != null)
                {
                    contact.BirthDate = DateTime.SpecifyKind(contact.BirthDate.Value, DateTimeKind.Utc);
                }
                if (contact.ImageFile != null)
                {
                    contact.ImageData = await _imageService.ConvertFileToByteArrayAsync(contact.ImageFile);
                    contact.ImageType = contact.ImageFile.ContentType;
                }

                _context.Add(contact);
                await _context.SaveChangesAsync();

                //add contact contegories
                foreach (int categoryId in CategoryList)
                {
                    //save each category to the database for the contact
                    await _addressBookService.AddContactToCategoryAsyn(categoryId, contact.Id);
                }

            }
            return RedirectToAction(nameof(Index));
        }

        // GET: Contacts/Edit/5
        [Authorize]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            string appUserId = _userManager.GetUserId(User);

            Contact? contact = await _context.Contact!.Where(c => c.Id == id && c.AppUserId == appUserId).FirstOrDefaultAsync();

            if (contact == null)
            {
                return NotFound();
            }
            ViewData["StatesList"] = new SelectList(Enum.GetValues(typeof(States)).Cast<States>().ToList());
            ViewData["CategoryList"] = new MultiSelectList(await _addressBookService.GetUserCategoriesAsync(appUserId), "Id", "Name", await _addressBookService.GetContactCategoryIdsAsync(contact.Id));

            return View(contact);
        }

        // POST: Contacts/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,AppUserId,FirstName,LastName,BirthDate,Address1,Address2,City,State,ZipCode,Email,PhoneNumber,Created,ImageData,ImageType,ImageFile")] Contact contact, List<int> CategoryList)
        {
            if (id != contact.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    contact.Created = DateTime.SpecifyKind(contact.Created, DateTimeKind.Utc);

                    if (contact.BirthDate != null)
                    {
                        contact.BirthDate = DateTime.SpecifyKind(contact.BirthDate.Value, DateTimeKind.Utc);
                    }
                    if (contact.ImageFile != null)
                    {
                        contact.ImageData = await _imageService.ConvertFileToByteArrayAsync(contact.ImageFile);
                        contact.ImageType = contact.ImageFile.ContentType;
                    }

                    _context.Update(contact);

                    //remove all the categories associated with the contact

                    //get a list of the old categories
                    List<Category> oldCategories = (await _addressBookService.GetContactCategoriesAsync(contact.Id)).ToList();
                    foreach (Category category in oldCategories)
                    {
                        //remove the category from the contact
                        await _addressBookService.RemoveContactFromCategoryAsyn(category.Id, contact.Id);

                    }

                    //then we add back the selcted categories to the contact
                    foreach (int categoryId in CategoryList)
                    {
                        //Add each one to the database
                        await _addressBookService.AddContactToCategoryAsyn(categoryId, contact.Id);
                    }


                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ContactExists(contact.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["AppUserId"] = new SelectList(_context.Users, "Id", "Id", contact.AppUserId);
            return View(contact);
        }

        // GET: Contacts/Delete/5
        [Authorize]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Contact == null)
            {
                return NotFound();
            }

            var contact = await _context.Contact
                .Include(c => c.AppUser)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (contact == null)
            {
                return NotFound();
            }

            return View(contact);
        }

        // POST: Contacts/Delete/5
        [Authorize]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Contact == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Contact'  is null.");
            }
            var contact = await _context.Contact.FindAsync(id);
            if (contact != null)
            {
                _context.Contact.Remove(contact);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ContactExists(int id)
        {
            return (_context.Contact?.Any(e => e.Id == id)).GetValueOrDefault();
        }

        [Authorize]
        public async Task<IActionResult> EmailContact(int id)
        {
            string appUserId = _userManager.GetUserId(User);
            Contact? contact = await _context.Contact!.Where(c => c.Id == id && c.AppUserId == appUserId)
                                                   .FirstOrDefaultAsync();
            if (contact == null)
            {
                return NotFound();
            }
            EmailData emailData = new EmailData()
            {
                EmailAddress = contact.Email!,
                FirstName = contact.FirstName,
                LastName = contact.LastName
            };
            EmailContactViewModel model = new EmailContactViewModel()
            {
                Contact = contact,
                EmailData = emailData
            };
            return View(model);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> EmailContact(EmailContactViewModel ecvm)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await _emailService.SendEmailAsync(ecvm.EmailData!.EmailAddress, ecvm.EmailData.Subject, ecvm.EmailData.Body);
                    return RedirectToAction("Index", "Contacts", new {swalMessage = "Success: Email Sent!"});
                }
                catch
                {
                    return RedirectToAction("Index", "Contacts", new {swalMessage = "Error: Email Send Failed!"});
                    throw;
                }

            }
            return View(ecvm);
        }
    }
}
