using Microsoft.VisualStudio.TestPlatform.ObjectModel.Client;
using Xunit;
using Xunit.Abstractions;

namespace Raktarkezelo.Tests
{
    public class LoginTests
    {
        private readonly ITestOutputHelper _output;

        public LoginTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void T01_HelyesEmailEsJelszo_SikeresBelepes()
        {
            TestLogger.LogStep(_output, "Teszt indul: helyes e-mail + jelszó ellenőrzése");

            var service = new FakeAuthService();
            string email = "admin@raktar.hu";
            string password = "admin123";

            var result = service.Login(email, password, isActive: true);

            Assert.True(result.Success);
            Assert.Equal("Sikeres belépés", result.Message);

            TestLogger.Log(_output, "T-01", "Helyes e-mail + jelszó", true);
        }

        [Fact]
        public void T02_HibasJelszo_Hibauzenet()
        {
            TestLogger.LogStep(_output, "Teszt indul: hibás jelszó ellenőrzése");

            var service = new FakeAuthService();
            string email = "admin@raktar.hu";
            string password = "rosszjelszo";

            var result = service.Login(email, password, isActive: true);

            Assert.False(result.Success);
            Assert.Equal("Hibás e-mail vagy jelszó", result.Message);

            TestLogger.Log(_output, "T-02", "Hibás jelszó", true);
        }

        [Fact]
        public void T03_InaktivFelhasznalo_BelepesTiltva()
        {
            TestLogger.LogStep(_output, "Teszt indul: inaktív felhasználó belépésének tiltása");

            var service = new FakeAuthService();
            string email = "admin@raktar.hu";
            string password = "admin123";

            var result = service.Login(email, password, isActive: false);

            Assert.False(result.Success);
            Assert.Equal("A felhasználó inaktív", result.Message);

            TestLogger.Log(_output, "T-03", "Inaktív felhasználó", true);
        }
    }

    public class FakeAuthService
    {
        public (bool Success, string Message) Login(string email, string password, bool isActive)
        {
            if (!isActive)
                return (false, "A felhasználó inaktív");

            if (email == "admin@raktar.hu" && password == "admin123")
                return (true, "Sikeres belépés");

            return (false, "Hibás e-mail vagy jelszó");
        }
    }
}