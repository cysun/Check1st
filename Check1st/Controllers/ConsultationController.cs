using Check1st.Models;
using Check1st.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text;
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

        public IActionResult Index(int? assignmentId)
        {
            Assignment assignment = null;
            if (assignmentId.HasValue)
            {
                assignment = _assignmentService.GetAssignment((int)assignmentId);
                if (assignment == null || assignment.IsDeleted)
                    return NotFound();
            }

            List<Assignment> assignments;
            if (User.IsInRole(Role.Admin.ToString()))
                assignments = _assignmentService.GetAssignments();
            else if (User.IsInRole(Role.Teacher.ToString()))
                assignments = _assignmentService.GetAssignmentsByTeacher(User.Identity.Name);
            else
                assignments = _assignmentService.GetAssignmentsByStudent(User.Identity.Name);

            if (assignment == null)
                assignment = assignments.FirstOrDefault();

            List<Consultation> consultations = null;
            if (assignment != null)
            {
                if (User.IsInRole(Role.Admin.ToString()) || User.IsInRole(Role.Teacher.ToString()))
                    consultations = _consultationService.GetConsultations(assignment.Id);
                else
                    consultations = _consultationService.GetConsultations(assignment.Id, User.Identity.Name);
            }

            ViewBag.Assignment = assignment;
            ViewBag.Assignments = assignments;
            return View(consultations);
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
                if (!User.IsInRole(Role.Admin.ToString()) && !User.IsInRole(Role.Teacher.ToString())
                    && _consultationService.GetConsultationCount(id, User.Identity.Name) >= _aiService.PerAssignmentLimit)
                {
                    _logger.LogWarning("{user} reached consultation limit for assignment {assignment}", User.Identity.Name, assignment.Id);
                    return View("Error", new ErrorViewModel
                    {
                        Message = "You have reached the limit of consultations for this assignment"
                    });
                }

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

            var authResult = await _authorizationService.AuthorizeAsync(User, consultation, Policy.CanReadConsultation);
            if (!authResult.Succeeded)
                return Forbid();

            if (consultation.StudentName == User.Identity.Name && !consultation.IsCompleted)
                return RedirectToAction("Check", new { id });

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
                if (file != null)
                {
                    consultation.AddFile(file);
                    _logger.LogInformation("{user} uploaded file {file} to consultation {consultation}",
                        User.Identity.Name, file.Id, consultation.Id);
                }
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
            var result = Verify(consultation, false);
            if (result != null) return result;

            if (!consultation.IsCompleted)
            {
                consultation.Files.ForEach(_fileService.LoadContent);
                var success = await _aiService.ConsultAsync(consultation);
                _consultationService.SaveChanges();
                _logger.LogInformation("{user} received feedback for consultation {consultation}. Success: {success}",
                    User.Identity.Name, consultation.Id, success);
            }

            return View(consultation);
        }

        [HttpPut("Consultation/{id}/FeedbackRating")]
        public IActionResult RateFeedback(int id, int rating)
        {
            var consultation = _consultationService.GetConsultation(id);
            var result = Verify(consultation, false);
            if (result != null) return result;

            if (rating < 1 || rating > 4)
            {
                _logger.LogWarning("{user} rated {rating} of consultation {consultation}", User.Identity.Name, rating, consultation.Id);
                return BadRequest("Rating must be between 1 and 5");
            }

            consultation.FeedbackRating = rating;
            _consultationService.SaveChanges();
            _logger.LogInformation("{user} rated {rating} of consultation {consultation}", User.Identity.Name, rating, consultation.Id);

            return Ok();
        }

        [HttpPost]
        public IActionResult CommentFeedback(int id, string comments)
        {
            if (!string.IsNullOrWhiteSpace(comments))
            {
                var consultation = _consultationService.GetConsultation(id);
                var result = Verify(consultation, false);
                if (result != null) return result;

                consultation.FeedbackComments = comments;
                _consultationService.SaveChanges();
                _logger.LogInformation("{user} commented on {consultation}", User.Identity.Name, consultation.Id);
            }

            return RedirectToAction("Index", "Consultation");
        }

        [Authorize(Roles = "Admin,Teacher")]
        public IActionResult Delete(int id)
        {
            var consultation = _consultationService.GetConsultation(id);
            if (consultation == null)
                return NotFound();

            if (!User.IsInRole(Role.Admin.ToString()) && consultation.Assignment.TeacherName != User.Identity.Name)
                return Forbid();

            _consultationService.DeleteConsultation(consultation);
            _logger.LogInformation("{user} deleted consultation {consultation}", User.Identity.Name, consultation.Id);

            return RedirectToAction("Index");
        }

        private IActionResult Verify(Consultation consultation, bool checkCompletion = true)
        {
            if (consultation == null)
                return NotFound();

            if (consultation.StudentName != User.Identity.Name)
                return Forbid();

            if (checkCompletion && consultation.IsCompleted)
                return View("Error", new ErrorViewModel { Message = "This consultation is already completed" });

            return null;
        }

        public IActionResult DownloadCsv()
        {
            List<Consultation> consultations;
            if (User.IsInRole(Role.Admin.ToString()))
                consultations = _consultationService.GetConsultations();
            else if (User.IsInRole(Role.Teacher.ToString()))
                consultations = _consultationService.GetConsultationsAsTeacher(User.Identity.Name);
            else
                consultations = _consultationService.GetConsultationsAsStudent(User.Identity.Name);

            string[] properties = {"Id", "AssignmentId", "StudentName", "TimeCreated", "TimeCompleted",
                "FeedbackRating", "Model", "PromptTokens", "CompletionTokens"};
            var csv = new StringBuilder();
            csv.AppendLine(string.Join(",", properties));
            foreach (var consultation in consultations)
            {
                csv.AppendLine(string.Join(",", properties.Select(p => p switch
                {
                    "TimeCreated" => consultation.TimeCreated.ToString("yyyy-MM-dd HH:mm:ss"),
                    "TimeCompleted" => consultation.TimeCompleted?.ToString("yyyy-MM-dd HH:mm:ss") ?? "",
                    _ => consultation.GetType().GetProperty(p).GetValue(consultation)?.ToString()
                })));
            }

            var filename = $"consultations_{DateTime.Now:yyyyMMdd}.csv";
            return File(Encoding.UTF8.GetBytes(csv.ToString()), "text/csv", filename);
        }
    }
}
