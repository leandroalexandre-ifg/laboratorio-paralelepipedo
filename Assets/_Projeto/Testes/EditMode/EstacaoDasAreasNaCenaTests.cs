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
    /// Percorre a Estação 3 na cena gerada, sem XR, chegando a ela pelas Estações 1 e 2: cada teste abre a cena de
    /// novo e a fecha sem salvar.
    /// </summary>
    public class EstacaoDasAreasNaCenaTests
    {
        private const string CaminhoDaCena = "Assets/_Projeto/Cenas/Laboratorio.unity";
        private static readonly Vector3 OlhosDoAluno = new Vector3(0f, 1.4f, 0f);
        private Scene _cena;
        private EstacaoDasAreasNaCena _estacao;
        private EstacaoDasDiagonaisNaCena _diagonais;
        private EstacaoDosElementosNaCena _elementos;
        private VisualDaCaixa _visual;
        private CaixaPegavel _caixa;
        private PlanificacaoNaCena _planificacao;

        [SetUp]
        public void Abrir()
        {
            _cena = EditorSceneManager.OpenScene(CaminhoDaCena, OpenSceneMode.Additive);
            _estacao = Todos<EstacaoDasAreasNaCena>().Single();
            _diagonais = Todos<EstacaoDasDiagonaisNaCena>().Single();
            _elementos = Todos<EstacaoDosElementosNaCena>().Single();
            _visual = Todos<VisualDaCaixa>().Single();
            _caixa = Todos<CaixaPegavel>().Single();
            _planificacao = Todos<PlanificacaoNaCena>().Single();
            _elementos.Inicializar();
            _diagonais.Inicializar();
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

        private int Indice(System.Func<OpcaoDeResposta, bool> condicao)
        {
            var opcoes = _estacao.Estacao.Opcoes;
            for (int i = 0; i < opcoes.Count; i++)
                if (condicao(opcoes[i])) return i;
            return -1;
        }

        // O caminho do aluno: a Estação 1 inteira, a 2 até o fechamento e "Ir para a Estação 3".
        private void ChegarAEstacao3()
        {
            _elementos.Comecar();
            foreach (string face in new[] { "ADHE", "ABFE", "ABCD" })
            {
                foreach (char letra in face) _elementos.TocarVertice(letra.ToString());
                _elementos.ContinuarAgora();
            }
            for (int pergunta = 0; pergunta < 2; pergunta++)
            {
                var opcoes = _elementos.Estacao.Opcoes;
                for (int i = 0; i < opcoes.Count; i++)
                    if (opcoes[i].Correta) _elementos.Responder(i);
                _elementos.ContinuarAgora();
            }
            _elementos.IrParaAProximaEstacao();

            foreach (string vertice in new[] { "E", "G", "E", "C" }) _diagonais.TocarVertice(vertice);
            _diagonais.MostrarOpcoesAgora();
            var deD = _diagonais.Estacao.Opcoes;
            for (int i = 0; i < deD.Count; i++)
                if (deD[i].Correta) _diagonais.Responder(i);
            _diagonais.ConcluirAgora();
            Assert.That(_diagonais.Estacao.Etapa, Is.EqualTo(EtapaDasDiagonais.Fechamento));
            _diagonais.IrParaAProximaEstacao();
        }

        private void AbrirACaixa()
        {
            _estacao.Abrir();
            _estacao.TerminarAnimacao();
        }

        private void AcharOsTresPares()
        {
            foreach (string face in new[] { "ABCD", "ADHE", "ABFE" }) _estacao.TocarFace(face);
            _estacao.ContinuarAgora();
        }

        private void ResponderCerto()
        {
            _estacao.Responder(Indice(o => o.Correta));
            _estacao.ContinuarAgora();
        }

        private Collider ColisorDaFace(string nome) => _planificacao.Face(nome).GetComponent<BoxCollider>();

        // Mão direita de um aluno na posição inicial; mira no meio da face (o colisor dela) e aperta o gatilho.
        private RaioDoControle RaioNaMao()
        {
            var objeto = new GameObject("RaioDeTeste");
            SceneManager.MoveGameObjectToScene(objeto, _cena);
            objeto.transform.position = new Vector3(0.2f, 1.1f, 0.25f);
            var raio = objeto.AddComponent<RaioDoControle>();
            raio.Configurar(OVRInput.Controller.RTouch, objeto.GetComponent<LineRenderer>(), null, null, null);
            return raio;
        }

        private void Gatilho(RaioDoControle raio, Vector3 ponto)
        {
            Physics.SyncTransforms();
            Vector3 origem = raio.transform.position;
            raio.Mirar(new Ray(origem, (ponto - origem).normalized));
            raio.Confirmar();
        }

        [Test]
        public void Chegada_PelaEstacao2_CaixaFechadaNaMesa_PainelNoLugarDoDaEstacao2()
        {
            Assert.That(Campo<GameObject>("_painelDaEstacao").activeSelf, Is.False, "escondido até a Estação 2 terminar");
            ChegarAEstacao3();

            Assert.That(_estacao.Estacao.Etapa, Is.EqualTo(EtapaDasAreas.CaixaFechada));
            Assert.That(Campo<GameObject>("_painelDaEstacao").activeSelf, Is.True);
            Assert.That(Todos<Transform>().Single(t => t.name == "PainelEstacao").gameObject.activeSelf, Is.False);
            Assert.That(_visual.CaixaAVista, Is.True);
            Assert.That(_caixa.Pegavel, Is.True);
            Assert.That(_visual.TamanhoNaCena, Is.EqualTo(new Vector3(0.4f, 0.2f, 0.3f)), "caixa 4 × 3 × 2");
            Assert.That(Campo<BotaoDoPainel>("_abrirOuFechar").gameObject.activeSelf, Is.True);
            Assert.That(Campo<BotaoDoPainel>("_abrirOuFechar").Texto, Does.Contain("Abrir"));
            Assert.That(Campo<BotaoDoPainel>("_refazer").gameObject.activeSelf, Is.False);
            Assert.That(_estacao.TocarFace("ABCD"), Is.EqualTo(ResultadoDoToque.Ignorado), "fechada, não há faces a tocar");
        }

        [Test]
        public void IrParaAEstacao3_SoNoFechamentoDaEstacao2()
        {
            var botao = (BotaoDoPainel)new SerializedObject(_diagonais).FindProperty("_irParaAProxima").objectReferenceValue;
            Assert.That(botao.gameObject.activeSelf, Is.False);
            ChegarAEstacao3();
            Assert.That(_diagonais.Saiu, Is.True);
            Assert.That(_diagonais.VerticesEscolhiveis, Is.Zero);
        }

        [Test]
        public void Abrir_EscondeACaixa_EAsFacesFicamTocaveisNoFim()
        {
            ChegarAEstacao3();
            _estacao.Abrir();

            Assert.That(_planificacao.Animando, Is.True);
            Assert.That(_visual.CaixaAVista, Is.False, "as faces da planificação tomam o lugar da caixa");
            Assert.That(_caixa.Pegavel, Is.False);
            Assert.That(ColisorDaFace("ABCD").enabled, Is.False, "nada a tocar durante a animação");
            _planificacao.Avancar(PlanificacaoNaCena.DuracaoDaAbertura / 2);
            Assert.That(_planificacao.Fracao, Is.InRange(0.05f, 0.95f));
            // No meio da abertura, as faces ficam acima do tampo e perto do meio da mesa (saem da caixa, não somem).
            foreach (var face in Todos<MeshRenderer>().Where(r => r.transform.parent == _planificacao.Face("ABCD").parent))
            {
                Assert.That(face.bounds.min.y, Is.GreaterThan(0.76f), face.name);
                Assert.That(face.bounds.center.z, Is.InRange(0.6f, 1.4f), face.name);
            }

            _estacao.TerminarAnimacao();
            Assert.That(_planificacao.Aberta, Is.True);
            Assert.That(_estacao.Estacao.Etapa, Is.EqualTo(EtapaDasAreas.Pares));
            Assert.That(ColisorDaFace("ABCD").enabled, Is.True);
            Assert.That(_planificacao.Rotulo("ABCD").gameObject.activeSelf, Is.True);
            Assert.That(_planificacao.RotuloDaFace("ABCD"), Does.Contain("4 × 3"), "só os lados, sem a área");
            Assert.That(_planificacao.RotuloDaFace("ABCD"), Does.Not.Contain(" = "));
            Assert.That(Campo<BotaoDoPainel>("_abrirOuFechar").Texto, Does.Contain("Fechar"));
        }

        [Test]
        public void Fechar_DobraAsFacesDeVolta_EACaixaReapareceNoFim()
        {
            ChegarAEstacao3();
            AbrirACaixa();
            _estacao.Fechar();

            Assert.That(_planificacao.Animando, Is.True);
            Assert.That(_visual.CaixaAVista, Is.False, "a caixa só volta quando as faces terminam de dobrar");
            _planificacao.Avancar(PlanificacaoNaCena.DuracaoDaAbertura / 2);
            Assert.That(_planificacao.Fracao, Is.InRange(0.05f, 0.95f));
            _estacao.TerminarAnimacao();

            Assert.That(_planificacao.Fracao, Is.Zero);
            Assert.That(_visual.CaixaAVista, Is.True);
            Assert.That(_caixa.Pegavel, Is.True);
            Assert.That(_planificacao.Face("ABCD").gameObject.activeInHierarchy, Is.False);
            Assert.That(_estacao.TocarFace("ABCD"), Is.EqualTo(ResultadoDoToque.Ignorado));
        }

        [Test]
        public void PeloRaio_TocarUmaFace_AchaOParDela_ComACorEAArea()
        {
            ChegarAEstacao3();
            AbrirACaixa();
            Material neutra = _planificacao.MaterialDaFace("ABCD");
            RaioDoControle raio = RaioNaMao();

            Gatilho(raio, ColisorDaFace("EFGH").bounds.center);
            raio.Mirar(new Ray(raio.transform.position, Vector3.up));   // o raio sai da face: some o azul do apontar

            Assert.That(_estacao.Estacao.ParesEncontrados.Count, Is.EqualTo(1));
            Assert.That(_planificacao.MaterialDaFace("ABCD"), Is.Not.SameAs(neutra));
            Assert.That(_planificacao.MaterialDaFace("EFGH"), Is.SameAs(_planificacao.MaterialDaFace("ABCD")));
            Assert.That(_planificacao.MaterialDaFace("ADHE"), Is.SameAs(neutra), "os outros pares ainda neutros");
            Assert.That(_planificacao.RotuloDaFace("ABCD"), Does.Contain("4 · 3 = 12"));
            Assert.That(_planificacao.RotuloDaFace("EFGH"), Does.Contain("4 · 3 = 12"));
            Assert.That(_estacao.TextoDoPainel, Does.Contain("12"));

            Gatilho(raio, ColisorDaFace("ABCD").bounds.center);
            Assert.That(_estacao.Estacao.ParesEncontrados.Count, Is.EqualTo(1), "a paralela não conta de novo");
        }

        [Test]
        public void PeloRaio_TodasAsSeisFacesSaoTocaveis()
        {
            ChegarAEstacao3();
            AbrirACaixa();
            RaioDoControle raio = RaioNaMao();
            foreach (string face in new[] { "ABCD", "EFGH", "ADHE", "BCGF", "ABFE", "DCGH" })
            {
                Physics.SyncTransforms();
                Vector3 origem = raio.transform.position;
                Vector3 alvo = ColisorDaFace(face).bounds.center;
                Assert.That(Physics.Raycast(origem, (alvo - origem).normalized, out RaycastHit acerto, 3f), Is.True, face);
                Assert.That(acerto.collider.GetComponent<AlvoDeFace>()?.Nome, Is.EqualTo(face), face);
            }
        }

        [Test]
        public void TresPares_LevamAPergunta_ComOBotaoDeAbrirNaTerceiraLinha()
        {
            ChegarAEstacao3();
            AbrirACaixa();
            _estacao.TocarFace("ABCD");
            _estacao.TocarFace("ADHE");
            _estacao.TocarFace("ABFE");
            Assert.That(_estacao.Estacao.Etapa, Is.EqualTo(EtapaDasAreas.Pares), "espera para o aluno ver o último par");
            float yAntes = Campo<BotaoDoPainel>("_abrirOuFechar").transform.localPosition.y;
            float fundoAntes = _estacao.AlturaDoFundo;

            _estacao.ContinuarAgora();

            Assert.That(_estacao.Estacao.Etapa, Is.EqualTo(EtapaDasAreas.Pergunta));
            Assert.That(_estacao.Estacao.Desafio.Grandeza, Is.EqualTo(Nucleo.Grandeza.AreaDaBase), "base primeiro");
            for (int i = 0; i < 4; i++) Assert.That(Opcao(i).gameObject.activeSelf, Is.True);
            Assert.That(Campo<BotaoDoPainel>("_abrirOuFechar").transform.localPosition.y, Is.LessThan(yAntes));
            Assert.That(_estacao.AlturaDoFundo, Is.GreaterThan(fundoAntes), "o fundo cresce com as opções");
            Assert.That(_planificacao.Aberta, Is.True, "a planificação continua aberta para consulta");
        }

        [Test]
        public void RespostaErrada_BotaoLaranja_EDicaDoDistrator()
        {
            ChegarAEstacao3();
            AbrirACaixa();
            AcharOsTresPares();
            int errada = Indice(o => o.Texto == "7");   // a + b, em vez de a · b

            Assert.That(_estacao.Responder(errada), Is.False);

            Assert.That(Opcao(errada).Estado, Is.EqualTo(EstadoDoBotao.Errado));
            Assert.That(_estacao.Estacao.ChaveDaDica, Is.EqualTo(Nucleo.Dados.ChavesDeTexto.AreasDicaSomou));
            Assert.That(_estacao.Estacao.Etapa, Is.EqualTo(EtapaDasAreas.Pergunta));
        }

        [Test]
        public void Acerto_MostraAContaComOsValores_Vibra_ESegue()
        {
            ChegarAEstacao3();
            AbrirACaixa();
            AcharOsTresPares();
            int acertos = 0;
            _estacao.Acertou += () => acertos++;
            int certa = Indice(o => o.Correta);

            Assert.That(_estacao.Responder(certa), Is.True);

            Assert.That(acertos, Is.EqualTo(1));
            Assert.That(Opcao(certa).Estado, Is.EqualTo(EstadoDoBotao.Certo));
            Assert.That(_estacao.Estacao.Etapa, Is.EqualTo(EtapaDasAreas.Acertou));
            Assert.That(_estacao.TextoDoPainel, Does.Contain("4 · 3 = 12"), "a conta com as medidas da caixa");
            _estacao.ContinuarAgora();
            Assert.That(_estacao.Estacao.Desafio.Grandeza, Is.EqualTo(Nucleo.Grandeza.AreaLateral));
            for (int i = 0; i < 4; i++) Assert.That(Opcao(i).Estado, Is.EqualTo(EstadoDoBotao.Normal), "botões limpos");
        }

        [Test]
        public void Fechamento_TresFormulas_ERefazerVoltaACaixaFechada()
        {
            ChegarAEstacao3();
            AbrirACaixa();
            AcharOsTresPares();
            for (int i = 0; i < 3; i++) ResponderCerto();

            Assert.That(_estacao.Estacao.Etapa, Is.EqualTo(EtapaDasAreas.Fechamento));
            Assert.That(_estacao.TextoDoPainel, Does.Contain("A<sub>b</sub>"));
            Assert.That(_estacao.TextoDoPainel, Does.Contain("A<sub>L</sub>"));
            Assert.That(_estacao.TextoDoPainel, Does.Contain("A<sub>T</sub>"));
            Assert.That(Campo<BotaoDoPainel>("_refazer").gameObject.activeSelf, Is.True);
            for (int i = 0; i < 4; i++) Assert.That(Opcao(i).gameObject.activeSelf, Is.False);
            // As fórmulas cabem grandes acima dos botões (que descem uma linha) e o fundo cobre os botões.
            var contas = Campo<TMPro.TextMeshPro>("_contas");
            contas.ForceMeshUpdate();
            Assert.That(contas.fontSize, Is.GreaterThan(0.3f), "fórmulas sem encolher");
            Transform botao = Campo<BotaoDoPainel>("_refazer").transform;
            Assert.That(contas.textBounds.min.y + contas.transform.localPosition.y, Is.GreaterThan(botao.localPosition.y + 0.055f));
            Transform fundo = Campo<Transform>("_fundo");
            Assert.That(fundo.localPosition.y - fundo.localScale.y / 2, Is.LessThan(botao.localPosition.y - 0.055f));

            _estacao.Refazer();

            Assert.That(_estacao.Estacao.Etapa, Is.EqualTo(EtapaDasAreas.CaixaFechada));
            Assert.That(_estacao.Estacao.ParesEncontrados, Is.Empty);
            Assert.That(_visual.CaixaAVista, Is.True);
            Assert.That(_caixa.Pegavel, Is.True);
            Assert.That(_planificacao.Fracao, Is.Zero);
            Assert.That(Campo<BotaoDoPainel>("_refazer").gameObject.activeSelf, Is.False);
        }

        [Test]
        public void Diagnostico_MostraAEstacao3()
        {
            ChegarAEstacao3();
            AbrirACaixa();
            _estacao.TocarFace("ABCD");
            var painel = Todos<PainelDeDiagnostico>().Single();
            Assert.That(new SerializedObject(painel).FindProperty("_estacaoDasAreas").objectReferenceValue, Is.EqualTo(_estacao));
            painel.Atualizar();

            string texto = ((TMPro.TMP_Text)new SerializedObject(painel).FindProperty("_texto").objectReferenceValue).text;
            Assert.That(texto, Does.Contain("Estação 3"));
            Assert.That(texto, Does.Contain("pares 1/3"));
            Assert.That(texto, Does.Contain("caixa aberta: sim"));
        }

        [Test]
        public void Aberta_InclinadaParaOAluno_EDentroDaMesa()
        {
            ChegarAEstacao3();
            AbrirACaixa();
            Transform grupo = _planificacao.Face("ABCD").parent;
            Vector3 paraOsOlhos = (OlhosDoAluno - grupo.position).normalized;

            // Normal das faces aberta (o "para cima" do grupo) entre 30° e 40° para o aluno.
            float inclinacao = Vector3.Angle(grupo.up, Vector3.up);
            Assert.That(inclinacao, Is.InRange(30f, 40f));
            Assert.That(Vector3.Dot(grupo.up, Vector3.back), Is.GreaterThan(0f), "inclinada para o lado do aluno");
            Assert.That(Vector3.Angle(grupo.up, paraOsOlhos), Is.LessThan(45f), "o aluno vê as faces quase de frente");

            // Nenhuma face entra no tampo da mesa, e a planificação cabe na largura da mesa.
            var limites = Todos<MeshRenderer>().Where(r => r.transform.parent == grupo).Select(r => r.bounds).ToArray();
            Assert.That(limites.Length, Is.EqualTo(6));
            Assert.That(limites.Min(b => b.min.y), Is.GreaterThan(0.76f));
            Assert.That(limites.Min(b => b.min.x), Is.GreaterThan(-0.6f));
            Assert.That(limites.Max(b => b.max.x), Is.LessThan(0.6f));
        }

        [Test]
        public void ComAsAreas_CadaRotuloCabeNaSuaFace()
        {
            ChegarAEstacao3();
            AbrirACaixa();
            foreach (string face in new[] { "ABCD", "ADHE", "ABFE" }) _estacao.TocarFace(face);
            foreach (string face in new[] { "ABCD", "EFGH", "ADHE", "BCGF", "ABFE", "DCGH" })
            {
                TMPro.TextMeshPro rotulo = _planificacao.Rotulo(face);
                rotulo.ForceMeshUpdate();
                float largura = rotulo.textBounds.size.x * rotulo.transform.lossyScale.x;
                float daFace = ColisorDaFace(face).bounds.size.x;
                Assert.That(rotulo.text, Does.Contain(" = "), face);
                Assert.That(largura, Is.LessThan(daFace * 0.9f), face);
            }
        }

        [Test]
        public void Aberta_RotulosLegiveisDaPosicaoInicial()
        {
            ChegarAEstacao3();
            AbrirACaixa();
            foreach (string face in new[] { "ABCD", "EFGH", "ADHE", "BCGF", "ABFE", "DCGH" })
            {
                TMPro.TextMeshPro rotulo = _planificacao.Rotulo(face);
                rotulo.GetComponent<RotuloVoltadoParaOAluno>().VirarPara(OlhosDoAluno);
                rotulo.ForceMeshUpdate();
                float distancia = Vector3.Distance(OlhosDoAluno, rotulo.transform.position);
                float altura = rotulo.bounds.size.y * rotulo.transform.lossyScale.y;
                // Texto de duas linhas com pelo menos ~1,5° por linha, a menos de 1,5 m.
                float graus = Mathf.Atan2(altura / 2, distancia) * Mathf.Rad2Deg;
                Assert.That(distancia, Is.LessThan(1.5f), face);
                Assert.That(graus, Is.GreaterThan(1.5f), face);
                Vector3 paraOsOlhos = (OlhosDoAluno - rotulo.transform.position).normalized;
                Assert.That(Vector3.Dot(-rotulo.transform.forward, paraOsOlhos), Is.GreaterThan(0.95f), $"{face} virado para o aluno");
            }
        }
    }
}
