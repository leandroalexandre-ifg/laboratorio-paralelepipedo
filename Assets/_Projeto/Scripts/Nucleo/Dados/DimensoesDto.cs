using System;

namespace LaboratorioParalelepipedo.Nucleo.Dados
{
    /// <summary>Dimensões da caixa em texto: a (comprimento), b (largura) e c (altura).</summary>
    [Serializable]
    public sealed class DimensoesDto
    {
        public string a;
        public string b;
        public string c;
    }
}
