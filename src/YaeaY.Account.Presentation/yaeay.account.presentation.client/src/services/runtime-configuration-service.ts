export type RuntimeConfiguration = {
  showTestModeBanner: boolean
  testModeBannerText: string
}

let configurationRequest: Promise<RuntimeConfiguration> | undefined

export function getRuntimeConfiguration(): Promise<RuntimeConfiguration> {
  configurationRequest ??= fetch('/api/runtime-configuration', {
    credentials: 'same-origin',
    headers: { Accept: 'application/json' },
  }).then(async (response) => {
    if (!response.ok)
      throw new Error('Não foi possível consultar a configuração do ambiente.')

    return await response.json() as RuntimeConfiguration
  })

  return configurationRequest
}
