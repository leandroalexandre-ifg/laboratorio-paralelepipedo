using LaboratorioParalelepipedo.Nucleo;
using NUnit.Framework;

namespace LaboratorioParalelepipedo.Testes
{
    public class DesafioDeRespostaTests
    {
        private const double Eps = 1e-9;

        private static DesafioDeResposta Criar(
            Estacao estacao = Estacao.Diagonais,
            Grandeza grandeza = Grandeza.DiagonalDaBase,
            Medida[] distratores = null,
            double? tolerancia = null,
            string id = "diag-01",
            string bncc = "EF09MA14",
            string enunciado = "Quanto mede a diagonal da base?",
            double a = 2, double b = 3, double c = 6) =>
            new DesafioDeResposta(id, estacao, bncc, enunciado, a, b, c, grandeza, distratores, tolerancia);

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

        [TestCase(Grandeza.NumeroDeVertices, 8)]
        [TestCase(Grandeza.NumeroDeArestas, 12)]
        [TestCase(Grandeza.NumeroDeFaces, 6)]
        public void Contagens_TemRespostaInteiraEToleranciaPadraoZero(Grandeza grandeza, int esperada)
        {
            var desafio = Criar(Estacao.Elementos, grandeza, bncc: "EF06MA17");

            Assert.That(desafio.RespostaCorreta, Is.EqualTo(esperada));
            Assert.That(desafio.Tolerancia, Is.EqualTo(0));
            Assert.That(DesafioDeResposta.ToleranciaPadraoPara(grandeza), Is.EqualTo(0));
        }

        [Test]
        public void Contagem_AceitaToleranciaZeroExplicitaEDistratores()
        {
            var desafio = Criar(Estacao.Elementos, Grandeza.NumeroDeArestas,
                new Medida[] { 8, 6, 10 }, tolerancia: 0);

            Assert.That(desafio.Distratores.Count, Is.EqualTo(3));
        }

        [Test]
        public void Contagem_RecusaDistratorIgualAResposta()
        {
            Assert.Throws<DesafioInvalidoException>(() =>
                Criar(Estacao.Elementos, Grandeza.NumeroDeFaces, new Medida[] { 8, 6 }));
        }

        [Test]
        public void ToleranciaZero_EhRecusadaForaDasContagens()
        {
            var erro = Assert.Throws<DesafioInvalidoException>(() => Criar(tolerancia: 0));

            Assert.That(erro.Message, Does.Contain("maior que zero"));
        }

        [Test]
        public void GrandezasReais_TemToleranciaPadraoDeUmCentesimo()
        {
            foreach (Grandeza g in new[]
                     {
                         Grandeza.DiagonalDaBase, Grandeza.Diagonal, Grandeza.AreaDaBase,
                         Grandeza.AreaLateral, Grandeza.AreaTotal, Grandeza.Volume,
                     })
                Assert.That(DesafioDeResposta.ToleranciaPadraoPara(g), Is.EqualTo(0.01), g.ToString());
        }

        [Test]
        public void GuardaOsDadosDoDesafio()
        {
            var desafio = Criar(distratores: new[] { new Medida(5, "5"), new Medida(2.2360679774997898, "√5") });

            Assert.That(desafio.Id, Is.EqualTo("diag-01"));
            Assert.That(desafio.Tipo, Is.EqualTo(TipoDeDesafio.Resposta));
            Assert.That(desafio.Estacao, Is.EqualTo(Estacao.Diagonais));
            Assert.That(desafio.CodigoBncc, Is.EqualTo("EF09MA14"));
            Assert.That(desafio.Enunciado, Is.EqualTo("Quanto mede a diagonal da base?"));
            Assert.That(desafio.Distratores[1].Texto, Is.EqualTo("√5"));
            Assert.That(desafio.Caixa.Diagonal, Is.EqualTo(7).Within(Eps));
        }

