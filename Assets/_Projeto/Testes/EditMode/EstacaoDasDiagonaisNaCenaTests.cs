using System.Linq;
using LaboratorioParalelepipedo.Apresentacao;
using LaboratorioParalelepipedo.Nucleo.Estacoes;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LaboratorioParalelepipedo.Testes
{
    /// <summary>
    /// Percorre a Estação 2 na cena gerada, sem XR: cada teste abre a cena de novo e a fecha sem salvar.
    /// </summary>
    public class EstacaoDasDiagonaisNaCenaTests
    {
        private const string CaminhoDaCena = "Assets/_Projeto/Cenas/Laboratorio.unity";
        private Scene _cena;
        private EstacaoDasDiagonaisNaCena _estacao;
        private VisualDaCaixa _visual;
        private DesenhoDasDiagonais _desenho;

        [SetUp]
        public void Abrir()
        {
            _cena = EditorSceneManager.OpenScene(CaminhoDaCena, OpenSceneMode.Additive);
            _estacao = Todos<EstacaoDasDiagonaisNaCena>().Single();
            _visual = Todos<VisualDaCaixa>().Single();
            _desenho = Todos<DesenhoDasDiagonais>().Single();
            _estacao.Inicializar();
        }

        [TearDown]
        public void Fechar()
        {
            if (_cena.IsValid()) EditorSceneManager.CloseScene(_cena, true);
        }

        private T[] Todos<T>() where T : Component =>
            _cena.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<T>(true)).ToArray();

        private T Campo<T>(string nome) where T : Object =>
            (T)new SerializedObject(_estacao).FindProperty(nome).objectReferenceValue;

        private BotaoDoPainel Opcao(int i) =>
            (BotaoDoPainel)new SerializedObject(_estacao).FindProperty("_opcoes").GetArrayElementAtIndex(i).objectReferenceValue;

        private AlvoDeVertice Alvo(string nome) => _visual.Vertice(nome).GetComponent<AlvoDeVertice>();

        private int Indice(System.Func<OpcaoDeResposta, bool> condicao)
        {
            var opcoes = _estacao.Estacao.Opcoes;
            for (int i = 0; i < opcoes.Count; i++)
                if (condicao(opcoes[i])) return i;
            return -1;
        }

        private void AteAPergunta()
        {
            _estacao.Comecar();
            _estacao.TocarVertice("E");
            _estacao.TocarVertice("G");
            _estacao.TocarVertice("E");
            _estacao.TocarVertice("C");
            _estacao.MostrarOpcoesAgora();
        }

        [Test]
        public void Abertura_PainelEscondido_CaixaComOsOitoVerticesClicaveis()
        {
            Assert.That(_estacao.Estacao.Etapa, Is.EqualTo(EtapaDasDiagonais.Abertura));
            Assert.That(Campo<GameObject>("_painelDaEstacao").activeSelf, Is.False);
            foreach (string nome in new[] { "A", "B", "C", "D", "E", "F", "G", "H" })
            {
                Assert.That(Alvo(nome), Is.Not.Null, nome);
                Assert.That(Alvo(nome).GetComponent<SphereCollider>(), Is.Not.Null, nome);
            }
            Assert.That(_desenho.transform.Find(DesenhoDasDiagonais.NomeDoGrupo).parent, Is.EqualTo(_visual.transform),
                "o desenho gira com a caixa na mão");
        }

        [Test]
        public void Comecar_MostraOPainelEABaseDestacada()
        {
            _estacao.Comecar();

            Assert.That(Campo<GameObject>("_painelDaEstacao").activeSelf, Is.True);
            Assert.That(_desenho.Base.activeSelf, Is.True);
            Assert.That(_desenho.Passo1.activeSelf, Is.False);
            Assert.That(_estacao.TextoDoPainel, Does.Contain("a = 2").And.Contain("b = 3").And.Contain("c = 6"));
        }

        [Test]
        public void VerticeErrado_Pisca_ESoAvancaComAsPontasCertas()
        {
            _estacao.Comecar();

            Assert.That(_estacao.TocarVertice("B"), Is.EqualTo(ResultadoDoToque.Errado));
            Assert.That(Alvo("B").Piscando, Is.True);
            Assert.That(_estacao.TocarVertice("G"), Is.EqualTo(ResultadoDoToque.Escolheu), "vale começar pelo G");
            Assert.That(_estacao.TocarVertice("E"), Is.EqualTo(ResultadoDoToque.Ligou));
            Assert.That(_desenho.Passo1.activeSelf, Is.True);
            Assert.That(_estacao.TextoDoPainel, Does.Contain("√13"));
        }

        [Test]
        public void Passo2_TrianguloSobe_EAsOpcoesSoAparecemDepois()
        {
            _estacao.Comecar();
            _estacao.TocarVertice("E");
            _estacao.TocarVertice("G");
            _estacao.TocarVertice("C");
            _estacao.TocarVertice("E");

            Assert.That(_desenho.Passo2.activeSelf, Is.True);
            Assert.That(_desenho.Subindo, Is.True);
            Assert.That(_estacao.OpcoesAVista, Is.False, "espera a subida");
            _desenho.Avancar(DesenhoDasDiagonais.DuracaoDaSubida);
            Assert.That(_desenho.Subindo, Is.False);

            _estacao.MostrarOpcoesAgora();
            Assert.That(_estacao.OpcoesAVista, Is.True);
            Assert.That(_estacao.TextoDoPainel, Does.Contain("49").And.Contain("D = ?"));
            for (int i = 0; i < 4; i++)
                Assert.That(Opcao(i).Texto, Is.EqualTo(_estacao.Estacao.Opcoes[i].Texto));
        }

        [Test]
        public void RespostaErrada_BotaoLaranja_EDicaDoDistrator()
        {
            AteAPergunta();
            int quadrado = Indice(o => o.Texto == "49");

            Assert.That(_estacao.Responder(quadrado), Is.False);
            Assert.That(Opcao(quadrado).Estado, Is.EqualTo(EstadoDoBotao.Errado));
            Assert.That(_estacao.TextoDoPainel, Does.Contain("Falta tirar a raiz"));
            Assert.That(_estacao.Estacao.Etapa, Is.EqualTo(EtapaDasDiagonais.Pergunta), "tenta de novo");
        }

        [Test]
        public void Acerto_BotaoVerde_Vibra_EFechamentoTrocaACaixa()
        {
            AteAPergunta();
            int acertos = 0;
            _estacao.Acertou += () => acertos++;
            int certa = Indice(o => o.Correta);

            Assert.That(_estacao.Responder(certa), Is.True);
            Assert.That(acertos, Is.EqualTo(1));
            Assert.That(Opcao(certa).Estado, Is.EqualTo(EstadoDoBotao.Certo));
            Opcao(certa).AoAcionar();
            Assert.That(acertos, Is.EqualTo(1), "botão já certo não responde de novo");

            _estacao.ConcluirAgora();
            Assert.That(_estacao.Estacao.Etapa, Is.EqualTo(EtapaDasDiagonais.Fechamento));
            Assert.That(Campo<BotaoDoPainel>("_proximoDesafio").gameObject.activeSelf, Is.True);
            Assert.That(Campo<BotaoDoPainel>("_refazer").gameObject.activeSelf, Is.True);
            Assert.That(_estacao.TextoDoPainel, Does.Contain("√(a² + b² + c²)"));

            Vector3 antes = _visual.TamanhoNaCena;
            _estacao.ProximoDesafio();
            Assert.That(_estacao.Estacao.Etapa, Is.EqualTo(EtapaDasDiagonais.DiagonalDaBase));
            Assert.That(_visual.TamanhoNaCena, Is.Not.EqualTo(antes), "outra caixa");
            Assert.That(Alvo("E").GetComponent<SphereCollider>(), Is.Not.Null, "vértices clicáveis na caixa nova");
            Assert.That(_desenho.Passo1.activeSelf, Is.False);
            Assert.That(_estacao.OpcoesAVista, Is.False);
        }

        [Test]
        public void Refazer_MesmaCaixa_VoltaAoPasso1()
        {
            AteAPergunta();
            _estacao.Responder(Indice(o => o.Correta));
            _estacao.ConcluirAgora();
            Vector3 antes = _visual.TamanhoNaCena;

            _estacao.Refazer();

            Assert.That(_estacao.Estacao.Etapa, Is.EqualTo(EtapaDasDiagonais.DiagonalDaBase));
            Assert.That(_visual.TamanhoNaCena, Is.EqualTo(antes));
            Assert.That(_desenho.Base.activeSelf, Is.True);
            Assert.That(Campo<BotaoDoPainel>("_refazer").gameObject.activeSelf, Is.False);
        }
    }
}
