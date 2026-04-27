using Microsoft.VisualStudio.TestPlatform.ObjectModel.Client;
using Xunit;
using Xunit.Abstractions;

namespace Raktarkezelo.Tests
{
    public class JogosultsagTests
    {
        private readonly ITestOutputHelper _output;

        public JogosultsagTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void T13_JogosulatlanHozzaferesAdminOldalhoz_Tiltas()
        {
            TestLogger.LogStep(_output, "Teszt indul: nem admin felhasználó admin oldal elérése");

            var service = new FakeAuthorizationService();
            string role = "user";

            var result = service.CanAccessAdminPage(role);

            Assert.False(result);

            TestLogger.Log(_output, "T-13", "Jogosulatlan hozzáférés admin oldalhoz", true);
        }

        [Fact]
        public void T13_AdminHozzaferhetAdminOldalhoz_Engedelyezett()
        {
            TestLogger.LogStep(_output, "Teszt indul: admin hozzáférés ellenőrzése");

            var service = new FakeAuthorizationService();
            string role = "admin";

            var result = service.CanAccessAdminPage(role);

            Assert.True(result);

            TestLogger.Log(_output, "T-13+", "Admin hozzáférhet admin oldalhoz", true);
        }

        [Fact]
        public void T14_AdminTiltasVedelme_AdminNeLegyenDeaktivalhato()
        {
            TestLogger.LogStep(_output, "Teszt indul: admin tiltás védelme");

            var service = new FakeAuthorizationService();
            string targetRole = "admin";

            var result = service.CanDeactivateUser(targetRole);

            Assert.False(result);

            TestLogger.Log(_output, "T-14", "Admin ne legyen deaktiválható", true);
        }

        [Fact]
        public void T14_NormalFelhasznalo_Deaktivalhato()
        {
            TestLogger.LogStep(_output, "Teszt indul: normál felhasználó deaktiválhatósága");

            var service = new FakeAuthorizationService();
            string targetRole = "user";

            var result = service.CanDeactivateUser(targetRole);

            Assert.True(result);

            TestLogger.Log(_output, "T-14+", "Normál felhasználó deaktiválható", true);
        }
    }

    public class FakeAuthorizationService
    {
        public bool CanAccessAdminPage(string role)
        {
            return role == "admin";
        }

        public bool CanDeactivateUser(string targetRole)
        {
            return targetRole != "admin";
        }
    }
}