        [Test]
        public void GuardaOTextoDasDimensoes()
        {
            var desafio = new DesafioDeResposta("vol-02", Estacao.Volume, "EF09MA19", "Qual é o volume?",
                2.5, 1.2, new Medida(System.Math.Sqrt(2), "√2"), Grandeza.Volume);

            Assert.That(desafio.Comprimento.Texto, Is.EqualTo("2,5"));
            Assert.That(desafio.Largura.Texto, Is.EqualTo("1,2"));
            Assert.That(desafio.Altura.Texto, Is.EqualTo("√2"));
            Assert.That(desafio.RespostaCorreta, Is.EqualTo(3 * System.Math.Sqrt(2)).Within(Eps));
        }

        [Test]
        public void SemTolerancia_UsaOPadraoDeUmCentesimo()
        {
            Assert.That(Criar().Tolerancia, Is.EqualTo(0.01));
            Assert.That(DesafioDeResposta.ToleranciaPadrao, Is.EqualTo(0.01));
        }

        [Test]
        public void ToleranciaDoDesafio_SubstituiOPadrao()
        {
            Assert.That(Criar(tolerancia: 0.1).Tolerancia, Is.EqualTo(0.1));
        }

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

        [TestCase(0, 3, 6, "a")]
        [TestCase(2, -3, 6, "b")]
        [TestCase(2, 3, double.NaN, "c")]
        public void DimensaoInvalida_DaErroComADimensao(double a, double b, double c, string dimensao)
        {
            var erro = Assert.Throws<DesafioInvalidoException>(() => Criar(a: a, b: b, c: c));

            Assert.That(erro.Message, Does.Contain($"dimensão {dimensao}"));
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
            Assert.DoesNotThrow(() => Criar(Estacao.Volume, Grandeza.Volume, a: a, b: b, c: c));
        }

        [TestCase(10.5, 3, 3, "a")]
        [TestCase(4, 11, 3, "b")]
        [TestCase(4, 3, 12, "c")]
        public void EstacaoDeVolume_RecusaDimensaoAcimaDe10(double a, double b, double c, string dimensao)
        {
            var erro = Assert.Throws<DesafioInvalidoException>(() =>
                Criar(Estacao.Volume, Grandeza.Volume, a: a, b: b, c: c));

            Assert.That(erro.Message, Does.Contain($"dimensão {dimensao}"));
        }

        [Test]
        public void OutrasEstacoes_AceitamDimensoesAcimaDe10()
        {
            Assert.DoesNotThrow(() => Criar(Estacao.Diagonais, Grandeza.Diagonal, a: 3, b: 4, c: 12));
        }

        [Test]
        public void DistratorIgualAResposta_EhRecusado()
        {
            var erro = Assert.Throws<DesafioInvalidoException>(() =>
                Criar(grandeza: Grandeza.Diagonal, distratores: new Medida[] { 5, 7 }));

            Assert.That(erro.Message, Does.Contain("distrator 2").And.Contain("resposta correta"));
        }

        [Test]
        public void DistratorIgualARespostaDentroDaTolerancia_EhRecusado()
        {
            // √13 ≈ 3,6056; 3,61 seria aceito como resposta, então não pode ser distrator.
            Assert.Throws<DesafioInvalidoException>(() => Criar(distratores: new Medida[] { 3.61 }));
        }

        [Test]
        public void DistratorProximoMasForaDaTolerancia_EhAceito()
        {
            Assert.DoesNotThrow(() => Criar(distratores: new Medida[] { 3.7 }));
        }

        [Test]
        public void DistratoresRepetidos_SaoRecusados()
        {
            var erro = Assert.Throws<DesafioInvalidoException>(() =>
                Criar(distratores: new Medida[] { 5, 13, 5 }));

            Assert.That(erro.Message, Does.Contain("distratores 1 (5) e 3 (5)"));
        }

        [TestCase(double.NaN)]
        [TestCase(double.PositiveInfinity)]
        public void DistratorQueNaoEhNumero_EhRecusado(double distrator)
        {
            Assert.Throws<DesafioInvalidoException>(() => Criar(distratores: new Medida[] { distrator }));
        }

        [Test]
        public void SemDistratores_ListaVazia()
        {
            Assert.That(Criar().Distratores, Is.Empty);
        }
    }
}
