using System;
using System.Collections.Generic;
using System.Text;
using Dsw2026Tpi.CrossCutting.Exceptions;

namespace Dsw2026Tpi.CrossCutting.Helpers;

public static class DayOfWeekExtensions
{
    private static readonly Dictionary<string, DayOfWeek> Map = new(StringComparer.OrdinalIgnoreCase)
    {
        ["LUNES"] = DayOfWeek.Monday,
        ["MARTES"] = DayOfWeek.Tuesday,
        ["MIERCOLES"] = DayOfWeek.Wednesday,
        ["MIÉRCOLES"] = DayOfWeek.Wednesday,
        ["JUEVES"] = DayOfWeek.Thursday,
        ["VIERNES"] = DayOfWeek.Friday,
        ["SABADO"] = DayOfWeek.Saturday,
        ["SÁBADO"] = DayOfWeek.Saturday,
        ["DOMINGO"] = DayOfWeek.Sunday
    };

    public static DayOfWeek ToDayOfWeek(this string day)
    {
        if (Map.TryGetValue(day, out var result)) return result;
        throw new ValidationException("Dia invalido: " + day, "INVALID_DAY");
    }

    public static string ToSpanish(this DayOfWeek day) => day switch
    {
        DayOfWeek.Monday => "LUNES",
        DayOfWeek.Tuesday => "MARTES",
        DayOfWeek.Wednesday => "MIERCOLES",
        DayOfWeek.Thursday => "JUEVES",
        DayOfWeek.Friday => "VIERNES",
        DayOfWeek.Saturday => "SABADO",
        DayOfWeek.Sunday => "DOMINGO",
        _ => throw new ArgumentOutOfRangeException()
    };
}
