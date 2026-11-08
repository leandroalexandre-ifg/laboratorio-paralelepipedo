using System;
using LaboratorioParalelepipedo.Nucleo;
using NUnit.Framework;

namespace LaboratorioParalelepipedo.Testes
{
    public class ParalelepipedoValidacaoTests
    {
        private static readonly double[] DimensoesInvalidas =
        {
            0, -0.0, -1, -2.5, double.NaN, double.PositiveInfinity, double.NegativeInfinity,
        };

        [Test]
        public void DimensoesPositivas_SaoAceitas()
        {
            var caixa = new Paralelepipedo(2.5, 1.2, Math.Sqrt(2));

            Assert.That(caixa.A, Is.EqualTo(2.5));
            Assert.That(caixa.B, Is.EqualTo(1.2));
            Assert.That(caixa.C, Is.EqualTo(Math.Sqrt(2)));
        }

        [Test]
        public void DimensaoMuitoPequena_MasPositiva_EhAceita()
        {
            Assert.DoesNotThrow(() => new Paralelepipedo(1e-6, 1, 1));
        }

        [TestCaseSource(nameof(DimensoesInvalidas))]
        public void ComprimentoInvalido_EhRecusado(double a)
        {
            var erro = Assert.Throws<ArgumentOutOfRangeException>(() => new Paralelepipedo(a, 3, 2));
            Assert.That(erro.ParamName, Is.EqualTo("a"));
        }

        [TestCaseSource(nameof(DimensoesInvalidas))]
        public void LarguraInvalida_EhRecusada(double b)
        {
            var erro = Assert.Throws<ArgumentOutOfRangeException>(() => new Paralelepipedo(4, b, 2));
            Assert.That(erro.ParamName, Is.EqualTo("b"));
        }

        [TestCaseSource(nameof(DimensoesInvalidas))]
        public void AlturaInvalida_EhRecusada(double c)
        {
            var erro = Assert.Throws<ArgumentOutOfRangeException>(() => new Paralelepipedo(4, 3, c));
            Assert.That(erro.ParamName, Is.EqualTo("c"));
        }
    }
}
