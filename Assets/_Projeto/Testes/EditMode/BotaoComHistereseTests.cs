using LaboratorioParalelepipedo.Nucleo.Interacao;
using NUnit.Framework;

namespace LaboratorioParalelepipedo.Testes
{
    public class BotaoComHistereseTests
    {
        [Test]
        public void ApertaESoltaUmaVezSo_ComHisterese()
        {
            var botao = new BotaoComHisterese();

            Assert.That(botao.Atualizar(0.4), Is.EqualTo(TransicaoDoBotao.Nenhuma));
            Assert.That(botao.Atualizar(0.6), Is.EqualTo(TransicaoDoBotao.Apertou));
            Assert.That(botao.Atualizar(0.45), Is.EqualTo(TransicaoDoBotao.Nenhuma), "entre os limiares continua apertado");
            Assert.That(botao.Atualizar(0.9), Is.EqualTo(TransicaoDoBotao.Nenhuma));
            Assert.That(botao.Atualizar(0.3), Is.EqualTo(TransicaoDoBotao.Soltou));
            Assert.That(botao.Atualizar(0.5), Is.EqualTo(TransicaoDoBotao.Nenhuma));
        }

        [Test]
        public void BotaoDigital_ENaN()
        {
            var botao = new BotaoComHisterese();

            Assert.That(botao.Atualizar(true), Is.EqualTo(TransicaoDoBotao.Apertou));
            Assert.That(botao.Atualizar(double.NaN), Is.EqualTo(TransicaoDoBotao.Soltou));
        }

        [Test]
        public void Reiniciar_SoltaSemAvisar()
        {
            var botao = new BotaoComHisterese();
            botao.Atualizar(1.0);

            botao.Reiniciar();

            Assert.That(botao.Apertado, Is.False);
            Assert.That(botao.Atualizar(1.0), Is.EqualTo(TransicaoDoBotao.Apertou));
        }
    }
}
