using Microsoft.EntityFrameworkCore;
using MySite.Application.Interfaces;
using MySite.DataAccess.Entities;
using MySite.Domain.Models;

namespace MySite.DataAccess.Persistence.Repositories;
public class UsersRepository : IUsersRepository
{
    private readonly MySiteDbContext _context;

    public UsersRepository(MySiteDbContext context)
    {
        _context = context;
    }

    public async Task Add(User user)
    {
        var userEntity = new UserEntity()
        {
            Id = user.Id,
            Login = user.Login,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            ProfileImage = user.ProfileImage,
            Created = user.Created,
            Modified = user.Modified,
            PasswordHash = user.PasswordHash,
            Right = user.Right
        };

        await _context.AddAsync(userEntity);
        await _context.SaveChangesAsync();
    }

    public async Task<User> GetByEmailAsync(string email)
    {
        var userEntity = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Email == email) 
                         ?? throw new Exception($"Пользователь с email: {email} не найден.");

        return User.Create(userEntity.Id,
            userEntity.Created,
            userEntity.Modified,
            userEntity.Login,
            userEntity.FirstName,
            userEntity.LastName,
            userEntity.Email,
            userEntity.PasswordHash,
            userEntity.Right,
            userEntity.ProfileImage).user;
    }
}
