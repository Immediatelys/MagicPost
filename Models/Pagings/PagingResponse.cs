namespace MagicPost.Models.Pagings
{
    public class PagingResponse<TData>
    {
        public int Total { get; set; }
        public IEnumerable<TData> Datas { get; set; }
        public PagingResponse(int total, IEnumerable<TData> datas)
        {
            Total = total;
            Datas = datas;
        }
    }
}
