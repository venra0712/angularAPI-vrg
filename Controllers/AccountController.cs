using API.Models;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using System.Data;
using Microsoft.AspNetCore.Http;
using System.Text.RegularExpressions;
using System.Text;
using API.Helpers;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;

namespace API.Controllers
{
    public class AccountController : BaseApiController  
    {
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _env;
        public AccountController(IConfiguration configuration, IWebHostEnvironment env)
        {
            _configuration = configuration;
            _env = env;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            string query = @"SELECT * FROM UserAccount";
            DataTable dt = new DataTable();
            string mySqlDataSource = _configuration.GetConnectionString("DBConnection");
            MySqlDataReader mySqlDataReader;
            await using (MySqlConnection con = new MySqlConnection(mySqlDataSource))
            {
                con.Open();
                await using (MySqlCommand cmd = new MySqlCommand(query, con))
                {
                    mySqlDataReader = cmd.ExecuteReader();
                    dt.Load(mySqlDataReader);
                    mySqlDataReader.Close();
                    con.Close();
                }
            }

            return new JsonResult(dt);
        }
        
        [Route("register")]
        [HttpPost]
        public async Task<IActionResult> Post(Account acc)
        {
            if (acc == null)
                return new JsonResult("Bad Request");
            if (UsernameExists(acc.Username))
            {
                return new JsonResult("Username already existed!");
            }
            if (!AccountIdExists(acc.AccountId))
            {

                var pass = CheckPasswordStrength(acc.Password);
                if (!string.IsNullOrEmpty(pass))
                    return new JsonResult(pass.ToString());

                acc.Password = PasswordHasher.HashPassword(acc.Password);

                string query = @"INSERT INTO UserAccount (AccountId, EmployeeName, Username, Password, Token, Role, DateCreated) VALUES 
                           (@AccountId, @EmployeeName, @Username, @Password, @Token, @Role, @DateCreated)";
                DataTable dt = new DataTable();
                string mySqlDataSource = _configuration.GetConnectionString("DBConnection");
                MySqlDataReader mySqlDataReader;
                await using (MySqlConnection con = new MySqlConnection(mySqlDataSource))
                {
                    con.Open();
                    using (MySqlCommand cmd = new MySqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@AccountId", acc.AccountId = Guid.NewGuid());
                        cmd.Parameters.AddWithValue("@EmployeeName", acc.EmployeeName);
                        cmd.Parameters.AddWithValue("@Username", acc.Username);
                        cmd.Parameters.AddWithValue("@Password", acc.Password);
                        cmd.Parameters.AddWithValue("@Token", acc.Token);
                        cmd.Parameters.AddWithValue("@Role", acc.Role);
                        cmd.Parameters.AddWithValue("@DateCreated", acc.DateCreated);
                        mySqlDataReader = cmd.ExecuteReader();
                        dt.Load(mySqlDataReader);
                        mySqlDataReader.Close();
                        con.Close();
                    }
                }

            }

            return new JsonResult("Added Successfully"); ;
        }
        [Route("changepass")]
        [HttpPut]
        public async Task<IActionResult> Put(Account acc)
        {
            if (acc == null)
                return new JsonResult("Bad Request");
            var pass = CheckPasswordStrength(acc.Password);
            if (!string.IsNullOrEmpty(pass))
                return new JsonResult(pass.ToString());

            acc.Password = PasswordHasher.HashPassword(acc.Password);

            string query = @"UPDATE UserAccount SET Password = @Password WHERE AccountId = @AccountId";
            string mySqlDataSource = _configuration.GetConnectionString("DBConnection");
            MySqlDataReader mySqlDataReader;
            await using (MySqlConnection con = new MySqlConnection(mySqlDataSource))
            {
                con.Open();
                using (MySqlCommand cmd = new MySqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@AccountId", acc.AccountId);
                    cmd.Parameters.AddWithValue("@Password", acc.Password);
                    int rowsAffected = await cmd.ExecuteNonQueryAsync();
                    if (rowsAffected > 0)
                    {
                        return new JsonResult("Updated Successfully");
                    }
                    else
                    {
                        return new JsonResult("Failed to change password");
                    }
                }
            }
        }
        [Route("authenticate")]
        [HttpPost]
        public async Task<IActionResult> Authenticate(Account userObj)
        {

            if (!PasswordHasher.VerifyPassword(userObj.Password, GetPassword(userObj.Username)))
            {
                return BadRequest(new { Message = "Password is not correct!" });
            }
            var user = GetUserByUsername(userObj.Username);
            var token = GenerateJwtToken(user, _configuration);
            return Ok(new
            {
                Message = "Login Successfully!",
                Token = token
            });
        }
        private string GenerateJwtToken(Account account)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.Name, account.Username),
                new Claim(ClaimTypes.Role, account.Role), // Assuming user.Role contains the role information
                // Add more claims as needed
            };

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Secret"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        public bool UsernameExists(string username)
        {
            string query = @"SELECT COUNT(*) FROM UserAccount WHERE Username = @Username";
            string mySqlDataSource = _configuration.GetConnectionString("DBConnection");
            using (var con = new MySqlConnection(mySqlDataSource))
            {
                con.Open();

                using (var command = new MySqlCommand(query,con))
                {
                    command.Parameters.AddWithValue("@Username", username);
                    var result = command.ExecuteScalar();

                    
                    return Convert.ToInt32(result) > 0;
                }
            }
        }
        public bool AccountIdExists(Guid accountid)
        {
            string query = @"SELECT COUNT(*) FROM UserAccount WHERE accountid = @accountid";
            DataTable dt = new DataTable();
            string mySqlDataSource = _configuration.GetConnectionString("DBConnection");
            using (var con = new MySqlConnection(mySqlDataSource))
            {
                con.Open();

                using (var command = new MySqlCommand(query, con))
                {
                    command.Parameters.AddWithValue("@accountid", accountid);
                    var result = command.ExecuteScalar();


                    return Convert.ToInt32(result) > 0;
                }
            }
        }
        private string CheckPasswordStrength(string password)
        {
            StringBuilder _stringBuilder = new StringBuilder();
            if (password.Length < 8)
                _stringBuilder.Append("Password must not be lesser than 8" + Environment.NewLine);
            if (!(Regex.IsMatch(password, "[a-z]") && Regex.IsMatch(password, "[A-Z]")
                && Regex.IsMatch(password, "[0-9]")))
                _stringBuilder.Append("Password should be Alphanumeric" + Environment.NewLine);
            if (!(Regex.IsMatch(password, "[!,@,#,$,%,^,&,*,(,),_,-,+,=,{,\\[,{,\\],|,\\,:,;,\",',<,>,?,/,.]")))
                _stringBuilder.Append("Password must contain a special character" + Environment.NewLine);
            return _stringBuilder.ToString();

        }

        public string GetPassword(string username)
        {
            string password = null;
            string query = @"SELECT password FROM UserAccount WHERE Username = @Username";
            DataTable dt = new DataTable();
            string mySqlDataSource = _configuration.GetConnectionString("DBConnection");
            using (MySqlConnection con = new MySqlConnection(mySqlDataSource))
            {
                con.Open();
                using (MySqlCommand cmd = new MySqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@Username", username);
                    using (MySqlDataReader mySqlDataReader = cmd.ExecuteReader())
                    {
                        if (mySqlDataReader.Read())
                        {
                            password = mySqlDataReader.GetString("password");
                        }
                    }
                }
            }
            return password;
        }
        public Account GetUserByUsername(string username)
        {
            Account user = null;
            string query = @"SELECT * FROM UserAccount WHERE Username = @Username";
            string mySqlDataSource = _configuration.GetConnectionString("DBConnection");
            using (MySqlConnection con = new MySqlConnection(mySqlDataSource))
            {
                con.Open();
                using (MySqlCommand cmd = new MySqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@Username", username);
                    using (MySqlDataReader mySqlDataReader = cmd.ExecuteReader())
                    {
                        if (mySqlDataReader.Read())
                        {
                            // Retrieve user details from the database
                            user = new Account
                            {
                                // Assign the values from the database to the user object
                                AccountId = mySqlDataReader.GetGuid("AccountId"),
                                EmployeeName = mySqlDataReader.GetString("EmployeeName"),
                                Username = mySqlDataReader.GetString("Username"),
                                Password = mySqlDataReader.GetString("Password"),
                                Token = mySqlDataReader.GetString("Token"),
                                Role = mySqlDataReader.GetString("Role"),
                                DateCreated = mySqlDataReader.GetDateTime("DateCreated")
                                // You may add other properties here based on your database schema
                            };
                        }
                    }
                }
            }
            return user;
        }

    }
}
