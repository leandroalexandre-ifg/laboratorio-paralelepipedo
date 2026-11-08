using System.Collections.Generic;
using LaboratorioParalelepipedo.Nucleo;
using NUnit.Framework;

namespace LaboratorioParalelepipedo.Testes
{
    public class ParalelepipedoElementosTests
    {
        private const double Eps = 1e-12;
        private Paralelepipedo _caixa;

        [SetUp]
        public void Preparar() => _caixa = new Paralelepipedo(4, 3, 2);

        [Test]
        public void TemOitoVerticesDozeArestasESeisFaces()
        {
            Assert.That(_caixa.Vertices.Count, Is.EqualTo(8));
            Assert.That(_caixa.Arestas.Count, Is.EqualTo(12));
            Assert.That(_caixa.Faces.Count, Is.EqualTo(6));
        }

        [Test]
        public void NomesSeguemAConvencao()
        {
            Assert.That(Nomes(_caixa.Vertices), Is.EqualTo(new[] { "A", "B", "C", "D", "E", "F", "G", "H" }));
            Assert.That(Nomes(_caixa.Arestas), Is.EqualTo(new[]
            {
                "AB", "BC", "CD", "DA", "EF", "FG", "GH", "HE", "AE", "BF", "CG", "DH",
            }));
            Assert.That(Nomes(_caixa.Faces), Is.EqualTo(new[] { "ABCD", "EFGH", "ADHE", "BCGF", "ABFE", "DCGH" }));
        }

        [Test]
        public void DimensoesCorrespondemAsArestasEH_HG_AE()
        {
            Assert.That(Distancia("E", "H"), Is.EqualTo(_caixa.A).Within(Eps));
            Assert.That(Distancia("H", "G"), Is.EqualTo(_caixa.B).Within(Eps));
            Assert.That(Distancia("A", "E"), Is.EqualTo(_caixa.C).Within(Eps));
        }

        [Test]
        public void VerticesDoTopoFicamSobreOsDeBaixo()
        {
            AssertSobre("A", "E");
            AssertSobre("B", "F");
            AssertSobre("C", "G");
            AssertSobre("D", "H");
        }

        [Test]
        public void PosicoesNaFrenteEsquerda()
        {
            // A frente-esquerda no topo; B e F ao fundo; D e H à direita.
            Assert.That(_caixa.ObterVertice("A").Posicao, Is.EqualTo(new Vetor3(0, 2, 0)));
            Assert.That(_caixa.ObterVertice("B").Posicao.Z, Is.EqualTo(3));
            Assert.That(_caixa.ObterVertice("D").Posicao.X, Is.EqualTo(4));
            Assert.That(_caixa.ObterVertice("E").Posicao, Is.EqualTo(new Vetor3(0, 0, 0)));
        }

        [Test]
        public void HaQuatroArestasDeCadaDimensao()
        {
            int comA = 0, comB = 0, comC = 0;
            foreach (var aresta in _caixa.Arestas)
            {
                if (Igual(aresta.Comprimento, _caixa.A)) comA++;
                else if (Igual(aresta.Comprimento, _caixa.B)) comB++;
                else if (Igual(aresta.Comprimento, _caixa.C)) comC++;
            }

            Assert.That(new[] { comA, comB, comC }, Is.EqualTo(new[] { 4, 4, 4 }));
        }

        [Test]
        public void VerticesConsecutivosDeCadaFaceSaoLigadosPorAresta()
        {
            foreach (var face in _caixa.Faces)
            {
                Assert.That(face.Vertices.Count, Is.EqualTo(4), face.Nome);
                for (int i = 0; i < 4; i++)
                {
                    var p = face.Vertices[i];
                    var q = face.Vertices[(i + 1) % 4];
                    Assert.That(ExisteAresta(p, q), Is.True, $"{face.Nome}: falta a aresta {p}{q}");
                }
            }
        }

