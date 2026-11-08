using System;

namespace LaboratorioParalelepipedo.Nucleo.Interacao
{
    /// <summary>
    /// Põe o aluno no ponto de partida, de frente para a mesa. No Quest, a frente do espaço de rastreamento é a do
    /// Guardian, não a do aluno: sem isto, a mesa pode aparecer às costas. Quem se move é o rig (como no teleporte e no
    /// giro), nunca a câmera: gira em torno da cabeça até ela olhar para <c>giroInicial</c> e anda até o ponto de
    /// partida no chão. A altura não muda. Em metros e graus (0 = +z, 90 = +x), como no Unity.
    /// </summary>
    public static class PosicaoInicialDoAluno
    {
        // Olhando quase reto para cima ou para baixo, a frente no chão é incerta: usa o "para cima" da cabeça.
        private const double HorizontalMinima = 0.1;

        /// <summary>Direção horizontal do olhar, em graus em [0, 360), ignorando a inclinação da cabeça.</summary>
        public static double DirecaoDoOlhar(Vetor3 frente, Vetor3 cima)
        {
            double x = frente.X, z = frente.Z;
            if (Math.Sqrt(x * x + z * z) < HorizontalMinima)
            {
                // Olhando para baixo, o "para cima" aponta para a frente; olhando para cima, para trás.
                double sinal = frente.Y < 0 ? 1 : -1;
                x = cima.X * sinal;
                z = cima.Z * sinal;
            }
            return MovimentoDoRig.NormalizarAngulo(Math.Atan2(x, z) * 180 / Math.PI);
        }

        /// <summary>Diferença de <paramref name="de"/> para <paramref name="para"/>, em graus em [-180, 180).</summary>
        public static double DiferencaDeAngulo(double de, double para) =>
            MovimentoDoRig.NormalizarAngulo(para - de + 180) - 180;

        /// <summary>
        /// Pose nova do rig para a cabeça ficar em (<paramref name="xInicial"/>, <paramref name="zInicial"/>) no chão,
        /// olhando para <paramref name="giroInicial"/>. A cabeça mantém a altura e a inclinação.
        /// </summary>
        public static PoseDoRig Calcular(PoseDoRig rig, Vetor3 cabeca, double direcaoDoOlhar,
            double xInicial, double zInicial, double giroInicial = 0)
        {
            double giro = DiferencaDeAngulo(direcaoDoOlhar, giroInicial);
            Vetor3 girado = MovimentoDoRig.GirarEmTornoDaCabeca(rig.Posicao, cabeca, giro);
            Vetor3 posicao = MovimentoDoRig.Teleportar(girado, cabeca, new Vetor3(xInicial, 0, zInicial));
            return new PoseDoRig(posicao, MovimentoDoRig.NormalizarAngulo(rig.Giro + giro));
        }
    }
}
