using LaboratorioParalelepipedo.Nucleo.Interacao;
using UnityEngine;

namespace LaboratorioParalelepipedo.XR
{
    /// <summary>
    /// Atalho do professor: os dois analógicos afundados ao mesmo tempo por <see cref="Segundos"/> s mostram ou escondem
    /// o painel de diagnóstico. Difícil de acionar sem querer. Sem alocação por frame.
    /// </summary>
    public sealed class AtalhoDoDiagnostico : MonoBehaviour
    {
        public const double Segundos = 3;

        [SerializeField] private PainelDeDiagnostico _painel;

        private readonly SeguraPorTempo _segura = new SeguraPorTempo(Segundos);

        /// <summary>Liga o componente (usado pelo gerador da cena).</summary>
        public void Configurar(PainelDeDiagnostico painel) => _painel = painel;

        private void Update()
        {
            bool apertados = OVRInput.Get(OVRInput.Button.PrimaryThumbstick, OVRInput.Controller.LTouch) &&
                             OVRInput.Get(OVRInput.Button.PrimaryThumbstick, OVRInput.Controller.RTouch);
            if (_segura.Atualizar(apertados, Time.unscaledDeltaTime) && _painel != null) _painel.Alternar();
        }
    }
}
