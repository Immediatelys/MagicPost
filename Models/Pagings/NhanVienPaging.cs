namespace MagicPost.Models.Pagings
{
	public class NhanVienPaging : PagingRequest
	{
        public int? MaDiemTapKet { get; set; }
        public int? MaDiemGiaoDich { get; set; }
        public string VaiTro { get; set; }
    }
}
