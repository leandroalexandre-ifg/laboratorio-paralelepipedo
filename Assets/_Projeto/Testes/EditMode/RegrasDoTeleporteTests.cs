using LaboratorioParalelepipedo.Nucleo;
using LaboratorioParalelepipedo.Nucleo.Interacao;
using NUnit.Framework;

namespace LaboratorioParalelepipedo.Testes
{
    public class RegrasDoTeleporteTests
    {
        // Sala de 8 m (paredes de 10 cm: chão por dentro de 7,9 m), mesa de 1,2 × 0,8 m com tampo a 0,76 m e
        // centro em z = 1, caixa de 40 × 30 × 20 cm em cima dela. Como na cena.
        private static readonly AreaRetangular Sala = new AreaRetangular(0, 0, 7.9, 7.9);
        private static readonly ObstaculoDoTeleporte Mesa =
            new ObstaculoDoTeleporte(new AreaRetangular(0, 1, 1.2, 0.8), 0, 0.76, MotivoDoTeleporte.NaMesa);
        private static readonly ObstaculoDoTeleporte CaixaNaMesa =
            new ObstaculoDoTeleporte(new AreaRetangular(0, 1, 0.4, 0.3), 0.76, 0.96, MotivoDoTeleporte.NaCaixa);

        private static RegrasDoTeleporte Regras(ObstaculoDoTeleporte caixa)
        {
            var regras = new RegrasDoTeleporte(Sala, 2);
            regras.DefinirObstaculo(0, Mesa);
            regras.DefinirObstaculo(1, caixa);
            return regras;
        }

        [Test]
        public void DistanciaMinimaDasParedesEh30Centimetros()
        {
            Assert.That(RegrasDoTeleporte.DistanciaMinimaDasParedes, Is.EqualTo(0.3));
        }

        [TestCase(0, 0)]
        [TestCase(2, -1)]
        [TestCase(3.64, 3.64)]
        [TestCase(0, 0.55)]   // logo à frente da mesa
        public void ChaoLivre_Vale(double x, double z)
        {
            Assert.That(Regras(CaixaNaMesa).Avaliar(x, z), Is.EqualTo(MotivoDoTeleporte.Nenhum));
        }

        [TestCase(3.66, 0)]
        [TestCase(0, -3.7)]
        [TestCase(-3.9, 2)]
        [TestCase(5, 5)]
        public void AMenosDe30CentimetrosDaParede_NaoVale(double x, double z)
        {
            Assert.That(Regras(CaixaNaMesa).Avaliar(x, z), Is.EqualTo(MotivoDoTeleporte.PertoDaParede));
        }

        [TestCase(0.5, 0.7)]
        [TestCase(-0.59, 1.39)]
        [TestCase(0.3, 1.0)]
        public void EmCimaDaMesa_NaoVale(double x, double z)
        {
            Assert.That(Regras(CaixaNaMesa).Avaliar(x, z), Is.EqualTo(MotivoDoTeleporte.NaMesa));
        }

        [Test]
        public void DentroDaCaixa_NaoVale_MesmoForaDaMesa()
        {
            // O aluno segurando a caixa no meio da sala: o chão embaixo dela não vale.
            var caixaNaMao = new ObstaculoDoTeleporte(new AreaRetangular(-2, -1, 0.4, 0.3), 0.8, 1.0, MotivoDoTeleporte.NaCaixa);
            var regras = new RegrasDoTeleporte(Sala, 2);
            regras.DefinirObstaculo(0, caixaNaMao);
            regras.DefinirObstaculo(1, Mesa);

            Assert.That(regras.Avaliar(-2.1, -0.9), Is.EqualTo(MotivoDoTeleporte.NaCaixa));
            Assert.That(regras.Avaliar(-2.5, -1), Is.EqualTo(MotivoDoTeleporte.Nenhum));
        }

        [Test]
        public void VagaVazia_NaoBloqueia()
        {
            var regras = new RegrasDoTeleporte(Sala, 3);
            regras.DefinirObstaculo(0, Mesa);

            Assert.That(regras.Avaliar(0, 0), Is.EqualTo(MotivoDoTeleporte.Nenhum));
        }

        [Test]
        public void Arco_ChegaAoChao_ComOUltimoPontoNoPouso()
        {
            var pontos = new Vetor3[120];
            // Controle a 1 m de altura apontando na horizontal para trás (−z), longe da mesa: cai a uns 2,7 m.
            var resultado = Regras(CaixaNaMesa).TracarArco(new Vetor3(0, 1, 0), new Vetor3(0, 0, -1), pontos);

            Assert.That(resultado.Valido, $"motivo: {resultado.Motivo}");
            Assert.That(resultado.Pouso.Y, Is.EqualTo(0).Within(1e-9));
            Assert.That(resultado.Pouso.Z, Is.LessThan(-1.5).And.GreaterThan(-3.65));
            Assert.That(pontos[0], Is.EqualTo(new Vetor3(0, 1, 0)));
            Assert.That(pontos[resultado.QuantidadeDePontos - 1], Is.EqualTo(resultado.Pouso));
        }

        [Test]
        public void Arco_BateNaMesa()
        {
            // Da frente da mesa, apontando para ela e um pouco para baixo.
            var resultado = Regras(CaixaNaMesa).TracarArco(new Vetor3(0.45, 1.1, 0.1), new Vetor3(0, -0.3, 1), new Vetor3[120]);

            Assert.That(resultado.Valido, Is.False);
            Assert.That(resultado.Motivo, Is.EqualTo(MotivoDoTeleporte.NaMesa));
        }

        [Test]
        public void Arco_BateNaCaixa()
        {
            var resultado = Regras(CaixaNaMesa).TracarArco(new Vetor3(0, 1.0, 0.4), new Vetor3(0, -0.2, 1), new Vetor3[120]);

            Assert.That(resultado.Motivo, Is.EqualTo(MotivoDoTeleporte.NaCaixa));
        }

        [Test]
        public void Arco_ParaAParedeLonge_NaoVale()
        {
            // Bem perto da parede direita, apontando para ela.
            var resultado = Regras(CaixaNaMesa).TracarArco(new Vetor3(3.2, 1, 0), new Vetor3(1, 0.6, 0), new Vetor3[120]);

            Assert.That(resultado.Motivo, Is.EqualTo(MotivoDoTeleporte.PertoDaParede));
        }

        [Test]
        public void Arco_ParaOCeu_SemPouso()
        {
            var resultado = Regras(CaixaNaMesa).TracarArco(new Vetor3(0, 1, 0), new Vetor3(0, 1, 0), new Vetor3[10]);

            Assert.That(resultado.Motivo, Is.EqualTo(MotivoDoTeleporte.SemPouso));
            Assert.That(resultado.QuantidadeDePontos, Is.EqualTo(10));
        }

        [Test]
        public void SalaPequenaDemais_Recusada()
        {
            Assert.Throws<System.ArgumentException>(() => new RegrasDoTeleporte(new AreaRetangular(0, 0, 0.5, 5), 0));
        }
    }
}
