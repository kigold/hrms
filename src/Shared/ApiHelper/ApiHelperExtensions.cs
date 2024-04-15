using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Shared.ViewModels;

namespace Shared.ApiHelper
{
    public static class ApiHelperExtensions
    {
        public static ValidationProblem ValidationProblem(this ResultModel result)
        {
            return TypedResults.ValidationProblem(new Dictionary<string, string[]>() { { "Error", result.ErrorMessages.ToArray() } });
        }
    }
}
