
namespace Books.Wardrobe
{
    public struct AssetMetadata
    {
        public readonly string ItemName;
        public readonly ItemType ItemType;
        public readonly EnvironmentType EnvironmentType;
        public readonly LightMode LightMode;
        public readonly string FileName;
        public readonly string CharacterName;
        public readonly int SuitLayer;
        public readonly string ColorName;

        public AssetMetadata(
            string itemName = null, 
            string fileName = null,
            ItemType itemType = ItemType.None, 
            EnvironmentType environmentType = Wardrobe.EnvironmentType.None, 
            LightMode lightMode = LightMode.None, 
            string characterName = null,
            int suitLayer = default, 
            string colorName = null)
        {
            ItemName = itemName;
            FileName = fileName ?? ItemName;
            ItemType = itemType;
            EnvironmentType = environmentType;
            LightMode = lightMode;
            CharacterName = characterName;
            SuitLayer = suitLayer;
            ColorName = colorName;
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
        [DisplayName("ГГ")] Main = 1,
        Other = 2,
    }
}