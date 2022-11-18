using ContactPro.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using ContactPro.Models;
using ContactPro.Data;

namespace ContactPro.Services
{
    public class AddressBookService : IAddressBookService
    {
        private readonly ApplicationDbContext _context;

        public AddressBookService(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task AddContactToCategoryAsyn(int categoryId, int contactId)
        {
            try
            {
                //check to see if the catecgory has already been added
                if(!await IsContactInCategory(categoryId, contactId))
                {
                    //add the category to the database
                    Contact? contact = await _context.Contact!.FindAsync(contactId);
                    Category? category = await _context.Category!.FindAsync(categoryId);

                    if(contact != null && category != null){
                        category.Contacts.Add(contact);
                        await _context.SaveChangesAsync();
                    }
                }
            }
            catch
            {
                throw;
            }
        }

        public async Task<IEnumerable<Category>> GetContactCategoriesAsync(int contactId)
        {
            try
            {
                Contact? contact = await _context.Contact!.Include(c=>c.Categories)
                                                         .FirstOrDefaultAsync(c=>c.Id == contactId);
                return contact!.Categories;
            }
            catch
            {
                throw;
            }
        }

        public async Task<IEnumerable<int>> GetContactCategoryIdsAsync(int contactId)
        {
            try
            {
                Contact? contact = await _context.Contact!.Include(c=>c.Categories)
                                                        .FirstOrDefaultAsync(c=>c.Id == contactId);

                List<int> categoryIds = contact!.Categories.Select(c => c.Id).ToList();
                return categoryIds;
            }
            catch
            {
                throw;
            }
        }

        public async Task<IEnumerable<Category>> GetUserCategoriesAsync(string appUserId)
        {
            List<Category> categories = new List<Category>();

            try
            {
                categories = await _context.Category!.Where(c => c.AppUserId == appUserId)
                                                    .OrderBy(c=>c.Name)
                                                    .ToListAsync();
            }
            catch
            {
                throw;
            }
            return categories;
        }


        public async Task<bool> IsContactInCategory(int categoryId, int contactId)
        {
            try
            {
                Contact? contact = await _context.Contact!.FindAsync(contactId);

                return await _context.Category!
                                     .Include(c=>c.Contacts)
                                     .Where(c=>c.Id == categoryId && c.Contacts.Contains(contact!))
                                     .AnyAsync();
            }
            catch
            {
                throw;
            }
        }

        public async Task RemoveContactFromCategoryAsyn(int categoryId, int contactId)
        {
            Contact? contact = await _context.Contact!.FindAsync(contactId);
            Category? category = await _context.Category!.FindAsync(categoryId);
            
            if(category != null && contact != null)
            {
                category.Contacts.Remove(contact);
                await _context.SaveChangesAsync();
            }
        }
    }
}
