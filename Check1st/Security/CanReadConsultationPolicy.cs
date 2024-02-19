using Check1st.Models;
using Microsoft.AspNetCore.Authorization;

namespace Check1st.Security;

public class CanReadConsultationRequirement : IAuthorizationRequirement
{
}

public class CanReadConsultationHandler : AuthorizationHandler<CanReadConsultationRequirement, Consultation>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context,
        CanReadConsultationRequirement requirement, Consultation consultation)
    {
        string userName = context.User.Identity.Name;
        if (userName == consultation.StudentName || userName == consultation.Assignment.TeacherName
            || context.User.IsInRole(Constants.Role.Admin.ToString()))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}