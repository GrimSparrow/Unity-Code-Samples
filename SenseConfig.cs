using System.Diagnostics.CodeAnalysis;
using UnityEngine;

namespace Drawn2U.Agents.CharacterAbilities
{
    /// <summary>
    /// Конфигурация чутья.
    /// </summary>
    [CreateAssetMenu(fileName = "SenseConfig", menuName = "Drawn2U/Config/Sense")]
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Local")]
    public class SenseConfig : ScriptableObject
    {
        /// <summary>
        /// Радиус чутья.
        /// </summary>
        [field: Header("Sense base parameters")]
        [field: SerializeField] public float SenseRadius { get; private set; } = 6f;
        
        /// <summary>
        /// Максимальное число крюков, которое может быть обнаружено за один раз.
        /// </summary>
        [field: SerializeField] public int MaxHooksCountToDetect { get; private set; } = 20;
        
        /// <summary>
        /// Задержка перед активацией крюков при использовании умения.
        /// </summary>
        [field: SerializeField] public int ActivationDelay { get; private set; } = 1;
        
        /// <summary>
        /// Целевое значение альфа канала блюр-шейдера для его активации. 
        /// </summary>
        [field: SerializeField] public float TargetAlpha { get; private set; } = 0.85f;
        
        /// <summary>
        /// Скорость появления и затухания эффекта размытия.
        /// </summary>
        [field: SerializeField] public float FadingRate { get; private set; } = 1f;
        
        /// <summary>
        /// Период времени, с которым происходит поиск крюков.
        /// </summary>
        [field: SerializeField] public float LookingForHooksPeriod { get; private set; } = 0.1f;
        
        /// <summary>
        /// Альфа внутреннего контура.
        /// </summary>
        [field: Space(20)]
        [field: Header("Material parameters")]
        [field: SerializeField] public float[] InnerOutlineAlpha { get; private set; }
        
        /// <summary>
        /// Свечение внутреннего контура.
        /// </summary>
        [field: SerializeField] public float[] InnerOutlineGlow { get; private set; }
        
        /// <summary>
        /// Непрозрачность оверлея.
        /// </summary>
        [field: SerializeField] public float[] OverlayOpacity { get; private set; }
        
        /// <summary>
        /// Сила наложения оверлея.
        /// </summary>
        [field: SerializeField] public float[] OverlayBlend { get; private set; }
        
        /// <summary>
        /// Контраст оверлея.
        /// </summary>
        [field: SerializeField] public float[] Contrast { get; private set; }
        
        /// <summary>
        /// Яркость оверлея.
        /// </summary>
        [field: SerializeField] public float[] Brightness { get; private set; }
    }
}