        [Test]
        public void CadaVerticeEstaEmTresArestasETresFaces()
        {
            foreach (var v in _caixa.Vertices)
            {
                int arestas = 0, faces = 0;
                foreach (var aresta in _caixa.Arestas)
                    if (aresta.Inicio == v || aresta.Fim == v) arestas++;
                foreach (var face in _caixa.Faces)
                    if (face.Contem(v)) faces++;

                Assert.That(arestas, Is.EqualTo(3), v.Nome);
                Assert.That(faces, Is.EqualTo(3), v.Nome);
            }
        }

        [Test]
        public void AreasDasFacesSeguemOsPares()
        {
            Assert.That(_caixa.ObterFace("ABCD").Area, Is.EqualTo(12).Within(Eps));
            Assert.That(_caixa.ObterFace("ADHE").Area, Is.EqualTo(8).Within(Eps));
            Assert.That(_caixa.ObterFace("ABFE").Area, Is.EqualTo(6).Within(Eps));
            Assert.That(_caixa.ObterFace("ABCD").Par, Is.EqualTo(ParDeFaces.Bases));
            Assert.That(_caixa.ObterFace("BCGF").Par, Is.EqualTo(ParDeFaces.FrenteEFundo));
            Assert.That(_caixa.ObterFace("DCGH").Par, Is.EqualTo(ParDeFaces.EsquerdaEDireita));
        }

        [Test]
        public void FacesParalelasSaoCongruentesENaoTemVerticeEmComum()
        {
            foreach (var par in new[] { ParDeFaces.Bases, ParDeFaces.FrenteEFundo, ParDeFaces.EsquerdaEDireita })
            {
                var doPar = new List<Face>();
                foreach (var face in _caixa.Faces)
                    if (face.Par == par) doPar.Add(face);

                Assert.That(doPar.Count, Is.EqualTo(2), par.ToString());
                Assert.That(doPar[0].Area, Is.EqualTo(doPar[1].Area).Within(Eps), par.ToString());
                foreach (var v in doPar[0].Vertices)
                    Assert.That(doPar[1].Contem(v), Is.False, $"{par}: {v} nas duas faces");
            }
        }

        [Test]
        public void NomesInexistentesDevolvemNulo()
        {
            Assert.That(_caixa.ObterVertice("Z"), Is.Null);
            Assert.That(_caixa.ObterFace("ABFG"), Is.Null);
        }

        private double Distancia(string p, string q) =>
            Vetor3.Distancia(_caixa.ObterVertice(p).Posicao, _caixa.ObterVertice(q).Posicao);

        private void AssertSobre(string topo, string baixo)
        {
            var t = _caixa.ObterVertice(topo).Posicao;
            var b = _caixa.ObterVertice(baixo).Posicao;
            Assert.That(t.X, Is.EqualTo(b.X), $"{topo} sobre {baixo}");
            Assert.That(t.Z, Is.EqualTo(b.Z), $"{topo} sobre {baixo}");
            Assert.That(t.Y - b.Y, Is.EqualTo(_caixa.C).Within(Eps), $"{topo} sobre {baixo}");
        }

        private bool ExisteAresta(Vertice p, Vertice q)
        {
            foreach (var aresta in _caixa.Arestas)
                if (aresta.Liga(p, q)) return true;
            return false;
        }

        private static bool Igual(double x, double y) => System.Math.Abs(x - y) < Eps;

        private static string[] Nomes(IReadOnlyList<Vertice> itens)
        {
            var nomes = new string[itens.Count];
            for (int i = 0; i < nomes.Length; i++) nomes[i] = itens[i].Nome;
            return nomes;
        }

        private static string[] Nomes(IReadOnlyList<Aresta> itens)
        {
            var nomes = new string[itens.Count];
            for (int i = 0; i < nomes.Length; i++) nomes[i] = itens[i].Nome;
            return nomes;
        }

        private static string[] Nomes(IReadOnlyList<Face> itens)
        {
            var nomes = new string[itens.Count];
            for (int i = 0; i < nomes.Length; i++) nomes[i] = itens[i].Nome;
            return nomes;
        }
    }
}
