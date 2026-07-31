using Dsw2026Tpi.CrossCutting.Helpers;

namespace Dsw2026Tpi.Tests;

public class ValidationsTests
{
    [Theory]
    [InlineData("test@mail.com", true)]
    [InlineData("nombre.apellido@dominio.com.ar", true)]
    [InlineData("sinarroba.com", false)]
    [InlineData("sin@punto", false)]
    [InlineData("", false)]
    [InlineData(null, false)]
    public void IsEmailValid_ValidaFormatoDeEmail(string? email, bool esperado)
    {
        var resultado = email.IsEmailValid();

        Assert.Equal(esperado, resultado);
    }

    [Theory]
    [InlineData(12345678, true)]
    [InlineData(1234567, true)]
    [InlineData(123456, false)]
    [InlineData(123456789, false)]
    [InlineData(0, false)]
    [InlineData(-5, false)]
    public void IsDniValid_ValidaCantidadDeDigitos(long dni, bool esperado)
    {
        var resultado = dni.IsDniValid();

        Assert.Equal(esperado, resultado);
    }
}