using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MagicPost.Datas
{
    [Table("dongiao")]
    public class DonGiao
    {
        [Key]
        [Column("ma")]
        public int Ma { get; set; }
        [Column("tennguoigui")]
        public string TenNguoiGui { get; set; }
        [Column("diachigui")]
        public string DiaChiGui { get; set; }
        [Column("dienthoaigui")]
        public string DienThoaiGui { get; set; }
        [Column("diemgui")]
        public int DiemGui { get; set; }
        [Column("tennguoinhan")]
        public string TenNguoiNhan { get; set; }
        [Column("diachinhan")]
        public string DiaChiNhan { get; set; }
        [Column("dienthoainhan")]
        public string DienThoaiNhan { get; set; }
        [Column("diemnhan")]
        public int DiemNhan { get; set; }
        [Column("xulykhikhonggiaoduoc")]
        public string XuLyKhiKhongGiaoDuoc { get; set; }
        [Column("ghichu")]
        public string GhiChu { get; set; }
        [Column("masanpham")]
        public int MaSanPham { get; set; }
        [Column("tensanpham")]
        public string TenSanPham { get; set; }
        [Column("trongluong", TypeName = "float")]
        public double TrongLuong { get; set; }
        [Column("giacuoc")]
        public int GiaCuoc { get; set; }
        [Column("VAT")]
        public int VAT { get; set; }
        [Column("khoankhac")]
        public int KhoanKhac { get; set; }
        [Column("cod")]
        public int COD { get; set; }
        [Column("thukhac")]
        public int ThuKhac { get; set; }
        [Column("thoigiangui")]
        public DateTime ThoiGianGui { get; set; }
        [Column("thoigiannha")]
        public DateTime ThoiGianNhan { get; set; }

        [ForeignKey(nameof(DiemGui))]
        public virtual DiemGiaoDich DiemGiaoDichGuiHang { get; set; }
        [ForeignKey(nameof(DiemNhan))]
        public virtual DiemGiaoDich DiemGiaoDichNhanHang { get; set; }
    }
}
