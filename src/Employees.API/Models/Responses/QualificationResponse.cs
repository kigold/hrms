namespace Employees.API.Models.Responses
{
    public record QualificationResponse(
            long Id,
            string Title,
            string Description,
            string QualificationType,
            string EducationLevel,
            DateTime? DateReceived,
            DateTime? ExpiryDate,
            string? MediaFile,
            Guid? MediaFileId
        );
}
