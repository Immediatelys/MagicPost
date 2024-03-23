using MagicPost.Datas;
using MagicPost.Models.Pagings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MagicPost.Controllers
{
	[ApiController]
	[Authorize]
	[Route("/api/nhan-vien")]
	public class NhanVienController : ControllerBase
	{
		private readonly MagicPostDbContext _context;

		public NhanVienController(MagicPostDbContext context)
		{
			_context = context;
		}

		/// <summary>
		/// Danh sách vai trò
		/// </summary>
		[HttpGet("vai-tro")]
		[Authorize(Roles = "giamdoc")]
		public IActionResult GetVaiTro()
		{
			var vaitro = _context.VaiTros.ToList();
			return Ok(vaitro);
		}

		/// <summary>
		/// Danh sách nhân viên
		/// </summary>
		[HttpGet]
		public IActionResult GetNhanVien([FromQuery] NhanVienPaging model)
		{
			var query = _context.TaiKhoans.Include(x => x.VaiTro).AsNoTracking();
			if (!User.IsInRole("giamdoc"))
			{
				var current = _context.TaiKhoans.FirstOrDefault(x => x.TenDangNhap == User.Identity.Name);
				if (current == null)
					return Unauthorized();
				if (current.DiemTapKet.HasValue)
					query = query.Where(x => x.DiemTapKet == current.DiemTapKet);
				if (current.DiemGiaoDich.HasValue)
					query = query.Where(x => x.DiemGiaoDich == current.DiemGiaoDich);
			}
			if (model.MaDiemTapKet.HasValue)
				query = query.Where(x => x.DiemTapKet == model.MaDiemTapKet);
			if (model.MaDiemGiaoDich.HasValue)
				query = query.Where(x => x.DiemGiaoDich == model.MaDiemGiaoDich);
			if (!string.IsNullOrEmpty(model.SearchKey))
				query = query.Where(x => x.TenDangNhap.ToLower().Contains(model.SearchKey.ToLower()) || x.Ten.ToLower().Contains(model.SearchKey.ToLower())
									|| x.Email.ToLower().Contains(model.SearchKey.ToLower()) || x.SoDienThoai.Contains(model.SearchKey.ToLower()));
			if (!string.IsNullOrEmpty(model.VaiTro))
				query = query.Where(x => x.MaVaiTro == model.VaiTro);
			var total = query.Count();

			var diemgiaodich = _context.DiemGiaoDiches.ToList();
			var diemtapket = _context.DiemTapKets.ToList();
			var data = query.Skip((model.PageIndex - 1) * model.PageSize).Take(model.PageSize).ToList().Select(x => new TaiKhoan
			{
				Ma = x.Ma,
				Ten = x.Ten,
				TenDangNhap = x.TenDangNhap,
				Email = x.Email,
				SoDienThoai = x.SoDienThoai,
				NgaySinh = x.NgaySinh,
				DiaChi = x.DiaChi,
				DiemGiaoDich = x.DiemGiaoDich,
				TenDiemGiaoDich = diemgiaodich.FirstOrDefault(d => d.Ma == x.DiemGiaoDich)?.Ten,
				DiemTapKet = x.DiemTapKet,
				TenDiemTapKet = diemtapket.FirstOrDefault(d => d.Ma == x.DiemTapKet)?.Ten,
				MaVaiTro = x.MaVaiTro,
				TenVaiTro = x.VaiTro?.Ten
			}).ToList();
			return Ok(new PagingResponse<TaiKhoan>(total, data));
		}

		/// <summary>
		/// Lấy danh sách nhân viên theo mã
		/// </summary>
		[HttpGet("{ma}")]
		public async Task<IActionResult> GetNhanVienByMa(int ma)
		{
			var account = await _context.TaiKhoans.Include(x => x.VaiTro).FirstOrDefaultAsync(x => x.Ma == ma);
			if (account == null)
				return NotFound();
			if (account.DiemGiaoDich.HasValue)
			{
				var diemgiaodich = await _context.DiemGiaoDiches.FirstOrDefaultAsync(x => x.Ma == account.DiemGiaoDich);
				account.TenDiemGiaoDich = diemgiaodich?.Ten;
			}
			if (account.DiemTapKet.HasValue)
			{
				var diemtapket = await _context.DiemTapKets.FirstOrDefaultAsync(x => x.Ma == account.DiemTapKet);
				account.TenDiemTapKet = diemtapket?.Ten;
			}
			account.VaiTro.TaiKhoans = null;
			return Ok(account);
		}

		/// <summary>
		/// Tạo nhân viên mới
		/// </summary>
		[HttpPost]
		public async Task<IActionResult> Create([FromBody] TaiKhoan model)
		{
			if (!await _context.VaiTros.AnyAsync(x => x.Ma == model.MaVaiTro))
				ModelState.AddModelError(nameof(model.MaVaiTro), "Vai trò không chính xác");
			if (!model.DiemGiaoDich.HasValue && !model.DiemTapKet.HasValue)
				ModelState.AddModelError(nameof(model.DiemGiaoDich), "Điểm giao dịch hoặc điểm làm việc không được bỏ trống");
			if (model.DiemTapKet.HasValue && model.DiemTapKet.HasValue)
				ModelState.AddModelError(nameof(model.DiemGiaoDich), "Điểm giao dịch hoặc điểm làm việc không chính xác");
			if (await _context.TaiKhoans.AnyAsync(x => x.TenDangNhap == model.TenDangNhap))
				ModelState.AddModelError(nameof(model.TenDangNhap), "Tên đăng nhập đã được sử dụng");
			if (!ModelState.IsValid)
				return BadRequest(ModelState.MappingError(HttpContext.TraceIdentifier));

			await _context.AddAsync(model);
			await _context.SaveChangesAsync();
			return Ok(model);
		}

		/// <summary>
		/// Chỉnh sửa tài khoản sinh viên
		/// </summary>
		[HttpPut]
		public async Task<IActionResult> Update([FromBody] TaiKhoan model)
		{
			var account = await _context.TaiKhoans.FirstOrDefaultAsync(x => x.Ma == model.Ma);
			if (account == null)
				return NotFound();
			if (!await _context.VaiTros.AnyAsync(x => x.Ma == model.MaVaiTro))
				ModelState.AddModelError(nameof(model.MaVaiTro), "Vai trò không chính xác");
			if (!model.DiemGiaoDich.HasValue && !model.DiemTapKet.HasValue)
				ModelState.AddModelError(nameof(model.DiemGiaoDich), "Điểm giao dịch hoặc điểm làm việc không được bỏ trống");
			if (model.DiemTapKet.HasValue && model.DiemTapKet.HasValue)
				ModelState.AddModelError(nameof(model.DiemGiaoDich), "Điểm giao dịch hoặc điểm làm việc không chính xác");
			if (!ModelState.IsValid)
				return BadRequest(ModelState.MappingError(HttpContext.TraceIdentifier));

			account.Ten = model.Ten;
			account.Email = model.Email;
			account.SoDienThoai = model.SoDienThoai;
			account.NgaySinh = model.NgaySinh;
			account.DiaChi = model.DiaChi;
			account.DiemGiaoDich = model.DiemGiaoDich;
			account.DiemTapKet = model.DiemTapKet;
			account.MaVaiTro = model.MaVaiTro;

			_context.Entry(account).State = EntityState.Modified;
			await _context.SaveChangesAsync();
			return Ok(model);
		}

		/// <summary>
		/// Xóa tài khoản
		/// </summary>
		[HttpDelete("{ma}")]
		public async Task<IActionResult> Remove(int ma)
		{
			var account = await _context.TaiKhoans.FirstOrDefaultAsync(x => x.Ma == ma);
			if (account == null)
				return NotFound();

			_context.TaiKhoans.Remove(account);
			await _context.SaveChangesAsync();
			return NoContent();
		}
	}
}
