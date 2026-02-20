using Microsoft.AspNetCore.Identity;
using System.Security.Cryptography.X509Certificates;

namespace Raktarkezelo.Models.User_Info
{
    public class Userinfo
    {
        public int UserId { get; set; }
        public string Name { get; set; }
        public bool Admin { get; set; }
        public bool IsActive { get; set; }
        
       
        public PasswordHasher<Userinfo> PasswordHasher { get; set; } = new PasswordHasher<Userinfo>();


    }
}
