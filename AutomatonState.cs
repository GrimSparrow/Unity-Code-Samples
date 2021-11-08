using System;

namespace Drawn2U.Logic.Automata
{
    /// <summary>
    /// Базовый класс для состояний конечного автомата <see cref="FiniteAutomatonBase"/>.
    /// </summary>
    /// <typeparam name="TAutomaton">Класс автомата, к которому относится данное состояние.</typeparam>
    /// <typeparam name="TOwner">Класс, который описывает владельца конечного автомата для данного состояния.</typeparam>
    public abstract class AutomatonState<TAutomaton, TOwner> : AutomatonStateBase<TOwner>
        where TAutomaton : FiniteAutomatonBase
    {
        /// <summary>
        /// Конечный автомат, к которому относится состояние.
        /// </summary>
        protected TAutomaton Automaton;

        /// <summary>
        /// Установить конечный автомат <see cref="value"/>, которому принадлежит данное состояние.
        /// </summary>
        public sealed override void SetAutomaton(FiniteAutomatonBase value)
        {
            if (value is TAutomaton automaton)
            {
                Automaton = automaton;
            }
            else
            {
                throw new ArgumentException();
            }
        }
    }
}