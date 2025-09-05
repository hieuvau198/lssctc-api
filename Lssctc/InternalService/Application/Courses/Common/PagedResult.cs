namespace InternalService.Application.Courses.Common
{
    public class PagedResult<T>
    {
        public IEnumerable<T> Items { get; set; } = default!;
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    }
}
