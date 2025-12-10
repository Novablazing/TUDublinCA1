# Configure the Azure provider
terraform {
  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "~> 4.0.0"
    }
  }

  required_version = ">= 1.1.0"
}

provider "azurerm" {
  features {}
  resource_provider_registrations = "none"
  subscription_id = "0a1013dc-61ea-4bd0-9dce-d896a30773fe"
}