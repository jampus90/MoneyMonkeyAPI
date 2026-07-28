using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MoneyMonkey.Communication.Request;
using MoneyMonkey.Communication.Response;
using MoneyMonkey.Data.Entities;

namespace MoneyMonkey.Data.Repository;
public class UserRepository
{
    private readonly MoneyMonkeyDbContext _context;
    private readonly IPasswordHasher<User> _passwordHasher;

    public UserRepository(MoneyMonkeyDbContext context, IPasswordHasher<User> passwordHasher)
    {
        _context = context;
        _passwordHasher = passwordHasher;
    }

    public async Task<UserResponseList> GetAllUsers()
    {
        var users = await _context.Users
            .Select(u => new UserResponse
            {
                UserId = u.UserId,
                FirstName = u.FirstName,
                LastName = u.LastName,
                UserType = u.Type
            })
            .ToListAsync();

        return new UserResponseList { UserResponses = users };
    }

    public async Task<UserResponse> CreateUser(UserRequest request)
    {
        var user = new User
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Type = request.UserType
        };

        await using var transaction = await _context.Database.BeginTransactionAsync();

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var credential = new Credential
        {
            UserId = user.UserId,
            Username = request.Username,
            Password = _passwordHasher.HashPassword(user, request.Password)
        };

        _context.Credentials.Add(credential);
        await _context.SaveChangesAsync();

        await transaction.CommitAsync();

        return new UserResponse
        {
            UserId = user.UserId,
            FirstName = user.FirstName,
            LastName = user.LastName,
            UserType = user.Type
        };
    }

    public async Task<User?> Authenticate(string username, string password)
    {
        var credential = await _context.Credentials.FirstOrDefaultAsync(c => c.Username == username);
        if (credential is null)
        {
            return null;
        }

        var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == credential.UserId);
        if (user is null)
        {
            return null;
        }

        var result = _passwordHasher.VerifyHashedPassword(user, credential.Password, password);
        if (result == PasswordVerificationResult.Failed)
        {
            return null;
        }

        return user;
    }
}
