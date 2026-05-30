using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using PersonalCabinetEducationProgram.Models;
using PersonalCabinetEducationProgram.Services;

namespace PersonalCabinetEducationProgram.Controllers
{
    public class ManagerHomeController : Controller
    {
        private readonly IFileStorageService _fileStorageService;
        private readonly FileStorageSettings _storageSettings;

        public ManagerHomeController(IFileStorageService fileStorageService, IOptions<FileStorageSettings> storageSettings)
        {
            _fileStorageService = fileStorageService;
            _storageSettings = storageSettings.Value;
        }
        public IActionResult Index()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            string fileName = await _fileStorageService.SaveFileAsync(file);

            ViewBag.FilePath = _storageSettings.BaseUrl + fileName;
            return View("Index");
        }
    }
}
