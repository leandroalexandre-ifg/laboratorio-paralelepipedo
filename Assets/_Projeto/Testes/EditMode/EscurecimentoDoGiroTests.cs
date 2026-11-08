using LaboratorioParalelepipedo.Nucleo.Interacao;
using NUnit.Framework;

namespace LaboratorioParalelepipedo.Testes
{
    public class EscurecimentoDoGiroTests
    {
        [Test]
        public void DuraCercaDe100ms()
        {
            Assert.That(EscurecimentoDoGiro.Duracao, Is.EqualTo(0.1).Within(0.02));
        }

        [Test]
        public void EscureceGiraNoMeioEClareia()
        {
            var escurecimento = new EscurecimentoDoGiro();
            Assert.That(escurecimento.Iniciar());

            const double quarto = EscurecimentoDoGiro.Duracao / 4;
            double a = escurecimento.Avancar(quarto, out bool girou1);
            double b = escurecimento.Avancar(quarto, out bool girou2);
            double c = escurecimento.Avancar(quarto, out bool girou3);
            double d = escurecimento.Avancar(quarto * 1.2, out bool girou4);

            Assert.That(a, Is.EqualTo(0.5).Within(1e-9));
            Assert.That(b, Is.EqualTo(1.0).Within(1e-9), "escuro no meio");
            Assert.That(c, Is.EqualTo(0.5).Within(1e-9));
            Assert.That(d, Is.EqualTo(0));
            Assert.That(new[] { girou1, girou2, girou3, girou4 }, Is.EqualTo(new[] { false, true, false, false }), "gira uma vez, no escuro");
            Assert.That(escurecimento.Ativo, Is.False);
        }

        [Test]
        public void QuadroLongo_AindaGiraUmaVez()
        {
            var escurecimento = new EscurecimentoDoGiro();
            escurecimento.Iniciar();

            double opacidade = escurecimento.Avancar(0.5, out bool girou);

            Assert.That(girou);
            Assert.That(opacidade, Is.EqualTo(0));
        }

        [Test]
        public void ParadoNaoEscurece_ENaoRepeteDuranteOEscurecimento()
        {
            var escurecimento = new EscurecimentoDoGiro();

            Assert.That(escurecimento.Avancar(0.05, out bool girou), Is.EqualTo(0));
            Assert.That(girou, Is.False);

            escurecimento.Iniciar();
            Assert.That(escurecimento.Iniciar(), Is.False, "um giro de cada vez");
        }
    }
}
