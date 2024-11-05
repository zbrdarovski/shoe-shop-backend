// UserController.cs
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

[ApiController]
[Route("api/users")]
public class UserController : ControllerBase
{
    private readonly MongoDbContext _dbContext;

    public UserController(MongoDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpPost("register")]
    public IActionResult Register([FromBody] UserRegistrationDto userDto)
    {
        if (string.IsNullOrWhiteSpace(userDto?.Username) || string.IsNullOrWhiteSpace(userDto?.Password))
        {
            return BadRequest(new { Message = "Username and password are required" });
        }

        var existingUser = _dbContext.Users.Find(u => u.Username == userDto.Username).FirstOrDefault();
        if (existingUser != null)
        {
            return Conflict(new { Message = "Username already exists" });
        }

        // Hash the password before storing
        var hashedPassword = HashPassword(userDto.Password);

        var user = new User { Username = userDto.Username, Password = hashedPassword };

        _dbContext.Users.InsertOne(user);

        return Ok(new { Message = "Registration successful" });
    }

    [HttpPost("login")]
    public IActionResult Login([FromBody] UserLoginDto userDto)
    {
        var user = _dbContext.Users.Find(u => u.Username == userDto.Username).FirstOrDefault();
        if (user == null || userDto == null || !VerifyPassword(userDto.Password, user.Password))
        {
            return Unauthorized(new { Message = "Invalid credentials" });
        }

        // Perform any additional login logic if needed

        return Ok(new { Message = "Login successful" });
    }

    [HttpPost("change-password")]
    public IActionResult ChangePassword([FromBody] ChangePasswordDto changePasswordDto)
    {
        var user = _dbContext.Users.Find(u => u.Username == changePasswordDto.Username).FirstOrDefault();
        if (user == null || changePasswordDto == null || !VerifyPassword(changePasswordDto.CurrentPassword, user.Password))
        {
            return Unauthorized(new { Message = "Invalid credentials" });
        }

        // Hash the new password before updating
        var hashedNewPassword = HashPassword(changePasswordDto.NewPassword);
        user.Password = hashedNewPassword;

        // Use the correct identifier property for ReplaceOne
        _dbContext.Users.ReplaceOne(u => u.Id == user.Id, user);

        return Ok(new { Message = "Password changed successfully" });
    }

    [HttpGet]
    public IActionResult GetAllUsers()
    {
        var users = _dbContext.Users.Find(_ => true).ToList();
        return Ok(users);
    }

    [HttpGet("{id}")]
    public IActionResult GetUserById(string id)
    {
        var user = _dbContext.Users.Find(u => u.Id == id).FirstOrDefault();
        if (user == null)
        {
            return NotFound(new { Message = "User not found" });
        }

        return Ok(user);
    }

    [HttpPut("{id}")]
    public IActionResult UpdateUser(string id, [FromBody] UserUpdateDto userDto)
    {
        var user = _dbContext.Users.Find(u => u.Id == id).FirstOrDefault();
        if (user == null)
        {
            return NotFound(new { Message = "User not found" });
        }

        // Update user properties based on the UserUpdateDto
        user.Username = userDto.Username;
        // You might want to handle password updates separately

        _dbContext.Users.ReplaceOne(u => u.Id == id, user);

        return Ok(new { Message = "User updated successfully" });
    }

    [HttpDelete("{id}")]
    public IActionResult DeleteUser(string id)
    {
        var user = _dbContext.Users.Find(u => u.Id == id).FirstOrDefault();
        if (user == null)
        {
            return NotFound(new { Message = "User not found" });
        }

        _dbContext.Users.DeleteOne(u => u.Id == id);

        return Ok(new { Message = "User deleted successfully" });
    }

    [HttpGet("username/{username}")]
    public IActionResult GetUserByUsername(string username)
    {
        var user = _dbContext.Users.Find(u => u.Username == username).FirstOrDefault();
        if (user == null)
        {
            return NotFound(new { Message = "User not found" });
        }

        return Ok(user);
    }

    private string HashPassword(string? password)
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            throw new ArgumentNullException(nameof(password));
        }

        return BCrypt.Net.BCrypt.HashPassword(password, BCrypt.Net.BCrypt.GenerateSalt());
    }

    private bool VerifyPassword(string? inputPassword, string? hashedPassword)
    {
        if (string.IsNullOrWhiteSpace(inputPassword) || string.IsNullOrWhiteSpace(hashedPassword))
        {
            return false;
        }

        return BCrypt.Net.BCrypt.Verify(inputPassword, hashedPassword);
    }
}