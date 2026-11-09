using UnityEngine;

namespace LaboratorioParalelepipedo.Apresentacao
{
    /// <summary>
    /// Mantém um rótulo (vértice, medida) virado para os olhos do aluno, mesmo com a caixa na mão. Usa o olho dado ou,
    /// sem ele, a câmera principal (CenterEyeAnchor), procurada uma vez. Sem alocação por frame.
    /// </summary>
    public sealed class RotuloVoltadoParaOAluno : MonoBehaviour
    {
        [SerializeField] private Transform _olho;

        private static Transform _olhoDaCena;

        /// <summary>Liga o olho (opcional; usado pelo gerador da cena).</summary>
        public void Configurar(Transform olho) => _olho = olho;

        private void LateUpdate()
        {
            Transform olho = _olho != null ? _olho : OlhoDaCena();
            if (olho != null) VirarPara(olho.position);
        }

        private static Transform OlhoDaCena()
        {
            if (_olhoDaCena == null && Camera.main != null) _olhoDaCena = Camera.main.transform;
            return _olhoDaCena;
        }

        /// <summary>Vira o rótulo para um ponto (público para a prévia e os testes no Editor).</summary>
        public void VirarPara(Vector3 olho)
        {
            Vector3 paraLonge = transform.position - olho;
            if (paraLonge.sqrMagnitude < 1e-8f) return;
            transform.rotation = Quaternion.LookRotation(paraLonge, Vector3.up);
        }
    }
}
