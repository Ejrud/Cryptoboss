[System.Serializable]
public class CardData
{

    public bool Used;               // Если игрок воспользовался картой, то карта заполнится новыми данными при обновлении (если у игрока будет 2 карты)
    // Интерфейсные параметры
    public string Guid;             // Глобальный идентификатор
    public string Version;          // Версия (колличество вайпов)

    // Геймплейные параметры
    public string Name;             // Название карты
    public string Type;            // Потребление энергии
    public int EnergyDamage;        // Урон по энергии
    public int CapitalDamage;       // Урон по капиталу
    public int EnergyHealth;        // Лечение энергии
    public int CapitalEarnings;     // Заработок
    public int DamageResistance;    // Защита от урона по капиталу
    public int CardCost;            // Потребление энергии
    public string ChipId;
}
