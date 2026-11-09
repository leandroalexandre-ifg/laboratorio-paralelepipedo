using System.Linq;
using LaboratorioParalelepipedo.Nucleo;
using LaboratorioParalelepipedo.Nucleo.Dados;
using LaboratorioParalelepipedo.Nucleo.Estacoes;
using NUnit.Framework;

namespace LaboratorioParalelepipedo.Testes
{
    public class EstacaoDasAreasTests
    {
        private static Medida M(string texto) => InterpretadorDeMedida.Interpretar(texto);

        private static DesafioDeResposta Area(string id, Grandeza grandeza, params string[] distratores) =>
            new DesafioDeResposta(id, Nucleo.Estacao.Areas, "EF08MA19", "Qual é a área?", 4, 3, 2, grandeza,
                distratores.Select(M).ToArray());

        // Os 3 desafios do desafios.json, na mesma ordem: base, lateral, total.
        private static EstacaoDasAreas Nova(int semente = 0) => new EstacaoDasAreas(new[]
        {
            Area("area-01", Grandeza.AreaDaBase, "7", "14", "24"),
            Area("area-02", Grandeza.AreaLateral, "52", "14", "24"),
            Area("area-03", Grandeza.AreaTotal, "24", "26", "40"),
        }, semente);

        private static EstacaoDasAreas NaPrimeiraPergunta(int semente = 0)
        {
            var estacao = Nova(semente);
            estacao.Comecar();
            estacao.Abrir();
            foreach (string face in new[] { "ABCD", "ADHE", "ABFE" }) estacao.TocarFace(face);
            estacao.Continuar();
            return estacao;
        }

        private static int Indice(EstacaoDasAreas estacao, string texto) => estacao.Opcoes.ToList().FindIndex(o => o.Texto == texto);

        private static int Correta(EstacaoDasAreas estacao) => estacao.Opcoes.ToList().FindIndex(o => o.Correta);

        [Test]
        public void Abertura_ComecarDeixaACaixaFechada_EAbrirComecaOsPares()
        {
            var estacao = Nova();
            estacao.Abrir();
            Assert.That(estacao.Aberta, Is.False, "na abertura nada acontece");
            Assert.That(estacao.Etapa, Is.EqualTo(EtapaDasAreas.Abertura));

            estacao.Comecar();
            Assert.That(estacao.Etapa, Is.EqualTo(EtapaDasAreas.CaixaFechada));
            Assert.That(estacao.TocarFace("ABCD"), Is.EqualTo(ResultadoDoToque.Ignorado), "com a caixa fechada, não");

            estacao.Abrir();
            Assert.That(estacao.Aberta, Is.True);
            Assert.That(estacao.Etapa, Is.EqualTo(EtapaDasAreas.Pares));
        }

        [Test]
        public void TocarUmaFace_AchaOParDela_EAFaceParalela()
        {
            var estacao = Nova();
            estacao.Comecar();
            estacao.Abrir();

            Assert.That(estacao.TocarFace("EFGH"), Is.EqualTo(ResultadoDoToque.Ligou));
            Assert.That(estacao.UltimaFace.Nome, Is.EqualTo("EFGH"));
            Assert.That(estacao.UltimaParalela.Nome, Is.EqualTo("ABCD"));
            Assert.That(estacao.ParEncontrado(ParDeFaces.Bases), Is.True);
            Assert.That(estacao.TocarFace("ABCD"), Is.EqualTo(ResultadoDoToque.Ignorado), "o par já foi achado");
            Assert.That(estacao.TocarFace("XYZW"), Is.EqualTo(ResultadoDoToque.Ignorado));
            Assert.That(estacao.ParesEncontrados, Has.Count.EqualTo(1));
        }

