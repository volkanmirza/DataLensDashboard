using System.ComponentModel.DataAnnotations;

namespace DataLens.Models
{
    public class ChangePasswordViewModel
    {
        [Required(ErrorMessage = "Mevcut şifre gereklidir.")]
        [DataType(DataType.Password)]
        [Display(Name = "Mevcut Şifre")]
        public string CurrentPassword { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Yeni şifre gereklidir.")]
        [StringLength(100, ErrorMessage = "Şifre en az {2} ve en fazla {1} karakter olmalıdır.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Yeni Şifre")]
        public string NewPassword { get; set; } = string.Empty;
        
        [DataType(DataType.Password)]
        [Display(Name = "Yeni Şifre Onayı")]
        [Compare("NewPassword", ErrorMessage = "Yeni şifre ve şifre onayı eşleşmiyor.")]
        public string ConfirmNewPassword { get; set; } = string.Empty;
    }
}