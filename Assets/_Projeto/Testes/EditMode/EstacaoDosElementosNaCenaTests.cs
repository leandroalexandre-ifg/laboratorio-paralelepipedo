using System.Linq;
using LaboratorioParalelepipedo.Apresentacao;
using LaboratorioParalelepipedo.Nucleo.Estacoes;
using LaboratorioParalelepipedo.XR;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LaboratorioParalelepipedo.Testes
{
    /// <summary>
    /// Percorre a Estação 1 na cena gerada, sem XR: cada teste abre a cena de novo e a fecha sem salvar.
    /// </summary>
    public class EstacaoDosElementosNaCenaTests
    {
        private const string CaminhoDaCena = "Assets/_Projeto/Cenas/Laboratorio.unity";
        private Scene _cena;
        private EstacaoDosElementosNaCena _estacao;
        private EstacaoDasDiagonaisNaCena _diagonais;
        private VisualDaCaixa _visual;
        private DesenhoDaFace _desenho;

        [SetUp]
        public void Abrir()
        {
            _cena = EditorSceneManager.OpenScene(CaminhoDaCena, OpenSceneMode.Additive);
            _estacao = Todos<EstacaoDosElementosNaCena>().Single();
            _diagonais = Todos<EstacaoDasDiagonaisNaCena>().Single();
            _visual = Todos<VisualDaCaixa>().Single();
            _desenho = Todos<DesenhoDaFace>().Single();
            _estacao.Inicializar();
            _diagonais.Inicializar();
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

        private void Tocar(string letras)
        {
            foreach (char letra in letras) _estacao.TocarVertice(letra.ToString());
        }

        private void AteAPerguntaDasArestas()
        {
            _estacao.Comecar();
            Tocar("ABFE");
            _estacao.ContinuarAgora();
            Tocar("ABCD");
            _estacao.ContinuarAgora();
        }

        // Mão direita de um aluno na posição inicial, segurando o controle à frente do corpo.
        private RaioDoControle RaioNaMao()
        {
            var objeto = new GameObject("RaioDeTeste");
            SceneManager.MoveGameObjectToScene(objeto, _cena);
            objeto.transform.position = new Vector3(0.2f, 1.1f, 0.25f);
            var raio = objeto.AddComponent<RaioDoControle>();
            raio.Configurar(OVRInput.Controller.RTouch, objeto.GetComponent<LineRenderer>(), null, null, null);
            return raio;
        }

        private static void Gatilho(RaioDoControle raio, Transform alvo)
        {
            Physics.SyncTransforms();
            Vector3 origem = raio.transform.position;
            raio.Mirar(new Ray(origem, (alvo.position - origem).normalized));
            raio.Confirmar();
        }

        private static Transform FundoDe(BotaoDoPainel botao) => botao.transform.Find("Fundo");

        [Test]
        public void Abertura_PainelEscondido_CaixaDaEstacao1ComOsOitoVerticesClicaveis()
        {
            Assert.That(_estacao.Estacao.Etapa, Is.EqualTo(EtapaDosElementos.Abertura));
            Assert.That(Campo<GameObject>("_painelDaEstacao").activeSelf, Is.False);
            Assert.That(Campo<BotaoDoPainel>("_comecar").gameObject.activeSelf, Is.True);
            Assert.That(_visual.TamanhoNaCena.x, Is.EqualTo(0.4f).Within(1e-4f), "a caixa 4 × 3 × 2");
            Assert.That(_estacao.VerticesEscolhiveis, Is.EqualTo(8));
            Assert.That(_desenho.transform.Find(DesenhoDaFace.NomeDoGrupo).parent, Is.EqualTo(_visual.transform),
                "o desenho gira com a caixa na mão");
            Assert.That(new[] { "A", "B", "C", "D", "E", "F", "G", "H" }.Any(n => Alvo(n).Pulsando), Is.False);
        }

        [Test]
        public void Comecar_MostraOPainelComOEnunciado_SemMedidas()
        {
            _estacao.Comecar();

            Assert.That(Campo<GameObject>("_painelDaEstacao").activeSelf, Is.True);
            Assert.That(Campo<BotaoDoPainel>("_comecar").gameObject.activeSelf, Is.False);
            Assert.That(_estacao.Instrucao, Does.Contain("ABFE"), "o enunciado no destaque do alto");
            Assert.That(_estacao.TextoDoPainel, Does.Contain("Desafio 1 de 4"));
            Assert.That(_estacao.TextoDoPainel, Does.Not.Contain("a = "), "a Estação 1 não fala em medidas");
            Assert.That(_visual.GetComponentsInChildren<TMPro.TMP_Text>().Select(t => t.text),
                Has.None.Contains("="), "na caixa, só as letras dos vértices");
        }

        [Test]
        public void MontarFace_ArestasAparecem_EAFaceProntaGanhaACorDoPar()
        {
            _estacao.Comecar();
            Tocar("AB");
            Assert.That(_desenho.ArestasVisiveis, Is.EqualTo(1));
            Assert.That(Alvo("A").GetComponent<Renderer>().sharedMaterial, Is.EqualTo(Alvo("B").GetComponent<Renderer>().sharedMaterial));
            Assert.That(_estacao.TextoDoPainel, Does.Contain("A – B"));
            int acertos = 0;
            _estacao.Acertou += () => acertos++;

            Tocar("FE");

            Assert.That(_estacao.Estacao.Etapa, Is.EqualTo(EtapaDosElementos.Acertou));
            Assert.That(acertos, Is.EqualTo(1));
            Assert.That(_desenho.ArestasVisiveis, Is.EqualTo(4), "a aresta que fecha o contorno também");
            Assert.That(_desenho.FacePreenchida, Is.True);
            Assert.That(_desenho.NomeDaFace, Is.EqualTo("ABFE"));
            Assert.That(_desenho.MaterialDaFace.name, Is.EqualTo("FaceMontadaEsquerdaEDireita"), "ABFE é b·c");
            Assert.That(_estacao.Instrucao, Does.Contain("Você montou a face ABFE"));

            _estacao.ContinuarAgora();
            Assert.That(_estacao.Estacao.Desafio.Id, Is.EqualTo("elem-02"));
            Assert.That(_desenho.ArestasVisiveis, Is.Zero, "a próxima face começa do zero");
            Assert.That(_desenho.FacePreenchida, Is.False);
            Assert.That(_estacao.VerticesEscolhiveis, Is.EqualTo(8));
        }

        [Test]
        public void VerticeErrado_Pisca_MantemOsCertos_EDoisErrosFazemAFacePulsar()
        {
            _estacao.Comecar();
            Tocar("AB");
            Assert.That(_estacao.TocarVertice("C"), Is.EqualTo(ResultadoDoToque.Errado));
            Assert.That(Alvo("C").Piscando, Is.True);
            Assert.That(_estacao.Instrucao, Does.Contain("O vértice C não é da face ABFE"));
            Assert.That(_estacao.TextoDoPainel, Does.Contain("A – B"), "os certos ficam");
            Assert.That(_desenho.ArestasVisiveis, Is.EqualTo(1));

            _estacao.TocarVertice("H");
            foreach (string nome in new[] { "A", "B", "F", "E" }) Assert.That(Alvo(nome).Pulsando, Is.True, nome);
            foreach (string nome in new[] { "C", "D", "G", "H" }) Assert.That(Alvo(nome).Pulsando, Is.False, nome);

            Tocar("FE");
            Assert.That(new[] { "A", "B", "F", "E" }.Any(n => Alvo(n).Pulsando), Is.False, "a face pronta não pulsa");
        }

        [Test]
        public void Pergunta_DicaDoDistrator_EAcertoComSoOBotaoCertoVerde()
        {
            AteAPerguntaDasArestas();
            Assert.That(Opcao(0).gameObject.activeSelf, Is.True);
            float comBotoes = _estacao.AlturaDoFundo;

            _estacao.Responder(Indice(o => o.Texto == "9"));
            Assert.That(_estacao.Instrucao, Does.Contain("arestas escondidas"));
            _estacao.Responder(Indice(o => o.Texto == "8"));
            Assert.That(_estacao.Instrucao, Does.Contain("8 é o número de vértices"));
            int certa = Indice(o => o.Correta);
            Assert.That(_estacao.Responder(certa), Is.True);
            Assert.That(_estacao.Instrucao, Does.Contain("Isso! São 12."));
            Assert.That(Opcao(certa).Estado, Is.EqualTo(EstadoDoBotao.Certo));
            for (int i = 0; i < 4; i++)
                if (i != certa) Assert.That(Opcao(i).Estado, Is.EqualTo(EstadoDoBotao.Normal), "erradas voltam ao normal");

            _estacao.ContinuarAgora();
            Assert.That(_estacao.Estacao.Desafio.Id, Is.EqualTo("elem-04"));
            for (int i = 0; i < 4; i++) Assert.That(Opcao(i).Estado, Is.EqualTo(EstadoDoBotao.Normal), "pergunta nova, botões normais");
            Assert.That(_estacao.AlturaDoFundo, Is.EqualTo(comBotoes).Within(1e-4f));
        }

        [Test]
        public void Painel_SemGrandeAreaVazia_CresceComOsBotoes()
        {
            _estacao.Comecar();
            float soTexto = _estacao.AlturaDoFundo;
            Tocar("ABFE");
            _estacao.ContinuarAgora();
            Tocar("ABCD");
            _estacao.ContinuarAgora();

            Assert.That(soTexto, Is.LessThan(0.6f), "montar face: só título, instrução, desafio e vértices");
            Assert.That(_estacao.AlturaDoFundo, Is.GreaterThan(soTexto + 0.2f), "pergunta: entram os botões");
        }

        [Test]
        public void Fechamento_Resumo_ERefazerVoltaAoPrimeiroDesafio()
        {
            AteAPerguntaDasArestas();
            _estacao.Responder(Indice(o => o.Correta));
            _estacao.ContinuarAgora();
            _estacao.Responder(Indice(o => o.Correta));
            _estacao.ContinuarAgora();

            Assert.That(_estacao.Estacao.Etapa, Is.EqualTo(EtapaDosElementos.Fechamento));
            Assert.That(_estacao.Instrucao, Does.Contain("8 vértices, 12 arestas e 6 faces"));
            Assert.That(Campo<BotaoDoPainel>("_irParaAProxima").gameObject.activeSelf, Is.True);
            Assert.That(Campo<BotaoDoPainel>("_refazer").gameObject.activeSelf, Is.True);
            Assert.That(Opcao(0).gameObject.activeSelf, Is.False);

            _estacao.Refazer();
            Assert.That(_estacao.Estacao.Desafio.Id, Is.EqualTo("elem-01"));
            Assert.That(Campo<BotaoDoPainel>("_refazer").gameObject.activeSelf, Is.False);
            Assert.That(_estacao.VerticesEscolhiveis, Is.EqualTo(8));
        }

        [Test]
        public void PeloRaio_AEstacaoInteira_DoComecarAEstacao2()
        {
            RaioDoControle raio = RaioNaMao();
            Gatilho(raio, FundoDe(Campo<BotaoDoPainel>("_comecar")));
            Assert.That(_estacao.Estacao.Etapa, Is.EqualTo(EtapaDosElementos.MontarFace), "o gatilho no Começar");

            // B e F ficam atrás da caixa, vistos da mão: o raio passa pelo corpo dela até o vértice.
            foreach (string nome in new[] { "A", "B", "F", "E" }) Gatilho(raio, _visual.Vertice(nome));
            Assert.That(_estacao.Estacao.FaceMontada?.Nome, Is.EqualTo("ABFE"));
            _estacao.ContinuarAgora();
            foreach (string nome in new[] { "E", "F", "G", "H" }) Gatilho(raio, _visual.Vertice(nome));
            Assert.That(_estacao.Estacao.Etapa, Is.EqualTo(EtapaDosElementos.Acertou), "qualquer face: EFGH");
            _estacao.ContinuarAgora();

            for (int pergunta = 0; pergunta < 2; pergunta++)
            {
                Gatilho(raio, FundoDe(Opcao(Indice(o => o.Correta))));
                Assert.That(_estacao.Estacao.Etapa, Is.EqualTo(EtapaDosElementos.Acertou));
                _estacao.ContinuarAgora();
            }

            Gatilho(raio, FundoDe(Campo<BotaoDoPainel>("_irParaAProxima")));
            Assert.That(_estacao.Saiu, Is.True);
            Assert.That(_diagonais.Estacao.Etapa, Is.EqualTo(EtapaDasDiagonais.DiagonalDaBase));
            Gatilho(raio, _visual.Vertice("E"));
            Assert.That(_diagonais.Estacao.VerticeEscolhido, Is.EqualTo("E"), "os vértices agora são da Estação 2");
            Assert.That(_estacao.Estacao.Etapa, Is.EqualTo(EtapaDosElementos.Fechamento), "a Estação 1 não escuta mais");
        }

        [Test]
        public void VerticesSeparadosNaVistaInicial_EmTodasAsCaixasDaEstacao()
        {
            // Olhos na origem (1,4 m e 1,6 m): cada par de vértices fica a mais de duas esferas de distância, em ângulo.
            var desafios = EstacaoDosElementos.DesafiosDaEstacao(Todos<DadosDoLaboratorio>().Single().Desafios);
            Assert.That(desafios.Count, Is.GreaterThan(0));
            foreach (var desafio in desafios)
            {
                _visual.Construir(desafio.Caixa);
                Transform[] vertices = desafio.Caixa.Vertices.Select(v => _visual.Vertice(v.Nome)).ToArray();
                foreach (float altura in new[] { 1.4f, 1.6f })
                {
                    var olhos = new Vector3(0f, altura, 0f);
                    for (int i = 0; i < vertices.Length; i++)
                        for (int j = i + 1; j < vertices.Length; j++)
                        {
                            Vector3 p = vertices[i].position - olhos, q = vertices[j].position - olhos;
                            float angulo = Vector3.Angle(p, q) * Mathf.Deg2Rad;
                            float esfera = _visual.DiametroDoVertice / Mathf.Min(p.magnitude, q.magnitude);
                            Assert.That(angulo, Is.GreaterThan(2f * esfera),
                                $"{desafio.Id}: {vertices[i].name} e {vertices[j].name} com olhos a {altura} m");
                        }
                }
            }
        }
    }
}