        [Test]
        public void ComACaixaFechadaNoMeio_OsParesFicam_EOToqueNaoVale()
        {
            var estacao = Nova();
            estacao.Comecar();
            estacao.Abrir();
            estacao.TocarFace("ADHE");
            estacao.Fechar();
            Assert.That(estacao.Aberta, Is.False);
            Assert.That(estacao.Etapa, Is.EqualTo(EtapaDasAreas.Pares));
            Assert.That(estacao.TocarFace("ABCD"), Is.EqualTo(ResultadoDoToque.Ignorado));
            estacao.Abrir();
            Assert.That(estacao.ParEncontrado(ParDeFaces.FrenteEFundo), Is.True);
        }

        [Test]
        public void TresPares_EContinuar_LevaAPrimeiraPergunta_DaBase()
        {
            var estacao = Nova();
            estacao.Comecar();
            estacao.Abrir();
            estacao.TocarFace("ABCD");
            estacao.TocarFace("BCGF");
            estacao.Continuar();
            Assert.That(estacao.Etapa, Is.EqualTo(EtapaDasAreas.Pares), "faltava um par");
            estacao.TocarFace("DCGH");
            Assert.That(estacao.TodosOsPares, Is.True);
            Assert.That(estacao.Etapa, Is.EqualTo(EtapaDasAreas.Pares), "a frase do último par fica à vista");

            estacao.Continuar();
            Assert.That(estacao.Etapa, Is.EqualTo(EtapaDasAreas.Pergunta));
            Assert.That(estacao.Desafio.Grandeza, Is.EqualTo(Grandeza.AreaDaBase));
            Assert.That(estacao.Opcoes.Select(o => o.Texto), Is.EquivalentTo(new[] { "12", "7", "14", "24" }));
        }

        [TestCase(0, "7", ChavesDeTexto.AreasDicaSomou)]
        [TestCase(0, "14", ChavesDeTexto.AreasDicaPerimetro)]
        [TestCase(0, "24", ChavesDeTexto.AreasDicaVolume)]
        [TestCase(1, "52", ChavesDeTexto.AreasDicaComAsBases)]
        [TestCase(1, "14", ChavesDeTexto.AreasDicaSemDobrar)]
        [TestCase(1, "24", ChavesDeTexto.AreasDicaVolume)]
        [TestCase(2, "24", ChavesDeTexto.AreasDicaVolume)]
        [TestCase(2, "26", ChavesDeTexto.AreasDicaSemDobrar)]
        [TestCase(2, "40", ChavesDeTexto.AreasDicaFaltouPar)]
        public void CadaDistrator_TemADicaDoErroQueImita(int pergunta, string distrator, string chave)
        {
            var estacao = NaPrimeiraPergunta();
            for (int i = 0; i < pergunta; i++)
            {
                estacao.Responder(Correta(estacao));
                estacao.Continuar();
            }

            Assert.That(estacao.Responder(Indice(estacao, distrator)), Is.False);
            Assert.That(estacao.ChaveDaDica, Is.EqualTo(chave));
            Assert.That(estacao.Etapa, Is.EqualTo(EtapaDasAreas.Pergunta));
        }

        [Test]
        public void DicaDaArea_SoLateral_ParaAAreaTotal_EGeral()
        {
            var caixa = new Paralelepipedo(4, 3, 2);
            Assert.That(DicaDaArea.Chave(caixa, Grandeza.AreaTotal, 28, 0.01), Is.EqualTo(ChavesDeTexto.AreasDicaSoLateral));
            Assert.That(DicaDaArea.Chave(caixa, Grandeza.AreaTotal, 2 * (12 + 6), 0.01), Is.EqualTo(ChavesDeTexto.AreasDicaFaltouPar));
            Assert.That(DicaDaArea.Chave(caixa, Grandeza.AreaTotal, 99, 0.01), Is.EqualTo(ChavesDeTexto.AreasDicaGeral));
            Assert.That(DicaDaArea.Chave(caixa, Grandeza.AreaDaBase, 13, 0), Is.EqualTo(ChavesDeTexto.AreasDicaGeral));
        }

