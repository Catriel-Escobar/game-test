using System;
using UnityEngine;

public class MobResources : MonoBehaviour
{
	public int CurrentHp { get; private set; }
	public int MaxHp { get; private set; }

	public bool IsDead => CurrentHp <= 0;

	public event Action OnDeath;
	public event Action<int, int> OnHealthChanged;
	public event Action OnHit;

	public void Initialize(Enemy enemyConfig, float healthMultiplier = 1f)
	{
		int baseHp = enemyConfig != null ? Mathf.RoundToInt(enemyConfig.health) : 1;
		int startingHp = Mathf.RoundToInt(baseHp * healthMultiplier);
		CurrentHp = startingHp;
		MaxHp = startingHp;

		OnHealthChanged?.Invoke(CurrentHp, MaxHp);
	}

	public void TakeDamage(int damage, bool isCritical = false)
	{
		if (IsDead)
			return;

		CurrentHp = Mathf.Max(CurrentHp - damage, 0);
		OnHealthChanged?.Invoke(CurrentHp, MaxHp);

		if (DamageNumberManager.Instance != null)
		{
			DamageNumberManager.Instance.Show(
				transform.position + Vector3.up * 2f,
				damage,
				isCritical);
		}

		if (CurrentHp <= 0)
			Die();
		else
			OnHit?.Invoke();
	}

	public void Heal(int amount)
	{
		if (IsDead)
			return;

		CurrentHp = Mathf.Min(CurrentHp + amount, MaxHp);
		OnHealthChanged?.Invoke(CurrentHp, MaxHp);
	}

	private void Die()
	{
		OnDeath?.Invoke();
	}
}
