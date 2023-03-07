using ContactPro.Models;

namespace ContactPro.Services.Interfaces
{
    public interface IAddressBookService
    {
        Task<IEnumerable<Category>> GetUserCategoriesAsync(string appUserId);
        Task AddContactToCategoryAsyn(int categoryId, int contactId);
        Task<IEnumerable<Category>> GetContactCategoriesAsync(int contactId);
        Task RemoveContactFromCategoryAsyn(int categoryId, int contactId);
        Task<IEnumerable<int>> GetContactCategoryIdsAsync(int contactId); 
    }
}
