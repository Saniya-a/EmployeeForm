using EmployeeForm.DAL;
using EmployeeForm.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Data.OleDb;
using System.Net;
using System.Reflection;


namespace EmployeeForm.Controllers
{
    public class EmployeeController : Controller
    {
        private readonly EmployeeDAL _employeeDAL;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public EmployeeController(EmployeeDAL employeeDAL, IWebHostEnvironment webHostEnvironment)
        {
            _employeeDAL = new EmployeeDAL();
            _webHostEnvironment = webHostEnvironment;
        }

        [HttpGet]
        public IActionResult Index()
        {
            List<Employee> employees = new List<Employee>();
            try
            {
                employees = _employeeDAL.GetAll();
            }
            catch (Exception ex)
            {
                TempData["errorMessage: "] = ex.Message;
            }

            return View(employees);
        }
        public IActionResult GetStatesByCountry(int countryId)
        {
            List<State> states = _employeeDAL.GetStatesByCountry(countryId);
            return Json(states);
        }

        public IActionResult AddAddressRow()
        {
            List<AddressType> addressTypes = _employeeDAL.GetAddressTypes();
            List<Country> countries = _employeeDAL.GetCountries();
            EmployeeAddress employeeAddress = new EmployeeAddress()
            {
                AddressTypes = addressTypes,
                Countries = countries,
           };
            return PartialView("_AddressPartial", employeeAddress);
        }

        [HttpGet]
        public IActionResult Upsert(int id)
        {
            Employee employee = new Employee();
            if (id == 0)
            {
                //List<CasteCategory> categoryList = _employeeDAL.GetCategoryList();
                return View(employee);
            }

            else
            {
                employee.AddressList = new List<EmployeeAddress>();
                try
                {
                    employee = _employeeDAL.GetById(id);

                    List<AddressType> addressTypes = _employeeDAL.GetAddressTypes();
                    List<Country> countries = _employeeDAL.GetCountries();

                    foreach (var item in employee.AddressList)
                    {
                        item.AddressTypes = addressTypes;
                        item.Countries = countries;
                    }

                    // Get distinct country IDs from the AddressList in employee
                    var distinctCountryIds = employee.AddressList.Select(a => a.CountryId).Distinct();

                    foreach (var item in employee.AddressList)
                    {
                        // Get the corresponding country for the current Address
                        var country = countries.FirstOrDefault(c => c.Id == item.CountryId);

                        if (country != null)
                        {
                            // Assign the state list for the current country to the Address
                            item.States = _employeeDAL.GetStatesByCountry(country.Id);
                        }
                    }

                    if (employee.Id == 0)
                    {
                        TempData["errorMessage"] = $"Employee Details not found with Id: {id}";
                        return RedirectToAction("Index");
                    }
                    return View(employee);

                }
                catch (Exception ex)
                {
                    TempData["errorMessage: "] = ex.Message;
                    return View();
                }
            }

        }

        [HttpPost]
        public IActionResult Upsert(Employee model)
        {
            var files = HttpContext.Request.Form.Files;
            string webRootPath = _webHostEnvironment.WebRootPath;
            if (model.Id==0)
            {
                try
                {
                    string fileName = Guid.NewGuid().ToString();
                    string extension = Path.GetExtension(files[0].FileName);
                    string upload = webRootPath + WC.ImagePath;

                    using (var fileStream = new FileStream(Path.Combine(upload, fileName + extension), FileMode.Create))
                    {
                        files[0].CopyTo(fileStream);
                    }

                    model.Image = fileName + extension;

                    bool result = _employeeDAL.UpdateAll(model);

                    if (!result)
                    {
                        TempData["errorMessage"] = "Unable to save the data";
                        return View();
                    }
                    TempData["successMessage"] = "Employee Details Saved";
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    TempData["errorMessage: "] = ex.Message;
                    return View();
                }
            }
            else
            {
                string oldImage = _employeeDAL.GetImage(model.Id);
                try
                {
                    if (files.Count > 0)
                    {
                        string upload = webRootPath + WC.ImagePath;
                        string fileName = Guid.NewGuid().ToString();
                        string extension = Path.GetExtension(files[0].FileName);

                        var oldFile = Path.Combine(upload, oldImage);

                        if (System.IO.File.Exists(oldFile))
                        {
                            System.IO.File.Delete(oldFile);
                        }

                        using (var fileStream = new FileStream(Path.Combine(upload, fileName + extension), FileMode.Create))
                        {
                            files[0].CopyTo(fileStream);
                        }
                        model.Image = fileName + extension;
                    }
                    else
                    {
                        model.Image = oldImage;
                    }
                    bool result = _employeeDAL.UpdateAll(model);
                    if (!result)
                    {
                        TempData["errorMessage"] = "Unable to update the data";
                        return View();
                    }
                    TempData["successMessage"] = "Employee Details upadated";
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    TempData["errorMessage: "] = ex.Message;
                    return View();
                }
            }
        }


        [HttpGet]
        public IActionResult Delete(int id)
        {

            try
            {
                Employee employee = _employeeDAL.GetById(id);

                List<AddressType> addressTypes = _employeeDAL.GetAddressTypes();
                List<Country> countries = _employeeDAL.GetCountries();

                foreach (var item in employee.AddressList)
                {
                    item.AddressTypes = addressTypes;
                    item.Countries = countries;
                }
                if (employee.Id == 0)
                {
                    TempData["errorMessage"] = $"Employee Details not found with Id: {id}";
                    return RedirectToAction("Index");
                }
                return View(employee);

            }
            catch (Exception ex)
            {
                TempData["errorMessage: "] = ex.Message;
                return View();
            }
        }

        [HttpPost]
        public IActionResult DeleteConfirmation(Employee employee)
        {
            try
            {
                bool result = _employeeDAL.Delete(employee.Id);
                if (!result)
                {
                    TempData["errorMessage"] = "Unable to delete the data";
                    return View();
                }
                TempData["successMessage"] = "Employee details deleted";
                return RedirectToAction("Index");
            }
                    catch (Exception ex)
            {
                TempData["errorMessage: "] = ex.Message;
                return View();
            }
        }

        [HttpPost]
        public ActionResult SoftDeleteAddress(int addressId)
        {
            bool result = _employeeDAL.DeleteAddress(addressId);
            if (!result)
            {
                TempData["errorMessage"] = "Unable to delete the data";
                return View();
            }
            return Json(new { success = true });
        }

        public async Task<IActionResult> DownloadFile(string filePath)
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/" + filePath);

            var memory = new MemoryStream();
            using (var stream = new FileStream(path, FileMode.Open))
            {
                await stream.CopyToAsync(memory);
            }
            memory.Position = 0;
            var contentType = "APPLICATION/octet-stream";
            var fileName = Path.GetFileName(path);
            return File(memory, contentType, fileName);
        }
    }
}

