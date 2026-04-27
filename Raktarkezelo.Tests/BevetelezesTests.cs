using Microsoft.VisualStudio.TestPlatform.ObjectModel.Client;
using Xunit;
using Xunit.Abstractions;

namespace Raktarkezelo.Tests
{
    public class BevetelezesTests
    {
        private readonly ITestOutputHelper _output;

        public BevetelezesTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void T04_ErvenyesTermekEsMennyiseg_KeszletNo_TranzakcioMentodik()
        {
            TestLogger.LogStep(_output, "Teszt indul: érvényes bevételezés");

            var service = new FakeStockService();
            int currentStock = 10;
            int incomingQuantity = 5;
            int productId = 1;

            var result = service.Bevetelezes(currentStock, incomingQuantity, productId);

            Assert.True(result.Success);
            Assert.Equal(15, result.NewStock);
            Assert.True(result.TransactionSaved);
            Assert.Equal("Sikeres bevételezés", result.Message);

            TestLogger.Log(_output, "T-04", "Érvényes termék + mennyiség", true);
        }

        [Fact]
        public void T05_HianyosAdat_ValidaciosHiba()
        {
            TestLogger.LogStep(_output, "Teszt indul: hiányos adat validáció");

            var service = new FakeStockService();

            var result = service.Bevetelezes(currentStock: 10, incomingQuantity: 0, productId: 0);

            Assert.False(result.Success);
            Assert.Equal(10, result.NewStock);
            Assert.False(result.TransactionSaved);
            Assert.Equal("Hiányos vagy érvénytelen adat", result.Message);

            TestLogger.Log(_output, "T-05", "Hiányos adat", true);
        }
    }

    public class FakeStockService
    {
        public (bool Success, int NewStock, bool TransactionSaved, string Message) Bevetelezes(
            int currentStock,
            int incomingQuantity,
            int productId)
        {
            if (productId <= 0 || incomingQuantity <= 0)
                return (false, currentStock, false, "Hiányos vagy érvénytelen adat");

            return (true, currentStock + incomingQuantity, true, "Sikeres bevételezés");
        }
    }
}