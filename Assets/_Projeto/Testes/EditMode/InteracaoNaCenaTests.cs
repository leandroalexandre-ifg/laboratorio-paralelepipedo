using LaboratorioParalelepipedo.Apresentacao;
using LaboratorioParalelepipedo.Nucleo;
using LaboratorioParalelepipedo.Nucleo.Dados;
using LaboratorioParalelepipedo.XR;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace LaboratorioParalelepipedo.Testes
{
    /// <summary>Componentes de interação (caixa pegável, raio e locomoção) montados à mão, sem cena nem óculos.</summary>
    public class InteracaoNaCenaTests
    {
        private GameObject _raiz;

        [SetUp]
        public void Criar() => _raiz = new GameObject("Teste");

        [TearDown]
        public void Apagar() => Object.DestroyImmediate(_raiz);

        private CaixaPegavel Caixa(Material destacada = null)
        {
            var objeto = new GameObject("Caixa");
            objeto.transform.SetParent(_raiz.transform, false);
            objeto.transform.position = new Vector3(0f, 0.76f, 1f);
            var visual = objeto.AddComponent<VisualDaCaixa>();
            visual.Configurar(new Material(Shader.Find("Universal Render Pipeline/Unlit")), null, null, null);
            visual.Construir(new Paralelepipedo(4, 3, 2));
            var caixa = objeto.AddComponent<CaixaPegavel>();
            caixa.Configurar(destacada);
            caixa.AjustarColisor();
            caixa.GuardarPosicaoNaMesa();
            return caixa;
        }

        private Transform Mao(Vector3 posicao)
        {
            var mao = new GameObject("Mao").transform;
            mao.SetParent(_raiz.transform, false);
            mao.position = posicao;
            return mao;
        }

        private static void Perto(Vector3 obtido, Vector3 esperado) =>
            Assert.That(Vector3.Distance(obtido, esperado), Is.LessThan(1e-4f), $"{obtido} ≠ {esperado}");

        [Test]
        public void Caixa_ColisorCobreACaixaDesenhada()
        {
            var caixa = Caixa();
            var colisor = caixa.GetComponent<BoxCollider>();

            Perto(colisor.size, new Vector3(0.4f, 0.2f, 0.3f));
            Perto(colisor.center, new Vector3(0f, 0.1f, 0f));
        }

        [Test]
        public void Caixa_SegueAMao_EVoltaAMesaSuaveNaOrientacaoOriginal()
        {
            var caixa = Caixa();
            Transform mao = Mao(new Vector3(0f, 1f, 0.5f));

            caixa.Pegar(mao);
            mao.SetPositionAndRotation(new Vector3(1f, 1.2f, 0.5f), Quaternion.Euler(0f, 90f, 30f));
            caixa.Avancar(0f);

            // Mantém a distância e o ângulo do momento em que foi pega.
            Perto(caixa.transform.position, mao.position + mao.rotation * new Vector3(0f, -0.24f, 0.5f));
            Assert.That(Quaternion.Angle(caixa.transform.rotation, mao.rotation), Is.LessThan(0.01f));

            caixa.Soltar(mao);
            Vector3 aoSoltar = caixa.transform.position;
            caixa.Avancar(0.2f);   // metade do tempo: metade do caminho (smoothstep)
            Perto(caixa.transform.position, Vector3.Lerp(aoSoltar, new Vector3(0f, 0.76f, 1f), 0.5f));
            Assert.That(caixa.Voltando);

            caixa.Avancar(0.25f);
            Perto(caixa.transform.position, new Vector3(0f, 0.76f, 1f));
            Assert.That(Quaternion.Angle(caixa.transform.rotation, Quaternion.identity), Is.LessThan(0.01f), "orientação original");
            Assert.That(caixa.Voltando, Is.False);
        }

        [Test]
        public void Caixa_OutraMaoPega_ASoltaDaPrimeiraNaoDerruba()
        {
            var caixa = Caixa();
            Transform esquerda = Mao(Vector3.zero), direita = Mao(Vector3.one);

            caixa.Pegar(esquerda);
            caixa.Pegar(direita);
            caixa.Soltar(esquerda);

            Assert.That(caixa.EstaNaMaoDe(direita));
            Assert.That(caixa.EstaNaMaoDe(esquerda), Is.False);
            Assert.That(caixa.Voltando, Is.False);
        }

        [Test]
        public void Caixa_AcendeAsArestasEnquantoAlgumRaioAponta()
        {
            var destacada = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
            var caixa = Caixa(destacada);
            var aresta = caixa.transform.Find(VisualDaCaixa.GrupoArestas).GetChild(0).GetComponent<MeshRenderer>();
            Material normal = aresta.sharedMaterial;

            caixa.Destacar(true);
            caixa.Destacar(true);   // os dois raios
            caixa.Destacar(false);
            Assert.That(aresta.sharedMaterial, Is.SameAs(destacada), "um raio ainda aponta");

            caixa.Destacar(false);
            Assert.That(aresta.sharedMaterial, Is.SameAs(normal));
            Object.DestroyImmediate(destacada);
        }

        [Test]
        public void Caixa_ObstaculoDoTeleporteEhOVolumeDela()
        {
            var obstaculo = Caixa().ObstaculoNoPiso();

            Assert.That(obstaculo.Area.Contem(0.19, 1.14));
            Assert.That(obstaculo.Area.Contem(0.21, 1.0), Is.False);
            Assert.That(obstaculo.TopoY, Is.EqualTo(0.96).Within(1e-4));
        }

        [Test]
        public void Raio_AcendeACaixaAoApontar_EApagaAoSair()
        {
            var caixa = Caixa(new Material(Shader.Find("Universal Render Pipeline/Unlit")));
            var objeto = new GameObject("Raio");
            objeto.transform.SetParent(_raiz.transform, false);
            var raio = objeto.AddComponent<RaioDoControle>();
            raio.Configurar(OVRInput.Controller.RTouch, objeto.GetComponent<LineRenderer>(), null, null, null);
            Physics.SyncTransforms();

            Vector3 fim = raio.Mirar(new Ray(new Vector3(0f, 0.86f, 0f), Vector3.forward));
            Assert.That(raio.NoAlvo);
            Assert.That(caixa.Destacada);
            Assert.That(fim.z, Is.EqualTo(0.85f).Within(1e-3f), "o raio para na face da frente");

            raio.Mirar(new Ray(new Vector3(0f, 0.86f, 0f), Vector3.back));
            Assert.That(raio.NoAlvo, Is.False);
            Assert.That(caixa.Destacada, Is.False);
        }

        private LocomocaoDoAluno Locomocao(out Transform rig, out Transform cabeca)
        {
            rig = new GameObject("Rig").transform;
            rig.SetParent(_raiz.transform, false);
            cabeca = new GameObject("Cabeca").transform;
            cabeca.SetParent(rig, false);
            cabeca.localPosition = new Vector3(0.2f, 1.5f, -0.1f);   // a cabeça fora do centro do rig
            cabeca.localRotation = Quaternion.Euler(10f, 20f, 0f);
            var locomocao = rig.gameObject.AddComponent<LocomocaoDoAluno>();
            var serializado = new SerializedObject(locomocao);
            serializado.FindProperty("_rig").objectReferenceValue = rig;
            serializado.FindProperty("_cabeca").objectReferenceValue = cabeca;
            serializado.ApplyModifiedPropertiesWithoutUndo();
            return locomocao;
        }

        [Test]
        public void Teleporte_LevaACabecaAoDestino_SemMudarADirecaoDoOlhar()
        {
            var locomocao = Locomocao(out Transform rig, out Transform cabeca);
            Vector3 olharAntes = cabeca.forward;
            float alturaAntes = cabeca.position.y;

            locomocao.Teleportar(new Vetor3(-2, 0, 1.5));

            Perto(new Vector3(cabeca.position.x, 0f, cabeca.position.z), new Vector3(-2f, 0f, 1.5f));
            Assert.That(cabeca.position.y, Is.EqualTo(alturaAntes).Within(1e-5f));
            Perto(cabeca.forward, olharAntes);
        }

        [Test]
        public void Giro_45GrausEmTornoDaCabeca()
        {
            var locomocao = Locomocao(out Transform rig, out Transform cabeca);
            Vector3 cabecaAntes = cabeca.position;
            float giroAntes = cabeca.eulerAngles.y;

            locomocao.Girar(45);

            Perto(cabeca.position, cabecaAntes);
            Assert.That(Mathf.DeltaAngle(giroAntes, cabeca.eulerAngles.y), Is.EqualTo(45f).Within(0.01f));
        }

        [Test]
        public void Mirar_NaMesaNaoVale_NoChaoLivreVale()
        {
            var locomocao = Locomocao(out _, out _);

            Assert.That(locomocao.Mirar(new Vector3(0.45f, 1.1f, 0.1f), new Vector3(0f, -0.3f, 1f)).Motivo,
                Is.EqualTo(Nucleo.Interacao.MotivoDoTeleporte.NaMesa));
            Assert.That(locomocao.Mirar(new Vector3(0f, 1f, 0f), Vector3.back).Valido);
        }
        private TelaEscura Tela()
        {
            var esfera = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            esfera.transform.SetParent(_raiz.transform, false);
            Object.DestroyImmediate(esfera.GetComponent<Collider>());
            var tela = esfera.AddComponent<TelaEscura>();
            tela.Configurar(esfera.GetComponent<MeshRenderer>());
            return tela;
        }

        private RecentralizacaoDoAluno Recentralizacao(float olharDaCabeca, out Transform rig, out Transform cabeca,
            out TelaEscura tela)
        {
            rig = new GameObject("Rig").transform;
            rig.SetParent(_raiz.transform, false);
            rig.SetPositionAndRotation(new Vector3(1.5f, 0f, -0.8f), Quaternion.Euler(0f, 30f, 0f));
            cabeca = new GameObject("Cabeca").transform;
            cabeca.SetParent(rig, false);
            cabeca.localPosition = new Vector3(0.2f, 1.5f, -0.1f);
            // Inclinada para baixo, olhando (no mundo) para olharDaCabeca.
            cabeca.localRotation = Quaternion.Euler(15f, olharDaCabeca - 30f, 0f);
            tela = Tela();
            var recentralizacao = rig.gameObject.AddComponent<RecentralizacaoDoAluno>();
            var serializado = new SerializedObject(recentralizacao);
            serializado.FindProperty("_rig").objectReferenceValue = rig;
            serializado.FindProperty("_cabeca").objectReferenceValue = cabeca;
            serializado.FindProperty("_telaEscura").objectReferenceValue = tela;
            serializado.ApplyModifiedPropertiesWithoutUndo();
            return recentralizacao;
        }

        [TestCase(180f)]   // a mesa às costas, como apareceu no Quest 2
        [TestCase(-90f)]
        [TestCase(45f)]
        public void Recentralizacao_PoeOAlunoNoPontoDePartida_DeFrenteParaAMesa(float olhar)
        {
            var recentralizacao = Recentralizacao(olhar, out Transform rig, out Transform cabeca, out _);
            float altura = cabeca.position.y;
            float inclinacao = cabeca.eulerAngles.x;

            recentralizacao.Repor();

            Perto(new Vector3(cabeca.position.x, 0f, cabeca.position.z), Vector3.zero);
            Assert.That(cabeca.position.y, Is.EqualTo(altura).Within(1e-4f), "a altura não muda");
            Assert.That(Mathf.DeltaAngle(cabeca.eulerAngles.y, 0f), Is.EqualTo(0f).Within(0.01f), "olhando para +z, a mesa");
            Assert.That(Mathf.DeltaAngle(cabeca.eulerAngles.x, inclinacao), Is.EqualTo(0f).Within(0.01f));
            Assert.That(cabeca.localPosition, Is.EqualTo(new Vector3(0.2f, 1.5f, -0.1f)), "a câmera não é movida");
            Assert.That(rig.eulerAngles.x, Is.EqualTo(0f).Within(1e-4f), "o rig só gira no eixo vertical");
        }

        [Test]
        public void Recentralizacao_SoRepoeComATelaEscura_EDepoisClareia()
        {
            var recentralizacao = Recentralizacao(180f, out _, out Transform cabeca, out TelaEscura tela);
            recentralizacao.Pedir();
            const float quadro = 1f / 64f;
            int passos = 0;
            while (recentralizacao.Reposicoes == 0 && passos++ < 400)
            {
                recentralizacao.Avancar(true, quadro);
                if (recentralizacao.Reposicoes == 0)
                    Assert.That(Mathf.DeltaAngle(cabeca.eulerAngles.y, 180f), Is.EqualTo(0f).Within(0.01f), "nada se move antes do escuro");
            }

            Assert.That(recentralizacao.Reposicoes, Is.EqualTo(1));
            Assert.That(tela.OpacidadeDaRecentralizacao, Is.EqualTo(1f), "repôs no escuro");
            for (int i = 0; i < 30; i++) recentralizacao.Avancar(true, quadro);
            Assert.That(recentralizacao.Ativa, Is.False);
            Assert.That(tela.OpacidadeDaRecentralizacao, Is.EqualTo(0f));
            Assert.That(recentralizacao.Reposicoes, Is.EqualTo(1), "uma vez só por pedido");
            string eventos = "";
            for (int i = 0; i < recentralizacao.EventosGuardadosAgora; i++) eventos += recentralizacao.Evento(i) + "\n";
            Assert.That(eventos, Does.Contain("pedido").And.Contain("cabeça pronta").And.Contain("reposto"),
                "o diagnóstico mostra quando rodou");
        }

        [Test]
        public void Recentralizacao_SemPoseDaCabeca_NaoRepoe()
        {
            var recentralizacao = Recentralizacao(180f, out _, out _, out _);
            recentralizacao.Pedir();
            for (int i = 0; i < 64 * 4; i++) recentralizacao.Avancar(false, 1f / 64f);

            Assert.That(recentralizacao.Reposicoes, Is.EqualTo(0));
            Assert.That(recentralizacao.Ativa, Is.False, "clareia: o aluno não fica preso no escuro");
        }

        [Test]
        public void TelaEscura_MostraAMaiorOpacidade_EDesligaNoZero()
        {
            var tela = Tela();
            var esfera = tela.GetComponent<MeshRenderer>();

            tela.Aplicar();
            Assert.That(esfera.enabled, Is.False);

            tela.OpacidadeDoGiro = 0.4f;
            tela.OpacidadeDaRecentralizacao = 0.7f;
            tela.Aplicar();
            Assert.That(esfera.enabled);
            var bloco = new MaterialPropertyBlock();
            esfera.GetPropertyBlock(bloco);
            Assert.That(bloco.GetColor("_BaseColor").a, Is.EqualTo(0.7f).Within(1e-6f));

            tela.OpacidadeDoGiro = 0f;
            tela.OpacidadeDaRecentralizacao = 0f;
            tela.Aplicar();
            Assert.That(esfera.enabled, Is.False);
        }

        [Test]
        public void Dica_MostraOQueAMaoPodeFazer()
        {
            var dados = _raiz.AddComponent<DadosDoLaboratorio>();
            dados.Configurar(AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/_Projeto/Dados/textos.json"),
                AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/_Projeto/Dados/desafios.json"));
            Transform mao = Mao(new Vector3(0.2f, 1f, 0.3f));
            var objeto = new GameObject("Dica");
            objeto.transform.SetParent(mao, false);
            var conteudo = new GameObject("Conteudo");
            conteudo.transform.SetParent(objeto.transform, false);
            var texto = new GameObject("Texto", typeof(RectTransform)).AddComponent<TMPro.TextMeshPro>();
            texto.transform.SetParent(conteudo.transform, false);
            var dica = objeto.AddComponent<DicaDaMao>();
            dica.Configurar(dados, mao, null, conteudo, texto, null);

            dica.Atualizar();
            Assert.That(dica.ChaveMostrada, Is.Null);
            Assert.That(conteudo.activeSelf, Is.False);

            dica.ApontandoParaPegar = true;
            dica.Atualizar();
            Assert.That(dica.ChaveMostrada, Is.EqualTo(ChavesDeTexto.DicaPegar));
            Assert.That(conteudo.activeSelf);
            Assert.That(texto.text, Is.EqualTo(dados.Catalogo.Obter(ChavesDeTexto.DicaPegar)));
            Assert.That(objeto.transform.position.y, Is.GreaterThan(mao.position.y), "acima da mão");

            dica.Segurando = true;
            dica.Atualizar();
            Assert.That(dica.ChaveMostrada, Is.Null, "segurando a caixa, a dica de pegar some");

            dica.Mirar(true, false);
            dica.Atualizar();
            Assert.That(dica.ChaveMostrada, Is.EqualTo(ChavesDeTexto.DicaTeleporteInvalido));
            dica.Mirar(true, true);
            dica.Atualizar();
            Assert.That(texto.text, Is.EqualTo(dados.Catalogo.Obter(ChavesDeTexto.DicaTeleporteValido)));
        }
    }
}
