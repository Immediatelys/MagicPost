using MagicPost.Datas;
using MagicPost.Models.Pagings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MagicPost.Controllers
{
    [Route("api/hoa-don-diem-giao-dich")]
    [ApiController]
    [Authorize(Roles = "nvgiaodich")]
    public class HoaDonDiemGiaoDichController : ControllerBase
    {
        private readonly MagicPostDbContext _context;

        public HoaDonDiemGiaoDichController(MagicPostDbContext context)
        {
            _context = context;
        }

		#region đơn nhận
		/// <summary>
		/// Tạo hóa đơn nhận hàng từ khách hàng
		/// </summary>
		[HttpPost("don-nhan")]
        public async Task<IActionResult> NhanHang([FromBody] DonNhan model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var madiemgiaos = model.SanPhams.Select(x => x.ToiDiemGiaoDich).ToList();
            var diemgiaodichs = await _context.DiemGiaoDiches.Where(x => madiemgiaos.Contains(x.Ma)).ToListAsync();
            if (madiemgiaos.Any(x => !diemgiaodichs.Any(gd => gd.Ma == x)))
            {
                ModelState.AddModelError(nameof(SanPhamDonNhan.ToiDiemGiaoDich), "Mã điểm giao dịch nhận hàng không chính xác!");
                return BadRequest(ModelState.MappingError(HttpContext.TraceIdentifier));
            }
            if (!_context.TaiKhoans.Any(x => x.Ma == model.MaTaiKhoan))
			{
				ModelState.AddModelError(nameof(model.MaTaiKhoan), "Mã người gửi không chính xác!");
				return BadRequest(ModelState.MappingError(HttpContext.TraceIdentifier));
			}

			var account = await _context.TaiKhoans.FirstOrDefaultAsync(x => x.TenDangNhap == User.Identity.Name);

            var donnhan = new DonNhan
            {
                TenNguoiGui = model.TenNguoiGui,
                DiaChi = model.DiaChi,
                DienThoai = model.DienThoai,
                XulyKhiKhongGiaoDuoc = model.XulyKhiKhongGiaoDuoc,
                GhiChu = model.GhiChu,
                ThoiGianNhan = DateTime.Now,
                MaDiemNhanHang = account.DiemGiaoDich.Value,
                NhanVienNhanHang = account.Ma,
                MaTaiKhoan = model.MaTaiKhoan
            };
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    await _context.DonNhans.AddAsync(donnhan);
                    await _context.SaveChangesAsync();
                    var donnhanSanPhams = model.SanPhams.Select(x =>
                    {
                        var sanpham = _context.SanPhams.Add(new SanPham
                        {
                            Ten = x.TenSanPham,
                            TrongLuong = x.TrongLuong,
                            TrangThai = "Xử lý sau khi nhận hàng?",
                            MaTaiKhoan = model.MaTaiKhoan
                        }).Entity;
                        _context.SaveChanges();

                        var sp = new SanPhamDonNhan
                        {
                            MaSanPham = sanpham.Ma,
                            TenSanPham = x.TenSanPham,
                            TrongLuong = x.TrongLuong,
                            TenNguoiNhan = x.TenNguoiNhan,
                            DiaChi = x.DiaChi,
                            DienThoai = x.DienThoai,
                            ToiDiemGiaoDich = x.ToiDiemGiaoDich,
                            GiaCuoc = x.GiaCuoc,
                            VAT = x.VAT,
                            KhoanKhac = x.KhoanKhac,
                            Cod = x.Cod,
                            ThuKhac = x.ThuKhac,
                            MaDonNhan = donnhan.Ma

                        };
                        return sp;
                    }).ToList();
                    await _context.SanPhamDonNhans.AddRangeAsync(donnhanSanPhams);
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    donnhan.SanPhams = donnhanSanPhams;
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync();
                    return BadRequest(new { Message = "Đã có lỗi xảy ra trong lúc thực thi!" });
                }
            }
            donnhan.DiemNhanHang = null;
            donnhan.NguoiGui = null;
            donnhan.NguoiNhan = null;
            donnhan.SanPhams = null;

			return Ok(donnhan);
        }

		/// <summary>
		/// Danh sách hóa đơn nhận hàng
		/// </summary>
        [HttpGet("don-nhan")]
		public async Task<IActionResult> GetDonNhan([FromQuery] HoaDonPaging model)
        {
            var current = await _context.TaiKhoans.FirstOrDefaultAsync(x => x.TenDangNhap == User.Identity.Name);
            var query = _context.DonNhans.AsNoTracking().Where(x => x.MaDiemNhanHang == current.DiemGiaoDich);
            if (!string.IsNullOrEmpty(model.SearchKey))
                query = query.Where(x => x.TenNguoiGui.ToLower().Contains(model.SearchKey.ToLower()) || x.DienThoai.ToLower().Contains(model.SearchKey.ToLower()));
            if (model.SearchDate.HasValue)
                query = query.Where(x => x.ThoiGianNhan.Date == model.SearchDate.Value.Date);

            var total = query.Count();
            var data = query.Skip((model.PageIndex - 1) * model.PageSize).Take(model.PageSize).ToList();
            return Ok(new PagingResponse<DonNhan>(total, data));
        }

		/// <summary>
		/// Lấy hóa đơn nhận hàng bằng mã
		/// </summary>
        [HttpGet("don-nhan/{madonnhan}")]
		public async Task<IActionResult> GetDonNhanByMa(int madonnhan)
        {
            var donnhan = await _context.DonNhans.Include(x => x.SanPhams).FirstOrDefaultAsync(x => madonnhan == x.Ma);
            if (donnhan == null)
                return NotFound();

            donnhan.SanPhams = donnhan.SanPhams.Select(x => { x.DonNhan = null; return x; }).ToList();
            return Ok(donnhan);
        }

		/// <summary>
		/// Chỉnh sửa hóa đơn nhận hàng
		/// </summary>
		[HttpPut("don-nhan/{madonnhan}")]
        public async Task<IActionResult> SuaDonNhan(int madonnhan, [FromBody] DonNhan model)
        {
            var donnhan = await _context.DonNhans.FindAsync(madonnhan);
            if (donnhan == null)
                return NotFound();
            var sanphamDonNhans = await _context.SanPhamDonNhans.Where(x => x.MaDonNhan == madonnhan).ToListAsync();
            var masanphams = sanphamDonNhans.Select(x => x.MaSanPham).ToList();
            var sanphams = _context.SanPhams.Where(x => masanphams.Contains(x.Ma)).ToList();

			if (sanphams.Any(x => x.MaDiemTapKet.HasValue || x.MaDiemGiaoDich != donnhan.MaDiemNhanHang))
            {
				ModelState.AddModelError(nameof(model.MaTaiKhoan), "Đơn nhận hàng này đã được xử lý và chuyển đi, không thể chỉnh sửa");
				return BadRequest(ModelState.MappingError(HttpContext.TraceIdentifier));
			}    

			donnhan.TenNguoiGui = model.TenNguoiGui;
            donnhan.DiaChi = model.DiaChi;
            donnhan.DienThoai = model.DienThoai;
            donnhan.XulyKhiKhongGiaoDuoc = model.XulyKhiKhongGiaoDuoc;
            donnhan.GhiChu = model.GhiChu;
			if (donnhan.MaTaiKhoan != model.MaTaiKhoan && !_context.TaiKhoans.Any(x => x.Ma == model.MaTaiKhoan))
			{
				ModelState.AddModelError(nameof(model.MaTaiKhoan), "Mã người gửi không chính xác!");
				return BadRequest(ModelState.MappingError(HttpContext.TraceIdentifier));
			}
            donnhan.MaTaiKhoan = model.MaTaiKhoan;
            using(var transaction = _context.Database.BeginTransaction())
            {
                _context.DonNhans.Update(donnhan);
                var removes = sanphamDonNhans.Where(x => !model.SanPhams.Any(s => s.Ma == x.Ma)).ToList();
                var spMaRemoves = removes.Select(x => x.MaSanPham);
                var spremoves = sanphams.Where(x => spMaRemoves.Contains(x.Ma)).ToList();
                _context.SanPhamDonNhans.RemoveRange(removes);
                _context.RemoveRange(spremoves);
                var sanphamMoi = model.SanPhams.Select(x =>
                {
                    if (x.Ma != default)
                    {
                        var sp = sanphamDonNhans.FirstOrDefault(s => s.Ma == x.Ma);
                        var sanpham = sanphams.FirstOrDefault(s => sp.MaSanPham == s.Ma);
						sanpham.Ten = x.TenSanPham;
						sanpham.TrongLuong = x.TrongLuong;
						sanpham.TrangThai = "Xử lý sau khi nhận hàng?";
						sanpham.MaTaiKhoan = model.MaTaiKhoan;
                        sp.TenSanPham = x.TenSanPham;
                        sp.TrongLuong = x.TrongLuong;
                        sp.TenNguoiNhan = x.TenNguoiNhan;
                        sp.DiaChi = x.DiaChi;
                        sp.DienThoai = x.DienThoai;
                        sp.ToiDiemGiaoDich = x.ToiDiemGiaoDich;
                        sp.GiaCuoc = x.GiaCuoc;
                        sp.VAT = x.VAT;
                        sp.KhoanKhac = x.KhoanKhac;
                        sp.Cod = x.Cod;
                        sp.ThuKhac = x.ThuKhac;
                        sp.MaDonNhan = madonnhan;

                        _context.SanPhams.Update(sanpham);
                        return _context.SanPhamDonNhans.Update(sp).Entity;
					}
                    else
                    {
                        var sanpham = _context.SanPhams.Add(new SanPham
                        {
                            Ten = x.TenSanPham,
                            TrongLuong = x.TrongLuong,
                            TrangThai = "Xử lý sau khi nhận hàng?",
                            MaTaiKhoan = model.MaTaiKhoan
                        }).Entity;
                        _context.SaveChanges();

                        var sp = new SanPhamDonNhan
                        {
                            MaSanPham = sanpham.Ma,
                            TenSanPham = x.TenSanPham,
                            TrongLuong = x.TrongLuong,
                            TenNguoiNhan = x.TenNguoiNhan,
                            DiaChi = x.DiaChi,
                            DienThoai = x.DienThoai,
                            ToiDiemGiaoDich = x.ToiDiemGiaoDich,
                            GiaCuoc = x.GiaCuoc,
                            VAT = x.VAT,
                            KhoanKhac = x.KhoanKhac,
                            Cod = x.Cod,
                            ThuKhac = x.ThuKhac,
                            MaDonNhan = donnhan.Ma

                        };
                        return _context.SanPhamDonNhans.Add(sp).Entity;
                    }
                });
                try
                {
					await transaction.CommitAsync();
				}
                catch (Exception)
                {
                    ModelState.AddModelError(nameof(model.Ma), "Cập nhập hóa đơn thất bại");
					return BadRequest(ModelState.MappingError(HttpContext.TraceIdentifier));
				}
            }
			donnhan.DiemNhanHang = null;
			donnhan.NguoiGui = null;
			donnhan.NguoiNhan = null;
			donnhan.SanPhams = null;
			return Ok(donnhan);
        }

		/// <summary>
		/// Chỉnh sửa thông tin sản phẩm trong đơn nhận
		/// </summary>
		[HttpPut("don-nhan/sanpham/{masp}")]
        public async Task<IActionResult> SuaSanPhamTrongDonNhan(int masp, [FromBody] SanPhamDonNhan model)
        {
            return Ok();
        }
		#endregion

		#region đơn giao
		/// <summary>
		/// Tạo đơn giao hàng
		/// </summary>
		[HttpPost("don-giao/{masanpham}")]
        public async Task<IActionResult> DonGiao(int masanpham)
        {
            var sanpham = await _context.SanPhams.FindAsync(masanpham);
            if (sanpham == null)
                return NotFound();

            var chitietdonnhan = await _context.SanPhamDonNhans.Include(x => x.DonNhan).FirstOrDefaultAsync(x => x.MaSanPham == sanpham.Ma);
            if (chitietdonnhan == null || chitietdonnhan.DonNhan == null)
                return NotFound();

            if (sanpham.MaDiemGiaoDich != chitietdonnhan.ToiDiemGiaoDich)
                return BadRequest(new { Message = "Sản phẩm chưa đến điểm giao dịch cần giao hàng!" });

            var dongiao = new DonGiao()
            {
                TenNguoiGui = chitietdonnhan.DonNhan.TenNguoiGui,
                DiaChiGui = chitietdonnhan.DonNhan.DiaChi,
                DienThoaiGui = chitietdonnhan.DonNhan.DienThoai,
                DiemGui = chitietdonnhan.DonNhan.MaDiemNhanHang,
                TenNguoiNhan = chitietdonnhan.TenNguoiNhan,
                DiaChiNhan = chitietdonnhan.DiaChi,
                DienThoaiNhan = chitietdonnhan.DienThoai,
                DiemNhan = chitietdonnhan.ToiDiemGiaoDich,
                XuLyKhiKhongGiaoDuoc = chitietdonnhan.DonNhan.XulyKhiKhongGiaoDuoc,
                GhiChu = chitietdonnhan.DonNhan.GhiChu,
                MaSanPham = sanpham.Ma,
                TenSanPham = sanpham.Ten,
                TrongLuong = sanpham.TrongLuong,
                GiaCuoc = chitietdonnhan.GiaCuoc,
                VAT = chitietdonnhan.VAT,
                KhoanKhac = chitietdonnhan.KhoanKhac,
                COD = chitietdonnhan.Cod,
                ThuKhac = chitietdonnhan.ThuKhac,
                ThoiGianGui = chitietdonnhan.DonNhan.ThoiGianNhan,
                ThoiGianNhan = DateTime.Now
            };
            sanpham.MaDiemGiaoDich = null;
            sanpham.MaDiemTapKet = null;
            sanpham.TrangThai = "Đã giao";
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    _context.Entry(sanpham).State = EntityState.Modified;
                    await _context.DonGiaos.AddAsync(dongiao);
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync();
                    return BadRequest(new { Message = "Đã có lỗi xảy ra trong lúc thực thi!" });
                }
            }
            return Ok(dongiao);
        }

		/// <summary>
		/// Lấy danh sách hóa đơn giao hàng có phân trang
		/// </summary>
		[HttpGet("don-giao")]
        public async Task<IActionResult> GetDonGiao([FromQuery] HoaDonPaging model)
        {
            var current = await _context.TaiKhoans.FirstOrDefaultAsync(x => x.TenDangNhap == User.Identity.Name);
            var query = _context.DonGiaos.AsNoTracking().Where(x => x.DiemNhan == current.DiemGiaoDich);
            if (!string.IsNullOrEmpty(model.SearchKey))
                query = query.Where(x => x.TenNguoiGui.ToLower().Contains(model.SearchKey.ToLower()) || x.DienThoaiGui.ToLower().Contains(model.SearchKey.ToLower())
                                        || x.TenNguoiNhan.ToLower().Contains(model.SearchKey.ToLower()) || x.DienThoaiNhan.ToLower().Contains(model.SearchKey.ToLower()));
            if (model.SearchDate.HasValue)
                query = query.Where(x => x.ThoiGianGui.Date == model.SearchDate.Value.Date || x.ThoiGianNhan.Date == model.SearchDate.Value.Date);

            var total = query.Count();
            var data = query.Skip((model.PageIndex - 1) * model.PageSize).Take(model.PageSize).ToList();
            return Ok(new PagingResponse<DonGiao>(total, data));
        }

		/// <summary>
		/// Lấy hóa đơn giao hàng theo mã
		/// </summary>
		[HttpGet("don-giao/{madongiao}")]
        public async Task<IActionResult> GetDonGiaoByMa(int madongiao)
        {
            var dongiao = await _context.DonGiaos.Include(x => x.DiemGiaoDichGuiHang).Include(x => x.DiemGiaoDichNhanHang).FirstOrDefaultAsync(x => x.Ma == madongiao);
            if (dongiao == null)
                return NotFound();

            return Ok(dongiao);
        }
		#endregion đơn giao

		#region đơn chuyển
		/// <summary>
		/// Tạo hóa đơn vận chuyển
		/// </summary>
		[HttpPost("don-chuyen")]
        public async Task<IActionResult> DonChuyen([FromBody] DonVanChuyen model)
        {
            if (!model.MaDiemTapKetDen.HasValue)
                ModelState.AddModelError(nameof(model.MaDiemTapKetDen), "Điểm tập kết đến không được bỏ trống");
            if (model.MaSanPhams == null || !model.MaSanPhams.Any())
                ModelState.AddModelError(nameof(model.MaSanPhams), "Danh sách sản phẩm vận chuyển không được trống");

            var account = await _context.TaiKhoans.FirstOrDefaultAsync(x => x.TenDangNhap == User.Identity.Name);
            var sanphams = (await _context.SanPhams.Where(x => model.MaSanPhams.Contains(x.Ma)).ToListAsync()).Select(x => { x.TrangThai = "Đang vận chuyển"; return x; });
            var thongtinnhaps = await _context.SanPhamDonNhans.Where(x => model.MaSanPhams.Contains(x.MaSanPham)).ToListAsync();

            var donvan = new DonVanChuyen
            {
                MaDiemGiaoDichDi = account.DiemGiaoDich,
                MaDiemTapKetDen = model.MaDiemTapKetDen,
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
		/// Lấy hóa đơn vận chuyển
		/// </summary>
		[HttpGet("don-chuyen")]
        public async Task<IActionResult> GetDonChuyen([FromQuery] HoaDonPaging model)
        {
            var current = await _context.TaiKhoans.FirstOrDefaultAsync(x => x.TenDangNhap == User.Identity.Name);
            var query = _context.DonVanChuyens.AsNoTracking().Where(x => x.MaDiemGiaoDichDen == current.DiemGiaoDich || x.MaDiemGiaoDichDi == current.DiemGiaoDich);
            if (model.SearchDate.HasValue)
                query = query.Where(x => x.ThoiGianVanChuyen.Date == model.SearchDate.Value.Date || (x.ThoiGianDenKho.HasValue && x.ThoiGianDenKho.Value.Date == model.SearchDate.Value.Date));
            if (model.DiemTapKet.HasValue)
                query = query.Where(x => x.MaDiemTapKetDen == model.DiemTapKet || x.MaDiemTapKetDi == model.DiemTapKet);

            var total = query.Count();
            var data = query.Skip((model.PageIndex - 1) * model.PageSize).Take(model.PageSize).ToList();
            return Ok(new PagingResponse<DonVanChuyen>(total, data));
        }

		/// <summary>
		/// Lấy hóa đơn vận chuyển theo mã
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
		/// Xác nhận hóa đơn vận chuyển tới điểm giao dịch
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
                                var tt = donchuyen.SanPhams.FirstOrDefault(t => t.MaSanPham == x.Ma);
                                if (tt.ToiDiemGiaoDich == donchuyen.MaDiemGiaoDichDen)
                                    x.TrangThai = "Đã đến điểm giao dịch đích";

                                x.MaDiemGiaoDich = donchuyen.MaDiemGiaoDichDen;
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
