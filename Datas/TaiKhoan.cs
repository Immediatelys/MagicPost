using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MagicPost.Datas
{
    [Table("taikhoan")]
    public class TaiKhoan
    {
        [Key]
        [Column("ma")]
        public int Ma { get; set; }
        [Column("ten")]
        [Required(ErrorMessage = "Tên không được bỏ trống")]
        public string Ten { get; set; }
        [Column("tendangnhap")]
        [Required(ErrorMessage = "Tên đăng nhập không được bỏ trống")]
        public string TenDangNhap { get; set; }
        [Column("matkhau")]
        [Required(ErrorMessage = "Mật khẩu không được bỏ trống")]
        public string MatKhau { get; set; }
        [Column("email")]
        public string Email { get; set; }
        [Column("sodienthoai")]
        public string SoDienThoai { get; set; }
        [Column("ngaysinh")]
        public DateTime NgaySinh { get; set; }
        [Column("diachi")]
        public string DiaChi { get; set; }
        [Column("diemgiaodich")]
        public int? DiemGiaoDich { get; set; }
        [Column("diemtapket")]
        public int? DiemTapKet { get; set; }
        [Column("mavaitro")]
        public string MaVaiTro { get; set; }
        [NotMapped]
        public string TenVaiTro { get; set; }

        [ForeignKey(nameof(MaVaiTro))]
        public virtual VaiTro VaiTro{ get; set; }

        [NotMapped]
        public string TenDiemGiaoDich { get; set; }
        [NotMapped]
        public string TenDiemTapKet { get; set; }
    }
}
