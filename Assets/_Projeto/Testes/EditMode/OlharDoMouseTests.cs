using LaboratorioParalelepipedo.XR;
using NUnit.Framework;

namespace LaboratorioParalelepipedo.Testes
{
    public class OlharDoMouseTests
    {
        [Test]
        public void MouseParaADireita_GiraParaADireita()
        {
            var olhar = new OlharDoMouse();

            olhar.Mover(100f, 0f, 0.15f);

            Assert.That(olhar.Giro, Is.EqualTo(15f).Within(1e-4));
            Assert.That(olhar.Inclinacao, Is.EqualTo(0f));
        }

        [Test]
        public void MouseParaCima_OlhaParaCima()
        {
            var olhar = new OlharDoMouse();

            olhar.Mover(0f, 100f, 0.15f);

            Assert.That(olhar.Inclinacao, Is.EqualTo(-15f).Within(1e-4), "no Unity, inclinação negativa olha para cima");
        }

        [Test]
        public void Inclinacao_ParaEm80Graus()
        {
            var olhar = new OlharDoMouse();

            olhar.Mover(0f, -10000f, 1f);
            Assert.That(olhar.Inclinacao, Is.EqualTo(OlharDoMouse.InclinacaoMaxima));
            olhar.Mover(0f, 10000f, 1f);
            Assert.That(olhar.Inclinacao, Is.EqualTo(-OlharDoMouse.InclinacaoMaxima));
        }

        [Test]
        public void Giro_DaAVoltaCompleta()
        {
            var olhar = new OlharDoMouse();

            olhar.Mover(-30f, 0f, 1f);
            Assert.That(olhar.Giro, Is.EqualTo(330f).Within(1e-4));
            olhar.Mover(400f, 0f, 1f);
            Assert.That(olhar.Giro, Is.EqualTo(10f).Within(1e-4));
        }
    }
}
