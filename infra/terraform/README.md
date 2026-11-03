# API Navigator — Azure development infrastructure

This Terraform configuration describes a small Linux development environment
for the .NET 10 API:

- Resource group: `rg-api-navigator-dev`
- App Service plan: Linux Basic B1
- Web App: Linux App Service configured for .NET 10
- Preferred region default: South Africa North (override with `-var="location=..."` if another region is more suitable)

## Important safety notes

- **Nothing in this repository automatically creates Azure resources.**
- Terraform validation workflows do not run `terraform apply`.
- Do not apply until the exact region, Linux B1 price, .NET 10 runtime availability,
  promotional-credit eligibility, and expected charges have been checked.
- Basic B1 is a paid tier. Stopping the Web App does not stop App Service plan
  billing; delete or downgrade the plan when it is no longer needed.
- A budget alert is a notification, not a hard spending cap.
- Promotional credit expires according to the subscription's offer terms.
  Do not assume unused credit carries over or that charges after expiry are covered.
- This configuration intentionally creates no database, storage account,
  Application Insights resource, container registry, or other add-ons.

## Prerequisites before provisioning

1. An active Azure subscription.
2. Terraform CLI 1.9 or later.
3. Azure authentication for the intended subscription.
4. A globally unique Web App name.
5. Confirmation of current regional availability and price in Azure.

Set the required Web App name when running Terraform. Do not commit a
`terraform.tfvars` file containing account-specific values.

Example:

```bash
terraform init
terraform validate
terraform plan -var="web_app_name=your-unique-api-navigator-name"
```

To use a different region, pass `-var="location=Region Name"`. Review the
complete plan and cost implications before any apply. Applying is intentionally
not automated by this repository.

## Terraform state

The configuration currently uses Terraform's default local state. Do not commit
`terraform.tfstate`, state backups, or local variable files. Before team use
or automated provisioning, configure a secured remote state backend with
appropriate access controls and locking.

## GitHub Actions deployment

The separate `.github/workflows/azure-webapp-deploy.yml` workflow is manual
only. It requires an existing Web App and GitHub repository variables/secrets
for Azure OpenID Connect (OIDC). It does not create Azure resources.

Required repository variable:

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
