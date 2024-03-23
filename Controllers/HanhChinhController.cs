using MagicPost.Datas;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MagicPost.Controllers
{
    [Route("api")]
    [ApiController]
    public class HanhChinhController : ControllerBase
    {
        private readonly MagicPostDbContext _context;

        public HanhChinhController(MagicPostDbContext context)
        {
            _context = context;
        }

		/// <summary>
		/// Danh sách thành phố
		/// </summary>
		[HttpGet("thanh-pho")]
        public async Task<IActionResult> ThanhPho()
        {
            var result = await _context.ThanhPhos.ToListAsync();
            return Ok(result);
        }

		/// <summary>
		/// Danh sách quận huyện theo mã thành phố
		/// </summary>
		[HttpGet("thanh-pho/{matp}/quan-huyen")]
        public async Task<IActionResult> QuanHuyen(string matp)
        {
            var result = await _context.QuanHuyens.Where(x => x.MaThanhPho == matp).ToListAsync();
            return Ok(result);
        }

		/// <summary>
		/// Danh sách xã phường theo mã thành phố và mã quận huyện
		/// </summary>
		[HttpGet("thanh-pho/{matp}/quan-huyen/{maqh}/xa-phuong")]
        public async Task<IActionResult> XaPhuong(string matp, string maqh)
        {
            var result = await _context.XaPhuongs.Where(x => x.MaThanhPho == matp && x.MaQuanHuyen == maqh).ToListAsync();
            return Ok(result);
        }
    }
}
