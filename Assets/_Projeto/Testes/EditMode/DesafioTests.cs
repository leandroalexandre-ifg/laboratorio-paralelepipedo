using System;
using LaboratorioParalelepipedo.Nucleo;
using NUnit.Framework;

namespace LaboratorioParalelepipedo.Testes
{
    public class DesafioTests
    {
        private const double Eps = 1e-9;

        private static Desafio Criar(
            Estacao estacao = Estacao.Diagonais,
            Paralelepipedo caixa = null,
            Grandeza grandeza = Grandeza.DiagonalDaBase,
            double[] distratores = null,
            double? tolerancia = null,
            string id = "diag-01",
            string bncc = "EF09MA14",
            string enunciado = "Quanto mede a diagonal da base?") =>
            new Desafio(id, estacao, bncc, enunciado, caixa ?? new Paralelepipedo(2, 3, 6), grandeza,
                distratores, tolerancia);

        [TestCase(Grandeza.DiagonalDaBase, 3.6055512754639891)]
        [TestCase(Grandeza.Diagonal, 7)]
        [TestCase(Grandeza.AreaDaBase, 6)]
        [TestCase(Grandeza.AreaLateral, 60)]
        [TestCase(Grandeza.AreaTotal, 72)]
        [TestCase(Grandeza.Volume, 36)]
        public void RespostaCorreta_EhCalculadaPelaGrandeza(Grandeza grandeza, double esperada)
        {
            Assert.That(Criar(grandeza: grandeza).RespostaCorreta, Is.EqualTo(esperada).Within(Eps));
        }

        [Test]
        public void GuardaOsDadosDoDesafio()
        {
            var desafio = Criar(distratores: new[] { 5.0, 13.0 });

            Assert.That(desafio.Id, Is.EqualTo("diag-01"));
            Assert.That(desafio.Estacao, Is.EqualTo(Estacao.Diagonais));
            Assert.That(desafio.CodigoBncc, Is.EqualTo("EF09MA14"));
            Assert.That(desafio.Enunciado, Is.EqualTo("Quanto mede a diagonal da base?"));
            Assert.That(desafio.Distratores, Is.EqualTo(new[] { 5.0, 13.0 }));
        }

        [Test]
        public void SemTolerancia_UsaOPadraoDeUmCentesimo()
        {
            Assert.That(Criar().Tolerancia, Is.EqualTo(0.01));
            Assert.That(Desafio.ToleranciaPadrao, Is.EqualTo(0.01));
        }

        [Test]
        public void ToleranciaDoDesafio_SubstituiOPadrao()
        {
            Assert.That(Criar(tolerancia: 0.1).Tolerancia, Is.EqualTo(0.1));
        }

        [TestCase(0)]
        [TestCase(-0.01)]
        [TestCase(double.NaN)]
        [TestCase(double.PositiveInfinity)]
        public void ToleranciaInvalida_EhRecusada(double tolerancia)
        {
            Assert.Throws<DesafioInvalidoException>(() => Criar(tolerancia: tolerancia));
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("   ")]
        public void SemId_EhRecusado(string id)
        {
            Assert.Throws<DesafioInvalidoException>(() => Criar(id: id));
        }

        [Test]
        public void SemBnccOuSemEnunciado_EhRecusado()
        {
            Assert.Throws<DesafioInvalidoException>(() => Criar(bncc: ""));
            Assert.Throws<DesafioInvalidoException>(() => Criar(enunciado: " "));
        }

        [Test]
        public void SemCaixa_EhRecusado()
        {
            Assert.Throws<DesafioInvalidoException>(() =>
                new Desafio("x", Estacao.Volume, "EF09MA19", "?", null, Grandeza.Volume));
        }

        [Test]
        public void GrandezaOuEstacaoForaDaLista_SaoRecusadas()
        {
            Assert.Throws<DesafioInvalidoException>(() => Criar(grandeza: (Grandeza)99));
            Assert.Throws<DesafioInvalidoException>(() => Criar(estacao: (Estacao)99));
        }

        [Test]
        public void MensagemDeErro_TrazOIdDoDesafio()
        {
            var erro = Assert.Throws<DesafioInvalidoException>(() => Criar(id: "vol-07", enunciado: ""));

            Assert.That(erro.DesafioId, Is.EqualTo("vol-07"));
            Assert.That(erro.Message, Does.Contain("vol-07").And.Contain("enunciado"));
        }

        [TestCase(10, 10, 10)]
        [TestCase(4, 3, 3)]
        [TestCase(2.5, 1.2, 1.4142135623730951)]
        public void EstacaoDeVolume_AceitaDimensoesAte10(double a, double b, double c)
        {
            Assert.DoesNotThrow(() =>
                Criar(Estacao.Volume, new Paralelepipedo(a, b, c), Grandeza.Volume));
        }

        [TestCase(10.5, 3, 3, "a")]
        [TestCase(4, 11, 3, "b")]
        [TestCase(4, 3, 12, "c")]
        public void EstacaoDeVolume_RecusaDimensaoAcimaDe10(double a, double b, double c, string dimensao)
        {
            var erro = Assert.Throws<DesafioInvalidoException>(() =>
                Criar(Estacao.Volume, new Paralelepipedo(a, b, c), Grandeza.Volume));

            Assert.That(erro.Message, Does.Contain($"dimensão {dimensao}"));
        }

        [Test]
        public void OutrasEstacoes_AceitamDimensoesAcimaDe10()
        {
            Assert.DoesNotThrow(() => Criar(Estacao.Diagonais, new Paralelepipedo(3, 4, 12), Grandeza.Diagonal));
        }

        [Test]
        public void DistratorIgualAResposta_EhRecusado()
        {
            var erro = Assert.Throws<DesafioInvalidoException>(() =>
                Criar(grandeza: Grandeza.Diagonal, distratores: new[] { 5.0, 7.0 }));

            Assert.That(erro.Message, Does.Contain("distrator 2").And.Contain("resposta correta"));
        }

        [Test]
        public void DistratorIgualARespostaDentroDaTolerancia_EhRecusado()
        {
            // √13 ≈ 3,6056; 3,61 seria aceito como resposta, então não pode ser distrator.
            Assert.Throws<DesafioInvalidoException>(() => Criar(distratores: new[] { 3.61 }));
        }

        [Test]
        public void DistratorProximoMasForaDaTolerancia_EhAceito()
        {
            Assert.DoesNotThrow(() => Criar(distratores: new[] { 3.7 }));
        }

        [Test]
        public void DistratoresRepetidos_SaoRecusados()
        {
            var erro = Assert.Throws<DesafioInvalidoException>(() => Criar(distratores: new[] { 5.0, 13.0, 5.0 }));

            Assert.That(erro.Message, Does.Contain("distratores 1 e 3"));
        }

        [TestCase(double.NaN)]
        [TestCase(double.PositiveInfinity)]
        public void DistratorQueNaoEhNumero_EhRecusado(double distrator)
        {
            Assert.Throws<DesafioInvalidoException>(() => Criar(distratores: new[] { distrator }));
        }

        [Test]
        public void SemDistratores_ListaVazia()
        {
            Assert.That(Criar().Distratores, Is.Empty);
        }
    }
}
