using API.Models;

namespace API.Models
{
    public class ApiResponse<T>
    {
        public bool IsSuccess { get; set; }
        public T? Data { get; set; }
        public string? ErrorMessage { get; set; }
        public PaginationInfo? Pagination { get; set; }
    }
}
