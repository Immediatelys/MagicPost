using MagicPost.Datas;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MagicPost.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/thong-ke")]
    public class ThongKeController : ControllerBase
    {
        private readonly MagicPostDbContext _context;

        public ThongKeController(MagicPostDbContext context)
        {
            _context = context;
        }

		/// <summary>
		/// Thống kê số lượng sản phẩm tại điểm giao dịch, điểm tập kết theo ngày
		/// </summary>
		[HttpGet]
        public async Task<IActionResult> Index(int? diemgiaodich = null, int? diemtapket = null, DateTime? ngay = null)
        {
            if(ngay == null)
                ngay = DateTime.Now;
            var ngayquery = new[] { ngay.Value.AddDays(-2).Date, ngay.Value.AddDays(-1).Date, ngay.Value.Date };
            var donnhanquery = _context.DonNhans.Include(x => x.SanPhams).Where(x => ngayquery.Contains(x.ThoiGianNhan.Date));
            if (diemgiaodich.HasValue)
                donnhanquery = donnhanquery.Where(x => x.MaDiemNhanHang == diemgiaodich);
            var dongiaoquery = _context.DonGiaos.Where(x => ngayquery.Contains(x.ThoiGianGui));
            if (diemgiaodich.HasValue)
                dongiaoquery = dongiaoquery.Where(x => x.DiemGui == diemgiaodich);
            var donchuyenquery = _context.DonVanChuyens.Include(x => x.SanPhams).Where(x => (ngayquery.Contains(x.ThoiGianVanChuyen)) || (x.ThoiGianDenKho.HasValue && ngayquery.Contains(x.ThoiGianDenKho.Value.Date)));
            if (diemtapket.HasValue)
                donchuyenquery = _context.DonVanChuyens.Where(x => x.MaDiemGiaoDichDen == diemtapket);
            var sanphamquery = _context.SanPhams.AsNoTracking();
            if (diemgiaodich.HasValue)
                sanphamquery = sanphamquery.Where(x => x.MaDiemGiaoDich == diemgiaodich);
            if(diemtapket.HasValue)
                sanphamquery = sanphamquery.Where(x => x.MaDiemTapKet == diemtapket);

            var donnhans = donnhanquery.ToList();
            var dongiaos = dongiaoquery.ToList();
            var donchuyen = donchuyenquery.ToList();
            var sanphams = sanphamquery.ToList();

            if(diemgiaodich.HasValue)
            {
                var res = ngayquery.Select(x => new
                {
                    sanphamnhan = donnhans.Where(d => d.ThoiGianNhan.Date == x.Date).Sum(d => d.SanPhams.Count()),
                    sanphamgiao = dongiaos.Count(d => d.ThoiGianGui.Date == x.Date)
                });
                return Ok(res);
            }    
            if(diemtapket.HasValue)
            {
                var res = ngayquery.Select(x => new
                {
                    sanphamnhan = donchuyen.Where(d => d.MaDiemTapKetDen == diemtapket && d.ThoiGianDenKho.HasValue && d.ThoiGianDenKho.Value.Date == x.Date).Sum(d => d.SanPhams.Count()),
                    sanphamgiao = donchuyen.Where(d => d.MaDiemTapKetDi == diemtapket && d.ThoiGianVanChuyen.Date == x.Date).Sum(d => d.SanPhams.Count())
                });
                return Ok(res);
            }
            var data = ngayquery.Select(x => new
            {
                sanphamnhan = donnhans.Where(d => d.ThoiGianNhan.Date == x.Date).Sum(d => d.SanPhams.Count()),
                sanphamgiao = dongiaos.Count(d => d.ThoiGianGui.Date == x.Date),
                tonkho = sanphams.Count()
            });

            return Ok(data);
        }
    }
}
