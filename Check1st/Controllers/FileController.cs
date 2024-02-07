using Check1st.Security;
using Check1st.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Check1st.Controllers
{
    [Authorize]
    public class FileController : Controller
    {
        private readonly FileService _fileService;

        public FileController(FileService fileService)
        {
            _fileService = fileService;
        }

        public async Task<IActionResult> ViewAsync(int id)
        {
            return await DownloadAsync(id, true);
        }

        public async Task<IActionResult> DownloadAsync(int id, bool inline = false)
        {
            var file = _fileService.GetFile(id);
            if (file == null) return NotFound();

            if (User.Identity.Name == file.OwnerName || User.IsInRole(Constants.Role.Admin.ToString())
                || User.IsInRole(Constants.Role.Teacher.ToString()))
            {
                return Redirect(await _fileService.GetDownloadUrlAsync(file, inline));
            }
            else
            {
                return Forbid();
            }
        }
    }
}
