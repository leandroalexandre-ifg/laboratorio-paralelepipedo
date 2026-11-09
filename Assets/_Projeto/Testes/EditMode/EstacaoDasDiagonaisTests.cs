using System.Collections.Generic;
using System.Linq;
using LaboratorioParalelepipedo.Nucleo;
using LaboratorioParalelepipedo.Nucleo.Dados;
using LaboratorioParalelepipedo.Nucleo.Estacoes;
using NUnit.Framework;

namespace LaboratorioParalelepipedo.Testes
{
    public class EstacaoDasDiagonaisTests
    {
        private static Medida M(string texto) => InterpretadorDeMedida.Interpretar(texto);

        private static DesafioDeResposta Diag(string id, string a, string b, string c, params string[] distratores) =>
            new DesafioDeResposta(id, Nucleo.Estacao.Diagonais, "EF09MA14", "Quanto mede a diagonal D da caixa?",
                M(a), M(b), M(c), Grandeza.Diagonal, distratores.Select(M).ToArray());

        private static EstacaoDasDiagonais Nova(int semente = 0) => new EstacaoDasDiagonais(new[]
        {
            Diag("diag-02", "2", "3", "6", "11", "√13", "49"),
            Diag("diag-03", "3", "4", "12", "19", "5", "169"),
            Diag("diag-04", "2", "2", "2", "2√2", "6", "√6"),
        }, semente);

        private static EstacaoDasDiagonais NaPergunta(int semente = 0)
        {
            var estacao = Nova(semente);
            estacao.Comecar();
            estacao.TocarVertice("E");
            estacao.TocarVertice("G");
            estacao.TocarVertice("E");
            estacao.TocarVertice("C");
            return estacao;
        }

        [Test]
        public void ComecaNaAbertura_EComecarLevaAoPasso1DoPrimeiroDesafio()
        {
            var estacao = Nova();
            Assert.That(estacao.Etapa, Is.EqualTo(EtapaDasDiagonais.Abertura));
            Assert.That(estacao.TocarVertice("E"), Is.EqualTo(ResultadoDoToque.Ignorado), "vértices não valem na abertura");

            estacao.Comecar();
            Assert.That(estacao.Etapa, Is.EqualTo(EtapaDasDiagonais.DiagonalDaBase));
            Assert.That(estacao.Desafio.Id, Is.EqualTo("diag-02"));
            Assert.That(estacao.VerticeEsperado, Is.EqualTo("E"));
        }

        [Test]
        public void Passo1_EDepoisG_LigaADiagonalDaBase()
        {
            var estacao = Nova();
            estacao.Comecar();

            Assert.That(estacao.TocarVertice("E"), Is.EqualTo(ResultadoDoToque.Escolheu));
            Assert.That(estacao.VerticeEsperado, Is.EqualTo("G"));
            Assert.That(estacao.TocarVertice("E"), Is.EqualTo(ResultadoDoToque.Ignorado), "tocar de novo o mesmo não conta");
            Assert.That(estacao.TocarVertice("G"), Is.EqualTo(ResultadoDoToque.Ligou));
            Assert.That(estacao.Etapa, Is.EqualTo(EtapaDasDiagonais.DiagonalDaCaixa));
            Assert.That(estacao.VerticeEscolhido, Is.Null);
        }

        [Test]
        public void ValeOsDoisSentidos()
        {
            var estacao = Nova();
            estacao.Comecar();
            Assert.That(estacao.TocarVertice("G"), Is.EqualTo(ResultadoDoToque.Escolheu));
            Assert.That(estacao.VerticeEsperado, Is.EqualTo("E"));
            Assert.That(estacao.TocarVertice("E"), Is.EqualTo(ResultadoDoToque.Ligou));
            Assert.That(estacao.TocarVertice("C"), Is.EqualTo(ResultadoDoToque.Escolheu));
            Assert.That(estacao.TocarVertice("E"), Is.EqualTo(ResultadoDoToque.Ligou));
            Assert.That(estacao.Etapa, Is.EqualTo(EtapaDasDiagonais.Pergunta));
        }

