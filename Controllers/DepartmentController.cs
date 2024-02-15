using API.Controllers;
using API.Models;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using System.Data;

namespace API.Controllers
{
    public class DepartmentController : BaseApiController
    {
        private readonly IConfiguration _configuration;
        public DepartmentController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet]
        public JsonResult Get()
        {
            string query = @"SELECT * FROM department";
            DataTable dt = new DataTable();
            string mySqlDataSource = _configuration.GetConnectionString("DBConnection");
            MySqlDataReader mySqlDataReader;
            using(MySqlConnection con = new MySqlConnection(mySqlDataSource))
            {
                con.Open();
                using(MySqlCommand cmd = new MySqlCommand(query, con))
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
        public JsonResult Post(Department dept)
        {
            string query = @"INSERT INTO Department (DepartmentName) VALUES (@DepartmentName)";
            DataTable dt = new DataTable();
            string mySqlDataSource = _configuration.GetConnectionString("DBConnection");
            MySqlDataReader mySqlDataReader;
            using (MySqlConnection con = new MySqlConnection(mySqlDataSource))
            {
                con.Open();
                using (MySqlCommand cmd = new MySqlCommand(query, con))
                {

                    cmd.Parameters.AddWithValue("@DepartmentName", dept.DepartmentName);
                    mySqlDataReader = cmd.ExecuteReader();
                    dt.Load(mySqlDataReader);
                    mySqlDataReader.Close();
                    con.Close();
                }
            }

            return new JsonResult("Added Successfully");
        }

        [HttpPut]
        public JsonResult Put(Department dept)
        {
            string query = @"UPDATE Department SET DepartmentName = @DepartmentName WHERE DepartmentId = @DepartmentId";
            DataTable dt = new DataTable();
            string mySqlDataSource = _configuration.GetConnectionString("DBConnection");
            MySqlDataReader mySqlDataReader;
            using (MySqlConnection con = new MySqlConnection(mySqlDataSource))
            {
                con.Open();
                using (MySqlCommand cmd = new MySqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@DepartmentId", dept.DepartmentId);
                    cmd.Parameters.AddWithValue("@DepartmentName", dept.DepartmentName);
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
            string query = @"DELETE FROM Department WHERE DepartmentId = @DepartmentId";
            DataTable dt = new DataTable();
            string mySqlDataSource = _configuration.GetConnectionString("DBConnection");
            MySqlDataReader mySqlDataReader;
            using (MySqlConnection con = new MySqlConnection(mySqlDataSource))
            {
                con.Open();
                using (MySqlCommand cmd = new MySqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@DepartmentId", id);
                    mySqlDataReader = cmd.ExecuteReader();
                    dt.Load(mySqlDataReader);
                    mySqlDataReader.Close();
                    con.Close();
                }
            }

            return new JsonResult("Deleted Successfully");
        }
    }
}
