using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MagicPost.Datas
{
    [Table("vaitro")]
    public class VaiTro
    {
        [Key]
        [Column("ma")]
        public string Ma { get; set; }
        [Column("ten")]
        public string Ten { get; set; }

        public virtual IEnumerable<TaiKhoan> TaiKhoans { get; set; }
    }
}
