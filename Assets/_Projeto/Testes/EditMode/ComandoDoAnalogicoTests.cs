using LaboratorioParalelepipedo.Nucleo.Interacao;
using NUnit.Framework;

namespace LaboratorioParalelepipedo.Testes
{
    public class ComandoDoAnalogicoTests
    {
        [Test]
        public void ParaAFrente_MiraEAoSoltarTeleporta()
        {
            var comando = new ComandoDoAnalogico();

            Assert.That(comando.Atualizar(0, 0.9), Is.EqualTo(AcaoDoAnalogico.ComecarAMirar));
            Assert.That(comando.Mirando);
            Assert.That(comando.Atualizar(0.2, 0.6), Is.EqualTo(AcaoDoAnalogico.Nenhuma), "mirando, ainda empurrado");
            Assert.That(comando.Atualizar(0, 0.1), Is.EqualTo(AcaoDoAnalogico.Teleportar));
            Assert.That(comando.Mirando, Is.False);
        }

        [Test]
        public void MirandoOLadoNaoGira()
        {
            var comando = new ComandoDoAnalogico();
            comando.Atualizar(0, 0.9);

            Assert.That(comando.Atualizar(0.9, 0.4), Is.EqualTo(AcaoDoAnalogico.Nenhuma));
        }

        [Test]
        public void ParaTrasMirando_Cancela_ESoVoltaDepoisDoCentro()
        {
            var comando = new ComandoDoAnalogico();
            comando.Atualizar(0, 0.9);

            Assert.That(comando.Atualizar(0, -0.9), Is.EqualTo(AcaoDoAnalogico.CancelarMira));
            Assert.That(comando.Atualizar(0, 0.0), Is.EqualTo(AcaoDoAnalogico.Nenhuma), "cancelar não teleporta ao soltar");
            Assert.That(comando.Atualizar(0, 0.9), Is.EqualTo(AcaoDoAnalogico.ComecarAMirar));
        }

        [Test]
        public void ParaOLado_UmPassoPorEmpurrao()
        {
            var comando = new ComandoDoAnalogico();

            Assert.That(comando.Atualizar(0.9, 0), Is.EqualTo(AcaoDoAnalogico.GirarParaADireita));
            Assert.That(comando.Atualizar(1.0, 0), Is.EqualTo(AcaoDoAnalogico.Nenhuma), "segurado não repete");
            Assert.That(comando.Atualizar(0.5, 0), Is.EqualTo(AcaoDoAnalogico.Nenhuma), "ainda fora do centro");
            Assert.That(comando.Atualizar(0.1, 0), Is.EqualTo(AcaoDoAnalogico.Nenhuma));
            Assert.That(comando.Atualizar(-0.9, 0), Is.EqualTo(AcaoDoAnalogico.GirarParaAEsquerda));
        }

        [TestCase(0.5, 0.0)]
        [TestCase(0.0, 0.6)]
        [TestCase(0.0, -0.9)]
        [TestCase(0.6, 0.6)]
        public void EmpurraoFracoOuParaTras_NaoFazNada(double x, double y)
        {
            Assert.That(new ComandoDoAnalogico().Atualizar(x, y), Is.EqualTo(AcaoDoAnalogico.Nenhuma));
        }

        [Test]
        public void Reiniciar_EsqueceAMira()
        {
            var comando = new ComandoDoAnalogico();
            comando.Atualizar(0, 0.9);

            comando.Reiniciar();

            Assert.That(comando.Mirando, Is.False);
            Assert.That(comando.Atualizar(0, 0), Is.EqualTo(AcaoDoAnalogico.Nenhuma), "não teleporta depois de reiniciar");
        }
    }
}
