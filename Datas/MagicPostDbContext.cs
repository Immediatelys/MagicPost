using Microsoft.EntityFrameworkCore;

namespace MagicPost.Datas
{
    public class MagicPostDbContext : DbContext
    {
        public DbSet<DiemGiaoDich> DiemGiaoDiches { get; set; }
        public DbSet<DiemTapKet> DiemTapKets { get; set; }
        public DbSet<DonGiao> DonGiaos { get; set; }
        public DbSet<DonNhan> DonNhans { get; set; }
        public DbSet<SanPhamDonNhan> SanPhamDonNhans { get; set; }
        public DbSet<DonVanChuyen> DonVanChuyens { get; set; }
        public DbSet<DonVanChuyenSanPham> DonVanChuyenSanPhams { get; set; }
        public DbSet<SanPham> SanPhams { get; set; }
        public DbSet<TaiKhoan> TaiKhoans { get; set; }
        public DbSet<ThanhPho> ThanhPhos { get; set; }
        public DbSet<QuanHuyen> QuanHuyens { get; set; }
        public DbSet<XaPhuong> XaPhuongs { get; set; }
        public DbSet<VaiTro> VaiTros { get; set; }

        public MagicPostDbContext(DbContextOptions<MagicPostDbContext> options) : base(options)
        { }
    }
}
