import { McpServer } from "@modelcontextprotocol/server";
import { createMcpHandler } from "agents/mcp/server";
import { z } from "zod";

interface Env {
  MCP_BEARER_TOKEN: string;
  AZURE_TENANT_ID: string;
  AZURE_CLIENT_ID: string;
  AZURE_CLIENT_SECRET: string;
  AZURE_SUBSCRIPTION_ID: string;
  AZURE_RESOURCE_GROUP: string;
}

interface AzureTokenResponse {
  access_token?: string;
  expires_in?: number;
  error?: string;
  error_description?: string;
}

function jsonToolResult(value: unknown) {
  return {
    content: [{ type: "text" as const, text: JSON.stringify(value, null, 2) }],
  };
}

function errorToolResult(message: string) {
  return {
    isError: true,
    content: [{ type: "text" as const, text: message }],
  };
}

async function constantTimeTokenMatch(expected: string, supplied: string): Promise<boolean> {
  const encoder = new TextEncoder();
  const [a, b] = await Promise.all([
    crypto.subtle.digest("SHA-256", encoder.encode(expected)),
    crypto.subtle.digest("SHA-256", encoder.encode(supplied)),
  ]);
  const left = new Uint8Array(a);
  const right = new Uint8Array(b);
  let difference = 0;
  for (let i = 0; i < left.length; i += 1) difference |= left[i] ^ right[i];
  return difference === 0 && expected.length > 0;
}

function required(value: string | undefined, name: string): string {
  if (!value || !value.trim()) throw new Error(`Missing Worker secret: ${name}`);
  return value.trim();
}

async function getAzureAccessToken(env: Env): Promise<string> {
  const tenantId = required(env.AZURE_TENANT_ID, "AZURE_TENANT_ID");
  const clientId = required(env.AZURE_CLIENT_ID, "AZURE_CLIENT_ID");
  const clientSecret = required(env.AZURE_CLIENT_SECRET, "AZURE_CLIENT_SECRET");

  const body = new URLSearchParams({
    client_id: clientId,
    client_secret: clientSecret,
    scope: "https://management.azure.com/.default",
    grant_type: "client_credentials",
  });

  const response = await fetch(
    `https://login.microsoftonline.com/${encodeURIComponent(tenantId)}/oauth2/v2.0/token`,
    {
      method: "POST",
      headers: { "content-type": "application/x-www-form-urlencoded" },
      body,
    },
  );

  const data = (await response.json()) as AzureTokenResponse;
  if (!response.ok || !data.access_token) {
    // Do not return token endpoint payloads: they can contain sensitive diagnostic details.
    throw new Error("Azure sign-in failed. Check the tenant, app registration, client secret, and expiry.");
  }
  return data.access_token;
}

async function azureGet<T>(env: Env, path: string): Promise<T> {
  const subscriptionId = required(env.AZURE_SUBSCRIPTION_ID, "AZURE_SUBSCRIPTION_ID");
  const token = await getAzureAccessToken(env);
  const url = `https://management.azure.com/subscriptions/${encodeURIComponent(subscriptionId)}${path}`;

  const response = await fetch(url, {
    method: "GET",
    headers: {
      authorization: `Bearer ${token}`,
      accept: "application/json",
    },
  });

  if (!response.ok) {
    if (response.status === 401 || response.status === 403) {
      throw new Error("Azure denied this read request. Confirm the app has the Reader role on the configured resource group.");
    }
    throw new Error(`Azure Resource Manager returned HTTP ${response.status}.`);
  }
  return (await response.json()) as T;
}

function createServer(env: Env) {
  const server = new McpServer({
    name: "azure-infrastructure-agent",
    version: "0.1.0",
  });

  server.registerTool(
    "get_resource_group",
    {
      description: "Read the configured Azure resource group's details. Read-only; cannot change Azure.",
      inputSchema: {},
    },
    async () => {
      try {
        const group = encodeURIComponent(required(env.AZURE_RESOURCE_GROUP, "AZURE_RESOURCE_GROUP"));
        const result = await azureGet<unknown>(
          env,
          `/resourceGroups/${group}?api-version=2021-04-01`,
        );
        return jsonToolResult(result);
      } catch (error) {
        return errorToolResult(error instanceof Error ? error.message : "Azure request failed.");
      }
    },
  );

  server.registerTool(
    "list_resource_group_resources",
    {
      description: "List resources in the configured Azure resource group. Read-only; cannot create, update, or delete resources.",
      inputSchema: {
        filter: z.string().optional().describe("Optional Azure Resource Manager OData filter."),
      },
    },
    async ({ filter }) => {
      try {
        const group = encodeURIComponent(required(env.AZURE_RESOURCE_GROUP, "AZURE_RESOURCE_GROUP"));
        const query = new URLSearchParams({ "api-version": "2021-04-01" });
        if (filter?.trim()) query.set("$filter", filter.trim());
        const result = await azureGet<unknown>(
          env,
          `/resourceGroups/${group}/resources?${query.toString()}`,
        );
        return jsonToolResult(result);
      } catch (error) {
        return errorToolResult(error instanceof Error ? error.message : "Azure request failed.");
      }
    },
  );

  server.registerTool(
    "check_azure_connection",
    {
      description: "Check whether this agent can read the configured Azure resource group. Performs only a GET request.",
      inputSchema: {},
    },
    async () => {
      try {
        const group = encodeURIComponent(required(env.AZURE_RESOURCE_GROUP, "AZURE_RESOURCE_GROUP"));
        await azureGet<unknown>(
          env,
          `/resourceGroups/${group}?api-version=2021-04-01`,
        );
        return jsonToolResult({
          connected: true,
          access: "read-only check succeeded",
          resourceGroup: env.AZURE_RESOURCE_GROUP,
          note: "This agent currently exposes read-only Azure tools. No deployment or write operation is available.",
        });
      } catch (error) {
        return errorToolResult(error instanceof Error ? error.message : "Azure connection check failed.");
      }
    },
  );

  return server;
}

export default {
  async fetch(request: Request, env: Env, ctx: ExecutionContext): Promise<Response> {
    const url = new URL(request.url);

    if (url.pathname === "/healthz" && request.method === "GET") {
      return Response.json({ status: "ok", service: "azure-infrastructure-agent" });
    }

    if (url.pathname !== "/mcp") {
      return new Response("Not found", { status: 404 });
    }

    const expected = env.MCP_BEARER_TOKEN;
    const authorization = request.headers.get("authorization") ?? "";
    const match = /^Bearer\s+(.+)$/i.exec(authorization);
    const supplied = match?.[1]?.trim() ?? "";

    if (!expected || !supplied || !(await constantTimeTokenMatch(expected, supplied))) {
      return new Response("Unauthorized", {
        status: 401,
        headers: { "www-authenticate": "Bearer", "cache-control": "no-store" },
      });
    }

    const handler = createMcpHandler(() => createServer(env), {
      route: "/mcp",
      legacy: "stateless",
    });
    return handler(request, env, ctx);
  },
};
