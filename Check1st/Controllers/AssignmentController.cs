using AutoMapper;
using Check1st.Services;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace Check1st.Controllers
{
    public class AssignmentController : Controller
    {
        private readonly AssignmentService _assignmentService;

        private readonly IMapper _mapper;
        private readonly ILogger<AssignmentController> _logger;

        public AssignmentController(AssignmentService assignmentService, IMapper mapper, ILogger<AssignmentController> logger)
        {
            _assignmentService = assignmentService;
            _mapper = mapper;
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View(_assignmentService.GetAssignments());
        }
    }
}

namespace Check1st.Models
{
    public class AssignmentInputModel
    {
        [Required, MaxLength(100)]
        public string Name { get; set; }

        public string Description { get; set; }
        public string Prompt { get; set; }

        [Display(Name = "Publish Time")]
        public DateTime? TimePublished { get; set; }

        [Display(Name = "Close Time")]
        public DateTime? TimeClosed { get; set; }

        public bool IsPublished => TimePublished.HasValue && TimePublished < DateTime.UtcNow;
        public bool IsClosed => TimeClosed.HasValue && TimeClosed < DateTime.UtcNow;
    }
}