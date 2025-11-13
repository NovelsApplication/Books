
namespace Books.Wardrobe
{
    public struct AssetMetadata
    {
        public readonly string ItemName;
        public readonly ItemType ItemType;
        public readonly EnvironmentType EnvironmentType;
        public readonly LightMode LightMode;
        public readonly Character Character;
        public readonly int SuitLayer;
        public readonly int ColorVariant;

        public AssetMetadata(
            string itemName = null, 
            ItemType itemType = ItemType.None, 
            EnvironmentType environmentType = Wardrobe.EnvironmentType.None, 
            LightMode lightMode = LightMode.None, 
            Character character = Character.None, 
            int suitLayer = default, 
            int colorVariant = default)
        {
            ItemName = itemName;
            ItemType = itemType;
            EnvironmentType = environmentType;
            LightMode = lightMode;
            Character = character;
            SuitLayer = suitLayer;
            ColorVariant = colorVariant;
        }
    }

    public enum ItemType
    {
        None = 0,
        [DisplayName("Аксессуары")] Accessories = 1,
        [DisplayName("Причёски")] Hairstyles = 2,
        [DisplayName("Одежда")] Clothing = 3,
        [DisplayName("Внешность")] Appearance = 4,
        [DisplayName("Локации")] Location = 5,
        [DisplayName("Персонажи")] Character = 6, 
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

    public enum Character
    {
        None = 0,
        Main = 1,
        Other = 2,
    }
}