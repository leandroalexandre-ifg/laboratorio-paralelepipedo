using System;

namespace LaboratorioParalelepipedo.Nucleo.Interacao
{
    /// <summary>
    /// Para onde vai o rig do óculos (o "chão" que leva a cabeça) no teleporte e no giro por passos. A câmera nunca
    /// é movida por script: só o rig, e só nesses dois casos. Em metros e graus, y para cima, giro positivo para a
    /// direita (visto de cima, sentido horário), como no Unity.
    /// </summary>
    public static class MovimentoDoRig
    {
        /// <summary>Ângulo de cada passo do giro, em graus (decisão do projeto: 45°, 8 passos por volta).</summary>
        public const double AnguloDoGiroEmGraus = 45;

        /// <summary>
        /// Posição nova do rig para a cabeça chegar ao destino (só na horizontal). O rig não gira: a pessoa
        /// chega olhando para a mesma direção.
        /// </summary>
        public static Vetor3 Teleportar(Vetor3 posicaoDoRig, Vetor3 posicaoDaCabeca, Vetor3 destino) =>
            new Vetor3(posicaoDoRig.X + destino.X - posicaoDaCabeca.X, posicaoDoRig.Y,
                posicaoDoRig.Z + destino.Z - posicaoDaCabeca.Z);

        /// <summary>
        /// Posição nova do rig ao girar <paramref name="graus"/> em torno do eixo vertical que passa pela cabeça:
        /// a cabeça fica no mesmo lugar e só a direção muda.
        /// </summary>
        public static Vetor3 GirarEmTornoDaCabeca(Vetor3 posicaoDoRig, Vetor3 posicaoDaCabeca, double graus)
        {
            double radianos = graus * Math.PI / 180;
            double cos = Math.Cos(radianos), sen = Math.Sin(radianos);
            double x = posicaoDoRig.X - posicaoDaCabeca.X;
            double z = posicaoDoRig.Z - posicaoDaCabeca.Z;
            return new Vetor3(posicaoDaCabeca.X + x * cos + z * sen, posicaoDoRig.Y,
                posicaoDaCabeca.Z - x * sen + z * cos);
        }

        /// <summary>Ângulo em [0, 360).</summary>
        public static double NormalizarAngulo(double graus)
        {
            double resto = graus % 360;
            return resto < 0 ? resto + 360 : resto;
        }
    }
}
