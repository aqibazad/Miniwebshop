using Microsoft.AspNetCore.Mvc;
using Miniwebshop.Models;

namespace Miniwebshop.Controllers
{
    public class StudentAdmissionController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            return View(new StudentAdmissionViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Index(StudentAdmissionViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            TempData["AdmissionStudentName"] = model.StudentName;
            TempData["AdmissionBForm"] = model.BFormNumber;
            TempData["AdmissionAge"] = model.Age?.ToString();
            TempData["AdmissionBloodGroup"] = model.BloodGroup;
            TempData["AdmissionGuardianType"] = model.GuardianType;
            TempData["AdmissionGuardianName"] = model.GuardianName;
            TempData["AdmissionGuardianEmail"] = model.GuardianEmail;
            TempData["AdmissionPhone"] = model.PhoneNumber;
            TempData["AdmissionCnic"] = model.GuardianCnic;
            TempData["AdmissionAddress"] = model.GuardianAddress;

            return RedirectToAction(nameof(Success));
        }

        [HttpGet]
        public IActionResult Success()
        {
            return View();
        }
    }
}
