using System.Linq;
using LaboratorioParalelepipedo.Apresentacao;
using LaboratorioParalelepipedo.Nucleo;
using NUnit.Framework;
using TMPro;
using UnityEngine;

namespace LaboratorioParalelepipedo.Testes
{
    public class VisualDaCaixaTests
    {
        private const float Escala = 0.1f;
        private GameObject _objeto;
        private VisualDaCaixa _caixa;

        [SetUp]
        public void Criar()
        {
            _objeto = new GameObject("CaixaDeTeste");
            _caixa = _objeto.AddComponent<VisualDaCaixa>();
            _caixa.Configurar(null, null, null, null, Escala);
        }

        [TearDown]
        public void Destruir() => Object.DestroyImmediate(_objeto);

        private Transform Grupo(string nome) => _objeto.transform.Find(nome);

        private Vector3 PosicaoDoVertice(string nome) =>
            Grupo(VisualDaCaixa.GrupoVertices).Find($"Vertice {nome}").localPosition;

        [Test]
        public void Construir_CriaOsElementosDoParalelepipedo()
        {
            _caixa.Construir(new Paralelepipedo(4, 3, 2));

            Assert.That(Grupo(VisualDaCaixa.GrupoArestas).childCount, Is.EqualTo(12));
            Assert.That(Grupo(VisualDaCaixa.GrupoFaces).childCount, Is.EqualTo(6));
            Assert.That(Grupo(VisualDaCaixa.GrupoVertices).childCount, Is.EqualTo(8));
            Assert.That(Grupo(VisualDaCaixa.GrupoRotulos).childCount, Is.EqualTo(8));
        }

        [Test]
        public void BaseApoiadaEmZero_ECentradaEmXeZ()
        {
            _caixa.Construir(new Paralelepipedo(4, 3, 2));

            // E = (0,0,0) no núcleo: frente-esquerda-embaixo.
            Assert.That(Vector3.Distance(PosicaoDoVertice("E"), new Vector3(-0.2f, 0f, -0.15f)), Is.LessThan(1e-5));
            // C = (a, c, b): fundo-direita-em cima.
            Assert.That(Vector3.Distance(PosicaoDoVertice("C"), new Vector3(0.2f, 0.2f, 0.15f)), Is.LessThan(1e-5));
        }

        [Test]
        public void AFicaNaFrenteEsquerdaDoAluno()
        {
            _caixa.Construir(new Paralelepipedo(4, 3, 2));

            Vector3 a = PosicaoDoVertice("A");
            Assert.That(a.x, Is.LessThan(0f), "A à esquerda");
            Assert.That(a.z, Is.LessThan(0f), "A na frente (do lado do aluno, que olha para +z)");
            Assert.That(a.y, Is.EqualTo(0.2f).Within(1e-5), "A em cima");
        }

        [Test]
        public void Aresta_LigaOsDoisVertices()
        {
            _caixa.Construir(new Paralelepipedo(4, 3, 2));

            Transform ae = Grupo(VisualDaCaixa.GrupoArestas).Find("Aresta AE");
            Vector3 meio = (PosicaoDoVertice("A") + PosicaoDoVertice("E")) / 2;
            Assert.That(Vector3.Distance(ae.localPosition, meio), Is.LessThan(1e-5));
            Assert.That(Vector3.Angle(ae.localRotation * Vector3.forward, Vector3.up), Is.LessThan(0.01f).Or.GreaterThan(179.99f));
            Assert.That(ae.localScale.z, Is.GreaterThan(0.2f), "comprimento c = 2 vira 20 cm, mais a espessura");
        }

        [Test]
        public void Face_TemOsQuatroVerticesDaFace()
        {
            _caixa.Construir(new Paralelepipedo(4, 3, 2));

            var malha = Grupo(VisualDaCaixa.GrupoFaces).Find("Face ABCD").GetComponent<MeshFilter>().sharedMesh;
            Assert.That(malha.vertexCount, Is.EqualTo(4));
            Assert.That(malha.triangles.Length, Is.EqualTo(6));
            Assert.That(malha.vertices.All(v => Mathf.Abs(v.y - 0.2f) < 1e-5f), "a base de cima fica em y = c");
        }

        [Test]
        public void Rotulos_TemOsNomesDosVertices_EFicamForaDaMesa()
        {
            _caixa.Construir(new Paralelepipedo(4, 3, 2));

            var rotulos = Grupo(VisualDaCaixa.GrupoRotulos).GetComponentsInChildren<TextMeshPro>();
            Assert.That(rotulos.Select(r => r.text), Is.EquivalentTo(new[] { "A", "B", "C", "D", "E", "F", "G", "H" }));
            Assert.That(rotulos.All(r => r.transform.localPosition.y >= 0f), "nenhum rótulo entra na mesa");
        }

        [Test]
        public void ConstruirDeNovo_TrocaODesenho()
        {
            _caixa.Construir(new Paralelepipedo(4, 3, 2));
            _caixa.Construir(Paralelepipedo.Cubo(2));

            Assert.That(_objeto.transform.childCount, Is.EqualTo(4));
            Assert.That(Grupo(VisualDaCaixa.GrupoArestas).childCount, Is.EqualTo(12));
            Assert.That(Vector3.Distance(PosicaoDoVertice("E"), new Vector3(-0.1f, 0f, -0.1f)), Is.LessThan(1e-5));
            Assert.That(((Paralelepipedo)_caixa.Prisma).EhCubo);
        }

        [Test]
        public void RenderizadoresNaoProjetamSombra()
        {
            _caixa.Construir(new Paralelepipedo(4, 3, 2));

            var renderizadores = _objeto.GetComponentsInChildren<MeshRenderer>()
                .Where(r => r.GetComponent<TextMeshPro>() == null);
            Assert.That(renderizadores.All(r => r.shadowCastingMode == UnityEngine.Rendering.ShadowCastingMode.Off));
        }
    }
}
