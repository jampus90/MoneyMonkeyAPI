using MoneyMonkey.Communication.Request;
using MoneyMonkey.Communication.Response;
using MoneyMonkey.Data.Repository;

namespace MoneyMonkey.Application.Services;
public class CategoryService
{
    private readonly CategoryRepository _categoryRepository;

    public CategoryService(CategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    public async Task<CategoryResponseList> GetAllCategories(long userId)
    {
        return await _categoryRepository.GetAllCategories(userId);
    }

    public async Task<CategoryResponse> CreateCategory(long userId, CategoryRequest request)
    {
        return await _categoryRepository.CreateCategory(userId, request);
    }
}
