using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Raktarkezelo.Models.User_Info
{
    public class LoginData
    {
       
        [Required (ErrorMessage = "Helytelen jelszót adtál meg!")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Required (ErrorMessage = "Helytelen felhasználónevet adtál meg!")]
        public string Username { get; set; } = string.Empty;

    }
}
