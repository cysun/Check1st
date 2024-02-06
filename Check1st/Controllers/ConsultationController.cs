using Check1st.Models;
using Check1st.Services;
using Microsoft.AspNetCore.Mvc;

namespace Check1st.Controllers
{
    public class ConsultationController : Controller
    {
        private readonly AssignmentService _assignmentService;
        private readonly ConsultationService _consultationService;

        private readonly ILogger<ConsultationController> _logger;

        public ConsultationController(AssignmentService assignmentService, ConsultationService consultationService,
            ILogger<ConsultationController> logger)
        {
            _assignmentService = assignmentService;
            _consultationService = consultationService;
            _logger = logger;
        }

        public IActionResult Assignment(int id)
        {
            var assignment = _assignmentService.GetAssignment(id);
            if (assignment == null || assignment.IsClosed) return NotFound();

            var lastConsultation = _consultationService.GetLastConsultation(id, User.Identity.Name);
            if (lastConsultation == null)
                return RedirectToAction("Upload", new { assignmentId = id });
            else if (!lastConsultation.IsCompleted)
                return RedirectToAction("Upload", new { id = lastConsultation.Id });
            else
            {
                var consultation = new Consultation
                {
                    Assignment = assignment,
                    StudentName = User.Identity.Name,
                };
                consultation.Files.AddRange(lastConsultation.Files);
                _consultationService.AddConsultation(consultation);
                return RedirectToAction("Upload", new { id = consultation.Id });
            }
        }
    }
}
