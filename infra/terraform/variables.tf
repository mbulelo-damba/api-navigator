variable "location" {
  description = "Azure region for the development resources. Confirm F1 availability in this region before applying."
  type        = string
  default     = "South Africa North"
}

variable "web_app_name" {
  description = "Globally unique Azure App Service name (the resulting hostname is <name>.azurewebsites.net)."
  type        = string

  validation {
    condition     = can(regex("^[a-zA-Z0-9-]{2,60}$", var.web_app_name))
    error_message = "The web app name must contain 2-60 letters, numbers, or hyphens."
  }
}

variable "tags" {
  description = "Tags applied to the development resources."
  type        = map(string)
  default = {
    project     = "api-navigator"
    environment = "development"
    managed-by  = "terraform"
  }
}
