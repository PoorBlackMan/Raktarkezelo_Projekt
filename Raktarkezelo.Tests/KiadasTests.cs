using Microsoft.VisualStudio.TestPlatform.ObjectModel.Client;
using Xunit;
using Xunit.Abstractions;

namespace Raktarkezelo.Tests
{
    public class KiadasTests
    {
        private readonly ITestOutputHelper _output;

        public KiadasTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void T06_ElegKeszlet_KeszletCsokken()
        {
            TestLogger.LogStep(_output, "Teszt indul: elegendő készlet esetén kiadás");

            var service = new FakeIssueService();
            int currentStock = 20;
            int issueQuantity = 5;

            var result = service.Kiadas(currentStock, issueQuantity);

            Assert.True(result.Success);
            Assert.Equal(15, result.NewStock);
            Assert.Equal("Sikeres kiadás", result.Message);

            TestLogger.Log(_output, "T-06", "Elég készlet", true);
        }

        [Fact]
        public void T07_KevesKeszlet_Hibauzenet()
        {
            TestLogger.LogStep(_output, "Teszt indul: kevés készlet hibakezelése");

            var service = new FakeIssueService();
            int currentStock = 2;
            int issueQuantity = 5;

            var result = service.Kiadas(currentStock, issueQuantity);

            Assert.False(result.Success);
            Assert.Equal(2, result.NewStock);
            Assert.Equal("Nincs elegendő készlet", result.Message);

            TestLogger.Log(_output, "T-07", "Kevés készlet", true);
        }
    }

    public class FakeIssueService
    {
        public (bool Success, int NewStock, string Message) Kiadas(int currentStock, int issueQuantity)
        {
            if (issueQuantity <= 0)
                return (false, currentStock, "Érvénytelen mennyiség");

            if (currentStock < issueQuantity)
                return (false, currentStock, "Nincs elegendő készlet");

            return (true, currentStock - issueQuantity, "Sikeres kiadás");
        }
    }
}