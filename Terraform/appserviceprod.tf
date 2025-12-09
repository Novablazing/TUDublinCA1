resource "azurerm_service_plan" "planproduction" {
  name                = "live-Prod"
  location            = data.azurerm_resource_group.RG1.location
  resource_group_name = data.azurerm_resource_group.RG1.name
  os_type             = "Linux"
  sku_name            = "S1"
}

resource "azurerm_linux_web_app" "appproduction" {
  name                = "live-prod001"
  resource_group_name = data.azurerm_resource_group.RG1.name
  location            = azurerm_service_plan.planproduction.location
  service_plan_id     = azurerm_service_plan.planproduction.id

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

resource "azurerm_linux_web_app_slot" "stage" {
  name           = "example-slot"
  app_service_id = azurerm_linux_web_app.appproduction.id

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