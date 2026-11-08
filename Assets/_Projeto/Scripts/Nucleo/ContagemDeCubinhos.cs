using System;

namespace LaboratorioParalelepipedo.Nucleo
{
    /// <summary>
    /// Quantos cubinhos de aresta 1 cabem inteiros na caixa, camada por camada.
    /// A contagem é só um exemplo visual: ela coincide com o volume apenas quando a, b e c são naturais
    /// (<see cref="EhExata"/>). Para medidas reais quaisquer, a fórmula V = a·b·c continua valendo, mas os
    /// cubinhos não preenchem a caixa — e o app deve dizer isso ao aluno (<see cref="PrecisaDaRessalva"/>).
    /// </summary>
    public sealed class ContagemDeCubinhos
    {
        /// <summary>
        /// Máximo de cubinhos que a cena desenha numa caixa (GPU instancing desenha até 1023 por lote).
        /// </summary>
        public const int LimiteTecnico = 1000;

        public Paralelepipedo Caixa { get; }

        /// <summary>Cubinhos inteiros ao longo do comprimento: ⌊a⌋.</summary>
        public long NoComprimento { get; }

        /// <summary>Cubinhos inteiros ao longo da largura: ⌊b⌋.</summary>
        public long NaLargura { get; }

        /// <summary>Camadas completas de cubinhos: ⌊c⌋.</summary>
        public long Camadas { get; }

        /// <summary>Cubinhos em cada camada: ⌊a⌋·⌊b⌋.</summary>
        public long PorCamada => NoComprimento * NaLargura;

        /// <summary>Total de cubinhos inteiros que cabem na caixa.</summary>
        public long Total => PorCamada * Camadas;

        /// <summary>
        /// Verdadeiro só quando a, b e c são naturais: aí os cubinhos preenchem a caixa e Total = Volume.
        /// </summary>
        public bool EhExata { get; }

        /// <summary>Verdadeiro quando a caixa tem alguma dimensão não inteira e o app deve mostrar a ressalva.</summary>
        public bool PrecisaDaRessalva => !EhExata;

        /// <summary>Parte do volume que os cubinhos inteiros não preenchem: V − Total (zero quando é exata).</summary>
        public double VolumeNaoPreenchido => EhExata ? 0 : Caixa.Volume - Total;

        /// <summary>Verdadeiro se a caixa precisaria de mais cubinhos do que a cena desenha.</summary>
        public bool ExcedeLimite => Total > LimiteTecnico;

        public ContagemDeCubinhos(Paralelepipedo caixa)
        {
            Caixa = caixa ?? throw new ArgumentNullException(nameof(caixa));
            NoComprimento = Numeros.ParteInteira(caixa.A);
            NaLargura = Numeros.ParteInteira(caixa.B);
            Camadas = Numeros.ParteInteira(caixa.C);
            EhExata = Numeros.EhNaturalPositivo(caixa.A) &&
                      Numeros.EhNaturalPositivo(caixa.B) &&
                      Numeros.EhNaturalPositivo(caixa.C);
        }
    }
}
