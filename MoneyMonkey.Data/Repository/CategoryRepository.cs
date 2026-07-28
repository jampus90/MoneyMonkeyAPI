using Microsoft.EntityFrameworkCore;
using MoneyMonkey.Communication.Request;
using MoneyMonkey.Communication.Response;
using MoneyMonkey.Data.Entities;

namespace MoneyMonkey.Data.Repository;
public class CategoryRepository
{
    private readonly MoneyMonkeyDbContext _context;

    public CategoryRepository(MoneyMonkeyDbContext context)
    {
        _context = context;
    }

    public async Task<CategoryResponseList> GetAllCategories(long userId)
    {
        var categories = await _context.Categories
            .Where(c => c.UserId == userId)
            .Select(c => new CategoryResponse
            {
                CategoryId = c.CategoryId,
                Name = c.Name,
                Type = c.Type
            })
            .ToListAsync();

        return new CategoryResponseList { CategoryResponses = categories };
    }

    public async Task<CategoryResponse> CreateCategory(long userId, CategoryRequest request)
    {
        var category = new Category
        {
            UserId = userId,
            Name = request.Name,
            Type = request.Type
        };

        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        return new CategoryResponse
        {
            CategoryId = category.CategoryId,
            Name = category.Name,
            Type = category.Type
        };
    }
}
