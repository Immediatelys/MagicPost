using System.ComponentModel.DataAnnotations;

namespace MagicPost.Models
{
    public class SignInModel
    {
        [Required(ErrorMessage = "Tên đăng nhập không được bỏ trống")]
        public string Username { get; set; }
        [Required(ErrorMessage = "Mật khẩu không được bỏ trống")]
        public string Password { get; set; }
    }

    public class SignInRespone
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public object Jwt { get; set; }
    }
}
