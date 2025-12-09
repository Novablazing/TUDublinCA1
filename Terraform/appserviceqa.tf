resource "azurerm_service_plan" "planqadevelopment" {
  name                = "qa-NonProd"
  location            = data.azurerm_resource_group.RG1.location
  resource_group_name = data.azurerm_resource_group.RG1.name
  os_type             = "Linux"
  sku_name            = "F1"
}

resource "azurerm_linux_web_app" "appqadevelopment" {
  name                = "qa-nonprod001"
  resource_group_name = data.azurerm_resource_group.RG1.name
  location            = azurerm_service_plan.planqadevelopment.location
  service_plan_id     = azurerm_service_plan.planqadevelopment.id

  site_config {
    always_on = false

    application_stack {
      docker_image_name   = "ghcr.io/novablazing/bpcalculatorlatest"
    }
  }

  app_settings = {
    "ASPNETCORE_FORWARDEDHEADERS_ENABLED" = "true"
  }
}
