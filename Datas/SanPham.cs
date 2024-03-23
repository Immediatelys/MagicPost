using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MagicPost.Datas
{
    [Table("sanpham")]
    public class SanPham
    {
        [Key]
        [Column("ma")]
        public int Ma { get; set; }
        [Column("ten")]
        public string Ten { get; set; }
        [Column("trongluong", TypeName = "float")]
        public double TrongLuong { get; set; }
        [Column("trangthai")]
        public string TrangThai { get; set; }
        [Column("madiemgiaodich")]
        public int? MaDiemGiaoDich { get; set; }
        [Column("madiemtapket")]
        public int? MaDiemTapKet { get; set; }
        [Column("mataikhoan")]
        public int? MaTaiKhoan { get; set; }

        [ForeignKey(nameof(MaDiemGiaoDich))]
        public DiemGiaoDich DiemGiaoDich { get; set; }
        [ForeignKey(nameof(MaDiemTapKet))]
        public DiemTapKet DiemTapKet { get; set; }
        [ForeignKey(nameof(MaTaiKhoan))]
        public TaiKhoan TaiKhoan { get; set; }
    }
}
