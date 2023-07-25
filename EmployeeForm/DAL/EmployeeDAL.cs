using EmployeeForm.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Net;
using System.Reflection;

namespace EmployeeForm.DAL
{
    public class EmployeeDAL
    {
        SqlConnection _connection = null;
        SqlCommand _command = null;
        public static IConfiguration configuration { get; set; }

        private string GetConnectionString()
        {
            var builder = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json");

            configuration = builder.Build();
            return configuration.GetConnectionString("DefaultConnection");
        }

        public List<CasteCategory> GetCategoryList()
        {
            List<CasteCategory> categoryList = new List<CasteCategory>();

            using (_connection = new SqlConnection(GetConnectionString()))
            {
                string query = "SELECT Id, [Name] FROM CasteCategory";
                SqlCommand command = new SqlCommand(query, _connection);

                _connection.Open();

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        CasteCategory category = new CasteCategory
                        {
                            Id = reader.GetInt32(0),
                            Name = reader.GetString(1)
                        };

                        categoryList.Add(category);
                    }
                }
            }

            return categoryList;
        }

        public List<Country> DistinctCountries(int id)
        {
            List<Country> countries = new List<Country>();

            using (_connection = new SqlConnection(GetConnectionString()))
            {
                _connection.Open();

                using (SqlCommand command = new SqlCommand("SELECT DISTINCT c.Id, c.[Name] " +
                                                            "FROM Country c " +
                                                            "JOIN EmployeeAddressDetail a ON c.Id = a.CountryId " +
                                                            "WHERE a.EmployeeId = @EmployeeId", _connection))
                {
                    command.Parameters.AddWithValue("@EmployeeId", id);
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            countries.Add(new Country
                            {
                                Id = (int)reader["CountryId"],
                                Name = (string)reader["CountryName"]
                            });
                        }
                    }
                }
            }

            return countries;
        }

        public List<AddressType> GetAddressTypes()
        {
            List<AddressType> addressTypes = new List<AddressType>();

            using (_connection = new SqlConnection(GetConnectionString()))
            {
                string query = "SELECT Id, [Name] FROM AddressType";
                SqlCommand command = new SqlCommand(query, _connection);

                _connection.Open();

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        AddressType addressType = new AddressType
                        {
                            Id = reader.GetInt32(0),
                            Name = reader.GetString(1)
                        };

                        addressTypes.Add(addressType);
                    }
                }
            }

            return addressTypes;
        }

        public List<Country> GetCountries()
        {
            List<Country> countries = new List<Country>();

            using (_connection = new SqlConnection(GetConnectionString()))
            {
                string query = "SELECT Id, [Name] FROM Country";
                SqlCommand command = new SqlCommand(query, _connection);

                _connection.Open();

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Country country = new Country
                        {
                            Id = reader.GetInt32(0),
                            Name = reader.GetString(1)
                        };

                        countries.Add(country);
                    }
                }
            }

            return countries;
        }
        public List<State> GetStatesByCountry(int countryId)
        {
            List<State> states = new List<State>();

            using (_connection = new SqlConnection(GetConnectionString()))
            {
                string query = "SELECT Id, Name FROM State WHERE CountryId = @CountryId";
                SqlCommand command = new SqlCommand(query, _connection);
                command.Parameters.AddWithValue("@CountryId", countryId);

                _connection.Open();

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        State state = new State
                        {
                            Id = reader.GetInt32(0),
                            Name = reader.GetString(1)
                        };

                        states.Add(state);
                    }
                }
            }

            return states;
        }

        public string GetImage(int id)
        {
            string imagePath = null;
            using (_connection = new SqlConnection(GetConnectionString()))
            {
                string query = "SELECT ImagePath FROM EmployeeData WHERE Id = @Id";
                SqlCommand command = new SqlCommand(query, _connection);
                command.Parameters.AddWithValue("@Id", id);

                _connection.Open();

                imagePath = (string)command.ExecuteScalar();
            }

            return imagePath;
        }

        public List<Employee> GetAll()
        {
            List<Employee> employeeList = new List<Employee>();
            using (_connection = new SqlConnection(GetConnectionString()))
            {
                _command = _connection.CreateCommand();
                _command.CommandType = CommandType.StoredProcedure;
                _command.CommandText = "[dbo].[spGet_AllEmployees]";
                _connection.Open();
                SqlDataReader dr = _command.ExecuteReader();
                while (dr.Read())
                {
                    Employee employee = new Employee();
                    employee.Id = Convert.ToInt32(dr["Id"]);
                    employee.FirstName = Convert.ToString(dr["FirstName"]);
                    employee.LastName = Convert.ToString(dr["LastName"]);
                    employee.Gender = Convert.ToString(dr["Gender"]);
                    employee.DateOfBirth = Convert.ToDateTime(dr["DateOfBirth"]).Date;
                    employee.Email = Convert.ToString(dr["Email"]);
                    employee.MobileNumber = Convert.ToString(dr["MobileNumber"]);
                    employee.IsActive = Convert.ToBoolean(dr["IsActive"]);
                    employeeList.Add(employee);
                }
                _connection.Close();
            }
            return employeeList;
        }


       /* public bool InsertAll(Employee model)
        {
            int id = 0;
            using (_connection = new SqlConnection(GetConnectionString()))
            {
                _command = _connection.CreateCommand();
                _command.CommandType = CommandType.StoredProcedure;
                _command.CommandText = "[[dbo].[spUpdateAll_Employee]]";
                _command.Parameters.AddWithValue("@FirstName", model.FirstName);
                _command.Parameters.AddWithValue("@LastName", model.LastName);
                _command.Parameters.AddWithValue("@DateOfBirth", model.DateOfBirth.Date);
                _command.Parameters.AddWithValue("@Gender", Convert.ToInt32(model.Gender));
                _command.Parameters.AddWithValue("@Email", model.Email);
                _command.Parameters.AddWithValue("@MobileNumber", model.MobileNumber);
                _command.Parameters.AddWithValue("@IsActive", model.IsActive);
                _command.Parameters.AddWithValue("@ImagePath", model.Image);
                _command.Parameters.AddWithValue("@AddressList", JsonConvert.SerializeObject(model.AddressList));

                // Set the SQLDbType to NVarChar for the @AddressList parameter
                _command.Parameters["@AddressList"].SqlDbType = SqlDbType.NVarChar;
                _command.Parameters["@AddressList"].Size = -1;

                _connection.Open();
                id = _command.ExecuteNonQuery();
                _connection.Close();
            }
            return id > 0 ? true : false;
        }*/

        public Employee GetById(int id)
        {

            Employee employee = new Employee()
            {
                AddressList = new List<EmployeeAddress>()
            };

            using (_connection = new SqlConnection(GetConnectionString()))
            {
                _command = _connection.CreateCommand();
                _command.CommandType = CommandType.StoredProcedure;
                _command.CommandText = "[dbo].[spGet_DetailsById]";
                _command.Parameters.AddWithValue("@Id", id);
                _connection.Open();
                SqlDataReader dr = _command.ExecuteReader();
                while (dr.Read())
                {
                    employee.Id = Convert.ToInt32(dr["Id"]);
                    employee.FirstName = Convert.ToString(dr["FirstName"]);
                    employee.LastName = Convert.ToString(dr["LastName"]);
                    employee.Gender = Convert.ToString(dr["GenderId"]);
                    employee.DateOfBirth = Convert.ToDateTime(dr["DateOfBirth"]).Date;
                    employee.Email = Convert.ToString(dr["Email"]);
                    employee.MobileNumber = Convert.ToString(dr["MobileNumber"]);
                    employee.IsActive = Convert.ToBoolean(dr["IsActive"]);
                    employee.Image = Convert.ToString(dr["ImagePath"]);

                }
                employee.AddressList.Clear();

                dr.NextResult(); 
                while (dr.Read())
                {
                    EmployeeAddress address = new EmployeeAddress();
                    address.AddressId = Convert.ToInt32(dr["AddressId"]);
                    address.AddressTypeId = Convert.ToInt32(dr["AddrerssTypeID"]);
                    address.CountryId = Convert.ToInt32(dr["CountryId"]);
                    address.StateId = Convert.ToInt32(dr["StateId"]);
                    address.Address1 = Convert.ToString(dr["Address1"]);
                    address.Address2 = Convert.ToString(dr["Address2"]);
                    address.City = Convert.ToString(dr["City"]);
                    address.ZipCode = Convert.ToInt32(dr["ZipCOde"]);

                    // Add the address to the address list
                    employee.AddressList.Add(address);
                }

                _connection.Close();
            }
            return employee;
        }

        public bool UpdateAll(Employee model) 
        {
            int id = 0;
            using (_connection = new SqlConnection(GetConnectionString()))
            {
                _command = _connection.CreateCommand();
                _command.CommandType = CommandType.StoredProcedure;
                _command.CommandText = "[dbo].[spUpdateAll_Employee]";
                _command.Parameters.AddWithValue("@EmployeeId", model.Id);
                _command.Parameters.AddWithValue("@FirstName", model.FirstName);
                _command.Parameters.AddWithValue("@LastName", model.LastName);
                _command.Parameters.AddWithValue("@GenderId", model.Gender);
                _command.Parameters.AddWithValue("@DateOfBirth", model.DateOfBirth);
                _command.Parameters.AddWithValue("@Email", model.Email);
                _command.Parameters.AddWithValue("@MobileNumber", model.MobileNumber);
                _command.Parameters.AddWithValue("@IsActive", model.IsActive);
                _command.Parameters.AddWithValue("@ImagePath", model.Image);
                _command.Parameters.AddWithValue("@AddressesJson", JsonConvert.SerializeObject(model.AddressList));

                _connection.Open();
                id = _command.ExecuteNonQuery();
                _connection.Close();
            }

            return id > 0 ? true : false;
        }

        public bool Delete(int id)
        {
            int result = 0;
            using (_connection = new SqlConnection(GetConnectionString()))
            {
                _command = _connection.CreateCommand();
                _command.CommandType = CommandType.StoredProcedure;
                _command.CommandText = "[dbo].[spDelete_EmployeeAdd]";
                _command.Parameters.AddWithValue("@Id", id);
                _connection.Open();
                result = _command.ExecuteNonQuery();
                _connection.Close();
            }
            return result > 0 ? true : false;
        }

        public bool DeleteAddress(int addressId)
        {
            int result = 0;
            using (_connection = new SqlConnection(GetConnectionString()))
            {
                _command = _connection.CreateCommand();
                _command.CommandType = CommandType.StoredProcedure;
                _command.CommandText = "[dbo].[spSoftDelete_AddressDetails]";
                _command.Parameters.AddWithValue("@AddressId", addressId);

                _connection.Open();
                result = _command.ExecuteNonQuery();
                _connection.Close();
            }
            return result > 0 ? true : false;
        }

    }
}
