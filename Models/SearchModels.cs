using System.ComponentModel.DataAnnotations;

namespace LampStoreProjects.Models
{
    public class SearchCriteriaModel
    {
        public string? Keyword { get; set; }           // Từ khóa tìm kiếm
        public decimal? MinPrice { get; set; }         // Giá tối thiểu
        public decimal? MaxPrice { get; set; }         // Giá tối đa
        public Guid? CategoryId { get; set; }          // ID danh mục
        public List<string>? Tags { get; set; }        // Danh sách tags
        public bool? Status { get; set; }              // Trạng thái sản phẩm
        public int Page { get; set; } = 1;             // Trang hiện tại
        public int PageSize { get; set; } = 20;        // Số sản phẩm/trang
        public string SortBy { get; set; } = "name";   // Sắp xếp theo
        public string SortOrder { get; set; } = "asc"; // Thứ tự sắp xếp
    }

    public class SearchResultModel
    {
        public List<ProductModel> Products { get; set; } = new();
        public int TotalCount { get; set; }            // Tổng số sản phẩm    
        public int Page { get; set; }                  // Trang hiện tại
        public int PageSize { get; set; }              // Số sản phẩm/trang
        public int TotalPages { get; set; }            // Tổng số trang
        public SearchCriteriaModel Criteria { get; set; } = new(); // Tiêu chí tìm kiếm
    }
}
