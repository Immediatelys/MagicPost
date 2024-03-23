using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MagicPost.Datas
{
    [Table("donvanchuyen")]
    public class DonVanChuyen
    {
        [Key]
        [Column("ma")]
        public int Ma { get; set; }
        [Column("thoigianvanchuyen")]
        public DateTime ThoiGianVanChuyen { get; set; }
        [Column("thoigiandenkho")]
        public DateTime? ThoiGianDenKho { get; set; }
        [Column("trangthai")]
        public string TrangThai { get; set; }
        [Column("diemtapketdi")]
        public int? MaDiemTapKetDi { get; set; }
        [Column("diemtapketden")]
        public int? MaDiemTapKetDen { get; set; }
        [Column("diemgiaodichdi")]
        public int? MaDiemGiaoDichDi { get; set; }
        [Column("diemgiaodichden")]
        public int? MaDiemGiaoDichDen { get; set; }
        [Column("nguoigiao")]
        public int MaNguoiGiao { get; set; }
        [Column("nguoixacnhan")]
        public int? NguoiXacNhan { get; set; }
        [NotMapped]
        public IEnumerable<int> MaSanPhams { get; set; }

        [ForeignKey(nameof(MaDiemTapKetDi))]
        public virtual DiemTapKet DiemTapKetDi { get; set; }
        [ForeignKey(nameof(MaDiemTapKetDen))]
        public virtual DiemTapKet DiemTapKetDen { get; set; }
        [ForeignKey(nameof(MaDiemGiaoDichDi))]
        public virtual DiemGiaoDich DiemGiaoDichDi { get; set; }
        [ForeignKey(nameof(MaDiemGiaoDichDen))]
        public virtual DiemGiaoDich DiemGiaoDichDen { get; set; }
        public virtual IEnumerable<DonVanChuyenSanPham> SanPhams { get; set; }
    }
}
