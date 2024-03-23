using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MagicPost.Datas
{
    [Table("donnhan")]
    public class DonNhan
    {
        [Key]
        [Column("ma")]
        public int Ma { get; set; }
        [Column("tennguoigui")]
        [Required(ErrorMessage = "Tên người gửi không được bỏ trống")]
        public string TenNguoiGui { get; set; }
        [Column("diachi")]
        [Required(ErrorMessage = "Địa chỉ người gửi không được bỏ trống")]
        public string DiaChi { get; set; }
        [Column("dienthoai")]
        [Required(ErrorMessage = "Số điện thoại người gửi không được bỏ trống")]
        public string DienThoai { get; set; }
        [Column("xulykhikhonggiaoduoc")]
        [Required(ErrorMessage = "Cách xử lý khi không giao được không được bỏ trống")]
        public string XulyKhiKhongGiaoDuoc{ get; set; }
        [Column("ghichu")]
        public string GhiChu { get; set; }
        [Column("thoigiannhan")]
        public DateTime ThoiGianNhan { get; set; }
        [Column("diemnhanhang")]
        public int MaDiemNhanHang { get; set; }
        [Column("mataikhoan")]
        public int MaTaiKhoan { get; set; }
        [Column("nguoinhan")]
        public int NhanVienNhanHang { get; set; }

        [ForeignKey(nameof(MaDiemNhanHang))]
        public virtual DiemGiaoDich DiemNhanHang { get; set; }
        [ForeignKey(nameof(MaTaiKhoan))]
        public virtual TaiKhoan NguoiGui { get; set; }
        [ForeignKey(nameof(NhanVienNhanHang))]
        public virtual TaiKhoan NguoiNhan { get; set; }
        [Required(ErrorMessage = "Danh sách sản phẩm nhận không được bỏ trống")]
        public virtual IEnumerable<SanPhamDonNhan> SanPhams { get; set; }
    }
}
