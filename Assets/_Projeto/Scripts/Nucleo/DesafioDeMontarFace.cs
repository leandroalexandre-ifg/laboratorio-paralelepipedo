using System;
using System.Collections.Generic;

namespace LaboratorioParalelepipedo.Nucleo
{
    /// <summary>
    /// Desafio em que o aluno monta uma face tocando os vértices em ordem (estação Elementos).
    /// A face pedida é escrita em qualquer ordem cíclica válida ("ABFE", "FEAB", "EFBA"…) ou
    /// "qualquer", quando vale montar qualquer uma das faces.
    /// </summary>
    public sealed class DesafioDeMontarFace : Desafio
    {
        /// <summary>Valor do campo "face" que aceita qualquer face da caixa.</summary>
        public const string QualquerFace = "qualquer";

        private readonly ValidadorDeFace _validador;

        public override TipoDeDesafio Tipo => TipoDeDesafio.MontarFace;

        /// <summary>Face pedida (nome na convenção, ex.: "ABFE"), ou null quando vale qualquer face.</summary>
        public Face FaceAlvo { get; }

        public bool AceitaQualquerFace => FaceAlvo == null;

        /// <exception cref="DesafioInvalidoException">Se algum dado do desafio for inválido ou a face não existir.</exception>
        public DesafioDeMontarFace(
            string id,
            Estacao estacao,
            string codigoBncc,
            string enunciado,
            Medida comprimento,
            Medida largura,
            Medida altura,
            string face)
            : base(id, estacao, codigoBncc, enunciado, comprimento, largura, altura)
        {
            _validador = new ValidadorDeFace(Caixa);

            if (string.IsNullOrWhiteSpace(face))
                throw new DesafioInvalidoException(id, $"falta a face (ex.: \"ABFE\" ou \"{QualquerFace}\").");

            string limpa = face.Trim();
            if (string.Equals(limpa, QualquerFace, StringComparison.OrdinalIgnoreCase))
                return;

            if (!_validador.TentarIdentificar(SepararVertices(limpa), out var alvo))
                throw new DesafioInvalidoException(id,
                    $"a face \"{limpa}\" não existe na caixa. Escreva os 4 vértices de uma face em ordem " +
                    $"(ex.: ABFE ou ADHE) ou \"{QualquerFace}\".");
            FaceAlvo = alvo;
        }

        /// <summary>Avalia os vértices tocados; certo se formam a face pedida (ou qualquer face, se for o caso).</summary>
        internal ResultadoDaMontagem Avaliar(IReadOnlyList<string> toques)
        {
            if (!_validador.TentarIdentificar(toques, out var montada))
                return new ResultadoDaMontagem(false, null);
            return new ResultadoDaMontagem(AceitaQualquerFace || ReferenceEquals(montada, FaceAlvo), montada);
        }

        private static string[] SepararVertices(string face)
        {
            var nomes = new List<string>(face.Length);
            foreach (char letra in face)
                if (!char.IsWhiteSpace(letra) && letra != '-')
                    nomes.Add(char.ToUpperInvariant(letra).ToString());
            return nomes.ToArray();
        }
    }
}
