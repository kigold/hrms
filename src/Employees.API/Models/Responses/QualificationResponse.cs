namespace Employees.API.Models.Responses
{
    public class QualificationResponse(
            string Title,
            string Description,
            string QualificationType,
            string EducationLevel,
            DateTime? DateReceived,
            DateTime? ExpiryDate
        );
}
