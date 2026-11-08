namespace LaboratorioParalelepipedo.Nucleo.Interacao
{
    /// <summary>O que o movimento do analógico pede neste quadro.</summary>
    public enum AcaoDoAnalogico
    {
        Nenhuma,
        /// <summary>Empurrou para a frente: mostrar o arco do teleporte.</summary>
        ComecarAMirar,
        /// <summary>Soltou o analógico mirando: teleportar (se o destino valer).</summary>
        Teleportar,
        /// <summary>Puxou para trás mirando: desistir do teleporte.</summary>
        CancelarMira,
        /// <summary>Um passo de giro para a direita.</summary>
        GirarParaADireita,
        /// <summary>Um passo de giro para a esquerda.</summary>
        GirarParaAEsquerda,
    }
}
