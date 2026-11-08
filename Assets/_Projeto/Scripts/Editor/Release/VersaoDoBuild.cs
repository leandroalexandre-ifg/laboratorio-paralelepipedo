using System;

namespace LaboratorioParalelepipedo.Editor.Release
{
    /// <summary>
    /// versionCode automático por data e hora (UTC), no formato aaDDDHHmm (ano, dia do ano, hora, minuto),
    /// como no Missão Coração. Sempre cresce (a loja recusa versionCode igual ou menor), não precisa de arquivo
    /// para commitar e cabe no limite do Android (2.100.000.000) até 2099.
    /// </summary>
    public static class VersaoDoBuild
    {
        public const int MaiorVersionCode = 2100000000;

        public static int VersionCodePara(DateTime utc)
        {
            if (utc.Kind == DateTimeKind.Local) utc = utc.ToUniversalTime();
            return utc.Year % 100 * 10000000 + utc.DayOfYear * 10000 + utc.Hour * 100 + utc.Minute;
        }

        public static int Agora() => VersionCodePara(DateTime.UtcNow);
    }
}
