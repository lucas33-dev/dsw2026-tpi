using Dsw2026Tpi.CrossCutting.Helpers;

namespace Dsw2026Tpi.Tests;

public class DayOfWeekTests
{
    [Theory]
    [InlineData("LUNES", DayOfWeek.Monday)]
    [InlineData("MARTES", DayOfWeek.Tuesday)]
    [InlineData("MIERCOLES", DayOfWeek.Wednesday)]
    [InlineData("JUEVES", DayOfWeek.Thursday)]
    [InlineData("VIERNES", DayOfWeek.Friday)]
    [InlineData("SABADO", DayOfWeek.Saturday)]
    [InlineData("DOMINGO", DayOfWeek.Sunday)]
    [InlineData("lunes", DayOfWeek.Monday)]
    public void ToDayOfWeek_TraduceDiasValidos(string dia, DayOfWeek esperado)
    {
        var resultado = dia.ToDayOfWeek();

        Assert.Equal(esperado, resultado);
    }

    [Fact]
    public void ToDayOfWeek_LanzaExcepcionConDiaInvalido()
    {
        Assert.ThrowsAny<Exception>(() => "LUNESSS".ToDayOfWeek());
    }

    [Theory]
    [InlineData(DayOfWeek.Monday, "LUNES")]
    [InlineData(DayOfWeek.Friday, "VIERNES")]
    [InlineData(DayOfWeek.Sunday, "DOMINGO")]
    public void ToSpanish_TraduceAEspanol(DayOfWeek dia, string esperado)
    {
        var resultado = dia.ToSpanish();

        Assert.Equal(esperado, resultado);
    }
}