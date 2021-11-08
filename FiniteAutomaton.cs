using System;
using System.Collections.Generic;
using System.Linq;

namespace Drawn2U.Logic.Automata
{
    /// <summary>
    /// Базовый класс для создания конечных автоматов которые оперируют состояниями, основанными на
    /// <see cref="AutomatonStateBase{TOwner}"/>.
    /// </summary>
    /// <typeparam name="TState">Класс, который является бызовам для состояний, с котороми работает данный автомат.</typeparam>
    /// <typeparam name="TOwner">Класс, который описывает владельца данного конечного автомата.</typeparam>
    public abstract class FiniteAutomaton<TState, TOwner> : FiniteAutomatonBase
        where TState : AutomatonStateBase<TOwner>
    {
        /// <summary>
        /// Владелец данного автомата.
        /// </summary>
        protected TOwner Owner;

        /// <summary>
        /// Словарь состояний автомата.
        /// </summary>
        private readonly Dictionary<int, TState> _states = new Dictionary<int, TState>();

        /// <summary>
        /// Текущее состояние автомата.
        /// </summary>
        public TState CurrentState { get; private set; }

        /// <summary>
        /// Изменилось состояние автомата. В параметре передаётся хэшкод типа состояния.
        /// </summary>
        public event Action<int> StateChanged; 

        /// <summary>
        /// Запустить конечный автомат с начальным состояние <see cref="T"/>.
        /// </summary>
        /// <param name="owner">Владелец данного конечного автомата.</param>
        public void Initiate<T>(TOwner owner) where T : TState, new()
        {
            Owner = owner;
            
            // Инициализируем состояния, которые были добавлены ещё до запуска автомата.
            var states = _states.Values.ToList();
            foreach (var state in states)
            {
                state.SetOwner(Owner);
                state.Initialize();
            }
            
            var key = typeof(T).GetHashCode();
            
            if (!_states.ContainsKey(key))
            {
                AddState<T>();
            }

            CurrentState = _states[key];

            if (this is IAutomatonInitializer initializer)
            {
                initializer.OnInitiated();
            }
            
            CurrentState.OnStateEntry();
            StateChanged?.Invoke(key);
        }

        /// <summary>
        /// Перейти к состоянию <see cref="T"/>.
        /// </summary>
        /// <typeparam name="T">Класс, который описывает состояние, к которому будет осуществлен переход.</typeparam>
        public void Transit<T>() where T : TState, new()
        {
            var destinationKey = typeof(T).GetHashCode();
            var currentKey = CurrentState.GetType().GetHashCode();

            if (destinationKey == currentKey) return;
            
            CurrentState.OnStateExit();
            
            if (!_states.ContainsKey(destinationKey))
            {
                AddState<T>();
            }

            CurrentState = _states[destinationKey];
            CurrentState.OnStateEntry();
            StateChanged?.Invoke(destinationKey);
        }

        /// <summary>
        /// Получить состояние <see cref="T"/>.
        /// </summary>
        /// <typeparam name="T">Класс, который описывает данное состояние.</typeparam>
        public T GetState<T>() where T : TState, new()
        {
            var key = typeof(T).GetHashCode();

            if (!_states.ContainsKey(key))
            {
                AddState<T>();
            }

            return (T)_states[key];
        }

        /// <summary>
        /// Является ли указанное состояние <see cref="T"/> текущим?
        /// </summary>
        public bool IsCurrentState<T>() where T : TState, new()
        {
            var desiredKey = typeof(T).GetHashCode();
            var currentKey = CurrentState.GetType().GetHashCode();

            return desiredKey == currentKey;
        }

        /// <summary>
        /// Завершить работу автомата.
        /// </summary>
        /// <remarks>Будет произведён выход из текущего состояния.</remarks>
        public void Quit()
        {
            CurrentState?.OnStateExit();
        }
        
        /// <summary>
        /// Добавить новое состояние в автомат.
        /// </summary>
        /// <typeparam name="T">Класс, который описывает состояние, которое будет добавлено.</typeparam>
        private void AddState<T>() where T : TState, new()
        {
            var key = typeof(T).GetHashCode();

            if (_states.ContainsKey(key)) return;

            var state = new T();

            _states.Add(key, state);
            state.SetAutomaton(this);

            // Если автомат ещё не запущен, то откладываем инициализацию состояния. Инициализация произойдёт при запуске автомата.
            if (Owner == null) return;
            
            state.SetOwner(Owner);
            state.Initialize();
        }
    }
}