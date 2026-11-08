using LaboratorioParalelepipedo.Nucleo.Interacao;
using NUnit.Framework;

namespace LaboratorioParalelepipedo.Testes
{
    public class TransicaoSuaveTests
    {
        [Test]
        public void RetornoDaCaixaDuraCercaDe400ms()
        {
            Assert.That(TransicaoSuave.DuracaoDoRetornoDaCaixa, Is.EqualTo(0.4).Within(0.05));
        }

        [Test]
        public void ComecoEFimSuaves_TerminaEm1()
        {
            var transicao = new TransicaoSuave(0.4);
            transicao.Iniciar();

            double inicio = transicao.Avancar(0.04);
            double meio = transicao.Avancar(0.16);
            double fim = transicao.Avancar(0.3);

            Assert.That(inicio, Is.EqualTo(0.028).Within(1e-9), "começa devagar (smoothstep de 0,1)");
            Assert.That(meio, Is.EqualTo(0.5).Within(1e-9));
            Assert.That(fim, Is.EqualTo(1));
            Assert.That(transicao.Ativa, Is.False);
        }

        [Test]
        public void Parada_NaoMexe()
        {
            var transicao = new TransicaoSuave(0.4);
            transicao.Iniciar();
            transicao.Parar();

            Assert.That(transicao.Ativa, Is.False);
            Assert.That(transicao.Avancar(0.1), Is.EqualTo(1));
        }

        [Test]
        public void DuracaoInvalida_Recusada()
        {
            Assert.Throws<System.ArgumentOutOfRangeException>(() => new TransicaoSuave(0));
        }
    }
}
