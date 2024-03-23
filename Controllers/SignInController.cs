using MagicPost.Datas;
using MagicPost.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace MagicPost.Controllers
{
    [ApiController]
    [Route("api/dang-nhap")]
    public class SignInController : ControllerBase
    {
        private MagicPostDbContext _context;
        private readonly IConfiguration _configuration;

        public SignInController(MagicPostDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

		/// <summary>
		/// Đăng nhập với JWT token
		/// </summary>
		[HttpPost("")]
        public async Task<IActionResult> Index([FromBody] SignInModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var taikhoan = await _context.TaiKhoans.FirstOrDefaultAsync(x => x.TenDangNhap == model.Username);
            if (taikhoan == null || taikhoan.MatKhau != HashStr(model.Password))
            {
                ModelState.AddModelError(nameof(model.Username), "Tên đăng nhập hoặc mật khẩu không chính xác!");
                return BadRequest(ModelState.MappingError(HttpContext.TraceIdentifier));
            }

            var claims = new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, taikhoan.Ma.ToString()),
                new Claim(ClaimTypes.Name, taikhoan.TenDangNhap),
                new Claim(ClaimTypes.Role, taikhoan.MaVaiTro)
            };
            var signingCredentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["secretKey"])), SecurityAlgorithms.HmacSha256);

            var jwt = new JwtSecurityToken(claims: claims, signingCredentials: signingCredentials);
            var token = new JwtSecurityTokenHandler().WriteToken(jwt);
            var response = new SignInRespone { Success = true, Message = "Đăng nhập thành công", Jwt = token };
            return Ok(response);
        }

        [HttpGet("/api/thong-tin-cua-toi")]
        [Authorize]
		/// <summary>
		/// Thông tin cá nhân của tài khoản đang đăng nhập
		/// </summary>
		public async Task<IActionResult> MyInfo()
        {
            var tendangnhap = User.Identity.Name;
            if (string.IsNullOrEmpty(tendangnhap))
                return Unauthorized();

            var taikhoan = await _context.TaiKhoans.Include(x => x.VaiTro).FirstOrDefaultAsync(x => x.TenDangNhap == tendangnhap);
            if (taikhoan == null)
                return Unauthorized();
            var result = new ProfileInfo
            {
                Ma = taikhoan.Ma,
                TenDangNhap = taikhoan.TenDangNhap,
                Ten = taikhoan.Ten,
                Email = taikhoan.Email,
                SoDienThoai = taikhoan.SoDienThoai,
                NgaySinh = taikhoan.NgaySinh,
                DiaChi = taikhoan.DiaChi,
                MaDiemGiaoDich = taikhoan.DiemGiaoDich,
                MaDiemTapKet = taikhoan.DiemTapKet,
                MaVaiTro = taikhoan.MaVaiTro,
                TenVaiTro = taikhoan.VaiTro?.Ten
            };
            if(result.MaDiemGiaoDich.HasValue)
            {
                var diemgiaodich = await _context.DiemGiaoDiches.FirstOrDefaultAsync(x => x.Ma == result.MaDiemGiaoDich);
                result.DiemGiaoDich = diemgiaodich?.Ten;
            }
            if (result.MaDiemTapKet.HasValue)
            {
                var diemtapket = await _context.DiemTapKets.FirstOrDefaultAsync(x => x.Ma == result.MaDiemTapKet);
                result.DiemTapKet = diemtapket?.Ten;
            }
            return Ok(result);
        }

        private string HashStr(string input)
        {
            using var provider = System.Security.Cryptography.MD5.Create();
            StringBuilder builder = new StringBuilder();

            foreach (byte b in provider.ComputeHash(Encoding.UTF8.GetBytes(input)))
                builder.Append(b.ToString("x2").ToLower());

            return builder.ToString();
        }
    }
}
