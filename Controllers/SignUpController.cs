using System.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;

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
            string connectionString = ConfigurationManager.ConnectionStrings["DB"].ConnectionString;
            SqlConnection conn = new(connectionString);

            User userData = new();
            userData.Username = username;
            userData.Password = password;
            userData.PasswordConf = passConfirm;
            userData.UserID = "";
            userData.PassHash = "";
            userData.Salt = "";

            if (userData.Username.Length > 64)
            {
                Response.StatusCode = 400;
                return "Username is too long";
            }
            else if (userData.Username == "")
            {
                Response.StatusCode = 400;
                return "Username cannot be blank";
            }
            else if (userData.Password != passConfirm)
            {
                Response.StatusCode = 400;
                return "Password and confirmation must match";
            }
            else
            {
                SqlCommand testUser = new SqlCommand($"SELECT * FROM Users WHERE Username = @userName;", conn);
                testUser.Parameters.Add(new SqlParameter("@UserName", userData.Username));
                conn.Open();
                object testUserResult = testUser.ExecuteScalar();
                conn.Close();
                if (testUserResult != null)
                {
                    Response.StatusCode = 400;
                    return "Username already in use";
                }
                else
                {
                    userData.Salt = SQL.Random(16, false);
                    userData.PassHash = SQL.Sha256(userData.Password + userData.Salt);

                    bool idUnique = true;
                    while (idUnique == true)
                    {
                        userData.UserID = SQL.Random(12, true);
                        SqlCommand testID = new SqlCommand($"SELECT UserID FROM Users WHERE UserID = @UserID;", conn);
                        testID.Parameters.Add(new SqlParameter("@UserID", userData.UserID));
                        conn.Open();
                        if (testID.ExecuteScalar() == null)
                        {
                            idUnique = false;
                            conn.Close();
                        }
                    }
                    SqlCommand newUser = new SqlCommand($"INSERT INTO Users VALUES(@UserID, @UserName, @PassHash, @Salt);", conn);
                    newUser.Parameters.Add(new SqlParameter("@UserID", userData.UserID));
                    newUser.Parameters.Add(new SqlParameter("@UserName", userData.Username));
                    newUser.Parameters.Add(new SqlParameter("@PassHash", userData.PassHash));
                    newUser.Parameters.Add(new SqlParameter("@Salt", userData.Salt));

                    conn.Open();
                    newUser.ExecuteNonQuery();
                    conn.Close();

                    Response.StatusCode = 200;
                    return $"User {username} created";
                }
            }
        }
    }
}
