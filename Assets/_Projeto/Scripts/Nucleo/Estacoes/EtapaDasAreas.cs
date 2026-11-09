namespace LaboratorioParalelepipedo.Nucleo.Estacoes
{
    /// <summary>Etapas da estação de áreas, na ordem.</summary>
    public enum EtapaDasAreas
    {
        /// <summary>Antes de chegar (a Estação 2 leva até aqui).</summary>
        Abertura,
        /// <summary>Caixa fechada na mesa, esperando "Abrir a caixa".</summary>
        CaixaFechada,
        /// <summary>Caixa aberta: tocar as faces para achar os 3 pares de faces paralelas.</summary>
        Pares,
        /// <summary>Pergunta (área da base, lateral ou total); erro fica aqui, com uma dica.</summary>
        Pergunta,
        /// <summary>Acertou: a conta com os números, antes da próxima pergunta (ou do fechamento, na última).</summary>
        Acertou,
        /// <summary>"Estação concluída", com as três fórmulas e "Refazer".</summary>
        Fechamento,
    }
}
