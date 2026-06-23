using MediaEscolar.Api.Models;
using Xunit;

namespace MediaEscolar.Tests;

// Testes unitários da classe Calculadora (média, situação e validação de notas)
public class CalculadoraTests
{
    // Teste unitário: verifica se a média aritmética de duas notas é calculada corretamente
    [Fact]
    public void CalcularMedia_DeveRetornarMediaAritmeticaDasDuasNotas()
    {
        //ARRANGE
        var n1 = 8.0;
        var n2 = 6.0;

        //ACT
        var media = Calculadora.CalcularMedia(n1, n2);
        
        //ASSERT
        Assert.Equal(7, media);
    }

    // Teste unitário: média igual ao valor de corte (6.0) deve resultar em aprovação
    [Fact]
    public void CalcularSituacao_DeveRetornarAprovadoQuandoMediaMaiorOuIgualASeis()
    {
        var situacao = Calculadora.CalcularSituacao(6.0);
        Assert.Equal("Aprovado", situacao);
    }

    // Teste unitário: média abaixo do valor de corte (6.0) deve resultar em reprovação
    [Fact]
    public void CalcularSituacao_DeveRetornarReprovadoQuandoMediaMenorQueSeis()
    {
        var situacao = Calculadora.CalcularSituacao(5.9);
        Assert.Equal("Reprovado", situacao);
    }

    // Teste unitário: nota acima do limite máximo (10) deve lançar exceção de validação
    [Fact]
    public void ValidarNota_DeveLancarExcecaoQuandoNotaForMaiorQueDez()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => Calculadora.ValidarNota(10.1));
    }

    // Teste unitário: nota abaixo do limite mínimo (0) deve lançar exceção de validação
    [Fact]
    public void ValidarNota_DeveLancarExcecaoQuandoNotaForMenorQueZero()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => Calculadora.ValidarNota(-0.1));
    }


    //Teste de regressão(após todas alterações do código esse teste certifica que funções antigas continuem funcionando)
    // Reexecuta o cenário original de corte de aprovação/reprovação para garantir que nenhuma alteração futura quebre essa regra de negócio
    [Fact]
    public void CalcularSituacao_RegressaoDaNotaDeCorteSeis()
    {
        Assert.Equal("Aprovado", Calculadora.CalcularSituacao(6.0));
        Assert.Equal("Reprovado", Calculadora.CalcularSituacao(7.00));
    }
}
