using System;

namespace LaboratorioParalelepipedo.Nucleo.Dados
{
    /// <summary>Um texto de textos.json: chave (ex.: "estacao.volume.ressalva") e o texto mostrado ao aluno.</summary>
    [Serializable]
    public sealed class TextoDto
    {
        public string chave;
        public string texto;
    }
}
