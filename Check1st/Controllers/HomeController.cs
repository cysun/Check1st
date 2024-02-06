using Check1st.Models;
using Check1st.Services;
using Microsoft.AspNetCore.Mvc;

namespace Check1st.Controllers
{
    public class HomeController : Controller
    {
        private readonly AssignmentService _assignmentService;

        private readonly ILogger<HomeController> _logger;

        public HomeController(AssignmentService assignmentService, ILogger<HomeController> logger)
        {
            _assignmentService = assignmentService;
            _logger = logger;
        }

        public IActionResult Index()
        {
            List<Assignment> assignments = null;

            if (User.Identity.IsAuthenticated)
            {
                assignments = _assignmentService.GetCurrentAssignments();
            }

            return View(assignments);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult Error()
        {
            return View();
        }
    }
}
