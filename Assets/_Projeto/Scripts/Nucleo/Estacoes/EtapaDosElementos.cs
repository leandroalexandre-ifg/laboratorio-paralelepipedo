namespace LaboratorioParalelepipedo.Nucleo.Estacoes
{
    /// <summary>Etapas da estação de elementos, na ordem.</summary>
    public enum EtapaDosElementos
    {
        /// <summary>Painel de título com "Começar".</summary>
        Abertura,
        /// <summary>Desafio de montar face: tocar os 4 vértices da face, em ordem.</summary>
        MontarFace,
        /// <summary>Pergunta de contagem (vértices, arestas ou faces); erro fica aqui, com uma dica.</summary>
        Pergunta,
        /// <summary>Acertou: destaque e frase curta, antes do próximo desafio (ou do fechamento, no último).</summary>
        Acertou,
        /// <summary>"Estação concluída", com o resumo e os botões "Ir para a Estação 2" e "Refazer".</summary>
        Fechamento,
    }
}
