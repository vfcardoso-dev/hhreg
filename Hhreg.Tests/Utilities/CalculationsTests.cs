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
        [TestCase(new[] { "07:51", "12:00", "13:00", "17:00" }, 10, 480)]
        [TestCase(new[] { "08:09", "12:00", "13:00", "17:00" }, 10, 480)]
        [TestCase(new[] { "08:00", "12:00", "13:00", "17:09" }, 10, 480)]
        [TestCase(new[] { "08:00", "12:00", "13:00", "16:51" }, 10, 480)]
        [TestCase(new[] { "07:45", "12:00", "13:00", "17:00" }, 10, 495)]
        [TestCase(new[] { "08:15", "12:00", "13:00", "17:00" }, 10, 465)]
        [TestCase(new[] { "08:00", "12:00", "13:00", "17:15" }, 10, 495)]
        [TestCase(new[] { "08:00", "12:00", "13:00", "16:45" }, 10, 465)]
        [TestCase(new[] { "08:15", "12:00", "13:00", "17:00" }, 15, 465)]
        [TestCase(new[] { "08:14", "12:00", "13:00", "17:00" }, 15, 480)]
        [TestCase(new[] { "08:00", "12:00", "13:00", "17:00", "20:00", "22:00" }, 10, 600)]
        public void deve_realizar_calculos_de_horario_de_trabalho(IEnumerable<string> entries, double toleranceInMinutes, double expectedResult)
        {
            // Given/When
            var result = Calculations.GetTotalMinutes(entries, DayType.Work, toleranceInMinutes, _workdayInMinutes);

            // Then
            result.Should().Be(expectedResult);
        }

        [Test]
        public void deve_retornar_zero_de_bonus_quando_mes_nao_fechado()
        {
            var result = Calculations.ComputeMonthBonus(monthDelta: 120, monthClosed: false, bonusPercentage: 50);
            result.Should().Be(0);
        }

        [Test]
        public void deve_retornar_zero_de_bonus_quando_delta_negativo()
        {
            var result = Calculations.ComputeMonthBonus(monthDelta: -120, monthClosed: true, bonusPercentage: 50);
            result.Should().Be(0);
        }

        [Test]
        public void deve_retornar_zero_de_bonus_quando_delta_zero()
        {
            var result = Calculations.ComputeMonthBonus(monthDelta: 0, monthClosed: true, bonusPercentage: 50);
            result.Should().Be(0);
        }

        [TestCase(100d, 50d, 50d)]
        [TestCase(200d, 50d, 100d)]
        [TestCase(60d, 25d, 15d)]
        [TestCase(480d, 100d, 480d)]
        [TestCase(120d, 0d, 0d)]
        public void deve_calcular_bonus_sobre_delta_positivo_quando_mes_fechado(double monthDelta, double bonusPercentage, double expectedBonus)
        {
            var result = Calculations.ComputeMonthBonus(monthDelta, monthClosed: true, bonusPercentage);
            result.Should().Be(expectedBonus);
        }

        [Test]
        public void deve_agregar_meses_aplicando_bonus_apenas_em_meses_fechados_com_delta_positivo()
        {
            // Given
            var monthDeltas = new[]
            {
                ("2025-01", 100d),  // fechado, positivo -> bonus
                ("2025-02", -80d),  // fechado, negativo -> sem bonus
                ("2025-03", 200d)   // não fechado, mesmo positivo -> sem bonus
            };
            var limitDay = new DateOnly(2025, 3, 15); // dentro de março

            // When
            var result = Calculations.AggregateMonthlyBalance(monthDeltas, limitDay, bonusPercentage: 50);

            // Then
            result.Should().Be(100 + 50 /* bonus jan */ + (-80) + 200);
        }

        [Test]
        public void deve_considerar_mes_fechado_somente_quando_limit_day_passa_do_ultimo_dia_do_mes()
        {
            // Given
            var monthDeltas = new[] { ("2025-01", 100d) };

            // When + Then
            // Último dia de janeiro: mês ainda NÃO fechado
            Calculations.AggregateMonthlyBalance(monthDeltas, new DateOnly(2025, 1, 31), 50)
                .Should().Be(100, "mês ainda não virou no último dia do próprio mês");

            // Primeiro dia de fevereiro: janeiro fechou
            Calculations.AggregateMonthlyBalance(monthDeltas, new DateOnly(2025, 2, 1), 50)
                .Should().Be(150, "ao entrar em fevereiro, janeiro fechou e bonus se aplica");
        }

        [Test]
        public void deve_retornar_zero_para_lista_vazia_de_deltas_mensais()
        {
            var result = Calculations.AggregateMonthlyBalance(Array.Empty<(string, double)>(), new DateOnly(2025, 1, 1), 50);
            result.Should().Be(0);
        }
    }
}