        [Test]
        public void TresAcertos_LevamAoFechamento_ERefazerFechaACaixaESemPares()
        {
            var estacao = NaPrimeiraPergunta();
            for (int i = 0; i < 3; i++)
            {
                Assert.That(estacao.Responder(Correta(estacao)), Is.True);
                Assert.That(estacao.Etapa, Is.EqualTo(EtapaDasAreas.Acertou));
                estacao.Continuar();
            }
            Assert.That(estacao.Etapa, Is.EqualTo(EtapaDasAreas.Fechamento));

            estacao.Refazer();
            Assert.That(estacao.Etapa, Is.EqualTo(EtapaDasAreas.CaixaFechada));
            Assert.That(estacao.Aberta, Is.False);
            Assert.That(estacao.ParesEncontrados, Is.Empty);
            Assert.That(estacao.IndiceDoDesafio, Is.Zero);
        }

        [Test]
        public void MesmaSemente_MesmaOrdem()
        {
            Assert.That(NaPrimeiraPergunta(5).Opcoes.Select(o => o.Texto),
                Is.EqualTo(NaPrimeiraPergunta(5).Opcoes.Select(o => o.Texto)));
        }

        [Test]
        public void ForaDaEtapa_ComandosNaoFazemNada()
        {
            var estacao = Nova();
            estacao.Continuar();
            estacao.Refazer();
            estacao.Fechar();
            Assert.That(estacao.Responder(0), Is.False);
            Assert.That(estacao.Etapa, Is.EqualTo(EtapaDasAreas.Abertura));
            estacao.Comecar();
            estacao.Comecar();
            estacao.Refazer();
            Assert.That(estacao.Etapa, Is.EqualTo(EtapaDasAreas.CaixaFechada));
        }

        [Test]
        public void DesafiosDaEstacao_SoAsPerguntasDeArea_EOJsonComecaPelaBase()
        {
            var todos = new Desafio[]
            {
                Area("x", Grandeza.AreaTotal, "24"),
                new DesafioDeResposta("v", Nucleo.Estacao.Areas, "EF08MA19", "?", 4, 3, 2, Grandeza.Volume, new[] { M("9") }),
                new DesafioDeResposta("d", Nucleo.Estacao.Diagonais, "EF09MA14", "?", 2, 3, 6, Grandeza.Diagonal, new[] { M("11") }),
            };
            Assert.That(EstacaoDasAreas.DesafiosDaEstacao(todos).Select(d => d.Id), Is.EqualTo(new[] { "x" }));
            Assert.Throws<System.ArgumentException>(() => new EstacaoDasAreas(new DesafioDeResposta[0]));

            var doJson = EstacaoDasAreas.DesafiosDaEstacao(Apresentacao.CarregadorDeDados.CarregarDesafios(
                UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.TextAsset>("Assets/_Projeto/Dados/desafios.json")));
            Assert.That(doJson.Select(d => d.Grandeza),
                Is.EqualTo(new[] { Grandeza.AreaDaBase, Grandeza.AreaLateral, Grandeza.AreaTotal }));
            Assert.That(doJson.All(d => d.CodigoBncc == "EF08MA19"), Is.True, "retomada");
        }

        [Test]
        public void ContasDaArea_LadosComOMaiorPrimeiro()
        {
            var caixa = new Paralelepipedo(4, 3, 2);
            Assert.That(ContasDaArea.Lados(caixa.ObterFace("ABCD")), Is.EqualTo(("4", "3")));
            Assert.That(ContasDaArea.Lados(caixa.ObterFace("ABFE")), Is.EqualTo(("3", "2")));
            Assert.That(ContasDaArea.Area(caixa.ObterFace("ADHE")), Is.EqualTo("8"));
            Assert.That(EstacaoDasAreas.Paralela(caixa, caixa.ObterFace("ABFE")).Nome, Is.EqualTo("DCGH"));
        }
    }
}
