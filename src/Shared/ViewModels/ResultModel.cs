using System.ComponentModel.DataAnnotations;

namespace Shared.ViewModels
{
    public class ResultModel 
    {
        public ResultModel() { }

        public ResultModel(string errorMessage)
        {
            AddError(errorMessage);
        }

        public ResultModel(List<string> errorMessage)
        {
            errorMessage.AddRange(errorMessage);
        }

        public List<string> ErrorMessages { get; set; } = new List<string>();

        public string Message { get; set; }

        public bool HasError
        {
            get
            {
                if (ErrorMessages.Count > 0)
                {
                    return true;
                }

                return false;
            }
        }

        public void AddError(string error)
        {
            ErrorMessages.Add(error);
        }
    }

    public class ResultModel<T> : ResultModel
    {
        public ResultModel() { }
        public ResultModel(T data)
        {
            Data = data;
        }
        public T Data { get; set; } = default;
        public ResultModel(T data, string message = "")
        {
            Data = data;
            Message = message;
        }
        public ResultModel(string errorMessage)
        {
            AddError(errorMessage);
        }

        public ResultModel(List<string> errorMessage)
        {
            errorMessage.AddRange(errorMessage);
        }

    }
}
