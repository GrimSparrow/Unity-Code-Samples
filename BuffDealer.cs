using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Drawn2U.GameUI.GameSessionUI;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;

namespace Drawn2U.Agents.BuffSystem
{
    /// <summary>
    /// Утилита позволяет случайным образом получать баффы.
    /// </summary>
    public static class BuffDealer
    {
#if DEMO
        /// <summary>
        /// Ярлык, по которому ищутся ключи баффов для демо в Addressables.
        /// </summary>
        private const string BUFF_LABEL = "DemoBuff";
#else
        /// <summary>
        /// Ярлык, по которому ищутся ключи баффов в Addressables.
        /// </summary>
        private const string BUFF_LABEL = "Buff";
#endif
        /// <summary>
        /// Колоды с разными типами баффов.
        /// </summary>
        private static readonly Dictionary<BuffType, BuffDeck> DECKS = new Dictionary<BuffType, BuffDeck>
        {
            {BuffType.Curse, new BuffDeck()},
            {BuffType.Gift, new BuffDeck()},
            {BuffType.Epic, new BuffDeck()}
        };

        /// <summary>
        /// Ключ предмета "Лекарство для матери", используется для игнорирования его при выдаче награды за мотиваторы.
        /// </summary>
        private static string _medicineForMotherKey;

        /// <summary>
        /// Происходит замешивание колод.
        /// </summary>
        public static bool IsShuffling { get; private set; }

        /// <summary>
        /// Замешать баффы в колоды, чтобы потом можно было доставать случайный. 
        /// </summary>
        /// <param name="session">Номер сессии.</param>
        /// <param name="inventory">Инвентарь персонажа.</param>
        public static async void ShuffleBuffsAsync(int session, Inventory inventory)
        {
            if (IsShuffling) return;

            IsShuffling = true;
            DECKS.ToList().ForEach(deck => deck.Value.Clear());

            var locationHandle = GetLocationHandle();

            await locationHandle.Task;

            if (locationHandle.Status != AsyncOperationStatus.Succeeded) throw locationHandle.OperationException;

            var logMessage = new StringBuilder($"Buffs have been shuffled for session {session}:\n");

            foreach (var location in locationHandle.Result)
            {
                var buffHandle = Addressables.LoadAssetAsync<Buff>(location);

                await buffHandle.Task;

                if (buffHandle.Status != AsyncOperationStatus.Succeeded) throw buffHandle.OperationException;

                var key = location.PrimaryKey;
                var buff = buffHandle.Result;

                if (buff.sessionEffects == SessionEffects.MedicineForMother)
                {
                    _medicineForMotherKey = key;
                }

                // Если бафф уникален и он уже есть в инвентаре, то его не замешиваем в колоды.
                if (buff.isUnique && inventory.HasBuff(key)) continue;
                
                var probabilityIndex = session > buff.probabilities.Length 
                    ? buff.probabilities.Length - 1 
                    : session - 1;

                var probability = buff.probabilities[probabilityIndex];
                
                if (probability == 0) continue;

                DECKS[buff.type].PutBuff(key, buff.probabilities[probabilityIndex], buff.type == BuffType.Epic);
                
                logMessage.Append($"{buff.buffName} ({buff.type} | {probability})\n");
            }
            
            Debug.Log(logMessage);
            IsShuffling = false;
        }

        /// <summary>
        /// Получить все баффы, которые существуют.
        /// </summary>
        /// <param name="onComplete">Действие, которое будет выполнено по завершении операции.</param>
        public static IEnumerator GetAllBuffsAsync(Action<List<BuffRecord>> onComplete)
        {
            var locationHandle = GetLocationHandle();

            yield return locationHandle;

            if (locationHandle.Status != AsyncOperationStatus.Succeeded) throw locationHandle.OperationException;

            var result = new List<BuffRecord>();
            
            foreach (var location in locationHandle.Result)
            {
                var buffHandle = Addressables.LoadAssetAsync<Buff>(location);

                yield return buffHandle;

                if (buffHandle.Status != AsyncOperationStatus.Succeeded) throw buffHandle.OperationException;

                var key = location.PrimaryKey;
                var buff = buffHandle.Result;

                result.Add(new BuffRecord(key, buff));
            }
            
            onComplete?.Invoke(result);
        }
        
        /// <summary>
        /// Получить ключ случайного предмета, в первую очередь проверяется выпадение уникального предмета, если
        /// уникальный предмет не выпал проверятеся выпадение обычного предмета.
        /// </summary>
        /// <param name="ignoreMedicineForMother">Нужно ли игнорировать ключ предмета "Лекарство для матери". В текущий
        /// момент нужно для <see cref="MotivationPanel"/>.</param>
        public static string GetRandomItem(bool ignoreMedicineForMother = false)
        {
            if (IsShuffling) return string.Empty;
            
            if (DECKS[BuffType.Epic].TryGetRandomKey(out var key) 
                && (!ignoreMedicineForMother || key != _medicineForMotherKey)
                || DECKS[BuffType.Gift].TryGetRandomKey(out key))
            {
                return key;
            }

            return string.Empty;
        }
        
        /// <summary>
        /// Получить ключ случайного проклятия.
        /// </summary>
        public static string GetRandomCurse()
        {
            if (IsShuffling) return string.Empty;
            
            return DECKS[BuffType.Curse].TryGetRandomKey(out var key) ? key : string.Empty;
        }

        /// <summary>
        /// Получить пути к баффам в Addressables по ярлыку (Label), в зависимости от типа билда.
        /// </summary>
        private static AsyncOperationHandle<IList<IResourceLocation>> GetLocationHandle()
        {
            var locationHandle = Addressables.LoadResourceLocationsAsync(BUFF_LABEL);
            
            return locationHandle;
        }
    }
}