        [Test]
        public void VerticeErrado_NaoPenaliza_EGuardaAEscolha()
        {
            var estacao = Nova();
            estacao.Comecar();
            Assert.That(estacao.TocarVertice("H"), Is.EqualTo(ResultadoDoToque.Errado));
            Assert.That(estacao.VerticeErrado, Is.EqualTo("H"));
            Assert.That(estacao.Etapa, Is.EqualTo(EtapaDasDiagonais.DiagonalDaBase));

            estacao.TocarVertice("E");
            Assert.That(estacao.VerticeErrado, Is.Null, "acertar limpa o erro");
            Assert.That(estacao.TocarVertice("C"), Is.EqualTo(ResultadoDoToque.Errado), "C não é ponta de d");
            Assert.That(estacao.VerticeEscolhido, Is.EqualTo("E"), "a ponta escolhida continua");
            Assert.That(estacao.TocarVertice("G"), Is.EqualTo(ResultadoDoToque.Ligou));

            Assert.That(estacao.TocarVertice("G"), Is.EqualTo(ResultadoDoToque.Errado), "no passo 2, G não é ponta de D");
        }

        [Test]
        public void Pergunta_TemARespostaCalculadaEOsDistratores()
        {
            var estacao = NaPergunta();

            Assert.That(estacao.Opcoes.Select(o => o.Texto), Is.EquivalentTo(new[] { "7", "11", "√13", "49" }));
            Assert.That(estacao.Opcoes.Single(o => o.Correta).Texto, Is.EqualTo("7"));
            Assert.That(estacao.Opcoes.Single(o => o.Texto == "49").ChaveDaDica, Is.EqualTo(ChavesDeTexto.DiagonaisDicaSemRaiz));
            Assert.That(estacao.Opcoes.Single(o => o.Texto == "11").ChaveDaDica, Is.EqualTo(ChavesDeTexto.DiagonaisDicaSomou));
            Assert.That(estacao.Opcoes.Single(o => o.Texto == "√13").ChaveDaDica, Is.EqualTo(ChavesDeTexto.DiagonaisDicaDiagonalDaBase));
        }

        [Test]
        public void Pergunta_EmbaralhaDeFormaRepetivel()
        {
            var ordem1 = NaPergunta(5).Opcoes.Select(o => o.Texto).ToArray();
            var ordem2 = NaPergunta(5).Opcoes.Select(o => o.Texto).ToArray();
            Assert.That(ordem2, Is.EqualTo(ordem1), "mesma semente, mesma ordem");

            var posicoes = new HashSet<int>();
            for (int semente = 0; semente < 30; semente++)
                posicoes.Add(NaPergunta(semente).Opcoes.ToList().FindIndex(o => o.Correta));
            Assert.That(posicoes.Count, Is.GreaterThan(1), "a resposta certa não fica sempre no mesmo lugar");
        }

        [Test]
        public void Erro_DaADica_ETentaDeNovo_AcertoVaiAoFechamento()
        {
            var estacao = NaPergunta();
            int errada = estacao.Opcoes.ToList().FindIndex(o => o.Texto == "49");
            int certa = estacao.Opcoes.ToList().FindIndex(o => o.Correta);

            Assert.That(estacao.Responder(errada), Is.False);
            Assert.That(estacao.Etapa, Is.EqualTo(EtapaDasDiagonais.Pergunta), "tenta de novo");
            Assert.That(estacao.ChaveDaDica, Is.EqualTo(ChavesDeTexto.DiagonaisDicaSemRaiz));
            Assert.That(estacao.Erros, Is.EqualTo(1));

            Assert.That(estacao.Responder(certa), Is.True);
            Assert.That(estacao.Etapa, Is.EqualTo(EtapaDasDiagonais.Acertou));
            Assert.That(estacao.ChaveDaDica, Is.Null);
            Assert.That(estacao.OpcaoEscolhida, Is.EqualTo(certa));

            estacao.Concluir();
            Assert.That(estacao.Etapa, Is.EqualTo(EtapaDasDiagonais.Fechamento));
        }

