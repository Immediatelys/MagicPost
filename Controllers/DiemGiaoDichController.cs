using MagicPost.Datas;
using MagicPost.Models.Pagings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace MagicPost.Controllers
{
    [Route("api/diem-giao-dich")]
    [ApiController]
    [Authorize]
    public class DiemGiaoDichController : ControllerBase
    {
        private MagicPostDbContext _context;

        public DiemGiaoDichController(MagicPostDbContext context)
        {
            _context = context;
        }

		/// <summary>
		/// Lấy danh sách tất cả điểm giao dịch
		/// </summary>
		[HttpGet("tat-ca")]
        public async Task<IActionResult> GetAll()
        {
            var res = await _context.DiemGiaoDiches.ToListAsync();
            return Ok(res);
        }

		/// <summary>
		/// Lấy danh sách điểm giao dịch có phân trang và tìm kiếm
		/// </summary>
		[HttpGet("")]
        [Authorize(Roles = "truonggiaodich")]
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
		/// Lấy danh sách nhân viên tại điểm giao dịch
		/// </summary>
		[HttpGet("{ma}/nhan-vien")]
        [Authorize(Roles = "giamdoc,truonggiaodich")]
        public async Task<IActionResult> NhanVien(int ma)
        {
            var result = await _context.TaiKhoans.Where(x => x.DiemGiaoDich == ma).ToListAsync();
            return Ok(result);
        }

		/// <summary>
		/// Tìm kiếm điểm giao dịch bằng mã
		/// </summary>
		[HttpGet("{ma}")]
        [Authorize(Roles = "giamdoc")]
        public async Task<IActionResult> GetByMa(int ma)
        {
            var result = await _context.DiemGiaoDiches.FindAsync(ma);
            if(result == null)
                return NotFound();

            return Ok(result);
        }

		/// <summary>
		/// Tạo điểm giao dịch mới
		/// </summary>
		[HttpPost("")]
        [Authorize(Roles = "giamdoc")]
        public async Task<IActionResult> Create([FromBody] DiemGiaoDich model)
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
            
            var result = await _context.DiemGiaoDiches.AddAsync(model);
            await _context.SaveChangesAsync();
            return Ok(result.Entity);
        }

		/// <summary>
		/// Chỉnh sửa thông tin điểm giao dịch
		/// </summary>
		[HttpPut("")]
        [Authorize(Roles = "giamdoc")]
        public async Task<IActionResult> Update([FromBody] DiemGiaoDich model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var diemgiaodich = await _context.DiemGiaoDiches.FirstOrDefaultAsync(x => x.Ma == model.Ma);
            if (diemgiaodich == null)
                return NotFound();

            if (diemgiaodich.MaThanhPho != model.MaThanhPho)
            {
                var thanhpho = await _context.ThanhPhos.FirstOrDefaultAsync(x => x.Ma == model.MaThanhPho);
                if (thanhpho == null)
                {
                    ModelState.AddModelError(nameof(model.MaThanhPho), "Mã thành phố không chính xác!");
                }    
                else
                {
                    diemgiaodich.MaThanhPho = thanhpho.Ma;
                    diemgiaodich.TenThanhPho = thanhpho.Ten;
                }    
            }
            if (diemgiaodich.MaQuanHuyen != model.MaQuanHuyen)
            {
                var quanhuyen = await _context.QuanHuyens.FirstOrDefaultAsync(x => x.Ma == model.MaQuanHuyen && x.MaThanhPho == model.MaThanhPho);
                if (quanhuyen == null)
                {
                    ModelState.AddModelError(nameof(model.MaQuanHuyen), "Mã quận huyện không chính xác!");
                }
                else
                {
                    diemgiaodich.MaQuanHuyen = quanhuyen.Ma;
                    diemgiaodich.TenQuanHuyen = quanhuyen.Ten;
                }
            }
            if (diemgiaodich.MaXaPhuong != model.MaXaPhuong)
            {
                var xaphuong = await _context.XaPhuongs.FirstOrDefaultAsync(x => x.Ma == model.MaXaPhuong && x.MaQuanHuyen == model.MaQuanHuyen && x.MaThanhPho == model.MaThanhPho);
                if (xaphuong == null)
                {
                    ModelState.AddModelError(nameof(model.MaThanhPho), "Mã xã phường không chính xác!");
                }
                else
                {
                    diemgiaodich.MaXaPhuong = xaphuong.Ma;
                    diemgiaodich.TenXaPhuong = xaphuong.Ten;
                }
            }
            if (!ModelState.IsValid)
                return BadRequest(ModelState.MappingError(HttpContext.TraceIdentifier));

            diemgiaodich.Ten = model.Ten;
            diemgiaodich.DiaChi = model.DiaChi;
            _context.Entry(diemgiaodich).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return Ok(diemgiaodich);
        }

		/// <summary>
		/// Xóa điểm giao dịch
		/// </summary>
		[HttpDelete("{ma}")]
        [Authorize(Roles = "giamdoc")]
        public async Task<IActionResult> Delete(int ma)
        {
            var diemgiaodich = await _context.DiemGiaoDiches.FirstOrDefaultAsync(x => x.Ma == ma);
            if (diemgiaodich == null)
                return NotFound();

            var nhanviens = (await _context.TaiKhoans.Where(x => x.DiemGiaoDich == ma).ToListAsync()).Select(x => { x.DiemGiaoDich = null; return x; });
            diemgiaodich.BiXoa = true;

            using(var transaction = await _context.Database.BeginTransactionAsync())
            {
                _context.Entry(diemgiaodich).State = EntityState.Modified;
                _context.TaiKhoans.UpdateRange(nhanviens);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }    

            return NoContent();
        }

		/// <summary>
		/// Lấy tất cả sản phẩm tại điểm giao dịch
		/// </summary>
		[HttpGet("{ma}/san-pham")]
        [Authorize(Roles = "giamdoc,truonggiaodich,nvgiaodich")]
        public async Task<IActionResult> Sanpham(int ma)
        {
            var sanphams = await _context.SanPhams.Where(x => x.MaDiemGiaoDich == ma).ToListAsync();
            return Ok(sanphams);
        }
    }
}
