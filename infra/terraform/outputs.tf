output "resource_group_name" {
  description = "Name of the API Navigator development resource group."
  value       = azurerm_resource_group.api_navigator.name
}

output "app_service_plan_id" {
  description = "Resource ID of the Windows F1 App Service plan."
  value       = azurerm_service_plan.api_navigator.id
}

output "web_app_name" {
  description = "Name of the API Navigator development Web App."
  value       = azurerm_windows_web_app.api_navigator.name
}

output "web_app_url" {
  description = "HTTPS URL of the API Navigator development Web App."
  value       = "https://${azurerm_windows_web_app.api_navigator.default_hostname}"
}
