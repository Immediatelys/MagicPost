using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MagicPost.Datas
{
    [Table("thanhpho")]
    public class ThanhPho
    {
        [Key]
        [Column("ma")]
        public string Ma { get; set; }
        [Column("ten")]
        public string Ten { get; set; }
    }

    [Table("quanhuyen")]
    public class QuanHuyen
    {
        [Key]
        [Column("ma")]
        public string Ma { get; set; }
        [Column("mathanhpho")]
        public string MaThanhPho { get; set; }
        [Column("ten")]
        public string Ten { get; set; }
    }

    [Table("xaphuong")]
    public class XaPhuong
    {
        [Key]
        [Column("ma")]
        public string Ma { get; set; }
        [Column("mathanhpho")]
        public string MaThanhPho { get; set; }
        [Column("maquanhuyen")]
        public string MaQuanHuyen { get; set; }
        [Column("ten")]
        public string Ten { get; set; }
    }
}
