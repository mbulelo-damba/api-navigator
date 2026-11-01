# API Navigator — Azure development infrastructure

This Terraform configuration describes a small development environment for the
.NET 10 API:

- Resource group: `rg-api-navigator-dev`
- App Service plan: Windows F1
- Web App: Windows App Service configured for .NET 10
- Region default: South Africa North

## Important safety notes

- **Nothing in this repository automatically creates Azure resources.**
- The Terraform GitHub Actions workflow runs formatting and validation only.
  It does not run `terraform plan` or `terraform apply`.
- Do not run `terraform apply` until the subscription, region, F1 eligibility,
  current pricing, and expected charges have been checked in the Azure portal.
- F1 is a limited shared development/testing tier, not a production tier.
- A budget sends notifications; it is not a hard spending cap.
- The Azure Free Account promotional credit and the App Service F1 offer are
  separate. Other resources and usage can still consume credit or incur charges.
- Confirm that F1 is available for Windows App Service in the selected region.
  If South Africa North does not offer the required SKU, choose a region only
  after reviewing latency, residency requirements, and pricing.

## Prerequisites before provisioning

1. An active Azure subscription.
2. Terraform CLI 1.9 or later.
3. Azure CLI authentication for the intended subscription.
4. A globally unique Web App name.

Set the required Web App name when running Terraform. Do not commit a
`terraform.tfvars` file containing account-specific values.

Example:

```bash
terraform init
terraform validate
terraform plan -var="web_app_name=your-unique-api-navigator-name"
```

Review the complete plan and cost implications before any apply. Applying is
intentionally not automated by this repository.

## Terraform state

The configuration currently uses Terraform's default local state. Do not commit
`terraform.tfstate`, state backups, or local variable files. Before team use
or automated provisioning, configure a secured remote state backend with
appropriate access controls and locking.

## GitHub Actions deployment

The separate `.github/workflows/azure-webapp-deploy.yml` workflow is manual
only. It requires an existing Web App and GitHub repository variables/secrets
for Azure OpenID Connect (OIDC). It does not create Azure resources.

Required repository variables:

- `AZURE_WEBAPP_NAME`: the exact Web App name created after approval.

Required repository secrets:

- `AZURE_CLIENT_ID`
- `AZURE_TENANT_ID`
- `AZURE_SUBSCRIPTION_ID`

Configure a federated identity credential in Microsoft Entra ID for this
repository and the intended GitHub branch before running the deployment
workflow. Grant the identity only the permissions it needs. Never store a
client secret, password, or publish profile in source control.

The deployment workflow is manually triggered and publishes
`src/ApiNavigator.Api/ApiNavigator.Api.csproj`. It does not run Terraform.
