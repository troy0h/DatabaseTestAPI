using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;

namespace DatabaseTestAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SignInController : ControllerBase
    {
        // GET api/<SignInController>/5
        [HttpGet]
        public string Get(string username, string password)
        {
            User userData = new();
            userData.Username = username;
            userData.Password = password;
            userData.UserID = "";
            userData.PassHash = "";
            userData.Salt = "";

            if (username == "")
            {
                Response.StatusCode = 400;
                return "Username must be entered";
            }
            else if (password == "")
            {
                Response.StatusCode = 400;
                return "Password must be entered";
            }
            else
            {
                using SqlCommand findUser = new SqlCommand($"SELECT * FROM Users WHERE Username = @UserName;", SQL.conn);
                findUser.Parameters.Add(new SqlParameter("@UserName", userData.Username));
                SQL.conn.Open();
                using SqlDataReader reader = findUser.ExecuteReader();
                int count = reader.FieldCount;
                while (reader.Read())
                {
                    for (int i = 0; i < count; i++)
                    {
                        switch (i)
                        {
                            case 0:
                                userData.UserID = reader.GetValue(i).ToString();
                                break;

                            case 1:
                                break;

                            case 2:
                                userData.PassHash = reader.GetValue(i).ToString();
                                break;

                            case 3:
                                userData.Salt = reader.GetValue(i).ToString();
                                break;
                        }
                    }
                }
                SQL.conn.Close();

                if (userData.UserID == "")
                {
                    Response.StatusCode = 400;
                    return "User does not exist";
                }
                else
                {
                    password = SQL.Sha256(password + userData.Salt);
                    if (password != userData.PassHash)
                    {
                        Response.StatusCode = 400;
                        return "Password incorrect";
                    }
                    else
                    {
                        Response.StatusCode = 200;
                        return $"User {username} successfully logged in";
                    }
                }
            }
        }
    }
}
