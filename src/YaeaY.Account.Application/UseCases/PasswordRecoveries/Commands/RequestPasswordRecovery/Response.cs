namespace YaeaY.Account.Application.UseCases.PasswordRecoveries.Commands.RequestPasswordRecovery;

public sealed record Response(string Message)
{
    public static readonly Response Accepted = new("Se o endereço estiver associado a uma conta elegível, enviaremos um código de recuperação.");
}
