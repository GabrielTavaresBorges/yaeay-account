using Microsoft.AspNetCore.Mvc;

namespace YaeaY.Account.Presentation.Server.Controllers.RuntimeConfigurations;

[ApiController]
[Route("api/runtime-configuration")]
public sealed class RuntimeConfigurationController(
    IConfiguration configuration,
    IHostEnvironment hostEnvironment) : ControllerBase
{
    [HttpGet]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public IActionResult Get()
    {
        var showTestModeBanner = hostEnvironment.IsDevelopment()
            && configuration.GetValue<bool>("UserInterface:ShowTestModeBanner");
        var testModeBannerText = configuration["UserInterface:TestModeBannerText"]
            ?? "MODO DE TESTES - HOMOLOGAÇÃO";

        return Ok(new
        {
            showTestModeBanner,
            testModeBannerText
        });
    }
}
