using MagicPost.Datas;
using MagicPost.Models.Pagings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MagicPost.Controllers
{
    [Route("api/hoa-don-diem-tap-ket")]
    [ApiController]
    [Authorize(Roles = "nvtapket")]
    public class HoaDonDiemTapKet : ControllerBase
    {
        private readonly MagicPostDbContext _context;

        public HoaDonDiemTapKet(MagicPostDbContext context)
        {
            _context = context;
        }

		#region đơn chuyển
		/// <summary>
		/// Tạo hóa đơn chuyển
		/// </summary>
		[HttpPost("don-chuyen")]
        public async Task<IActionResult> DonChuyen([FromBody] DonVanChuyen model)
        {
            var account = await _context.TaiKhoans.FirstOrDefaultAsync(x => x.TenDangNhap == User.Identity.Name);

            if (!model.MaDiemTapKetDen.HasValue || !model.MaDiemGiaoDichDen.HasValue)
                ModelState.AddModelError(nameof(model.MaDiemTapKetDen), "Điểm vận chuyển tới không được bỏ trống");
            if ((model.MaDiemGiaoDichDen.HasValue && model.MaDiemTapKetDen.HasValue) || model.MaDiemTapKetDen == account.DiemTapKet)
                ModelState.AddModelError(nameof(model.MaDiemTapKetDen), "Điểm vận chuyển tới không chính xác");
            if (model.MaSanPhams == null || !model.MaSanPhams.Any())
                ModelState.AddModelError(nameof(model.MaSanPhams), "Danh sách sản phẩm vận chuyển không được trống");
            if(!ModelState.IsValid)
                return BadRequest(ModelState.MappingError(HttpContext.TraceIdentifier));

            var sanphams = (await _context.SanPhams.Where(x => model.MaSanPhams.Contains(x.Ma)).ToListAsync()).Select(x => { x.TrangThai = "Đang vận chuyển"; return x; });
            var thongtinnhaps = await _context.SanPhamDonNhans.Where(x => model.MaSanPhams.Contains(x.MaSanPham)).ToListAsync();

            var donvan = new DonVanChuyen
            {
                MaDiemTapKetDi = account.DiemTapKet,
                MaDiemTapKetDen = model.MaDiemTapKetDen,
                MaDiemGiaoDichDen = model.MaDiemGiaoDichDen,
                MaNguoiGiao = account.Ma,
                ThoiGianVanChuyen = DateTime.Now
            };
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    donvan = (await _context.DonVanChuyens.AddAsync(donvan)).Entity;
                    await _context.SaveChangesAsync();

                    var sanphamChuyen = sanphams.Select(x =>
                    {
                        var tt = thongtinnhaps.FirstOrDefault(x => x.MaSanPham == x.Ma);
                        var sp = new DonVanChuyenSanPham
                        {
                            MaSanPham = x.Ma,
                            TenSanPham = x.Ten,
                            TrongLuong = x.TrongLuong,
                            TenNguoiNhan = tt.TenNguoiNhan,
                            DiaChi = tt.DiaChi,
                            DienThoai = tt.DienThoai,
                            ToiDiemGiaoDich = tt.ToiDiemGiaoDich,
                            GiaCuoc = tt.GiaCuoc,
                            VAT = tt.VAT,
                            KhoanKhac = tt.KhoanKhac,
                            Cod = tt.Cod,
                            ThuKhac = tt.ThuKhac,
                            MaDonChuyen = donvan.Ma
                        };
                        return sp;
                    });
                    await _context.AddRangeAsync(sanphamChuyen);
                    _context.SanPhams.UpdateRange(sanphams);
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    donvan.SanPhams = sanphamChuyen;
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync();
                    return BadRequest(new { Message = "Đã có lỗi xảy ra trong lúc thực thi!" });
                }
            }

			donvan.DiemTapKetDi = null;
			donvan.DiemTapKetDen = null;
			donvan.DiemGiaoDichDi = null;
			donvan.DiemGiaoDichDen = null;
			donvan.SanPhams = null;
			return Ok(donvan);
        }

		/// <summary>
		/// Danh sách đơn vận chuyển có phân trang
		/// </summary>
		[HttpGet("don-chuyen")]
        public async Task<IActionResult> GetDonChuyen([FromQuery] HoaDonPaging model)
        {
            var current = await _context.TaiKhoans.FirstOrDefaultAsync(x => x.TenDangNhap == User.Identity.Name);
            var query = _context.DonVanChuyens.AsNoTracking().Where(x => x.MaDiemTapKetDen == current.DiemTapKet || x.MaDiemTapKetDi == current.DiemTapKet);
            if (model.SearchDate.HasValue)
                query = query.Where(x => x.ThoiGianVanChuyen.Date == model.SearchDate.Value.Date || (x.ThoiGianDenKho.HasValue && x.ThoiGianDenKho.Value.Date == model.SearchDate.Value.Date));
            if (model.DiemTapKet.HasValue)
                query = query.Where(x => x.MaDiemTapKetDen == model.DiemTapKet || x.MaDiemTapKetDi == model.DiemTapKet);
            if (model.DiemGiaoDich.HasValue)
                query = query.Where(x => x.MaDiemGiaoDichDi == model.DiemTapKet || x.MaDiemGiaoDichDen == model.DiemTapKet);

            var total = query.Count();
            var data = query.Skip((model.PageIndex - 1) * model.PageSize).Take(model.PageSize).ToList();
            return Ok(new PagingResponse<DonVanChuyen>(total, data));
        }

		/// <summary>
		/// Lấy đơn vận chuyển theo mã
		/// </summary>
		[HttpGet("don-chuyen/{madonchuyen}")]
        public async Task<IActionResult> GetDonChuyenByMa(int madonchuyen)
        {
            var donchuyen = await _context.DonVanChuyens.Include(x => x.SanPhams).FirstOrDefaultAsync(x => x.Ma == madonchuyen);
            if (donchuyen == null)
                return NotFound();

            donchuyen.SanPhams = donchuyen.SanPhams.Select(x => { x.DonVanChuyen = null; return x; }).ToList();
            return Ok(donchuyen);
        }

		/// <summary>
		/// Xác nhận đơn vận chuyển
		/// </summary>
		[HttpPut("don-chuyen/{madonchuyen}/xac-nhan")]
        public async Task<IActionResult> XacNhan(int madonchuyen)
        {
            var donchuyen = await _context.DonVanChuyens.Include(x => x.SanPhams).FirstOrDefaultAsync(x => x.Ma == madonchuyen);
            if (donchuyen == null || !donchuyen.MaDiemGiaoDichDen.HasValue)
                return NotFound();

            var masps = donchuyen.SanPhams.Select(x => x.MaSanPham).ToList();
            var sanphams = (await _context.SanPhams.Where(x => masps.Contains(x.Ma)).ToListAsync())
                            .Select(x =>
                            {
                                x.TrangThai = "Đang vận chuyển";
                                x.MaDiemTapKet = donchuyen.MaDiemTapKetDen;
                                return x;
                            }).ToList();

            var account = await _context.TaiKhoans.FirstOrDefaultAsync(x => x.TenDangNhap == User.Identity.Name);
            donchuyen.NguoiXacNhan = account.Ma;

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    _context.DonVanChuyens.Update(donchuyen);
                    _context.SanPhams.UpdateRange(sanphams);
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync();
                    return BadRequest(new { Message = "Đã có lỗi xảy ra trong lúc thực thi!" });
                }
            }

            return Ok(new { Message = "Xác nhận đơn vận chuyển tới thành công!" });
        }
        #endregion
    }
}
