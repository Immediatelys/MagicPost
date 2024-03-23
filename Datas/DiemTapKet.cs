using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MagicPost.Datas
{
    [Table("diemtapket")]
    public class DiemTapKet
    {
        [Key]
        [Column("ma")]
        public int Ma { get; set; }
        [Column("ten")]
        [Required(ErrorMessage = "Tên điểm tập kết không được bỏ trống")]
        public string Ten { get; set; }
        [Column("mathanhpho")]
        [Required(ErrorMessage = "Vui lòng chọn thành phố")]
        public string MaThanhPho { get; set; }
        [Column("tenthanhpho")]
        public string TenThanhPho { get; set; }
        [Column("maquanhuyen")]
        [Required(ErrorMessage = "Vui lòng chọn quận huyện")]
        public string MaQuanHuyen { get; set; }
        [Column("tenquanhuyen")]
        public string TenQuanHuyen { get; set; }
        [Column("maxaphuong")]
        [Required(ErrorMessage = "Vui lòng chọn xã phường")]
        public string MaXaPhuong { get; set; }
        [Column("tenxaphuong")]
        public string TenXaPhuong { get; set; }
        [Column("diachi")]
        [Required(ErrorMessage = "Địa chỉ không được bỏ trống")]
        public string DiaChi { get; set; }
        public bool BiXoa { get; set; }
    }
}
