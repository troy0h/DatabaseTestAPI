using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

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
            user userData = new();
            userData.username = username;
            userData.password = password;
            userData.userID = "";
            userData.passHash = "";
            userData.salt = "";

            if (username == "")
            {
                return "Username must be entered";
            }
            else if (password == "")
            {
                return "Password must be entered";
            }
            else
            {
                using SqlCommand findUser = new SqlCommand($"SELECT * FROM Users WHERE Username = @UserName;", SQL.conn);
                findUser.Parameters.Add(new SqlParameter("@UserName", userData.username));
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
                                userData.userID = reader.GetValue(i).ToString();
                                break;

                            case 1:
                                break;

                            case 2:
                                userData.passHash = reader.GetValue(i).ToString();
                                break;

                            case 3:
                                userData.salt = reader.GetValue(i).ToString();
                                break;
                        }
                    }
                }
                SQL.conn.Close();

                if (userData.userID == "")
                {
                    return "User does not exist";
                }
                else
                {
                    password = SQL.Sha256(password + userData.salt);
                    if (password != userData.passHash)
                    {
                        return "Password incorrect";
                    }
                    else
                    {
                        return $"User {username} successfully logged in";
                    }
                }
            }
        }
    }
}
