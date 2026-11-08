using System;
using LaboratorioParalelepipedo.Nucleo;
using NUnit.Framework;

namespace LaboratorioParalelepipedo.Testes
{
    public class ContagemDeCubinhosTests
    {
        private const double Eps = 1e-9;

        [Test]
        public void Caixa4x3x3_TemTresCamadasDe12_Total36_Exata()
        {
            var contagem = new ContagemDeCubinhos(new Paralelepipedo(4, 3, 3));

            Assert.That(contagem.Camadas, Is.EqualTo(3));
            Assert.That(contagem.PorCamada, Is.EqualTo(12));
            Assert.That(contagem.Total, Is.EqualTo(36));
            Assert.That(contagem.EhExata, Is.True);
            Assert.That(contagem.PrecisaDaRessalva, Is.False);
            Assert.That(contagem.VolumeNaoPreenchido, Is.EqualTo(0));
        }

        [Test]
        public void ContagemExata_CoincideComOVolume()
        {
            var caixa = new Paralelepipedo(5, 2, 7);
            var contagem = new ContagemDeCubinhos(caixa);

            Assert.That(contagem.Total, Is.EqualTo(caixa.Volume).Within(Eps));
        }

        [Test]
        public void Caixa2v5x1v2xRaiz2_NaoEhExataEPedeARessalva()
        {
            var caixa = new Paralelepipedo(2.5, 1.2, Math.Sqrt(2));
            var contagem = new ContagemDeCubinhos(caixa);

            Assert.That(contagem.NoComprimento, Is.EqualTo(2));
            Assert.That(contagem.NaLargura, Is.EqualTo(1));
            Assert.That(contagem.Camadas, Is.EqualTo(1));
            Assert.That(contagem.Total, Is.EqualTo(2));
            Assert.That(contagem.EhExata, Is.False);
            Assert.That(contagem.PrecisaDaRessalva, Is.True);
            Assert.That(contagem.VolumeNaoPreenchido, Is.EqualTo(3 * Math.Sqrt(2) - 2).Within(Eps));
        }

        [TestCase(4, 3, 2.5)]
        [TestCase(4.0001, 3, 3)]
        [TestCase(1, 1, 0.5)]
        public void BastaUmaDimensaoNaoInteira_ParaNaoSerExata(double a, double b, double c)
        {
            Assert.That(new ContagemDeCubinhos(new Paralelepipedo(a, b, c)).EhExata, Is.False);
        }

        [Test]
        public void DimensaoMenorQue1_NaoCabeNenhumCubinho()
        {
            var contagem = new ContagemDeCubinhos(new Paralelepipedo(4, 3, 0.5));

            Assert.That(contagem.Camadas, Is.EqualTo(0));
            Assert.That(contagem.Total, Is.EqualTo(0));
            Assert.That(contagem.PrecisaDaRessalva, Is.True);
        }

        [Test]
        public void ErroDeArredondamento_NaoTiraUmaCamada()
        {
            // 0,1 · 30 e 0,7 · 10 erram na última casa decimal; continuam valendo 3 e 7 cubinhos exatos.
            var contagem = new ContagemDeCubinhos(new Paralelepipedo(0.1 * 30, 0.7 * 10, 2.9999999999999996));

            Assert.That(contagem.NoComprimento, Is.EqualTo(3));
            Assert.That(contagem.NaLargura, Is.EqualTo(7));
            Assert.That(contagem.Camadas, Is.EqualTo(3));
            Assert.That(contagem.EhExata, Is.True);
        }

        [Test]
        public void CaixaDe10x10x10_AtingeOLimiteSemPassar()
        {
            var contagem = new ContagemDeCubinhos(new Paralelepipedo(10, 10, 10));

            Assert.That(contagem.Total, Is.EqualTo(ContagemDeCubinhos.LimiteTecnico));
            Assert.That(contagem.ExcedeLimite, Is.False);
        }

        [Test]
        public void CaixaGrande_ExcedeOLimite()
        {
            Assert.That(new ContagemDeCubinhos(new Paralelepipedo(11, 10, 10)).ExcedeLimite, Is.True);
        }

        [Test]
        public void CaixaNula_LancaErro()
        {
            Assert.Throws<ArgumentNullException>(() => new ContagemDeCubinhos(null));
        }
    }
}
