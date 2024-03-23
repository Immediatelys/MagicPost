namespace MagicPost.Models.Pagings
{
    public class HoaDonPaging : PagingRequest
    {
        public DateTime? SearchDate { get; set; }
        public int? DiemGiaoDich { get; set; }
        public int? DiemTapKet { get; set; }
    }
}