        [Test]
        public void ProximoDesafio_TrocaACaixa_ERecomecaOPasso1_VoltandoAoPrimeiroNoFim()
        {
            var estacao = NaPergunta();
            for (int rodada = 0; rodada < 3; rodada++)
            {
                estacao.Responder(estacao.Opcoes.ToList().FindIndex(o => o.Correta));
                estacao.Concluir();
                string anterior = estacao.Desafio.Id;
                estacao.ProximoDesafio();
                Assert.That(estacao.Etapa, Is.EqualTo(EtapaDasDiagonais.DiagonalDaBase));
                Assert.That(estacao.Desafio.Id, Is.Not.EqualTo(anterior));
                Assert.That(estacao.Opcoes, Is.Empty);
                Assert.That(estacao.Erros, Is.EqualTo(0));
                estacao.TocarVertice("E"); estacao.TocarVertice("G");
                estacao.TocarVertice("E"); estacao.TocarVertice("C");
            }
            Assert.That(estacao.Desafio.Id, Is.EqualTo("diag-02"), "depois do último, volta ao primeiro");
        }

        [Test]
        public void Refazer_MesmaCaixaDoPasso1()
        {
            var estacao = NaPergunta();
            estacao.Responder(estacao.Opcoes.ToList().FindIndex(o => o.Correta));
            estacao.Concluir();
            estacao.Refazer();
            Assert.That(estacao.Etapa, Is.EqualTo(EtapaDasDiagonais.DiagonalDaBase));
            Assert.That(estacao.Desafio.Id, Is.EqualTo("diag-02"));
        }

        [Test]
        public void BotoesForaDaHora_NaoFazemNada()
        {
            var estacao = Nova();
            estacao.ProximoDesafio();
            estacao.Refazer();
            estacao.Concluir();
            Assert.That(estacao.Responder(0), Is.False);
            Assert.That(estacao.Etapa, Is.EqualTo(EtapaDasDiagonais.Abertura));
        }

        [Test]
        public void Cubo_RespostaComRaizSimplificada()
        {
            var estacao = NaPergunta();
            for (int i = 0; i < 2; i++)
            {
                estacao.Responder(estacao.Opcoes.ToList().FindIndex(o => o.Correta));
                estacao.Concluir();
                estacao.ProximoDesafio();
                estacao.TocarVertice("E"); estacao.TocarVertice("G");
                estacao.TocarVertice("E"); estacao.TocarVertice("C");
            }
            Assert.That(estacao.Desafio.Id, Is.EqualTo("diag-04"));
            Assert.That(estacao.Opcoes.Single(o => o.Correta).Texto, Is.EqualTo("2√3"));
            Assert.That(estacao.Opcoes.Single(o => o.Texto == "2√2").ChaveDaDica, Is.EqualTo(ChavesDeTexto.DiagonaisDicaDiagonalDaBase));
            Assert.That(estacao.Opcoes.Single(o => o.Texto == "√6").ChaveDaDica, Is.EqualTo(ChavesDeTexto.DiagonaisDicaGeral));
        }

        [Test]
        public void DesafiosDaEstacao_SoOsDaDiagonalD_DoArquivoReal()
        {
            var todos = Apresentacao.CarregadorDeDados.CarregarDesafios(
                UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.TextAsset>("Assets/_Projeto/Dados/desafios.json"));
            var daEstacao = EstacaoDasDiagonais.DesafiosDaEstacao(todos);

            Assert.That(daEstacao.Select(d => d.Id), Is.EqualTo(new[] { "diag-02", "diag-03", "diag-04" }));
            Assert.That(daEstacao[0].Caixa.A, Is.EqualTo(2));
            Assert.That(daEstacao[0].Caixa.B, Is.EqualTo(3));
            Assert.That(daEstacao[0].Caixa.C, Is.EqualTo(6));
        }

        [Test]
        public void SemDesafios_ErroClaro()
        {
            Assert.That(() => new EstacaoDasDiagonais(new DesafioDeResposta[0]), Throws.ArgumentException);
        }
    }
}
