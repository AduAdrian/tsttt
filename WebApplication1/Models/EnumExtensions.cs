using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace WebApplication1.Models
{
    public static class EnumExtensions
    {
        public static string GetDisplayName(this Enum enumValue)
        {
            var displayAttribute = enumValue.GetType()
                .GetMember(enumValue.ToString())
                .First()
                .GetCustomAttribute<DisplayAttribute>();

            if (displayAttribute != null)
            {
                return displayAttribute.Name ?? enumValue.ToString();
            }

            return enumValue.ToString();
        }
    }
}