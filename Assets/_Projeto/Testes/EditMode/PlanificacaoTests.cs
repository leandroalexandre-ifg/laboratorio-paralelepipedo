using System;
using System.Linq;
using LaboratorioParalelepipedo.Nucleo;
using NUnit.Framework;

namespace LaboratorioParalelepipedo.Testes
{
    public class PlanificacaoTests
    {
        private static readonly Paralelepipedo Caixa = new Paralelepipedo(4, 3, 2);

        private static Vetor3[] Pontos(Planificacao planificacao, Face face, double fracao)
        {
            var pontos = new Vetor3[4];
            planificacao.PosicoesDaFace(face, fracao, pontos);
            return pontos;
        }

        private static void Iguais(Vetor3 p, Vetor3 q, string mensagem) =>
            Assert.That(Vetor3.Distancia(p, q), Is.LessThan(1e-9), $"{mensagem}: {p} ≠ {q}");

        [Test]
        public void Fechada_CadaFaceNoLugarDaCaixa()
        {
            var planificacao = new Planificacao(Caixa);
            foreach (Face face in Caixa.Faces)
            {
                Vetor3[] pontos = Pontos(planificacao, face, 0);
                for (int i = 0; i < 4; i++) Iguais(pontos[i], face.Vertices[i].Posicao, face.Nome);
            }
        }

        [TestCase(4, 3, 2)]
        [TestCase(2, 3, 6)]
        [TestCase(2, 2, 2)]
        public void Aberta_TudoPlanoNaMesa_SemFacesSobrepostas_DentroDoRetangulo(double a, double b, double c)
        {
            var caixa = new Paralelepipedo(a, b, c);
            var planificacao = new Planificacao(caixa);
            var retangulos = caixa.Faces.Select(face =>
            {
                Vetor3[] p = Pontos(planificacao, face, 1);
                Assert.That(p.All(v => Math.Abs(v.Y) < 1e-9), Is.True, $"{face.Nome} plana em y = 0");
                return (face.Nome, minX: p.Min(v => v.X), maxX: p.Max(v => v.X), minZ: p.Min(v => v.Z), maxZ: p.Max(v => v.Z));
            }).ToArray();

            double area = retangulos.Sum(r => (r.maxX - r.minX) * (r.maxZ - r.minZ));
            Assert.That(area, Is.EqualTo(caixa.AreaTotal).Within(1e-9), "as 6 faces abertas cobrem a área total");
            for (int i = 0; i < retangulos.Length; i++)
                for (int j = i + 1; j < retangulos.Length; j++)
                {
                    var r = retangulos[i];
                    var s = retangulos[j];
                    double sobreX = Math.Min(r.maxX, s.maxX) - Math.Max(r.minX, s.minX);
                    double sobreZ = Math.Min(r.maxZ, s.maxZ) - Math.Max(r.minZ, s.minZ);
                    Assert.That(sobreX <= 1e-9 || sobreZ <= 1e-9, Is.True, $"{r.Nome} e {s.Nome} não se sobrepõem");
                }

            Assert.That(retangulos.Min(r => r.minX), Is.EqualTo(-c).Within(1e-9));
            Assert.That(retangulos.Max(r => r.maxX), Is.EqualTo(a + c).Within(1e-9));
            Assert.That(retangulos.Min(r => r.minZ), Is.EqualTo(-c).Within(1e-9));
            Assert.That(retangulos.Max(r => r.maxZ), Is.EqualTo(2 * b + c).Within(1e-9));
            Assert.That(planificacao.LarguraAberta, Is.EqualTo(a + 2 * c).Within(1e-9));
            Assert.That(planificacao.ProfundidadeAberta, Is.EqualTo(2 * b + 2 * c).Within(1e-9));
            Iguais(planificacao.Centro(1), new Vetor3(a / 2, 0, b), "centro aberta");
            Iguais(planificacao.Centro(0), new Vetor3(a / 2, 0, b / 2), "centro fechada: o da base");
        }

        [TestCase(0.3)]
        [TestCase(0.5)]
        [TestCase(0.8)]
        public void NoMeioDaAbertura_AsFacesContinuamPresasPelasArestas_ESemDeformar(double fracao)
        {
            var planificacao = new Planificacao(Caixa);
            Vetor3 P(string face, string vertice) =>
                planificacao.Posicao(Caixa.ObterFace(face), Caixa.ObterVertice(vertice).Posicao, fracao);

            // Laterais presas à base; o topo, à face de trás pela aresta BC.
            foreach (var (face, vertice) in new[] { ("ADHE", "E"), ("ADHE", "H"), ("BCGF", "F"), ("BCGF", "G"),
                         ("ABFE", "E"), ("ABFE", "F"), ("DCGH", "H"), ("DCGH", "G") })
                Iguais(P(face, vertice), Caixa.ObterVertice(vertice).Posicao, $"{face} presa em {vertice}");
            Iguais(P("ABCD", "B"), P("BCGF", "B"), "topo preso em B");
            Iguais(P("ABCD", "C"), P("BCGF", "C"), "topo preso em C");

            foreach (Face face in Caixa.Faces)
            {
                Vetor3[] p = Pontos(planificacao, face, fracao);
                for (int i = 0; i < 4; i++)
                {
                    double antes = Vetor3.Distancia(face.Vertices[i].Posicao, face.Vertices[(i + 1) % 4].Posicao);
                    Assert.That(Vetor3.Distancia(p[i], p[(i + 1) % 4]), Is.EqualTo(antes).Within(1e-9),
                        $"{face.Nome}: as arestas não mudam de tamanho");
                }
            }
        }

        [Test]
        public void FracaoForaDe0a1_FicaNosLimites()
        {
            var planificacao = new Planificacao(Caixa);
            Face topo = Caixa.ObterFace("ABCD");
            Iguais(planificacao.Posicao(topo, Caixa.ObterVertice("A").Posicao, -1), Caixa.ObterVertice("A").Posicao, "fechada");
            Iguais(planificacao.Posicao(topo, Caixa.ObterVertice("A").Posicao, 2),
                planificacao.Posicao(topo, Caixa.ObterVertice("A").Posicao, 1), "aberta");
            Assert.Throws<ArgumentNullException>(() => new Planificacao(null));
            Assert.Throws<ArgumentException>(() => planificacao.PosicoesDaFace(topo, 1, new Vetor3[2]));
        }
    }
}
