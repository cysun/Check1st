using Check1st.Models;
using Check1st.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Check1st.Controllers
{
    [Authorize]
    public class ConsultationController : Controller
    {
        private readonly FileService _fileService;
        private readonly AssignmentService _assignmentService;
        private readonly ConsultationService _consultationService;

        private readonly ILogger<ConsultationController> _logger;

        public ConsultationController(FileService fileService, AssignmentService assignmentService,
            ConsultationService consultationService, ILogger<ConsultationController> logger)
        {
            _fileService = fileService;
            _assignmentService = assignmentService;
            _consultationService = consultationService;
            _logger = logger;
        }

        public IActionResult Assignment(int id)
        {
            var assignment = _assignmentService.GetAssignment(id);
            if (assignment == null || assignment.IsClosed) return NotFound();

            var lastConsultation = _consultationService.GetLastConsultation(id, User.Identity.Name);
            if (lastConsultation != null && !lastConsultation.IsCompleted)
                return RedirectToAction("UploadFiles", new { id = lastConsultation.Id });
            else
            {
                var consultation = new Consultation
                {
                    Assignment = assignment,
                    StudentName = User.Identity.Name,
                };
                _consultationService.AddConsultation(consultation);
                return RedirectToAction("UploadFiles", new { id = consultation.Id });
            }
        }

        [HttpGet]
        public IActionResult UploadFiles(int id)
        {
            var consultation = _consultationService.GetConsultation(id);
            if (consultation == null || consultation.StudentName != User.Identity.Name)
                return NotFound();

            if (consultation.IsCompleted) return RedirectToAction("Rate", new { id });

            return View(consultation);
        }

        [HttpPost]
        public async Task<IActionResult> UploadFiles(int id, List<IFormFile> uploadedFiles)
        {
            var consultation = _consultationService.GetConsultation(id);
            if (consultation == null || consultation.StudentName != User.Identity.Name)
                return NotFound();

            if (consultation.IsCompleted) return RedirectToAction("Rate", new { id });

            foreach (var uploadedFile in uploadedFiles)
            {
                var file = await _fileService.UploadFileAsync(uploadedFile, User.Identity.Name);
                consultation.AddFile(file);
            }
            _consultationService.SaveChanges();

            return RedirectToAction("UploadFiles", new { id });
        }

        public IActionResult RemoveFile(int id, int fileId)
        {
            var consultation = _consultationService.GetConsultation(id);
            if (consultation == null || consultation.StudentName != User.Identity.Name)
                return NotFound();

            consultation.Files.RemoveAll(f => f.Id == fileId);
            _consultationService.SaveChanges();

            return RedirectToAction("UploadFiles", new { id });
        }
    }
}
