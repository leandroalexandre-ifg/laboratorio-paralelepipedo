using LaboratorioParalelepipedo.Nucleo.Interacao;
using NUnit.Framework;

namespace LaboratorioParalelepipedo.Testes
{
    public class SeguraPorTempoTests
    {
        private const double Quadro = 1.0 / 64;

        [Test]
        public void DisparaUmaVezAoCompletarOTempo_ESoRearmaAoSoltar()
        {
            var segura = new SeguraPorTempo(3);
            int disparos = 0, quadroDoDisparo = -1;
            for (int i = 1; i <= 64 * 5; i++)
                if (segura.Atualizar(true, Quadro)) { disparos++; quadroDoDisparo = i; }

            Assert.That(disparos, Is.EqualTo(1));
            Assert.That(quadroDoDisparo * Quadro, Is.EqualTo(3).Within(1e-9));

            segura.Atualizar(false, Quadro);
            for (int i = 0; i < 64 * 3; i++) if (segura.Atualizar(true, Quadro)) disparos++;
            Assert.That(disparos, Is.EqualTo(2));
        }

        [Test]
        public void SoltarAntesRecomecaAContagem()
        {
            var segura = new SeguraPorTempo(3);
            for (int i = 0; i < 64 * 2; i++) Assert.That(segura.Atualizar(true, Quadro), Is.False);
            segura.Atualizar(false, Quadro);
            for (int i = 0; i < 64 * 2; i++) Assert.That(segura.Atualizar(true, Quadro), Is.False);
        }
    }
}
