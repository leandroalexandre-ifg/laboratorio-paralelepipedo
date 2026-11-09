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
    /// Percorre a Estação 2 na cena gerada, sem XR: cada teste abre a cena de novo e a fecha sem salvar.
    /// </summary>
    public class EstacaoDasDiagonaisNaCenaTests
    {
        private const string CaminhoDaCena = "Assets/_Projeto/Cenas/Laboratorio.unity";
        private Scene _cena;
        private EstacaoDasDiagonaisNaCena _estacao;
        private EstacaoDosElementosNaCena _elementos;
        private VisualDaCaixa _visual;
        private DesenhoDasDiagonais _desenho;

        [SetUp]
        public void Abrir()
        {
            _cena = EditorSceneManager.OpenScene(CaminhoDaCena, OpenSceneMode.Additive);
            _estacao = Todos<EstacaoDasDiagonaisNaCena>().Single();
            _visual = Todos<VisualDaCaixa>().Single();
            _desenho = Todos<DesenhoDasDiagonais>().Single();
            _elementos = Todos<EstacaoDosElementosNaCena>().Single();
            _elementos.Inicializar();
            _estacao.Inicializar();
        }

        // A Estação 1 inteira, pelos mesmos métodos que o gatilho chama, até o fechamento dela.
        private void ConcluirAEstacao1()
        {
            _elementos.Comecar();
            foreach (string nome in new[] { "A", "B", "F", "E" }) _elementos.TocarVertice(nome);
            _elementos.ContinuarAgora();
            foreach (string nome in new[] { "A", "B", "C", "D" }) _elementos.TocarVertice(nome);
            _elementos.ContinuarAgora();
            for (int pergunta = 0; pergunta < 2; pergunta++)
            {
                var opcoes = _elementos.Estacao.Opcoes;
                for (int i = 0; i < opcoes.Count; i++)
                    if (opcoes[i].Correta) _elementos.Responder(i);
                _elementos.ContinuarAgora();
            }
            Assert.That(_elementos.Estacao.Etapa, Is.EqualTo(EtapaDosElementos.Fechamento));
        }

        // "Ir para a Estação 2" no fechamento da Estação 1: o caminho do aluno até aqui.
        private void ChegarAEstacao2()
        {
            ConcluirAEstacao1();
            _elementos.IrParaAProximaEstacao();
        }

        [TearDown]
        public void Fechar()
        {
            if (_cena.IsValid()) EditorSceneManager.CloseScene(_cena, true);
        }

        private T[] Todos<T>() where T : Component =>
            _cena.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<T>(true)).ToArray();

        private T Campo<T>(string nome) where T : Object => CampoDe<T>(_estacao, nome);

        private static T CampoDe<T>(Object componente, string nome) where T : Object =>
            (T)new SerializedObject(componente).FindProperty(nome).objectReferenceValue;

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

        // Mira no centro do objeto e aperta o gatilho, como no óculos.
        private static void Gatilho(RaioDoControle raio, Transform alvo)
        {
            Physics.SyncTransforms();
            Vector3 origem = raio.transform.position;
            raio.Mirar(new Ray(origem, (alvo.position - origem).normalized));
            raio.Confirmar();
        }

        private Transform FundoDe(BotaoDoPainel botao) => botao.transform.Find("Fundo");

        private void AteAPergunta()
        {
            ChegarAEstacao2();
            _estacao.TocarVertice("E");
            _estacao.TocarVertice("G");
            _estacao.TocarVertice("E");
            _estacao.TocarVertice("C");
            _estacao.MostrarOpcoesAgora();
        }

        [Test]
        public void Abertura_PainelEscondido_AteAEstacao1Terminar()
        {
            Assert.That(_estacao.Estacao.Etapa, Is.EqualTo(EtapaDasDiagonais.Abertura));
            Assert.That(Campo<GameObject>("_painelDaEstacao").activeSelf, Is.False);
            Assert.That(_estacao.VerticesEscolhiveis, Is.Zero, "a caixa é da Estação 1");
            ConcluirAEstacao1();
            Assert.That(_estacao.Estacao.Etapa, Is.EqualTo(EtapaDasDiagonais.Abertura), "só pelo botão do fechamento");
            Assert.That(Campo<GameObject>("_painelDaEstacao").activeSelf, Is.False);
        }

        [Test]
        public void IrParaAEstacao2_TrocaOPainel_ECaixaComOsOitoVerticesClicaveis()
        {
            ChegarAEstacao2();
            Assert.That(_estacao.Estacao.Etapa, Is.EqualTo(EtapaDasDiagonais.DiagonalDaBase));
            Assert.That(Campo<GameObject>("_painelDaEstacao").activeSelf, Is.True);
            Assert.That(CampoDe<GameObject>(_elementos, "_painelDaEstacao").activeSelf, Is.False, "o painel da Estação 1 some");
            Assert.That(_visual.transform.Find(DesenhoDaFace.NomeDoGrupo), Is.Null, "o desenho da face some");
            foreach (string nome in new[] { "A", "B", "C", "D", "E", "F", "G", "H" })
            {
                Assert.That(Alvo(nome), Is.Not.Null, nome);
                Assert.That(Alvo(nome).GetComponent<SphereCollider>(), Is.Not.Null, nome);
            }
            Assert.That(_desenho.transform.Find(DesenhoDasDiagonais.NomeDoGrupo).parent, Is.EqualTo(_visual.transform),
                "o desenho gira com a caixa na mão");
        }

        [Test]
        public void PeloRaio_TodosOsVerticesSaoEscolhiveis_MesmoAtrasDaCaixa()
        {
            RaioDoControle raio = RaioNaMao();
            ConcluirAEstacao1();
            Gatilho(raio, FundoDe(CampoDe<BotaoDoPainel>(_elementos, "_irParaAProxima")));
            Assert.That(_estacao.Estacao.Etapa, Is.EqualTo(EtapaDasDiagonais.DiagonalDaBase), "o gatilho no Ir para a Estação 2");

            // B, C, F e G ficam dentro do colisor da caixa, vistos da mão: o raio passa por ela até o vértice.
            foreach (string nome in new[] { "A", "B", "C", "D", "F", "H" })
            {
                Gatilho(raio, _visual.Vertice(nome));
                Assert.That(Alvo(nome).Piscando, Is.True, $"o raio escolheu o {nome}");
            }
        }

        [Test]
        public void Vertice_EhODaCaixaNova_MesmoComOsDaAntigaAindaNaHierarquia()
        {
            // No óculos, o Destroy só apaga no fim do quadro: por um instante, as esferas antigas continuam ali com o
            // mesmo nome. Na 0.3.0, a busca pelo nome achava a antiga e o alvo do gatilho sumia com ela.
            ChegarAEstacao2();   // a Estação 2 refaz a caixa com os alvos dela
            var antigo = new GameObject(VisualDaCaixa.GrupoVertices).transform;
            antigo.SetParent(_visual.transform, false);
            antigo.SetSiblingIndex(0);
            new GameObject("Vertice E").transform.SetParent(antigo, false);

            Transform e = _visual.Vertice("E");

            Assert.That(e.parent, Is.Not.EqualTo(antigo));
            Assert.That(e.GetComponent<AlvoDeVertice>(), Is.Not.Null, "o alvo do gatilho está na esfera da caixa atual");
            Assert.That(e.GetComponent<SphereCollider>(), Is.Not.Null);
        }

        [Test]
        public void Diagnostico_ContaOsOitoVerticesEscolhiveis()
        {
            Assert.That(_elementos.VerticesEscolhiveis, Is.EqualTo(8), "na abertura, os da Estação 1");
            var painel = Todos<PainelDeDiagnostico>().Single();
            Assert.That(new SerializedObject(painel).FindProperty("_estacao").objectReferenceValue, Is.EqualTo(_estacao));
            Assert.That(new SerializedObject(painel).FindProperty("_estacaoDosElementos").objectReferenceValue, Is.EqualTo(_elementos));

            ChegarAEstacao2();
            Assert.That(_estacao.VerticesEscolhiveis, Is.EqualTo(8));
            Assert.That(_elementos.VerticesEscolhiveis, Is.Zero);
            _estacao.TocarVertice("E");
            _estacao.TocarVertice("G");
            _estacao.TocarVertice("E");
            _estacao.TocarVertice("C");
            _estacao.MostrarOpcoesAgora();
            _estacao.Responder(Indice(o => o.Correta));
            _estacao.ConcluirAgora();
            _estacao.ProximoDesafio();
            Assert.That(_estacao.VerticesEscolhiveis, Is.EqualTo(8), "a caixa nova também");
        }

        [Test]
        public void PeloRaio_AEstacaoInteira_DaEstacao1AoProximoDesafio()
        {
            RaioDoControle raio = RaioNaMao();
            ConcluirAEstacao1();
            Gatilho(raio, FundoDe(CampoDe<BotaoDoPainel>(_elementos, "_irParaAProxima")));
            foreach (string nome in new[] { "E", "G", "E", "C" })
                Gatilho(raio, _visual.Vertice(nome));
            Assert.That(_estacao.Estacao.Etapa, Is.EqualTo(EtapaDasDiagonais.Pergunta));

            _estacao.MostrarOpcoesAgora();
            Gatilho(raio, FundoDe(Opcao(Indice(o => o.Texto == "49"))));
            Assert.That(_estacao.Instrucao, Does.Contain("Falta tirar a raiz"));
            Gatilho(raio, FundoDe(Opcao(Indice(o => o.Correta))));
            Assert.That(_estacao.Estacao.Etapa, Is.EqualTo(EtapaDasDiagonais.Acertou));

            _estacao.ConcluirAgora();
            Gatilho(raio, FundoDe(Campo<BotaoDoPainel>("_proximoDesafio")));
            Assert.That(_estacao.Estacao.IndiceDoDesafio, Is.EqualTo(1));
            Assert.That(_estacao.Estacao.Etapa, Is.EqualTo(EtapaDasDiagonais.DiagonalDaBase));
        }

        [Test]
        public void PeloRaio_ApontarParaUmVerticeAindaPegaACaixa()
        {
            RaioDoControle raio = RaioNaMao();
            Physics.SyncTransforms();
            Vector3 origem = raio.transform.position;
            raio.Mirar(new Ray(origem, (_visual.Vertice("E").position - origem).normalized));

            Assert.That(raio.NoAlvo, Is.True);
            Assert.That(raio.Dica == null || raio.Dica.ApontandoParaPegar, Is.True);
        }

        [Test]
        public void Diagnostico_AbreAFrenteDeOndeOProfessorOlha()
        {
            var painel = Todos<PainelDeDiagnostico>().Single();
            var cabeca = (Transform)new SerializedObject(painel).FindProperty("_cabeca").objectReferenceValue;
            Assert.That(cabeca, Is.Not.Null);
            cabeca.SetPositionAndRotation(new Vector3(0f, 1.4f, 0f), Quaternion.Euler(10f, -90f, 0f));   // olhando à esquerda

            painel.Alternar();

            Assert.That(painel.Visivel, Is.True);
            Vector3 paraOPainel = painel.transform.position - cabeca.position;
            Assert.That(Vector3.Angle(new Vector3(paraOPainel.x, 0f, paraOPainel.z), Vector3.left), Is.LessThan(1f));
            Assert.That(painel.transform.position.y, Is.EqualTo(1.4f).Within(1e-4f), "na altura dos olhos");
            Assert.That(Vector3.Dot(painel.transform.forward, Vector3.left), Is.GreaterThan(0.999f), "virado para quem olha");
        }

        [Test]
        public void Comecar_MostraOPainelEABaseDestacada()
        {
            ChegarAEstacao2();

            Assert.That(Campo<GameObject>("_painelDaEstacao").activeSelf, Is.True);
            Assert.That(_desenho.Base.activeSelf, Is.True);
            Assert.That(_desenho.Passo1.activeSelf, Is.False);
            Assert.That(_estacao.TextoDoPainel, Does.Contain("a = 2").And.Contain("b = 3").And.Contain("c = 6"));
            Assert.That(_estacao.Instrucao, Does.Contain("vértice E"), "a instrução fica no destaque do alto");
            Assert.That(Alvo("E").Pulsando, Is.True, "o vértice pedido pulsa");
            Assert.That(new[] { "A", "B", "C", "D", "F", "G", "H" }.Any(n => Alvo(n).Pulsando), Is.False);
        }

        [Test]
        public void Painel_SemGrandeAreaVazia_CresceComOConteudo()
        {
            ChegarAEstacao2();
            float soTexto = _estacao.AlturaDoFundo;
            _estacao.TocarVertice("E");
            _estacao.TocarVertice("G");
            float comContas = _estacao.AlturaDoFundo;

            Assert.That(soTexto, Is.LessThan(0.5f), "passo 1: só título, instrução e medidas");
            Assert.That(comContas, Is.GreaterThan(soTexto + 0.15f), "passo 2: entram as contas");
        }

        [Test]
        public void Pulso_SegueOVerticePedido_EParaForaDosPassos()
        {
            ChegarAEstacao2();
            Vector3 normal = Alvo("E").transform.localScale;
            Alvo("E").Avancar(AlvoDeVertice.PeriodoDoPulso / 2);
            Assert.That(Alvo("E").transform.localScale.x, Is.GreaterThan(normal.x * 1.5f), "cresce no auge");

            _estacao.TocarVertice("E");
            Assert.That(Alvo("E").Pulsando, Is.False);
            Assert.That(Alvo("E").transform.localScale, Is.EqualTo(normal), "volta ao tamanho");
            Assert.That(Alvo("G").Pulsando, Is.True, "passo 1: E e depois G");

            _estacao.TocarVertice("G");
            Assert.That(Alvo("E").Pulsando, Is.True, "passo 2 começa pelo E");
            _estacao.TocarVertice("E");
            Assert.That(Alvo("C").Pulsando, Is.True, "e depois o C");

            _estacao.TocarVertice("C");
            Assert.That(_estacao.Estacao.Etapa, Is.EqualTo(EtapaDasDiagonais.Pergunta));
            foreach (string nome in new[] { "A", "B", "C", "D", "E", "F", "G", "H" })
                Assert.That(Alvo(nome).Pulsando, Is.False, nome);
        }

        [Test]
        public void VerticesSeparadosNaVistaInicial_EmTodasAsCaixasDaEstacao()
        {
            // Olhos na origem (1,4 m, como o modo Editor, e 1,6 m): cada par de vértices fica a mais de duas esferas
            // de distância, em ângulo, para nenhum esconder o outro.
            var desafios = EstacaoDasDiagonais.DesafiosDaEstacao(Todos<DadosDoLaboratorio>().Single().Desafios);
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

        [Test]
        public void VerticeErrado_Pisca_ESoAvancaComAsPontasCertas()
        {
            ChegarAEstacao2();

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
            ChegarAEstacao2();
            _estacao.TocarVertice("E");
            _estacao.TocarVertice("G");
            _estacao.TocarVertice("C");
            _estacao.TocarVertice("E");

            Assert.That(_desenho.Passo2.activeSelf, Is.True);
            Assert.That(_desenho.Subindo, Is.True);
            Assert.That(_estacao.OpcoesAVista, Is.False, "espera a subida");
            _desenho.Avancar(DesenhoDasDiagonais.DuracaoDaSubida);
            Assert.That(_desenho.Subindo, Is.False);
            float semOpcoes = _estacao.AlturaDoFundo;

            _estacao.MostrarOpcoesAgora();
            Assert.That(_estacao.OpcoesAVista, Is.True);
            Assert.That(_estacao.AlturaDoFundo, Is.GreaterThan(semOpcoes + 0.15f), "o painel cresce para os botões");
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
            _estacao.Responder(Indice(o => o.Texto == "49"));
            int acertos = 0;
            _estacao.Acertou += () => acertos++;
            int certa = Indice(o => o.Correta);

            Assert.That(_estacao.Responder(certa), Is.True);
            Assert.That(acertos, Is.EqualTo(1));
            Assert.That(Opcao(certa).Estado, Is.EqualTo(EstadoDoBotao.Certo));
            Assert.That(_estacao.Instrucao, Does.Contain("Isso!"), "acerto no destaque do alto");
            Opcao(certa).AoAcionar();
            Assert.That(acertos, Is.EqualTo(1), "botão já certo não responde de novo");
            for (int i = 0; i < 4; i++)
                if (i != certa) Assert.That(Opcao(i).Estado, Is.EqualTo(EstadoDoBotao.Normal), "erradas voltam ao normal");

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
