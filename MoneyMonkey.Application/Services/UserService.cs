using MoneyMonkey.Communication.Request;
using MoneyMonkey.Communication.Response;
using MoneyMonkey.Data.Repository;

namespace MoneyMonkey.Application.Services;
public class UserService
{
    private readonly UserRepository _userRepository;
    private readonly ITokenService _tokenService;

    public UserService(UserRepository userRepository, ITokenService tokenService)
    {
        _userRepository = userRepository;
        _tokenService = tokenService;
    }

    public async Task<UserResponseList?> GetAllUsers()
    {
        return await _userRepository.GetAllUsers();
    }

    public async Task<UserResponse> CreateUser(UserRequest request)
    {
        return await _userRepository.CreateUser(request);
    }

    public async Task<LoginResponse?> Login(LoginRequest request)
    {
        var user = await _userRepository.Authenticate(request.Username, request.Password);
        if (user is null)
        {
            return null;
        }

        var (token, expiresAt) = _tokenService.GenerateToken(user);

        return new LoginResponse
        {
            Token = token,
            ExpiresAt = expiresAt,
            UserId = user.UserId,
            FirstName = user.FirstName,
            LastName = user.LastName,
            UserType = user.Type
        };
    }
}
