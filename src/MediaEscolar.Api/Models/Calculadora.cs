namespace MediaEscolar.Api.Models;

public static class Calculadora
{
    public const double NotaMinima = 0;
    public const double NotaMaxima = 10;
    public const double MediaAprovacao = 6.0;

    public static void ValidarNota(double nota)
    {
        if (nota < NotaMinima || nota > NotaMaxima)
        {
            throw new ArgumentOutOfRangeException(
                nameof(nota),
                $"Nota deve estar entre {NotaMinima} e {NotaMaxima}.");
        }
    }

    public static double CalcularMedia(double nota1, double nota2) => (nota1 + nota2) / 2;

    public static string CalcularSituacao(double media) =>
        media >= MediaAprovacao ? "Aprovado" : "Reprovado";
}
