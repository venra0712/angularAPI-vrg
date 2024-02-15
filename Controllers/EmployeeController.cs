using API.Controllers;
using API.Models;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using System.Data;

namespace API.Controllers
{ 
    public class EmployeeController : BaseApiController
    {
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _env;
        public EmployeeController(IConfiguration configuration, IWebHostEnvironment env)
        {
            _configuration = configuration;
            _env = env;
        }

        [HttpGet]
        public JsonResult Get()
        {
            string query = @"SELECT * FROM Employeerjs";
            DataTable dt = new DataTable();
            string mySqlDataSource = _configuration.GetConnectionString("DBConnection");
            MySqlDataReader mySqlDataReader;
            using (MySqlConnection con = new MySqlConnection(mySqlDataSource))
            {
                con.Open();
                using (MySqlCommand cmd = new MySqlCommand(query, con))
                {
                    mySqlDataReader = cmd.ExecuteReader();
                    dt.Load(mySqlDataReader);
                    mySqlDataReader.Close();
                    con.Close();
                }
            }

            return new JsonResult(dt);
        }
        [HttpPost]
        public JsonResult Post(Employee emp)
        {
            string query = @"INSERT INTO Employeerjs (EmployeeName, Department, DateOfJoining, PhotoFileName) VALUES 
                           (@EmployeeName, @Department, @DateOfJoining, @PhotoFileName)";
            DataTable dt = new DataTable();
            string mySqlDataSource = _configuration.GetConnectionString("DBConnection");
            MySqlDataReader mySqlDataReader;
            using (MySqlConnection con = new MySqlConnection(mySqlDataSource))
            {
                con.Open();
                using (MySqlCommand cmd = new MySqlCommand(query, con))
                {

                    cmd.Parameters.AddWithValue("@EmployeeName", emp.EmployeeName);
                    cmd.Parameters.AddWithValue("@Department", emp.Department);
                    cmd.Parameters.AddWithValue("@DateOfJoining", emp.DateOfJoining);
                    cmd.Parameters.AddWithValue("@PhotoFileName", emp.PhotoFileName);
                    mySqlDataReader = cmd.ExecuteReader();
                    dt.Load(mySqlDataReader);
                    mySqlDataReader.Close();
                    con.Close();
                }
            }

            return new JsonResult("Added Successfully");
        }

        [HttpPut]
        public JsonResult Put(Employee emp)
        {
            string query = @"UPDATE Employeerjs SET EmployeeName = @EmployeeName, Department = @Department, 
            DateOfJoining = @DateOfJoining, PhotoFileName = @PhotoFileName WHERE EmployeeId = @EmployeeId";
            DataTable dt = new DataTable();
            string mySqlDataSource = _configuration.GetConnectionString("DBConnection");
            MySqlDataReader mySqlDataReader;
            using (MySqlConnection con = new MySqlConnection(mySqlDataSource))
            {
                con.Open();
                using (MySqlCommand cmd = new MySqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@EmployeeId", emp.EmployeeId);
                    cmd.Parameters.AddWithValue("@EmployeeName", emp.EmployeeName);
                    cmd.Parameters.AddWithValue("@Department", emp.Department);
                    cmd.Parameters.AddWithValue("@DateOfJoining", emp.DateOfJoining);
                    cmd.Parameters.AddWithValue("@PhotoFileName", emp.PhotoFileName);
                    mySqlDataReader = cmd.ExecuteReader();
                    dt.Load(mySqlDataReader);
                    mySqlDataReader.Close();
                    con.Close();
                }
            }

            return new JsonResult("Updated Successfully");
        }

        [HttpDelete("{id}")]
        public JsonResult Delete(int id)
        {
            string query = @"DELETE FROM Employeerjs WHERE EmployeeId = @EmployeeId";
            DataTable dt = new DataTable();
            string mySqlDataSource = _configuration.GetConnectionString("DBConnection");
            MySqlDataReader mySqlDataReader;
            using (MySqlConnection con = new MySqlConnection(mySqlDataSource))
            {
                con.Open();
                using (MySqlCommand cmd = new MySqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@EmployeeId", id);
                    mySqlDataReader = cmd.ExecuteReader();
                    dt.Load(mySqlDataReader);
                    mySqlDataReader.Close();
                    con.Close();
                }
            }

            return new JsonResult("Deleted Successfully");
        }

        [Route("SaveFile")]
        [HttpPost]

        public JsonResult SaveFile()
        {
            try
            {
                var httpRequest = Request.Form;
                var postedFile = httpRequest.Files[0];
                string filename = postedFile.FileName;
                var physicalPath = _env.ContentRootPath + "/Images/" + filename;

                using(var stream = new FileStream(physicalPath, FileMode.Create))
                {
                    postedFile.CopyTo(stream);
                }

                return new JsonResult(filename);
            }
            catch (Exception)
            {
                return new JsonResult("Anonymous.png");
            }
        }

    }
}
