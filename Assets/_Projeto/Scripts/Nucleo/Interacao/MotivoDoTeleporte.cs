namespace LaboratorioParalelepipedo.Nucleo.Interacao
{
    /// <summary>Por que um destino de teleporte não vale (<see cref="Nenhum"/>: vale).</summary>
    public enum MotivoDoTeleporte
    {
        Nenhum,
        /// <summary>A menos de <see cref="RegrasDoTeleporte.DistanciaMinimaDasParedes"/> de uma parede (ou fora da sala).</summary>
        PertoDaParede,
        /// <summary>Em cima da mesa (ou o arco bateu nela).</summary>
        NaMesa,
        /// <summary>Dentro da caixa (ou o arco bateu nela).</summary>
        NaCaixa,
        /// <summary>O arco não chegou ao chão.</summary>
        SemPouso,
    }
}
