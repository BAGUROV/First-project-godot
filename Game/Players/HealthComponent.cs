using Godot;
using System;

public partial class HealthComponent : Node
{
	private float _maxHealth;
	public float _currentHealth;

	[Signal]
	public delegate void DefeatEventHandler();
	[Signal]
	public delegate void HealthChangedEventHandler();

	public float MaxHealth 
	{ 
		get
		{
			return _maxHealth;
		} 
		set
		{
			_maxHealth = value;
		} 
	}
	public float CurrentHealth  
	{ 
		get
		{
			return _currentHealth;
		} 
		set
		{
			_currentHealth = Math.Max(value, 0.0f);
			if (_currentHealth == 0.0f)
				EmitSignal(SignalName.Defeat);

			EmitSignal(SignalName.HealthChanged);
		} 
	}

	// Обновления максимального здоровья
	public void UpdateMaxHealth(float maxHpIn)
	{
		MaxHealth = maxHpIn;
		CurrentHealth = MaxHealth;
	}
	
	// Получения урона
	public void TakeDamage(float damageIn)
		=> CurrentHealth -= damageIn; 
}
