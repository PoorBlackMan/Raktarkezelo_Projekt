using Xunit;
using Xunit.Abstractions;

namespace Raktarkezelo.Tests
{
    public class KeszletKorrekcioTests
    {
        private readonly ITestOutputHelper _output;

        public KeszletKorrekcioTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void T08_UjKeszletErtek_KeszletModosul()
        {
            var service = new FakeCorrectionService();

            var result = service.KeszletKorrekcio(10, 18);

            Assert.True(result.Success);
            Assert.Equal(18, result.FinalStock);

            _output.WriteLine("T-08 sikeres");
        }

        [Fact]
        public void T09_AzonosErtek_Hiba()
        {
            var service = new FakeCorrectionService();

            var result = service.KeszletKorrekcio(10, 10);

            Assert.False(result.Success);

            _output.WriteLine("T-09 sikeres");
        }
    }

    public class FakeCorrectionService
    {
        public (bool Success, int FinalStock) KeszletKorrekcio(int currentStock, int newStock)
        {
            if (newStock == currentStock)
                return (false, currentStock);

            return (true, newStock);
        }
    }
}