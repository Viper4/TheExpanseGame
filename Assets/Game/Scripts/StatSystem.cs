using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class StatSystem : MonoBehaviour
{
    public float health = 100;
    public float maxHealth = 100;
    public float armor = 0;
    public float maxArmor = 100;

    [SerializeField] private Animation damageAnimation;
    [SerializeField] private List<Slider> healthBars = new List<Slider>();
    [SerializeField] private List<TextMeshProUGUI> healthTexts = new List<TextMeshProUGUI>();
    [SerializeField] private List<Slider> armorBars = new List<Slider>();
    [SerializeField] private List<TextMeshProUGUI> armorTexts = new List<TextMeshProUGUI>();

    [SerializeField] private ParticleSystem damageParticles;

    [SerializeField] private UnityEvent onDamage;
    [SerializeField] private UnityEvent onHeal;
    [SerializeField] private UnityEvent onDeath;

    public void Damage(float amount, float armorPiercing)
    {
        float healthDamage = amount * (1 - armorPiercing);
        float armorDamage = amount * armorPiercing;
        armor -= armorDamage;
        if(armor < 0)
        {
            health += armor;
            armor = 0;
        }
        health -= healthDamage;

        if (damageAnimation != null)
        {
            damageAnimation.Play();
        }
        if (healthBars.Count > 0)
        {
            for(int i = 0; i < healthBars.Count; i++)
            {
                healthBars[i].value = health / maxHealth;
            }
        }
        if (armorBars.Count > 0)
        {
            for (int i = 0; i < armorBars.Count; i++)
            {
                armorBars[i].value = health / maxHealth;
            }
        }
        if (healthTexts.Count > 0)
        {
            for (int i = 0; i < healthTexts.Count; i++)
            {
                healthTexts[i].text = (health / maxHealth).ToString();
            }
        }
        if (armorTexts.Count > 0)
        {
            for (int i = 0; i < armorTexts.Count; i++)
            {
                armorTexts[i].text = (health / maxHealth).ToString();
            }
        }
        onDamage?.Invoke();

        if (health <= 0)
        {
            onDeath?.Invoke();
        }

        health = Mathf.Clamp(health, 0, maxHealth);
        armor = Mathf.Clamp(armor, 0, maxArmor);
    }

    public void Heal(float amount)
    {
        health += amount;
        health = Mathf.Clamp(health, 0, maxHealth);
        onHeal?.Invoke();
    }

    public void AddArmor(float amount)
    {
        armor += amount;
        armor = Mathf.Clamp(armor, 0, maxArmor);
    }

    public void DestroyTarget(Object obj)
    {
        Destroy(obj);
    }

    public void AddHealthBar(Slider slider)
    {
        healthBars.Add(slider);
    }

    public void AddArmorBar(Slider slider)
    {
        armorBars.Add(slider);
    }

    public void AddHealthText(TextMeshProUGUI text)
    {
        healthTexts.Add(text);
    }

    public void AddArmorText(TextMeshProUGUI text)
    {
        armorTexts.Add(text);
    }
}
