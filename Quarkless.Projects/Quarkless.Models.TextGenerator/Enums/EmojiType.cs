using System.ComponentModel;

namespace Quarkless.Models.TextGenerator.Enums
{
	public enum EmojiType
	{
		[Description("Positive")]
		Positive,
		[Description("Smileys")]
		Smileys,
		[Description("People and Fantasy")]
		PeopleFantasy,
		[Description("Clothing and Accessories")]
		ClothingAccessories,
		[Description("Animals & Nature")]
		AnimalsNature,
		[Description("Food & Drink")]
		FoodDrink,
		[Description("Activity and Sports")]
		ActivitySports,
		[Description("Travel & Places")]
		TravelPlaces,
		[Description("Objects")]
		Objects,
		[Description("Symbols")]
		Symbols,
	}
}
