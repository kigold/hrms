using Employees.API.Models.Requests;
using Employees.API.Models.Responses;
using Employees.API.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http.HttpResults;
using Shared.ApiHelper;
using Shared.Auth;
using Shared.Pagination;
using Shared.Permissions;
using System.Linq;

namespace Employees.API.Apis
{
    public static class EmployeeApi
    {
        private const string PATH_BASE = "employee";
        private const string TAGS = "employee api";

        public static WebApplication MapEmployeeEndpoints(this WebApplication app)
        {
            var employeeGroup = app.MapGroup(PATH_BASE).WithTags(TAGS)
                .RequireAuthorization(new AuthorizationOptions { AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme });

            employeeGroup.MapPost("/", CreatedEmployee).RequireAuthorization(Permission.EMPLOYEE_CREATE.ToString());
            employeeGroup.MapPut("/", UpdateEmployee).RequireAuthorization(Permission.EMPLOYEE_UPDATE.ToString());
            employeeGroup.MapPut("/qualification/add", AddQualificationEmployee).RequireAuthorization(Permission.EMPLOYEE_UPDATE.ToString());
            employeeGroup.MapPut("/qualification/remove/{qualificationId}", RemoveQualificationEmployee).RequireAuthorization(Permission.EMPLOYEE_UPDATE.ToString());
            employeeGroup.MapDelete("/{id}", DeleteEmployee).RequireAuthorization(Permission.EMPLOYEE_DELETE.ToString());
            employeeGroup.MapGet("/", GetEmployees).RequireAuthorization(Permission.EMPLOYEE_READ.ToString());

            return app;
        }

        //POST /Employee
        public static async Task<Results<Ok<EmployeeResponse>, ValidationProblem>> CreatedEmployee(IEmployeeService employeeService, CreateEmployee request)
        {
            var result = await employeeService.CreateEmployee(request);
            if (result.HasError)
                return result.ValidationProblem();

            return TypedResults.Ok(result.Data);
        }

        //PUT /Employee
        public static async Task<Results<NoContent, ValidationProblem>> UpdateEmployee(IEmployeeService employeeService, UpdateEmployee request)
        {
            var result = await employeeService.UpdateEmployee(1, request);
            if (result.HasError)
                return result.ValidationProblem();

            return TypedResults.NoContent();
        }

        //PUT /Employee/Qualification
        public static async Task<Results<Ok<QualificationResponse>, ValidationProblem>> AddQualificationEmployee(IEmployeeService employeeService, AddEmployeeQualification request)
        {
            var result = await employeeService.AddQualification(request);
            if (result.HasError)
                return result.ValidationProblem();

            return TypedResults.Ok(result.Data);
        }

        //DELETE /Employee/Qualification/{qualificationId}
        public static async Task<Results<NoContent, ValidationProblem>> RemoveQualificationEmployee(IEmployeeService employeeService, long qualificationId)
        {
            var result = await employeeService.RemoveQualification(qualificationId);
            if (result.HasError)
                return result.ValidationProblem();

            return TypedResults.NoContent();
        }

        //DELETE /Employee/{id}
        public static async Task<Results<NoContent, ValidationProblem>> DeleteEmployee(IEmployeeService employeeService, long employeeId)
        {
            var result = await employeeService.DeleteEmployee(1, employeeId);
            if (result.HasError)
                return result.ValidationProblem();

            return TypedResults.NoContent();
        }

        //GET /Employee
        public static async Task<Results<Ok<PagedList<EmployeeResponse>>, ValidationProblem>> GetEmployees(IEmployeeService employeeService, [AsParameters] PagedRequest request)
        {
            var result = await employeeService.GetEmployee(1, request);
            if (result.HasError)
                return result.ValidationProblem();

            Console.WriteLine($"Total Result Count {result.Data.TotalCount}");

            return TypedResults.Ok(result.Data);
        }
    }
}
