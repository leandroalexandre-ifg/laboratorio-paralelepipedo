using LaboratorioParalelepipedo.Nucleo;
using NUnit.Framework;

namespace LaboratorioParalelepipedo.Testes
{
    public class Vetor3Tests
    {
        [Test]
        public void Distancia_EntrePontos_UsaPitagorasEmTresDimensoes()
        {
            var p = new Vetor3(1, 2, 3);
            var q = new Vetor3(3, 5, 9);

            Assert.That(Vetor3.Distancia(p, q), Is.EqualTo(7).Within(1e-12));
        }

        [Test]
        public void Subtracao_DaOVetorEntreOsPontos()
        {
            Assert.That(new Vetor3(4, 3, 2) - new Vetor3(1, 1, 1), Is.EqualTo(new Vetor3(3, 2, 1)));
        }

        [Test]
        public void Aresta_TemNomeEComprimento()
        {
            var e = new Vertice("E", new Vetor3(0, 0, 0));
            var h = new Vertice("H", new Vetor3(4, 0, 0));

            var aresta = new Aresta(e, h);

            Assert.That(aresta.Nome, Is.EqualTo("EH"));
            Assert.That(aresta.Comprimento, Is.EqualTo(4).Within(1e-12));
            Assert.That(aresta.Liga(h, e), Is.True);
        }
    }
}
