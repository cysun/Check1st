using Check1st.Models;
using Check1st.Security;
using Check1st.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static Check1st.Security.Constants;

namespace Check1st.Controllers
{
    [Authorize]
    public class ConsultationController : Controller
    {
        private readonly AIService _aiService;
        private readonly FileService _fileService;
        private readonly AssignmentService _assignmentService;
        private readonly ConsultationService _consultationService;

        private readonly IAuthorizationService _authorizationService;

        private readonly ILogger<ConsultationController> _logger;

        public ConsultationController(AIService aiService, FileService fileService, AssignmentService assignmentService,
            ConsultationService consultationService, IAuthorizationService authorizationService,
            ILogger<ConsultationController> logger)
        {
            _aiService = aiService;
            _fileService = fileService;
            _assignmentService = assignmentService;
            _consultationService = consultationService;
            _authorizationService = authorizationService;
            _logger = logger;
        }

        public IActionResult Index()
        {
            Role role = User.IsInRole(Constants.Role.Admin.ToString()) ? Constants.Role.Admin :
                User.IsInRole(Constants.Role.Teacher.ToString()) ? Constants.Role.Teacher : Constants.Role.None;
            return View(_consultationService.GetRecentConsultations(User.Identity.Name, role));
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
                _logger.LogInformation("{user} created consultation {consultation}", User.Identity.Name, consultation.Id);
                return RedirectToAction("UploadFiles", new { id = consultation.Id });
            }
        }

        public async Task<IActionResult> ViewAsync(int id)
        {
            var consultation = _consultationService.GetConsultation(id);
            if (consultation == null)
                return NotFound();

            var authResult = await _authorizationService.AuthorizeAsync(User, consultation, Constants.Policy.CanReadConsultation);
            if (!authResult.Succeeded)
                return Forbid();

            if (consultation.StudentName == User.Identity.Name)
            {
                if (consultation.Feedback == null)
                    return RedirectToAction("UploadFiles", new { id });
                else if (!consultation.IsCompleted)
                    return RedirectToAction("Check", new { id });
            }

            return View(consultation);
        }

        [HttpGet]
        public IActionResult UploadFiles(int id)
        {
            var consultation = _consultationService.GetConsultation(id);
            var result = Verify(consultation);
            if (result != null) return result;

            return View(consultation);
        }

        [HttpPost]
        public async Task<IActionResult> UploadFiles(int id, List<IFormFile> uploadedFiles)
        {
            var consultation = _consultationService.GetConsultation(id);
            var result = Verify(consultation);
            if (result != null) return result;

            foreach (var uploadedFile in uploadedFiles)
            {
                if (uploadedFile.Length > consultation.Assignment.MaxFileSize)
                {
                    _logger.LogWarning("Ignore {user} uploaded file due to size: {size}", User.Identity.Name, uploadedFile.Length);
                    continue;
                }

                var extension = Path.GetExtension(uploadedFile.FileName);
                if (extension == ReadOnlySpan<char>.Empty || !consultation.Assignment.AcceptedFileTypes.Contains(extension))
                {
                    _logger.LogWarning("Ignore {user} uploaded file due to type: {type}", User.Identity.Name, extension);
                    continue;
                }

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
            var result = Verify(consultation);
            if (result != null) return result;

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
            var result = Verify(consultation);
            if (result != null) return result;

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
            var result = Verify(consultation);
            if (result != null) return result;

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
            var result = Verify(consultation);
            if (result != null) return result;

            consultation.FeedbackRating = rating;
            _consultationService.SaveChanges();
            _logger.LogInformation("{user} rated feedback for consultation {consultation}", User.Identity.Name, consultation.Id);

            return Ok();
        }

        private IActionResult Verify(Consultation consultation)
        {
            if (consultation == null)
                return NotFound();

            if (consultation.StudentName != User.Identity.Name)
                return Forbid();

            if (consultation.IsCompleted)
                return View("Error", new ErrorViewModel { Message = "This consultation is already completed" });

            return null;
        }
    }
}
