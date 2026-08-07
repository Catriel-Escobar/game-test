using System;
using System.Collections;
using UnityEngine;

public class SkillCaster : MonoBehaviour
{
    private Player _player;
    private PlayerSkills _playerSkills;
    private readonly SkillCooldownManager _cooldowns = new SkillCooldownManager();

    public bool IsCasting { get; private set; }
    public SkillDefinition CurrentSkill { get; private set; }
    public SkillCooldownManager Cooldowns => _cooldowns;

    public event Action<SkillDefinition> OnCastStarted;
    public event Action<SkillDefinition> OnCastCompleted;

    private Coroutine _castRoutine;

    public void Initialize(Player player)
    {
        _player = player;
        _playerSkills = player.Skills;
    }

    public bool TryCastSkill(string skillId)
    {
        if (IsCasting)
        {
            Debug.Log($"[Skills] Ya casteando ({CurrentSkill?.id})");
            return false;
        }

        SkillDefinition skill = _playerSkills.GetSkill(skillId);
        if (skill == null)
        {
            Debug.LogWarning($"[Skills] No existe skill: {skillId}");
            return false;
        }

        if (!_playerSkills.IsUnlocked(skill.id))
        {
            Debug.Log($"[Skills] Skill no desbloqueada: {skill.id} (requiere nivel {skill.requiresLevel})");
            return false;
        }

        if (!_cooldowns.IsReady(skill.id, skill.cooldown))
        {
            Debug.Log($"[Skills] Skill en cooldown: {skill.id} ({_cooldowns.GetRemaining(skill.id):F1}s restantes)");
            return false;
        }

        if (_player.Resources.CurrentMana < skill.manaCost)
        {
            Debug.Log($"[Skills] Mana insuficiente para {skill.id} (necesita {skill.manaCost}, tiene {_player.Resources.CurrentMana})");
            return false;
        }

        _player.Resources.ConsumeMana(skill.manaCost);
        StartCast(skill);
        return true;
    }

    private void StartCast(SkillDefinition skill)
    {
        CurrentSkill = skill;
        IsCasting = true;

        _player.Animation.PlaySkillCast(skill.animationId);
        Debug.Log($"[Skills] Casteando {skill.id}... (cast time {skill.castTime}s, mana -{skill.manaCost})");

        OnCastStarted?.Invoke(skill);

        if (_castRoutine != null)
            StopCoroutine(_castRoutine);

        _castRoutine = StartCoroutine(CastRoutine(skill));
    }

    private IEnumerator CastRoutine(SkillDefinition skill)
    {
        yield return new WaitForSeconds(Mathf.Max(0.01f, skill.castTime));

        CompleteCast(skill);
    }

    private void CompleteCast(SkillDefinition skill)
    {
        IsCasting = false;
        CurrentSkill = null;
        _castRoutine = null;

        _cooldowns.StartCooldown(skill.id, skill.cooldown);
        Debug.Log($"[Skills] Cast completado: {skill.id} — cooldown {skill.cooldown}s iniciado");

        OnCastCompleted?.Invoke(skill);
        ExecuteEffects(skill);
    }

    private void ExecuteEffects(SkillDefinition skill)
    {
        if (skill?.effects == null || skill.effects.Length == 0)
        {
            Debug.Log($"[Skills] {skill.id} no tiene efectos");
            return;
        }

        StartCoroutine(ExecuteEffectChain(skill));
    }

    private IEnumerator ExecuteEffectChain(SkillDefinition skill)
    {
        SkillCastContext context = new SkillCastContext
        {
            Player = _player,
            Skill = skill,
            Origin = _player.transform.position
        };

        ISkillTargeting targeting = SkillTargetingFactory.Create(skill.targeting);
        targeting.Resolve(context);

        for (int i = 0; i < skill.effects.Length; i++)
        {
            ISkillEffect effect = SkillEffectFactory.Create(skill.effects[i]);
            if (effect == null)
            {
                Debug.LogWarning($"[Skills] Efecto desconocido: {skill.effects[i].type}");
                continue;
            }

            Debug.Log($"[Skills] Aplicando efecto '{skill.effects[i].type}' de {skill.id} (center {context.Center}, dir {context.Direction})");
            yield return effect.Apply(context);
        }
    }

    private void OnDisable()
    {
        if (_castRoutine != null)
            StopCoroutine(_castRoutine);

        _castRoutine = null;
        IsCasting = false;
        CurrentSkill = null;
    }
}
