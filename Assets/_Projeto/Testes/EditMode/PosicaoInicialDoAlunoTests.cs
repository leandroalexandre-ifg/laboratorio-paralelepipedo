using System;
using LaboratorioParalelepipedo.Nucleo;
using LaboratorioParalelepipedo.Nucleo.Interacao;
using NUnit.Framework;

namespace LaboratorioParalelepipedo.Testes
{
    public class PosicaoInicialDoAlunoTests
    {
        private const double Tolerancia = 1e-9;

        private static Vetor3 Frente(double graus) =>
            new Vetor3(Math.Sin(graus * Math.PI / 180), 0, Math.Cos(graus * Math.PI / 180));

        // Gira um deslocamento do rig como o Unity faz com Quaternion.Euler(0, graus, 0).
        private static Vetor3 Girar(Vetor3 v, double graus)
        {
            double r = graus * Math.PI / 180, cos = Math.Cos(r), sen = Math.Sin(r);
            return new Vetor3(v.X * cos + v.Z * sen, v.Y, -v.X * sen + v.Z * cos);
        }

        [TestCase(0)]
        [TestCase(90)]
        [TestCase(180)]
        [TestCase(270)]
        [TestCase(30)]
        public void DirecaoDoOlhar_Horizontal(double graus)
        {
            double obtido = PosicaoInicialDoAluno.DirecaoDoOlhar(Frente(graus), new Vetor3(0, 1, 0));
            Assert.That(PosicaoInicialDoAluno.DiferencaDeAngulo(obtido, graus), Is.EqualTo(0).Within(1e-9));
        }

        [Test]
        public void DirecaoDoOlhar_IgnoraAInclinacao()
        {
            var frente = new Vetor3(0.5, -0.7, 0.5);   // olhando para baixo, entre +x e +z
            Assert.That(PosicaoInicialDoAluno.DirecaoDoOlhar(frente, new Vetor3(0, 1, 0)), Is.EqualTo(45).Within(1e-9));
        }

        [Test]
        public void DirecaoDoOlhar_OlhandoParaBaixo_UsaOParaCimaDaCabeca()
        {
            // Olhando reto para o chão com o rosto para +x: o "para cima" da cabeça aponta para +x.
            double obtido = PosicaoInicialDoAluno.DirecaoDoOlhar(new Vetor3(0, -1, 0), new Vetor3(1, 0, 0));
            Assert.That(obtido, Is.EqualTo(90).Within(1e-9));
        }

        [Test]
        public void DirecaoDoOlhar_OlhandoParaCima_UsaOContrarioDoParaCima()
        {
            // Olhando reto para o teto com o rosto para +x: o "para cima" da cabeça aponta para -x.
            double obtido = PosicaoInicialDoAluno.DirecaoDoOlhar(new Vetor3(0, 1, 0), new Vetor3(-1, 0, 0));
            Assert.That(obtido, Is.EqualTo(90).Within(1e-9));
        }

        [TestCase(10, 350, -20)]
        [TestCase(350, 10, 20)]
        [TestCase(0, 180, -180)]
        [TestCase(90, 0, -90)]
        public void DiferencaDeAngulo_PeloMenorCaminho(double de, double para, double esperado)
        {
            Assert.That(PosicaoInicialDoAluno.DiferencaDeAngulo(de, para), Is.EqualTo(esperado).Within(1e-9));
        }

        [TestCase(180)]   // mesa nas costas: o caso relatado no Quest 2
        [TestCase(90)]
        [TestCase(-135)]
        [TestCase(0)]
        public void Calcular_PoeACabecaNoPontoDePartida_OlhandoParaAMesa(double olhar)
        {
            // Rig girado e deslocado; a cabeça fora do centro do rig, a 1,6 m de altura.
            var rig = new PoseDoRig(new Vetor3(2, 0, -1), 30);
            var deslocamentoLocal = new Vetor3(0.2, 1.6, -0.3);
            Vetor3 cabeca = rig.Posicao + Girar(deslocamentoLocal, rig.Giro);
            double direcao = MovimentoDoRig.NormalizarAngulo(olhar);

            PoseDoRig novo = PosicaoInicialDoAluno.Calcular(rig, cabeca, direcao, 0, 0);

            // A cabeça anda junto com o rig: mesmo deslocamento local, girado pelo giro novo.
            Vetor3 novaCabeca = novo.Posicao + Girar(deslocamentoLocal, novo.Giro);
            Assert.That(novaCabeca.X, Is.EqualTo(0).Within(Tolerancia), "x");
            Assert.That(novaCabeca.Z, Is.EqualTo(0).Within(Tolerancia), "z");
            Assert.That(novaCabeca.Y, Is.EqualTo(cabeca.Y).Within(Tolerancia), "altura");
            Assert.That(novo.Posicao.Y, Is.EqualTo(rig.Posicao.Y), "o rig não sobe nem desce");

            double novoOlhar = direcao + (novo.Giro - rig.Giro);
            Assert.That(PosicaoInicialDoAluno.DiferencaDeAngulo(novoOlhar, 0), Is.EqualTo(0).Within(1e-9), "olhando para +z");
        }

        [Test]
        public void Calcular_JaNoLugar_NaoMudaNada()
        {
            var rig = new PoseDoRig(new Vetor3(0, 0, 0), 0);
            PoseDoRig novo = PosicaoInicialDoAluno.Calcular(rig, new Vetor3(0, 1.5, 0), 0, 0, 0);
            Assert.That(novo.Posicao.X, Is.EqualTo(0).Within(Tolerancia));
            Assert.That(novo.Posicao.Z, Is.EqualTo(0).Within(Tolerancia));
            Assert.That(novo.Giro, Is.EqualTo(0).Within(Tolerancia));
        }
    }
}
