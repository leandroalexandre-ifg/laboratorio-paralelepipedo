using System;
using LaboratorioParalelepipedo.Nucleo;
using NUnit.Framework;

namespace LaboratorioParalelepipedo.Testes
{
    public class ParalelepipedoDiagonaisTests
    {
        private const double Eps = 1e-9;

        [Test]
        public void Caixa2x3x6_TemDiagonalDaBaseRaizDe13EDiagonal7()
        {
            var caixa = new Paralelepipedo(2, 3, 6);

            Assert.That(caixa.DiagonalDaBase, Is.EqualTo(Math.Sqrt(13)).Within(Eps));
            Assert.That(caixa.Diagonal, Is.EqualTo(7).Within(Eps));
        }

        [Test]
        public void Caixa3x4x12_TemDiagonalDaBase5EDiagonal13()
        {
            var caixa = new Paralelepipedo(3, 4, 12);

            Assert.That(caixa.DiagonalDaBase, Is.EqualTo(5).Within(Eps));
            Assert.That(caixa.Diagonal, Is.EqualTo(13).Within(Eps));
        }

        [TestCase(2, 3, 6)]
        [TestCase(2.5, 1.2, 1.4142135623730951)]
        [TestCase(0.3, 7, 0.01)]
        public void Diagonal_EhAHipotenusaDoTrianguloDeCatetosDiagonalDaBaseEAltura(double a, double b, double c)
        {
            var caixa = new Paralelepipedo(a, b, c);
            double d = caixa.DiagonalDaBase;

            Assert.That(caixa.Diagonal, Is.EqualTo(Math.Sqrt(d * d + c * c)).Within(Eps));
        }

        [Test]
        public void Diagonais_CoincidemComAsDistanciasEntreVerticesOpostos()
        {
            var caixa = new Paralelepipedo(2, 3, 6);
            Vetor3 P(string nome) => caixa.ObterVertice(nome).Posicao;

            Assert.That(Vetor3.Distancia(P("E"), P("G")), Is.EqualTo(caixa.DiagonalDaBase).Within(Eps));
            Assert.That(Vetor3.Distancia(P("E"), P("C")), Is.EqualTo(caixa.Diagonal).Within(Eps));
            Assert.That(Vetor3.Distancia(P("A"), P("G")), Is.EqualTo(caixa.Diagonal).Within(Eps));
        }
    }
}
