namespace LaboratorioParalelepipedo.Nucleo
{
    /// <summary>Tipo do desafio; no JSON, "resposta" (padrão) ou "montarFace".</summary>
    public enum TipoDeDesafio
    {
        /// <summary>O aluno escolhe ou informa um valor (<see cref="DesafioDeResposta"/>).</summary>
        Resposta,

        /// <summary>O aluno toca os vértices de uma face em ordem (<see cref="DesafioDeMontarFace"/>).</summary>
        MontarFace,
    }
}
