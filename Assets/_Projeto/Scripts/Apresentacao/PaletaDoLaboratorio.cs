using UnityEngine;

namespace LaboratorioParalelepipedo.Apresentacao
{
    /// <summary>Cores do laboratório (paleta dos mockups, ver CLAUDE.md). Em espaço de cor sRGB, como no Inspector.</summary>
    public static class PaletaDoLaboratorio
    {
        /// <summary>Opacidade das faces translúcidas da caixa (~28%).</summary>
        public const float OpacidadeDasFaces = 0.28f;

        public static readonly Color Parede = Hex(0xDCE6EC);
        public static readonly Color Piso = Hex(0xB8C7D1);
        public static readonly Color TampoDaMesa = Hex(0x5E6E7C);
        public static readonly Color PeDaMesa = Hex(0x43515E);
        public static readonly Color Aresta = Hex(0x134B6E);
        public static readonly Color Face = Hex(0x9FD3F2, OpacidadeDasFaces);
        public static readonly Color DestaqueAzul = Hex(0x1E78C8);
        public static readonly Color DestaqueLaranja = Hex(0xC2410C);
        public static readonly Color BasesClaro = Hex(0xDCD3F0);
        public static readonly Color BasesEscuro = Hex(0x3B2A73);
        public static readonly Color FrenteEFundoClaro = Hex(0xCFE6F5);
        public static readonly Color FrenteEFundoEscuro = Hex(0x134B6E);
        public static readonly Color EsquerdaEDireitaClaro = Hex(0xF8DCC0);
        public static readonly Color EsquerdaEDireitaEscuro = Hex(0x7A3A0B);
        public static readonly Color CubinhoClaro = Hex(0xE9B949);
        public static readonly Color CubinhoEscuro = Hex(0xA9771F);
        public static readonly Color BotaoPrincipal = Hex(0xD9661F);
        public static readonly Color Teleporte = Hex(0x17807D);
        public static readonly Color Texto = Hex(0x1B2A3A);

        /// <summary>Fundo dos painéis de texto: branco, para o texto <see cref="Texto"/> ter bom contraste.</summary>
        public static readonly Color FundoDoPainel = Hex(0xFFFFFF);

        /// <summary>Cor a partir de um valor 0xRRGGBB.</summary>
        public static Color Hex(int rgb, float alfa = 1f) =>
            new Color(((rgb >> 16) & 0xFF) / 255f, ((rgb >> 8) & 0xFF) / 255f, (rgb & 0xFF) / 255f, alfa);
    }
}
