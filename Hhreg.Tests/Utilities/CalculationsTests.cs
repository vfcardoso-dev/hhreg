using AutoFixture;
using FluentAssertions;
using Hhreg.Business.Domain;
using Hhreg.Business.Utilities;
using Hhreg.Tests.Infrastructure;
using Hhreg.Tests.Infrastructure.Utilities;

namespace Hhreg.Tests.Utilities
{
    public class CalculationsTests : UnitTestsBase
    {
        private readonly double _workdayInMinutes = 480;

        [Test]
        public void deve_calcular_dias_do_tipo_diferente_de_work_como_contendo_a_quantidade_padrao_de_horas()
        {
            // Given
            var type = Fixture.CreateAnyBut(DayType.Work);
            var entries = Fixture.CreateMany<string>(4);
            var tolerance = Fixture.Create<double>();

            // When
            var result = Calculations.GetTotalMinutes(entries, type, tolerance, _workdayInMinutes);

            // Then
            result.Should().Be(_workdayInMinutes);
        }

        [TestCase(new[] { "08:00", "12:00", "13:00", "17:00" }, 10, 480)]
        [TestCase(new[] { "08:00", "12:00", "13:15", "17:00" }, 10, 465)]
        [TestCase(new[] { "08:00", "11:45", "13:00", "17:00" }, 10, 465)]
        [TestCase(new[] { "07:50", "12:00", "13:00", "17:00" }, 10, 480)]
        [TestCase(new[] { "08:10", "12:00", "13:00", "17:00" }, 10, 480)]
        [TestCase(new[] { "08:00", "12:00", "13:00", "17:10" }, 10, 480)]
        [TestCase(new[] { "08:00", "12:00", "13:00", "16:50" }, 10, 480)]
        [TestCase(new[] { "07:45", "12:00", "13:00", "17:00" }, 10, 495)]
        [TestCase(new[] { "08:15", "12:00", "13:00", "17:00" }, 10, 465)]
        [TestCase(new[] { "08:00", "12:00", "13:00", "17:15" }, 10, 495)]
        [TestCase(new[] { "08:00", "12:00", "13:00", "16:45" }, 10, 465)]
        [TestCase(new[] { "08:15", "12:00", "13:00", "17:00" }, 15, 480)]
        [TestCase(new[] { "08:15", "12:00", "13:00", "17:00" }, 15, 480)]
        [TestCase(new[] { "08:00", "12:00", "13:00", "17:00", "20:00", "22:00" }, 10, 600)]
        //[TestCase(new[] { "08:00", "12:00", "13:00", "17:00", "21:00", "01:00" }, 10, 720)]
        public void deve_realizar_calculos_de_horario_de_trabalho(IEnumerable<string> entries, double toleranceInMinutes, double expectedResult)
        {
            // Given/When
            var result = Calculations.GetTotalMinutes(entries, DayType.Work, toleranceInMinutes, _workdayInMinutes);

            // Then
            result.Should().Be(expectedResult);
        }
    }
}
