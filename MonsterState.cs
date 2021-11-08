using Drawn2U.Logic.Automata;
using UnityEngine;

namespace Drawn2U.Agents.Enemies
{
    /// <summary>
    /// Базовый класс состояния ЧМ.
    /// </summary>
    public abstract class MonsterState : AutomatonState<MonsterAutomaton, MonsterController>
    {
        /// <summary>
        /// Симуляция поведения. Чем-то схоже с FixedUpdate()
        /// </summary>
        /// <param name="step">Период времени, для которого будет произведена симуляция. В большинстве случаев равно
        ///     <see cref="Time.fixedDeltaTime"/>. Может даже не в большинстве, а всегда.</param>
        public abstract void Simulate(float step);
    }
}

