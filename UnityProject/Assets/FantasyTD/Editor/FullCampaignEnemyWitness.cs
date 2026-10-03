#if UNITY_EDITOR
using FantasyTD.MapA;
using UnityEngine;

namespace FantasyTD.CI
{
    // Editor-only read-only witness. No combat/economy/progression writes.
    public sealed class FullCampaignEnemyWitness : MonoBehaviour
    {
        private EnemyUnit _enemy;
        private GameSession _session;
        private bool _resolved;
        private int _priorEssence;
        private Economy _economy;

        internal void Initialize(EnemyUnit enemy,GameSession session)
        {
            _enemy=enemy;
            _session=session;
            _economy=session.Economy;
            _priorEssence=_economy.Essence;
            if(enemy.IsBoss) _economy.Changed+=ObserveEconomyEvent;
        }

        private void ObserveEconomyEvent(int essence)
        {
            if(!_resolved && _enemy!=null && _enemy.IsDead && _enemy.CurrentHp<=0f)
            {
                _resolved=true;
                FullCampaignPlayModeValidation.ObserveResolution(_enemy,_session,_priorEssence);
            }
            _priorEssence=essence;
        }

        private void LateUpdate()
        {
            if(_resolved || _enemy==null || _session==null) return;
            if(_enemy.IsDead)
            {
                _resolved=true;
                FullCampaignPlayModeValidation.ObserveResolution(_enemy,_session,_priorEssence);
            }
            else _priorEssence=_session.Economy.Essence;
        }

        private void OnDestroy()
        {
            if(_economy!=null) _economy.Changed-=ObserveEconomyEvent;
            if(_resolved || _enemy==null || _session==null) return;
            _resolved=true;
            FullCampaignPlayModeValidation.ObserveResolution(_enemy,_session,_priorEssence);
        }
    }
}
#endif
