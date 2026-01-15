using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ControlActividades.Models
{
    public enum Role{
        Docente,
        Alumno,
        Administrador
    }

    public enum SessionKeys
    {
        Email
    }

    public class ExternalLoginConfirmationViewModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Correo electrónico")]
        public string Email { get; set; }

        [Display(Name = "Nombres")]
        public string Nombre { get; set; }

        [Display(Name = "Apellido Paterno")]
        public string Paterno { get; set; }

        [Display(Name = "Apellido Materno")]
        public string Materno { get; set; }

        [Display(Name = "Contraseña")]
        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Display(Name = "Rol")]
        [Required]
        public Role Role { get; set; }

    }

    public class ExternalLoginLinkViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        public string LoginProvider { get; set; }
    }

    public class ExternalLoginListViewModel
    {
        public string ReturnUrl { get; set; }
    }

    public class SendCodeViewModel
    {
        public string SelectedProvider { get; set; }
        public ICollection<System.Web.Mvc.SelectListItem> Providers { get; set; }
        public string ReturnUrl { get; set; }
        public bool RememberMe { get; set; }
    }

    public class VerifyCodeViewModel
    {
        [Required]
        public string Provider { get; set; }

        [Required]
        [Display(Name = "Código")]
        public string Code { get; set; }
        public string ReturnUrl { get; set; }

        [Display(Name = "¿Recordar este explorador?")]
        public bool RememberBrowser { get; set; }

        public bool RememberMe { get; set; }
    }

    public class ForgotViewModel
    {
        [Required]
        [Display(Name = "Correo electrónico")]
        public string Email { get; set; }
    }

    public class LoginViewModel
    {
        [Required]
        [Display(Name = "Correo electrónico")]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña")]
        public string Password { get; set; }

        [Display(Name = "¿Recordar cuenta?")]
        public bool RememberMe { get; set; }
    }

    public class ValidateEmailViewModel
    {
        [Required]
        [Display(Name = "Correo electrónico")]
        [EmailAddress]
        public string Email { get; set; }
    }

    public class ConfirmEmailViewModel
    {
        [Required]
        public string Code1 { get; set; }

        [Required]
        public string Code2 { get; set; }

        [Required]
        public string Code3 { get; set; }

        [Required]
        public string Code4 { get; set; }

        [Required]
        public string Code5 { get; set; }
    }

    public class RegisterViewModel
    {
        [Required]
        [Display(Name = "Nombres")]
        public string Nombres { get; set; }

        [Required]
        [Display(Name = "Apellido Paterno")]
        public string ApellidoPaterno { get; set; }

        [Display(Name = "Apellido Materno")]
        public string ApellidoMaterno { get; set; } = string.Empty;

        //[Required]
        //[EmailAddress]
        //[Display(Name = "Correo electrónico")]
        //public string Email { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "El número de caracteres de {0} debe ser al menos {2}.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña")]
        public string Password { get; set; }

        [Required]
        [Display(Name = "Tipo usuario")]
        public Role Role { get; set; }
        //[DataType(DataType.Password)]
        //[Display(Name = "Confirmar contraseña")]
        //[Compare("Password", ErrorMessage = "La contraseña y la contraseña de confirmación no coinciden.")]
        //public string ConfirmPassword { get; set; }
    }

    public class ResetPasswordViewModel
    {

        [Required]
        [EmailAddress]
        [Display(Name = "Correo electrónico")]
        public string Email { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "El número de caracteres de {0} debe ser al menos {2}.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirmar contraseña")]
        [Compare("Password", ErrorMessage = "La contraseña y la contraseña de confirmación no coinciden.")]
        public string ConfirmPassword { get; set; }

        public string Code { get; set; }
    }

    public class ForgotPasswordViewModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Correo electrónico")]
        public string Email { get; set; }
    }
}
