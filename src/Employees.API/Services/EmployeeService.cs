using Employees.API.Data.Models;
using Employees.API.Models.Requests;
using Employees.API.Models.Responses;
using FluentValidation;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using Shared.Data;
using Shared.FileStorage;
using Shared.Messaging;
using Shared.Pagination;
using Shared.Repositories;
using Shared.ViewModels;
using System.Text.Json;
using static Shared.Messaging.PubMessageType;

namespace Employees.API.Services
{
    public interface IEmployeeService
    {
        public Task<ResultModel<EmployeeResponse>> CreateEmployee(CreateEmployee request);
        public Task<ResultModel> UpdateEmployee(int companyId, UpdateEmployee request);
        public Task<ResultModel> UpdateEmployeeStaffId(EmployeeStaffIdDTO request);
        public Task<ResultModel> DeleteEmployee(int companyId, long employeeId);
        public Task<ResultModel<QualificationResponse>> AddQualification(AddEmployeeQualification request);
        public Task<ResultModel> RemoveQualification(long qualificationId);
        public Task DeleteFile(MediaFile mediaFile);

        public Task<ResultModel<PagedList<EmployeeResponse>>> GetEmployee(int companyId, PagedRequest query);
        public Task<ResultModel<EmployeeDetailResponse>> GetEmployee(int companyId, long employeeId);
    }

    public class EmployeeService : IEmployeeService 
    {
        private readonly IRepository<Employee, long> _employeeRepo;
        private readonly IRepository<Qualification, long> _qualificationRepo;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IFileProvider _fileProvider;
        private readonly IFileStorageSetting _fileStorageSetting;
        private readonly IBus _bus;
        private const string QUALIFICATION_DIR = "Qualifications";

        public EmployeeService(
            IRepository<Employee, long> employeeRepo,
            IRepository<Qualification, long> qualificationRepo,
            IUnitOfWork unitOfWork,
            IFileProvider fileProvider,
            IOptions<FileStorageSetting> fileStorageSetting2,
            IBus bus) 
        {
            _employeeRepo = employeeRepo;
            _qualificationRepo = qualificationRepo;
            _unitOfWork = unitOfWork;
            _fileProvider = fileProvider;
            _fileStorageSetting = fileStorageSetting2.Value;
            _bus = bus;
        }

        private async Task<MediaFile?> SaveFileAsync(IFormFile file)
        {
            var ext = Path.GetExtension(file.FileName);
            if (!_fileStorageSetting.AllowedExtensions.Contains(ext.Replace(".", "")))
                return null;
            var fileId = Guid.NewGuid();
            var fileName = Path.Combine(QUALIFICATION_DIR, $"{fileId.ToString()}{ext}");
            var filePath = Path.Combine(_fileStorageSetting.BaseDirectory, fileName);
            using (var stream = File.Create(filePath))
            {
                await file.CopyToAsync(stream);
            }
            return new MediaFile { Id = fileId, Path = fileName, Mimetype = Path.GetExtension(file.FileName) };
        }

        public Task DeleteFile(MediaFile mediaFile)
        {
            var filePath = Path.Combine(_fileStorageSetting.BaseDirectory, mediaFile.Path);
            File.Delete(filePath);
            return Task.CompletedTask;
        }

        public async Task<ResultModel<QualificationResponse>> AddQualification(AddEmployeeQualification request)
        {
            MediaFile? file = null;
            if (request.File != null)
                file = await SaveFileAsync(request.File);

            if (file == null && request.File != null)
                return new ResultModel<QualificationResponse>("Unable to Process Uploaded File, probably due to unsupported file type");

            var validator = new AddEmployeeQualificationValidator();
            var result = validator.Validate(request);
            if (!result.IsValid)
            {
                return new ResultModel<QualificationResponse>() { ErrorMessages = result.Errors.Select(x => x.ErrorMessage).ToList() };
            }

            var qualification = new Qualification
            {
                EmployeeId = request.EmployeeId,
                DateReceived = request.DateReceived,
                EducationLevel = request.EducationLevel,
                ExpiryDate = request.ExpiryDate,
                Title = request.Title,
                Description = request.Description,
                QualificationType = request.QualificationType,
                MediaFileId = file?.Id,
                MediaFile = file
            };
            _qualificationRepo.Insert(qualification);
            await _unitOfWork.SaveChangesAsync();
            if (qualification.MediaFile != null)
                await _bus.Publish(new PublishMessage(JsonSerializer.Serialize(qualification.MediaFile), FILE_PROCESS_SHRINK_FILE));

            return new ResultModel<QualificationResponse>(qualification.ToQualificationResponse());
        }

        public async Task<ResultModel<EmployeeResponse>> CreateEmployee(CreateEmployee request)
        {
            var validator = new CreateEmployeeRequestValidator();
            var result = validator.Validate(request);
            if (!result.IsValid)
            {
                return new ResultModel<EmployeeResponse>() { ErrorMessages = result.Errors.Select(x => x.ErrorMessage).ToList() };
            }
            if (_employeeRepo.Get(x => x.Email.Equals(request.Email)).FirstOrDefault() != null)
            {
                return new ResultModel<EmployeeResponse>("User with email already exists");
            }

            var employee = new Employee
            {
                CompanyId = request.CompanyId,
                Email = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName,
                Country = request.Country,
                Phone = request.Phone,
                StaffId = string.Empty,
                Address = request.Address
            };

            _employeeRepo.Insert(employee);
            await _unitOfWork.SaveChangesAsync();
            await _bus.Publish(new PublishMessage(JsonSerializer.Serialize(employee.ToEmployeeMessage()), EMPLOYEE_CREATE_USER));

            return new ResultModel<EmployeeResponse>(employee.ToEmployeeResponse());
        }

