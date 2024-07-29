using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using react_chat_app_backend.Context;
using react_chat_app_backend.Models;

namespace react_chat_app_backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly ILogger<UserController> _logger;
    private readonly AppDbContext _appDbContext;

    public UserController(ILogger<UserController> logger, AppDbContext appDbContext)
    {
        _logger = logger;
        _appDbContext = appDbContext;
    }
    
    public async Task<bool> CheckUserExists(string userId)
    {
        return await _appDbContext.Users.AnyAsync(
            ur => ur.userId == userId
        );
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateUser(UserData userData)
    {
        var userId = userData.userId;
        
        if (await CheckUserExists(userId)) {
            return Conflict("There is already another user with this userID.");
        }

        await _appDbContext.Users.AddAsync(userData);
        await _appDbContext.SaveChangesAsync();

        return CreatedAtAction(nameof(CreateUser), userData);
    }
    
    [HttpDelete("{userId}")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> DeleteUser(string userId)
    {
        if (await CheckUserExists(userId))
        {
            var user = await _appDbContext.Users.FirstOrDefaultAsync(ur => ur.userId == userId);
            _appDbContext.Remove(user);
            _appDbContext.SaveChangesAsync();
            
            return Ok();
        }

        return NotFound();
    }
    
}