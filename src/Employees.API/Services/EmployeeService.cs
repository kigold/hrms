using Employees.API.Data.Models;
using Employees.API.Models.Requests;
using Employees.API.Models.Responses;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Shared.Messaging;
using Shared.Pagination;
using Shared.Repositories;
using Shared.ViewModels;
using System.Text.Json;

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

        public Task<ResultModel<PagedList<EmployeeResponse>>> GetEmployee(int companyId, PagedRequest query);
    }

    public class EmployeeService : IEmployeeService 
    {
        private readonly IRepository<Employee, long> _employeeRepo;
        private readonly IRepository<Qualification, long> _qualificationRepo;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IBus _bus;

        public EmployeeService(
            IRepository<Employee, long> employeeRepo,
            IRepository<Qualification, long> qualificationRepo,
            IUnitOfWork unitOfWork,
            IBus bus) 
        {
            _employeeRepo = employeeRepo;
            _qualificationRepo = qualificationRepo;
            _unitOfWork = unitOfWork;
            _bus = bus;
        }

        public async Task<ResultModel<QualificationResponse>> AddQualification(AddEmployeeQualification request)
        {
            var validator = new AddEmployeeQualificationValidator();
            var result = validator.Validate(request);
            if (!result.IsValid)
            {
                return new ResultModel<QualificationResponse>() { ErrorMessages = result.Errors.Select(x => x.ErrorMessage).ToList() };
            }

            //TODO Upload File and Get Id

            var qualification = new Qualification
            {
                EmployeeId = request.EmployeeId,
                DateReceived = request.DateReceived,
                EducationLevel = request.EducationLevel,
                ExpiryDate = request.ExpiryDate,
                Title = request.Title,
                Description = request.Description,
                MediaFileId = null,//TODO
                QualificationType = request.QualificationType
            };
            _qualificationRepo.Insert(qualification);
            await _unitOfWork.SaveChangesAsync();

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
            var message = JsonSerializer.Serialize(employee.ToEmployeeMessage());
            await _bus.Publish(new PublishMessage(message, PubMessageType.CreateUser));

            return new ResultModel<EmployeeResponse>(employee.ToEmployeeResponse());
        }

        public async Task<ResultModel> DeleteEmployee(int companyId, long employeeId)
        {
            var employee = await _employeeRepo.Get(x => x.CompanyId == companyId && x.Id == employeeId).FirstOrDefaultAsync();
            if (employee == null)
                return new ResultModel<EmployeeResponse>("Employee not found");

            _employeeRepo.Delete(employeeId);
            await _unitOfWork.SaveChangesAsync();
            var message = JsonSerializer.Serialize(employee.ToEmployeeMessage());
            await _bus.Publish(new PublishMessage(message, PubMessageType.DeleteUser));

            return new ResultModel();
        }

        public async Task<ResultModel<PagedList<EmployeeResponse>>> GetEmployee(int companyId, PagedRequest query)
        {
            var employees = _employeeRepo.Get(x => x.CompanyId == companyId).Include(x => x.Qualifications);

            var result = PagedList<Employee>.ToPagedList(employees, query.PageNumber, query.PageSize);

            return new ResultModel<PagedList<EmployeeResponse>>(new PagedList<EmployeeResponse>(result.Items.Select(x => x.ToEmployeeResponse()), result.TotalCount, query.PageNumber, query.PageSize));
        }

        public async Task<ResultModel> RemoveQualification(long qualificationId)
        {
            _qualificationRepo.Delete(qualificationId);

            await _unitOfWork.SaveChangesAsync();

            return new ResultModel();
        }

        public async Task<ResultModel> UpdateEmployee(int companyId, UpdateEmployee request)
        {
            var employee = await _employeeRepo.Get(x => x.CompanyId == companyId && x.Id == request.EmployeeId).FirstOrDefaultAsync();
            if (employee == null)
                return new ResultModel<EmployeeResponse>("Employee not found");

            employee.FirstName = request.FirstName;
            employee.LastName = request.LastName;
            employee.Phone = request.Phone;
            employee.Address = request.Address;
            employee.Country = request.Country;

            await _unitOfWork.SaveChangesAsync();

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
                    source.Title,
                    source.Description,
                    source.QualificationType.ToString(),
                    source.EducationLevel.ToString(),
                    source.DateReceived,
                    source.ExpiryDate);
        }

        public static EmployeeResponse ToEmployeeResponse(this Employee source)
        {
            return new EmployeeResponse(
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
