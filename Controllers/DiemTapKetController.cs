using MagicPost.Datas;
using MagicPost.Models.Pagings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MagicPost.Controllers
{
    [Route("api/diem-tap-ket")]
    [ApiController]
    [Authorize]
    public class DiemTapKetController : ControllerBase
    {
        private MagicPostDbContext _context;

        public DiemTapKetController(MagicPostDbContext context)
        {
            _context = context;
        }

		/// <summary>
		/// Danh sách tất cả điểm tập kết
		/// </summary>
		[HttpGet("tat-ca")]
        public async Task<ActionResult> GetAll()
        {
            var res = await _context.DiemTapKets.ToListAsync();
            return Ok(res);
        }

		/// <summary>
		/// Danh sách điểm tập kết có phân trang và tìm kiếm
		/// </summary>
		[HttpGet("")]
        [Authorize(Roles = "giamdoc")]
        public async Task<IActionResult> Paging([FromQuery] DiemPaging model)
        {
            var query = _context.DiemGiaoDiches.AsNoTracking();
            if (!string.IsNullOrEmpty(model.SearchKey))
                query = query.Where(x => x.Ten.ToLower().Contains(model.SearchKey.ToLower()));
            if (!string.IsNullOrEmpty(model.MaThanhPho))
                query = query.Where(x => x.MaThanhPho == model.MaThanhPho);
            if (!string.IsNullOrEmpty(model.MaXaPhuong))
                query = query.Where(x => x.MaXaPhuong == model.MaXaPhuong);
            if (!string.IsNullOrEmpty(model.MaQuanHuyen))
                query = query.Where(x => x.MaQuanHuyen == model.MaQuanHuyen);

            var total = query.Count();
            var data = query.Skip((model.PageIndex - 1) * model.PageSize).Take(model.PageSize).ToList();
            return Ok(new PagingResponse<DiemGiaoDich>(total, data));
        }

		/// <summary>
		/// Danh sách nhân viên tại điểm tập kết
		/// </summary>
		[HttpGet("{ma}/nhan-vien")]
        [Authorize(Roles = "giamdoc,truongtapket")]
        public async Task<IActionResult> NhanVien(int ma)
        {
            var result = await _context.TaiKhoans.Where(x => x.DiemTapKet == ma).ToListAsync();
            return Ok(result);
        }

		/// <summary>
		/// Tìm kiếm điểm tập kết theo mã
		/// </summary>
		[HttpGet("{ma}")]
        [Authorize(Roles = "giamdoc")]
        public async Task<IActionResult> GetByMa(int ma)
        {
            var result = await _context.DiemTapKets.FindAsync(ma);
            if (result == null)
                return NotFound();

            return Ok(result);
        }

		/// <summary>
		/// Tạo điểm tập kết
		/// </summary>
		[HttpPost("")]
        [Authorize(Roles = "giamdoc")]
        public async Task<IActionResult> Create([FromBody] DiemTapKet model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var thanhpho = await _context.ThanhPhos.FirstOrDefaultAsync(x => x.Ma == model.MaThanhPho);
            var quanhuyen = await _context.QuanHuyens.FirstOrDefaultAsync(x => x.Ma == model.MaQuanHuyen && x.MaThanhPho == model.MaThanhPho);
            var xaphuong = await _context.XaPhuongs.FirstOrDefaultAsync(x => x.Ma == model.MaXaPhuong && x.MaQuanHuyen == model.MaQuanHuyen && x.MaThanhPho == model.MaThanhPho);
            if (thanhpho == null)
                ModelState.AddModelError(nameof(model.MaThanhPho), "Mã thành phố không chính xác!");
            if (quanhuyen == null)
                ModelState.AddModelError(nameof(model.MaQuanHuyen), "Mã quận huyện không chính xác!");
            if (xaphuong == null)
                ModelState.AddModelError(nameof(model.MaXaPhuong), "Mã xã phường không chính xác!");
            if (!ModelState.IsValid)
                return BadRequest(ModelState.MappingError(HttpContext.TraceIdentifier));

            model.TenThanhPho = thanhpho.Ten;
            model.TenQuanHuyen = quanhuyen.Ten;
            model.TenXaPhuong = xaphuong.Ten;

            var result = await _context.DiemTapKets.AddAsync(model);
            await _context.SaveChangesAsync();
            return Ok(result.Entity);
        }

		/// <summary>
		/// Chỉnh sửa thông tin điểm tập kết
		/// </summary>
		[HttpPut("")]
        [Authorize(Roles = "giamdoc")]
        public async Task<IActionResult> Update([FromBody] DiemTapKet model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var diemtapket = await _context.DiemTapKets.FirstOrDefaultAsync(x => x.Ma == model.Ma);
            if (diemtapket == null)
                return NotFound();

            if (diemtapket.MaThanhPho != model.MaThanhPho)
            {
                var thanhpho = await _context.ThanhPhos.FirstOrDefaultAsync(x => x.Ma == model.MaThanhPho);
                if (thanhpho == null)
                {
                    ModelState.AddModelError(nameof(model.MaThanhPho), "Mã thành phố không chính xác!");
                }
                else
                {
                    diemtapket.MaThanhPho = thanhpho.Ma;
                    diemtapket.TenThanhPho = thanhpho.Ten;
                }
            }
            if (diemtapket.MaQuanHuyen != model.MaQuanHuyen)
            {
                var quanhuyen = await _context.QuanHuyens.FirstOrDefaultAsync(x => x.Ma == model.MaQuanHuyen && x.MaThanhPho == model.MaThanhPho);
                if (quanhuyen == null)
                {
                    ModelState.AddModelError(nameof(model.MaQuanHuyen), "Mã quận huyện không chính xác!");
                }
                else
                {
                    diemtapket.MaQuanHuyen = quanhuyen.Ma;
                    diemtapket.TenQuanHuyen = quanhuyen.Ten;
                }
            }
            if (diemtapket.MaXaPhuong != model.MaXaPhuong)
            {
                var xaphuong = await _context.XaPhuongs.FirstOrDefaultAsync(x => x.Ma == model.MaXaPhuong && x.MaQuanHuyen == model.MaQuanHuyen && x.MaThanhPho == model.MaThanhPho);
                if (xaphuong == null)
                {
                    ModelState.AddModelError(nameof(model.MaThanhPho), "Mã xã phường không chính xác!");
                }
                else
                {
                    diemtapket.MaXaPhuong = xaphuong.Ma;
                    diemtapket.TenXaPhuong = xaphuong.Ten;
                }
            }
            if (!ModelState.IsValid)
                return BadRequest(ModelState.MappingError(HttpContext.TraceIdentifier));

            diemtapket.Ten = model.Ten;
            diemtapket.DiaChi = model.DiaChi;
            _context.Entry(diemtapket).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return Ok(diemtapket);
        }

		/// <summary>
		/// Xóa điểm tập kết
		/// </summary>
		[HttpDelete("{ma}")]
        [Authorize(Roles = "giamdoc")]
        public async Task<IActionResult> Delete(int ma)
        {
            var diemtapket = await _context.DiemTapKets.FirstOrDefaultAsync(x => x.Ma == ma);
            if (diemtapket == null)
                return NotFound();

            var nhanviens = (await _context.TaiKhoans.Where(x => x.DiemTapKet == ma).ToListAsync()).Select(x => { x.DiemTapKet = null; return x; });
            diemtapket.BiXoa = true;

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                _context.Entry(diemtapket).State = EntityState.Modified;
                _context.TaiKhoans.UpdateRange(nhanviens);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }

            return NoContent();
        }

		/// <summary>
		/// Lấy danh sách sản phẩm tại điểm tập kết
		/// </summary>
		[HttpGet("{ma}/san-pham")]
        [Authorize(Roles = "giamdoc,truongtapket,nvtapket")]
        public async Task<IActionResult> Sanpham(int ma)
        {
            var sanphams = await _context.SanPhams.Where(x => x.MaDiemTapKet == ma).ToListAsync();
            return Ok(sanphams);
        }
    }
}
