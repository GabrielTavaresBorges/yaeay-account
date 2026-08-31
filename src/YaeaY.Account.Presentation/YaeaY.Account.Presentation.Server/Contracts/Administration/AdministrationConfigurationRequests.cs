namespace YaeaY.Account.Presentation.Server.Contracts.Administration;
public sealed record UpdateEmailConfirmationTemplateRequest(string Subject, string BodyHtml, string Justification);
public sealed record CreateIdentityRoleRequest(string Name, string Justification);
