using LaboratorioParalelepipedo.Apresentacao;
using LaboratorioParalelepipedo.Nucleo;
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
    }
}
