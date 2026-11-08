using System;

namespace LaboratorioParalelepipedo.Nucleo.Dados
{
    /// <summary>Formato de textos.json: <c>{ "versao": 1, "textos": [ { "chave": "...", "texto": "..." } ] }</c>.</summary>
    [Serializable]
    public sealed class ArquivoDeTextosDto
    {
        public int versao;
        public TextoDto[] textos;
    }
}
