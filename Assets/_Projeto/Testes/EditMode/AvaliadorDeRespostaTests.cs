using System;
using LaboratorioParalelepipedo.Nucleo;
using NUnit.Framework;

namespace LaboratorioParalelepipedo.Testes
{
    public class AvaliadorDeRespostaTests
    {
        private static Desafio DiagonalDaBase2x3x6(double? tolerancia = null) =>
            new Desafio("diag-01", Estacao.Diagonais, "EF09MA14", "Quanto mede d?",
                new Paralelepipedo(2, 3, 6), Grandeza.DiagonalDaBase, tolerancia: tolerancia);

        private static Desafio Com(Grandeza grandeza, double a, double b, double c) =>
            new Desafio("x", Estacao.Areas, "EF09MA19", "?", new Paralelepipedo(a, b, c), grandeza);

        [TestCase(3.61)]
        [TestCase(3.60)]
        [TestCase(3.6056)]
        public void RaizDe13_AceitaAproximacoesComDuasCasas(double resposta)
        {
            Assert.That(AvaliadorDeResposta.Avaliar(DiagonalDaBase2x3x6(), resposta).Correta, Is.True);
        }

        [Test]
        public void RaizDe13_AceitaOValorExato()
        {
            Assert.That(AvaliadorDeResposta.Avaliar(DiagonalDaBase2x3x6(), Math.Sqrt(13)).Correta, Is.True);
        }

        [TestCase(3.62)]
        [TestCase(3.59)]
        [TestCase(3.6)]
        public void RaizDe13_ComToleranciaMaisApertada(double resposta)
        {
            // Com ±0,001, só 3,606 / 3,605 passam; 3,6 e vizinhos não.
            Assert.That(AvaliadorDeResposta.Avaliar(DiagonalDaBase2x3x6(0.001), resposta).Correta, Is.False);
        }

        [TestCase(3.62)]
        [TestCase(3.59)]
        [TestCase(5)]
        [TestCase(13)]
        public void RaizDe13_RecusaRespostasForaDaTolerancia(double resposta)
        {
            Assert.That(AvaliadorDeResposta.Avaliar(DiagonalDaBase2x3x6(), resposta).Correta, Is.False);
        }

        [Test]
        public void NaBordaDaTolerancia_NaoRecusaPorArredondamento()
        {
            var diagonal = Com(Grandeza.Diagonal, 2, 3, 6); // D = 7
            var areaTotal = Com(Grandeza.AreaTotal, 4, 3, 2); // A_T = 52

            Assert.That(AvaliadorDeResposta.Avaliar(diagonal, 7.01).Correta, Is.True);
            Assert.That(AvaliadorDeResposta.Avaliar(diagonal, 6.99).Correta, Is.True);
            Assert.That(AvaliadorDeResposta.Avaliar(areaTotal, 52.01).Correta, Is.True);
            Assert.That(AvaliadorDeResposta.Avaliar(diagonal, 7.02).Correta, Is.False);
        }

        [Test]
        public void ExemplosDasEstacoes()
        {
            Assert.That(AvaliadorDeResposta.Avaliar(Com(Grandeza.Diagonal, 2, 3, 6), 7).Correta, Is.True);
            Assert.That(AvaliadorDeResposta.Avaliar(Com(Grandeza.AreaTotal, 4, 3, 2), 52).Correta, Is.True);
            Assert.That(AvaliadorDeResposta.Avaliar(Com(Grandeza.Volume, 4, 3, 3), 36).Correta, Is.True);
            Assert.That(AvaliadorDeResposta.Avaliar(Com(Grandeza.Volume, 4, 3, 3), 33).Correta, Is.False);
        }

        [Test]
        public void Resultado_TrazRespostasEDiferenca()
        {
            var resultado = AvaliadorDeResposta.Avaliar(Com(Grandeza.Diagonal, 2, 3, 6), 6.5);

            Assert.That(resultado.Correta, Is.False);
            Assert.That(resultado.RespostaDada, Is.EqualTo(6.5));
            Assert.That(resultado.RespostaCorreta, Is.EqualTo(7).Within(1e-9));
            Assert.That(resultado.Diferenca, Is.EqualTo(0.5).Within(1e-9));
        }

        [TestCase(double.NaN)]
        [TestCase(double.PositiveInfinity)]
        public void RespostaQueNaoEhNumero_EhErradaSemLancarErro(double resposta)
        {
            var resultado = AvaliadorDeResposta.Avaliar(DiagonalDaBase2x3x6(), resposta);

            Assert.That(resultado.Correta, Is.False);
            Assert.That(resultado.Diferenca, Is.NaN);
        }

        [Test]
        public void DesafioNulo_LancaErro()
        {
            Assert.Throws<ArgumentNullException>(() => AvaliadorDeResposta.Avaliar(null, 1));
        }
    }
}
