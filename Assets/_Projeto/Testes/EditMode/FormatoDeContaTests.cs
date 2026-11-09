using LaboratorioParalelepipedo.Nucleo;
using LaboratorioParalelepipedo.Nucleo.Estacoes;
using NUnit.Framework;

namespace LaboratorioParalelepipedo.Testes
{
    public class FormatoDeContaTests
    {
        [TestCase(13, "√13")]
        [TestCase(49, "7")]
        [TestCase(12, "2√3")]
        [TestCase(8, "2√2")]
        [TestCase(25, "5")]
        [TestCase(72, "6√2")]
        [TestCase(0, "0")]
        [TestCase(6.25, "√(6,25)")]
        public void Raiz(double quadrado, string esperado)
        {
            Assert.That(FormatoDeConta.Raiz(quadrado), Is.EqualTo(esperado));
        }

        [Test]
        public void Raiz_IgnoraErroDeArredondamento()
        {
            Assert.That(FormatoDeConta.Raiz(0.1 * 30 * 0.1 * 30 + 4), Is.EqualTo("√13"));
        }

        [TestCase(4, "4")]
        [TestCase(6.25, "6,25")]
        [TestCase(2.0000000001, "2")]
        public void Numero(double valor, string esperado)
        {
            Assert.That(FormatoDeConta.Numero(valor), Is.EqualTo(esperado));
        }

        [Test]
        public void ContasDaCaixa2x3x6()
        {
            var caixa = new Paralelepipedo(2, 3, 6);
            Assert.That(ContasDaDiagonal.QuadradoDeA(caixa), Is.EqualTo("4"));
            Assert.That(ContasDaDiagonal.QuadradoDeB(caixa), Is.EqualTo("9"));
            Assert.That(ContasDaDiagonal.DiagonalDaBase(caixa), Is.EqualTo("√13"));
            Assert.That(ContasDaDiagonal.QuadradoDaDiagonalDaBase(caixa), Is.EqualTo("13"));
            Assert.That(ContasDaDiagonal.QuadradoDeC(caixa), Is.EqualTo("36"));
            Assert.That(ContasDaDiagonal.QuadradoDaDiagonal(caixa), Is.EqualTo("49"));
            Assert.That(ContasDaDiagonal.Diagonal(caixa), Is.EqualTo("7"));
        }
    }
}