        public async Task<ResultModel> DeleteEmployee(int companyId, long employeeId)
        {
            var employee = await _employeeRepo.Get(x => x.CompanyId == companyId && x.Id == employeeId).FirstOrDefaultAsync();
            if (employee == null)
                return new ResultModel<EmployeeResponse>("Employee not found");

            _employeeRepo.Delete(employeeId);
            await _unitOfWork.SaveChangesAsync();
            await _bus.Publish(new PublishMessage(JsonSerializer.Serialize(employee.ToEmployeeMessage()), EMPLOYEE_DELETE_USER));

            return new ResultModel();
        }

        public async Task<ResultModel<PagedList<EmployeeResponse>>> GetEmployee(int companyId, PagedRequest query)
        {
            var employees = _employeeRepo.Get(x => x.CompanyId == companyId).OrderByDescending(x => x.Id).Include(x => x.Qualifications);

            var result = PagedList<Employee>.ToPagedList(employees, query.PageNumber, query.PageSize);

            return new ResultModel<PagedList<EmployeeResponse>>(new PagedList<EmployeeResponse>(result.Items.Select(x => x.ToEmployeeResponse()), result.TotalCount, query.PageNumber, query.PageSize));
        }

        public async Task<ResultModel<EmployeeDetailResponse>> GetEmployee(int companyId, long employeeId)
        {
            var employee = await _employeeRepo.Get(x => x.CompanyId == companyId)
                    .Include(x => x.Qualifications).ThenInclude(q => q.MediaFile)
                    .FirstOrDefaultAsync(x => x.Id == employeeId);
            if (employee == null)
                return new ResultModel<EmployeeDetailResponse>("Employee not found");

            var result = employee.ToEmployeeDetailsResponse();
            return new ResultModel<EmployeeDetailResponse>(result);
        }

        public async Task<ResultModel> RemoveQualification(long qualificationId)
        {
            var qualification = await _qualificationRepo.Get(x => x.Id == qualificationId).Include(x => x.MediaFile).FirstOrDefaultAsync();
            if (qualification == null)
                return new ResultModel("Qualification not found");

            _unitOfWork.BeginTransaction();
            if (qualification.MediaFile != null)
                await _bus.Publish(new PublishMessage(JsonSerializer.Serialize(qualification.MediaFile), FILE_PROCESS_DELETE_FILE));

            _qualificationRepo.Delete(qualification);
            await _unitOfWork.SaveChangesAsync();

            _unitOfWork.Commit();
            return new ResultModel();
        }

        public async Task<ResultModel> UpdateEmployee(int companyId, UpdateEmployee request)
        {
            var validator = new UpdateEmployeeRequestValidator();
            var result = validator.Validate(request);
            if (!result.IsValid)
            {
                return new ResultModel() { ErrorMessages = result.Errors.Select(x => x.ErrorMessage).ToList() };
            }
            var employee = await _employeeRepo.Get(x => x.CompanyId == companyId && x.Id == request.EmployeeId).FirstOrDefaultAsync();
            if (employee == null)
                return new ResultModel<EmployeeResponse>("Employee not found");

            employee.FirstName = request.FirstName;
            employee.LastName = request.LastName;
            employee.Phone = request.Phone;
            employee.Address = request.Address;
            employee.Country = request.Country;

            await _unitOfWork.SaveChangesAsync();
            await _bus.Publish(new PublishMessage(JsonSerializer.Serialize(employee.ToEmployeeMessage()), EMPLOYEE_UPDATE_USER));

            return new ResultModel();
        }

        public async Task<ResultModel> UpdateEmployeeStaffId(EmployeeStaffIdDTO request)
        {
            var employee = await _employeeRepo.Get(x => x.Email == request.Email)
                    .Include(x => x.Company)
                    .FirstOrDefaultAsync();
            if (employee == null)
                return new ResultModel<EmployeeResponse>("Employee not found");

            employee.StaffId = $"{employee.Company.Name}{request.StaffId}"; //TODO Generate Staff Id Prefix

            await _unitOfWork.SaveChangesAsync();

            return new ResultModel();
        }

        #region private

        #endregion
    }

    public static class MapperExtensions
    {
        public static QualificationResponse ToQualificationResponse(this Qualification source)
        {
            return new QualificationResponse(
                    source.Id,
                    source.Title,
                    source.Description,
                    source.QualificationType.ToString(),
                    source.EducationLevel.ToString(),
                    source.DateReceived,
                    source.ExpiryDate,
                    source?.MediaFile?.Path,
                    source?.MediaFileId
                    );
        }

        public static EmployeeResponse ToEmployeeResponse(this Employee source)
        {
            return new EmployeeResponse(
                    source.Id,
                    source.FirstName,
                    source.LastName,
                    source.Email,
                    source.Address,
                    source.Phone,
                    source.Country,
                    source.StaffId
                );
        }

        public static EmployeeDetailResponse ToEmployeeDetailsResponse(this Employee source)
        {
            return new EmployeeDetailResponse(
                    source.Id,
                    source.FirstName,
                    source.LastName,
                    source.Email,
                    source.Address,
                    source.Phone,
                    source.Country,
                    source.StaffId,
                    source.Qualifications.Select(x => x.ToQualificationResponse())
                );
        }

        public static EmployeeDTO ToEmployeeMessage(this Employee source)
        {
            return new EmployeeDTO(
                    source.Email,
                    source.FirstName,
                    source.LastName,
                    source.Phone
                );
        }
    }
}
