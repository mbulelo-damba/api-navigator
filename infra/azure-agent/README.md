# Azure Infrastructure Agent (read-only starter)

This folder contains an isolated Cloudflare Worker that exposes a small set of Azure Resource Manager **read-only** tools to an MCP client such as Claude.

## Safety boundary

- The only configured resource group is `rg-api-navigator-dev`.
- The MCP endpoint requires a bearer token.
- Azure access uses a service principal that should receive **Reader**, scoped to that resource group only.
- The Worker exposes GET-only Azure operations: inspect the resource group and list its resources.
- There are no deployment, create, update, delete, role-assignment, or subscription-wide enumeration tools.
- Do not grant this app Owner, User Access Administrator, or subscription-wide Contributor.
- The public `/healthz` endpoint reveals only a static health status.

This starter is not a production security certification. Keep the Worker token and Azure client secret private, rotate them if exposed, and review logs/access before expanding permissions.

## Cloudflare deployment layout

Configure the Cloudflare Worker Git integration to use this repository with:

- Production branch: `feature/azure-infrastructure-mcp-agent` during testing only.
- Root directory: `infra/azure-agent`.
- Build command: `npm install && npm run typecheck`.
- Deploy command: `npm run deploy`.

Do not point the Worker at the repository root: this repository also contains the .NET API project. Do not connect or deploy until the branch, root directory, and commands have been checked in the Cloudflare dashboard.

## Worker secrets

Add these as Worker secrets in Cloudflare (Workers & Pages > azure-infrastructure-agent > Settings > Variables and Secrets). Do not put secret values in GitHub files or chat:

- `MCP_BEARER_TOKEN`: a newly generated high-entropy random token. Use the same value as Claude's custom connector Bearer token.
- `AZURE_TENANT_ID`: Microsoft Entra tenant ID.
- `AZURE_CLIENT_ID`: app registration (service principal) application/client ID.
- `AZURE_CLIENT_SECRET`: client secret value (not the secret ID).
- `AZURE_SUBSCRIPTION_ID`: subscription ID containing the target resource group.

The resource group name is a non-secret Worker variable in `wrangler.toml` and is fixed to `rg-api-navigator-dev`.

## Azure identity setup (manual, no resources deployed)

1. In Azure Portal, open Microsoft Entra ID > App registrations > New registration. Name it `azure-infrastructure-agent-readonly`; leave supported account types at the single-tenant default.
2. Create a client secret under Certificates & secrets. Copy the secret **value** once into Cloudflare Worker Secrets. Never commit it or send it in chat.
3. Open the existing resource group `rg-api-navigator-dev` > Access control (IAM) > Add role assignment.
4. Assign the built-in **Reader** role to the new app registration's service principal at this resource-group scope only.
5. Do not create or deploy any Azure resources during this setup.

Azure credits are not a hard spending cap. Verify the promotional credit balance and expiry in Cost Management + Billing > Credits before authorizing any later deployment.

## Claude connector

In Claude on the web, open Customize > Connectors > Add custom connector (Web). Use the deployed Worker URL ending in `/mcp`. Configure bearer-token authentication using the exact `MCP_BEARER_TOKEN` value stored in Cloudflare. Never paste the token into a normal chat message.

Claude's current custom connector UI and plan limits can change. If connector setup is not available in the Android app, configure it in Chrome at claude.ai and then use the connected tool from the Android app.

## Available MCP tools

- `check_azure_connection`
- `get_resource_group`
- `list_resource_group_resources`

All tools are read-only. Provisioning is intentionally not implemented in this starter. It should be added only after the Azure cost, target configuration, and approval mechanism have been reviewed.
