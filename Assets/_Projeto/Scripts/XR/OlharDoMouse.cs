using System;

namespace LaboratorioParalelepipedo.XR
{
    /// <summary>
    /// Direção do olhar no modo Editor: o mouse gira para os lados (sem limite) e para cima e para baixo
    /// (até <see cref="InclinacaoMaxima"/>, para não virar de cabeça para baixo). Ângulos em graus.
    /// </summary>
    public struct OlharDoMouse
    {
        public const float InclinacaoMaxima = 80f;

        /// <summary>Giro para os lados (em torno do eixo vertical), entre 0 e 360.</summary>
        public float Giro { get; private set; }

        /// <summary>Inclinação para cima e para baixo; positivo olha para baixo, como no Unity.</summary>
        public float Inclinacao { get; private set; }

        /// <summary>Aplica o movimento do mouse (em pixels) com a sensibilidade em graus por pixel.</summary>
        public void Mover(float deltaX, float deltaY, float sensibilidade)
        {
            Giro = ((Giro + deltaX * sensibilidade) % 360f + 360f) % 360f;
            Inclinacao = Math.Max(-InclinacaoMaxima, Math.Min(InclinacaoMaxima, Inclinacao - deltaY * sensibilidade));
        }
    }
}
