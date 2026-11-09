namespace LaboratorioParalelepipedo.Nucleo.Estacoes
{
    /// <summary>Etapas da estação de diagonais, na ordem.</summary>
    public enum EtapaDasDiagonais
    {
        /// <summary>Painel de título com "Começar".</summary>
        Abertura,
        /// <summary>Passo 1: ligar E e G (diagonal d da base).</summary>
        DiagonalDaBase,
        /// <summary>Passo 2: ligar E e C (diagonal D da caixa).</summary>
        DiagonalDaCaixa,
        /// <summary>Pergunta com a resposta e os distratores; erro fica aqui, com uma dica.</summary>
        Pergunta,
        /// <summary>Acertou: destaque e frase curta, antes do fechamento.</summary>
        Acertou,
        /// <summary>"Estação concluída", com a fórmula geral e os botões "Próximo desafio" e "Refazer".</summary>
        Fechamento,
    }
}
