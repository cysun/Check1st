using Check1st.Models;
using Check1st.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Check1st.Controllers
{
    [Authorize]
    public class ConsultationController : Controller
    {
        private readonly AIService _aiService;
        private readonly FileService _fileService;
        private readonly AssignmentService _assignmentService;
        private readonly ConsultationService _consultationService;

        private readonly ILogger<ConsultationController> _logger;

        public ConsultationController(AIService aiService, FileService fileService, AssignmentService assignmentService,
            ConsultationService consultationService, ILogger<ConsultationController> logger)
        {
            _aiService = aiService;
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
                if (lastConsultation != null)
                    consultation.Files.AddRange(lastConsultation.Files);
                _consultationService.AddConsultation(consultation);
                _logger.LogInformation("{user} created consultation {consultation}", User.Identity.Name, consultation.Id);
                return RedirectToAction("UploadFiles", new { id = consultation.Id });
            }
        }

        public IActionResult View(int id)
        {
            var consultation = _consultationService.GetConsultation(id);
            if (consultation == null || consultation.StudentName != User.Identity.Name)
                return NotFound();

            return View(consultation);
        }

        [HttpGet]
        public IActionResult UploadFiles(int id)
        {
            var consultation = _consultationService.GetConsultation(id);
            if (consultation == null || consultation.StudentName != User.Identity.Name)
                return NotFound();

            if (consultation.IsCompleted)
                return View("Error", new ErrorViewModel { Message = "This consultation is already completed" });

            return View(consultation);
        }

        [HttpPost]
        public async Task<IActionResult> UploadFiles(int id, List<IFormFile> uploadedFiles)
        {
            var consultation = _consultationService.GetConsultation(id);
            if (consultation == null || consultation.StudentName != User.Identity.Name)
                return NotFound();

            if (consultation.IsCompleted)
                return View("Error", new ErrorViewModel { Message = "This consultation is already completed" });

            foreach (var uploadedFile in uploadedFiles)
            {
                var file = await _fileService.UploadFileAsync(uploadedFile, User.Identity.Name);
                consultation.AddFile(file);
                _logger.LogInformation("{user} uploaded file {file} to consultation {consultation}",
                    User.Identity.Name, file.Id, consultation.Id);
            }
            _consultationService.SaveChanges();

            return RedirectToAction("UploadFiles", new { id });
        }

        public IActionResult RemoveFile(int id, int fileId)
        {
            var consultation = _consultationService.GetConsultation(id);
            if (consultation == null || consultation.StudentName != User.Identity.Name)
                return NotFound();

            if (consultation.IsCompleted)
                return View("Error", new ErrorViewModel { Message = "This consultation is already completed" });

            consultation.Files.RemoveAll(f => f.Id == fileId);
            _consultationService.SaveChanges();
            _logger.LogInformation("{user} removed file {file} from consultation {consultation}",
                User.Identity.Name, fileId, consultation.Id);

            return RedirectToAction("UploadFiles", new { id });
        }

        [HttpGet]
        public async Task<IActionResult> CheckAsync(int id)
        {
            var consultation = _consultationService.GetConsultation(id);
            if (consultation == null || consultation.StudentName != User.Identity.Name)
                return NotFound();

            if (consultation.IsCompleted)
                return RedirectToAction("View", new { id });

            if (consultation.Feedback == null)
            {
                consultation.Files.ForEach(f => _fileService.LoadContent(f));
                var success = await _aiService.ConsultAsync(consultation);
                _consultationService.SaveChanges();
                _logger.LogInformation("{user} received feedback for consultation {consultation}. Success: {success}",
                    User.Identity.Name, consultation.Id, success);
            }

            return View(consultation);
        }

        [HttpPost]
        public IActionResult Check(int id, string feedbackComments)
        {
            var consultation = _consultationService.GetConsultation(id);
            if (consultation == null || consultation.StudentName != User.Identity.Name)
                return NotFound();

            if (consultation.IsCompleted)
                return BadRequest("This consultation is already completed");

            consultation.FeedbackComments = feedbackComments;
            consultation.TimeCompleted = DateTime.UtcNow;
            _consultationService.SaveChanges();
            _logger.LogInformation("{user} completed consultation {consultation}", User.Identity.Name, consultation.Id);

            return RedirectToAction("Index", "Home");
        }

        [HttpPut("Consultation/{id}/FeedbackRating")]
        public IActionResult RateFeedback(int id, int rating)
        {
            var consultation = _consultationService.GetConsultation(id);
            if (consultation == null || consultation.StudentName != User.Identity.Name)
                return NotFound();

            if (consultation.IsCompleted)
                return BadRequest("This consultation is already completed");

            consultation.FeedbackRating = rating;
            _consultationService.SaveChanges();
            _logger.LogInformation("{user} rated feedback for consultation {consultation}", User.Identity.Name, consultation.Id);

            return Ok();
        }
    }
}
