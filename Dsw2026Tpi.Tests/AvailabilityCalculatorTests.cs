using System;
using System.Collections.Generic;
using System.Text;
using Dsw2026Tpi.CrossCutting.Helpers;

namespace Dsw2026Tpi.Tests;

public class AvailabilityCalculatorTests
{
    [Fact]
    public void GetDatesForDayOfWeek_DevuelveTodosLosLunesDeUnMes()
    {
        var result = AvailabilityCalculator.GetDatesForDayOfWeek(2026, 7, DayOfWeek.Monday);

        Assert.Equal(4, result.Count);
        Assert.All(result, d => Assert.Equal(DayOfWeek.Monday, d.DayOfWeek));
        Assert.All(result, d => Assert.Equal(7, d.Month));
    }

    [Fact]
    public void GetDatesForDayOfWeek_TodasLasFechasSonDelMesCorrecto()
    {
        var result = AvailabilityCalculator.GetDatesForDayOfWeek(2026, 8, DayOfWeek.Friday);

        Assert.All(result, d => Assert.Equal(8, d.Month));
        Assert.All(result, d => Assert.Equal(DayOfWeek.Friday, d.DayOfWeek));
    }

    [Fact]
    public void GetDatesForDayOfWeek_FechasEstan7DiasAparte()
    {
        var result = AvailabilityCalculator.GetDatesForDayOfWeek(2026, 8, DayOfWeek.Wednesday);

        for (int i = 1; i < result.Count; i++)
        {
            var diff = result[i].DayNumber - result[i - 1].DayNumber;
            Assert.Equal(7, diff);
        }
    }

    [Fact]
    public void GetTimeSlots_TresHorasGeneraSeisBloques()
    {
        var start = new TimeOnly(9, 0);
        var end = new TimeOnly(12, 0);

        var result = AvailabilityCalculator.GetTimeSlots(start, end);

        Assert.Equal(6, result.Count);
    }

    [Fact]
    public void GetTimeSlots_CadaBloqueEs30Minutos()
    {
        var start = new TimeOnly(9, 0);
        var end = new TimeOnly(12, 0);

        var result = AvailabilityCalculator.GetTimeSlots(start, end);

        Assert.All(result, slot =>
        {
            var duracion = slot.End.ToTimeSpan() - slot.Start.ToTimeSpan();
            Assert.Equal(30, duracion.TotalMinutes);
        });
    }

    [Fact]
    public void GetTimeSlots_PrimerBloqueEmpiezaEnHoraInicio()
    {
        var start = new TimeOnly(9, 0);
        var end = new TimeOnly(12, 0);

        var result = AvailabilityCalculator.GetTimeSlots(start, end);

        Assert.Equal(start, result.First().Start);
    }

    [Fact]
    public void GetTimeSlots_UltimoBloqueTerminaEnHoraFin()
    {
        var start = new TimeOnly(9, 0);
        var end = new TimeOnly(12, 0);

        var result = AvailabilityCalculator.GetTimeSlots(start, end);

        Assert.Equal(end, result.Last().End);
    }

    [Fact]
    public void GetTimeSlots_RangoMenorA30MinutosDevuelveListaVacia()
    {
        var start = new TimeOnly(9, 0);
        var end = new TimeOnly(9, 20);

        var result = AvailabilityCalculator.GetTimeSlots(start, end);

        Assert.Empty(result);
    }
}
