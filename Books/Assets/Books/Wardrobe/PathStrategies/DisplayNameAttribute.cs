using System;

namespace Books.Wardrobe.PathStrategies
{
    [AttributeUsage(AttributeTargets.Field)]
    public class DisplayNameAttribute : Attribute
    {
        public string Name { get; }

        public DisplayNameAttribute(string name)
        {
            Name = name;
        }
    }
    
    public enum ItemType
    {
        None = 0,
        [DisplayName("Аксессуары")] Accessories = 1,
        [DisplayName("Причёски")] Hairstyles = 2,
        [DisplayName("Одежда")] Suit = 3,
        [DisplayName("Внешность")] Appearance = 4,
        [DisplayName("Локации")] Location = 5,
        [DisplayName("Персонажи")] Character = 6, 
    }

    public enum CategoryType
    {
        None = 0,
        Suit = 1,
        Hairstyles = 2,
        Appearance = 3,
        Accessories = 4,
    }
    
    public enum EnvironmentType
    {
        None = 0,
        [DisplayName("Вода")] Water = 1,
        [DisplayName("Суша")] Land = 2,
        [DisplayName("Универсал")] Universal = 3,
    }

    public enum LightMode
    {
        None = 0,
        [DisplayName("Тёмные")] Dark = 1,
        [DisplayName("Светлые")] Light = 2,
    }
}