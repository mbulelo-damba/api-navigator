# Azure infrastructure for the API Navigator development environment.
# This configuration creates resources only when an operator explicitly runs
# "terraform apply". The GitHub Actions validation workflow never applies it.

terraform {
  required_version = ">= 1.9.0"

  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "~> 5.0"
    }
  }
}

provider "azurerm" {
  features {}
}

resource "azurerm_resource_group" "api_navigator" {
  name     = "rg-api-navigator-dev"
  location = var.location

  tags = var.tags
}

resource "azurerm_service_plan" "api_navigator" {
  name                = "asp-api-navigator-dev"
  resource_group_name = azurerm_resource_group.api_navigator.name
  location            = azurerm_resource_group.api_navigator.location

  os_type  = "Windows"
  sku_name = "F1"

  tags = var.tags
}

resource "azurerm_windows_web_app" "api_navigator" {
  name                = var.web_app_name
  resource_group_name = azurerm_resource_group.api_navigator.name
  location            = azurerm_service_plan.api_navigator.location
  service_plan_id     = azurerm_service_plan.api_navigator.id

  https_only = true

  site_config {
    application_stack {
      current_stack  = "dotnet"
      dotnet_version = "v10.0"
    }
  }

  tags = var.tags
}
