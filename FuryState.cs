namespace Drawn2U.Agents.Enemies
{
  /// <summary>
  /// Состояние ярости монстра, при котором он не может достичь персонажа. Если персонаж остаётся недостижим, то по
  /// истечение определенного времени монстр исчезает.
  /// </summary>
  public class FuryState : MonsterState
  {
      /// <summary>
      /// Таймер нахождения монстра в данном состоянии.
      /// </summary>
      private float _timer;
      
      /// <summary>
      /// Произведен переход к данному состоянию.
      /// </summary>
      public override void OnStateEntry()
      {
          Owner.Animator.SetTrigger(MonsterAnimationParams.Fury);
          Owner.MonsterAI.SetFlag(MonsterFlags.CanFlip, false);
          _timer = 0;
      }

      /// <summary>
      /// Произведен выход из данного состояния.
      /// </summary>
      public override void OnStateExit()
      {
          Owner.MonsterAI.SetFlag(MonsterFlags.CanFlip, true);
      }

      /// <summary>
      /// Вычисляем время ожидания персонажа монстром. Если персонаж остаётся недостижим, то по истечении времени
      /// монстр исчезает.
      /// </summary>
      public override void Simulate(float step)
      {
          if (Owner.MonsterAI.Flags.HasFlag(MonsterFlags.CanStartAttack))
          {
              Automaton.Transit<CombatState>();
              
              return;
          }

          if (Owner.MonsterAI.Flags.HasFlag(MonsterFlags.CanMove))
          {
              Automaton.Transit<MoveState>();
              
              return;
          }
          
          _timer += step;
  
          if (_timer > Owner.MonsterAI.Config.CharacterWaitingTime)
          {
              Automaton.Transit<DisappearState>();
          }
      }
  }
}
