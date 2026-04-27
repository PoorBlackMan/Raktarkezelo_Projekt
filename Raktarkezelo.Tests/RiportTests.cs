using Microsoft.VisualStudio.TestPlatform.ObjectModel.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace Raktarkezelo.Tests
{
    public class RiportTests
    {
        private readonly ITestOutputHelper _output;

        public RiportTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void T10_Datumszures_SzurtLista()
        {
            TestLogger.LogStep(_output, "Teszt indul: dátum szerinti szűrés");

            var service = new FakeReportService();
            var data = GetSampleTransactions();

            var result = service.FilterByDate(data, new DateTime(2025, 1, 1), new DateTime(2025, 1, 31));

            Assert.NotEmpty(result);
            Assert.All(result, x => Assert.InRange(x.Date, new DateTime(2025, 1, 1), new DateTime(2025, 1, 31)));

            TestLogger.Log(_output, "T-10", "Dátumszűrés", true);
        }

        [Fact]
        public void T11_TipusSzerintiSzures_SzurtLista()
        {
            TestLogger.LogStep(_output, "Teszt indul: típus szerinti szűrés");

            var service = new FakeReportService();
            var data = GetSampleTransactions();

            var result = service.FilterByType(data, "Bevételezés");

            Assert.NotEmpty(result);
            Assert.All(result, x => Assert.Equal("Bevételezés", x.Type));

            TestLogger.Log(_output, "T-11", "Típus szerinti szűrés", true);
        }

        [Fact]
        public void T12_TermekSzerintiSzures_SzurtLista()
        {
            TestLogger.LogStep(_output, "Teszt indul: termék szerinti szűrés");

            var service = new FakeReportService();
            var data = GetSampleTransactions();

            var result = service.FilterByProduct(data, "Laptop");

            Assert.NotEmpty(result);
            Assert.All(result, x => Assert.Equal("Laptop", x.ProductName));

            TestLogger.Log(_output, "T-12", "Termék szerinti szűrés", true);
        }

        private List<FakeTransaction> GetSampleTransactions()
        {
            return new List<FakeTransaction>
            {
                new FakeTransaction { Date = new DateTime(2025, 1, 10), Type = "Bevételezés", ProductName = "Laptop" },
                new FakeTransaction { Date = new DateTime(2025, 1, 15), Type = "Kiadás", ProductName = "Egér" },
                new FakeTransaction { Date = new DateTime(2025, 2, 1), Type = "Bevételezés", ProductName = "Laptop" }
            };
        }
    }

    public class FakeReportService
    {
        public List<FakeTransaction> FilterByDate(List<FakeTransaction> data, DateTime from, DateTime to)
            => data.Where(x => x.Date >= from && x.Date <= to).ToList();

        public List<FakeTransaction> FilterByType(List<FakeTransaction> data, string type)
            => data.Where(x => x.Type == type).ToList();

        public List<FakeTransaction> FilterByProduct(List<FakeTransaction> data, string productName)
            => data.Where(x => x.ProductName == productName).ToList();
    }

    public class FakeTransaction
    {
        public DateTime Date { get; set; }
        public string Type { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
    }
}