namespace MagicPost.Models.Pagings
{
    public class PagingRequest
    {
        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string SearchKey { get; set; }
    }
}
