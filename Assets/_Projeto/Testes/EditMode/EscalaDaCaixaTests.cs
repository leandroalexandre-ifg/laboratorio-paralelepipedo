using System;
using LaboratorioParalelepipedo.Nucleo;
using NUnit.Framework;

namespace LaboratorioParalelepipedo.Testes
{
    public class EscalaDaCaixaTests
    {
        [TestCase(4, 3, 2, 0.1)]     // caixa dos mockups: 40 × 30 × 20 cm
        [TestCase(6, 1, 1, 0.1)]     // maior dimensão exatamente 0,6 m
        [TestCase(10, 5, 5, 0.06)]   // maior caixa da estação de volume: 10 vira 0,6 m
        [TestCase(2, 12, 3, 0.05)]   // a maior pode ser a largura
        [TestCase(3, 4, 20, 0.03)]   // ou a altura
        public void MaiorDimensaoCabeEm60Centimetros(double a, double b, double c, double esperada)
        {
            var caixa = new Paralelepipedo(a, b, c);

            double escala = EscalaDaCaixa.MetrosPorUnidade(caixa);

            Assert.That(escala, Is.EqualTo(esperada).Within(1e-12));
            Assert.That(Math.Max(a, Math.Max(b, c)) * escala, Is.LessThanOrEqualTo(0.6 + 1e-12));
        }

        [Test]
        public void MedidasIrracionais_TambemCabem()
        {
            var caixa = new Paralelepipedo(Math.Sqrt(50), 1, Math.PI);

            Assert.That(EscalaDaCaixa.MaiorDimensao(caixa), Is.EqualTo(Math.Sqrt(50)).Within(1e-12));
            Assert.That(EscalaDaCaixa.MaiorDimensao(caixa) * EscalaDaCaixa.MetrosPorUnidade(caixa),
                Is.EqualTo(0.6).Within(1e-12));
        }

        [Test]
        public void CubinhoAcompanhaAEscalaDaCaixa()
        {
            var pequena = new Paralelepipedo(4, 3, 3);
            var grande = new Paralelepipedo(10, 10, 10);

            Assert.That(EscalaDaCaixa.ArestaDoCubinhoEmMetros(pequena), Is.EqualTo(0.1).Within(1e-12));
            Assert.That(EscalaDaCaixa.ArestaDoCubinhoEmMetros(grande), Is.EqualTo(0.06).Within(1e-12));
            // 10 cubinhos enfileirados ocupam a aresta toda da caixa grande.
            Assert.That(10 * EscalaDaCaixa.ArestaDoCubinhoEmMetros(grande), Is.EqualTo(0.6).Within(1e-12));
        }

        [Test]
        public void EscalaPorDesafio_UsaACaixaDoDesafio()
        {
            var desafio = new DesafioDeResposta("vol-x", Estacao.Volume, "EF09MA19", "?",
                10, 2, 2, Grandeza.Volume, new Medida[] { 4 });

            Assert.That(EscalaDaCaixa.MetrosPorUnidade(desafio), Is.EqualTo(0.06).Within(1e-12));
        }

        [Test]
        public void Nulo_DaErro()
        {
            Assert.Throws<ArgumentNullException>(() => EscalaDaCaixa.MetrosPorUnidade((IPrisma)null));
            Assert.Throws<ArgumentNullException>(() => EscalaDaCaixa.MetrosPorUnidade((Desafio)null));
        }
    }
}
