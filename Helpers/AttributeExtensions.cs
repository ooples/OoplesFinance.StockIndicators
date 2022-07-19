//     Ooples Finance Stock Indicator Library
//     https://ooples.github.io/OoplesFinance.StockIndicators/
//
//     Copyright © Franklin Moormann, 2020-2022
//     cheatcountry@gmail.com
//
//     This library is free software and it uses the Apache 2.0 license
//     so if you are going to re-use or modify my code then I just ask
//     that you include my copyright info and my contact info in a comment

namespace OoplesFinance.StockIndicators.Helpers;

[AttributeUsage(AttributeTargets.Field)]
public class CategoryAttribute : Attribute
{
    public IndicatorType Type { get; set; }

    public CategoryAttribute(IndicatorType type)
    {
        Type = type;
    }
}

public static class AttributeExtensions
{
    /// <summary>
    /// Gets the type of the indicator.
    /// </summary>
    /// <param name="e">The e.</param>
    /// <returns></returns>
    public static IndicatorType GetIndicatorType(this IndicatorName e)
    {
        var name = Enum.GetName(typeof(IndicatorName), e);
        var member = e.GetType().GetMember(name ?? string.Empty).FirstOrDefault();
        var attr = member != null ? Attribute.GetCustomAttribute(member, typeof(CategoryAttribute)) as CategoryAttribute : default;

        return attr?.Type ?? default;
    }

    /// <summary>
    /// Converts to formattedstring.
    /// </summary>
    /// <param name="iName">Name of the i.</param>
    /// <returns></returns>
    public static string ToFormattedString(this IndicatorName iName)
    {
        var result = iName.ToString();
        return result[0] == '_' ? result.Replace('_', ' ').Trim() : result;
    }

    /// <summary>
    /// Gets the indicator names.
    /// </summary>
    /// <param name="iType">Type of the i.</param>
    /// <returns></returns>
    public static IEnumerable<IndicatorName> GetIndicatorNames(IndicatorType iType)
    {
        //Find all Members of Name Enum Type that have a categoryAttribute with the Category property assigned to the category parameter.
        var members = typeof(IndicatorName).GetMembers().ToList();
        var result = new List<IndicatorName>();
        foreach (var member in members)
        {
            //get attributes for member
            if (Attribute.GetCustomAttribute(member, typeof(CategoryAttribute)) is CategoryAttribute categoryAttribute && categoryAttribute.Type == iType)
            {
                //use the member name to get an instance of enumerated type.
                _ = Enum.TryParse(member.Name, out IndicatorName iName);
                result.Add(iName);
            }
        }

        return result;
    }
}
