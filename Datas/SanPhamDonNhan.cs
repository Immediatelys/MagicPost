using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MagicPost.Datas
{
    [Table("sanphamdonnhan")]
    public class SanPhamDonNhan
    {
        [Key]
        [Column("ma")]
        public int Ma { get; set; }
        [Column("masanpham")]
        public int MaSanPham { get; set; }
        [Column("ten")]
        [Required(ErrorMessage = "Tên sản phẩm không được bỏ trống")]
        public string TenSanPham { get; set; }
        [Column("trongluong")]
        [Required(ErrorMessage = "Trọng lượng sản phẩm không được bỏ trống")]
        public double TrongLuong { get; set; }
        [Column("tennguoinhan")]
        [Required(ErrorMessage = "Tên người nhận không được bỏ trống")]
        public string TenNguoiNhan { get; set; }
        [Column("diachi")]
        [Required(ErrorMessage = "Địa chỉ người nhận không được bỏ trống")]
        public string DiaChi { get; set; }
        [Column("dienthoai")]
        [Required(ErrorMessage = "Số điện thoại người nhận không được bỏ trống")]
        public string DienThoai { get; set; }
        [Column("diemgiaodich")]
        [Required(ErrorMessage = "Điểm giao hàng không được bỏ trống")]
        public int ToiDiemGiaoDich { get; set; }
        [Column("giacuoc")]
        [Required(ErrorMessage = "Giá cước vận chuyển không được bỏ trống")]
        public int GiaCuoc { get; set; }
        [Column("VAT")]
        public int VAT { get; set; }
        [Column("khoankhac")]
        public int KhoanKhac { get; set; }
        [Column("cod")]
        public int Cod { get; set; }
        [Column("thukhac")]
        public int ThuKhac { get; set; }
        [Column("madonnhan")]
        public int MaDonNhan { get; set; }

        [ForeignKey(nameof(ToiDiemGiaoDich))]
        public virtual DiemGiaoDich DiemGiaoHang { get; set; }
        [ForeignKey(nameof(MaDonNhan))]
        public virtual DonNhan DonNhan { get; set; }
    }
}
