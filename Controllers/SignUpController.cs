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
    public class SignUpController : ControllerBase
    {
        // POST api/<SignUpController>
        [HttpPost]
        public string Post(string username, string password, string passConfirm)
        {
            user userData = new();
            userData.username = username;
            userData.password = password;
            userData.passwordConf = passConfirm;
            userData.userID = "";
            userData.passHash = "";
            userData.salt = "";

            if (userData.username.Length > 64)
            {
                return "Username too long";
            }
            else if (userData.username == "")
            {
                return "Username cannot be blank";
            }
            else if (userData.password != passConfirm)
            {
                return "Password and confirmation must match";
            }
            else
            {
                SqlCommand testUser = new SqlCommand($"SELECT * FROM Users WHERE Username = @userName;", SQL.conn);
                testUser.Parameters.Add(new SqlParameter("@UserName", userData.username));
                SQL.conn.Open();
                object testUserResult = testUser.ExecuteScalar();
                SQL.conn.Close();
                if (testUserResult != null)
                {
                    return "Username already in use";
                }
                else
                {
                    userData.salt = SQL.Random(16, false);
                    userData.passHash = SQL.Sha256(userData.password + userData.salt);

                    bool idUnique = true;
                    while (idUnique == true)
                    {
                        userData.userID = SQL.Random(12, true);
                        SqlCommand testID = new SqlCommand($"SELECT UserID FROM Users WHERE UserID = @UserID;", SQL.conn);
                        testID.Parameters.Add(new SqlParameter("@UserID", userData.userID));
                        SQL.conn.Open();
                        if (testID.ExecuteScalar() == null)
                        {
                            idUnique = false;
                            SQL.conn.Close();
                        }
                    }
                    SqlCommand newUser = new SqlCommand($"INSERT INTO Users VALUES(@UserID, @UserName, @PassHash, @Salt);", SQL.conn);
                    newUser.Parameters.Add(new SqlParameter("@UserID", userData.userID));
                    newUser.Parameters.Add(new SqlParameter("@UserName", userData.username));
                    newUser.Parameters.Add(new SqlParameter("@PassHash", userData.passHash));
                    newUser.Parameters.Add(new SqlParameter("@Salt", userData.salt));

                    SQL.conn.Open();
                    newUser.ExecuteNonQuery();
                    SQL.conn.Close();
                    return $"User {username} created";
                }
            }
        }
    }
}
