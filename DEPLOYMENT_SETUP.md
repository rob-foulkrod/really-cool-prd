# Pre-flight Setup for CD Deployment

Before you trigger the first deployment via GitHub Actions, you'll need to set up:
1. Resource Group
2. User-Assigned Managed Identity
3. Federated Credentials (GitHub OIDC)
4. GitHub Secrets

## Prerequisites

- Azure CLI installed (`az`)
- GitHub repo owner access (to configure secrets)
- Already authenticated with Azure: `az login`

## Step 1: Set Variables

```bash
# Customize these for your environment
SUBSCRIPTION_ID="YOUR_SUBSCRIPTION_ID"
RESOURCE_GROUP="rg-really-cool-prd"
LOCATION="eastus"
MANAGED_IDENTITY_NAME="mi-really-cool-prd"
GITHUB_REPO_OWNER="YOUR_GITHUB_USERNAME"
GITHUB_REPO_NAME="really-cool-prd"
```

## Step 2: Create Resource Group

```bash
az group create \
  --name "$RESOURCE_GROUP" \
  --location "$LOCATION" \
  --subscription "$SUBSCRIPTION_ID"
```

## Step 3: Create User-Assigned Managed Identity

```bash
az identity create \
  --name "$MANAGED_IDENTITY_NAME" \
  --resource-group "$RESOURCE_GROUP" \
  --subscription "$SUBSCRIPTION_ID"
```

## Step 4: Assign Contributor Role

The managed identity needs Contributor role on the resource group to deploy resources:

```bash
IDENTITY_ID=$(az identity show \
  --name "$MANAGED_IDENTITY_NAME" \
  --resource-group "$RESOURCE_GROUP" \
  --subscription "$SUBSCRIPTION_ID" \
  --query id -o tsv)

az role assignment create \
  --role "Contributor" \
  --assignee "$IDENTITY_ID" \
  --scope "/subscriptions/$SUBSCRIPTION_ID/resourceGroups/$RESOURCE_GROUP"
```

## Step 5: Create Federated Credentials

This allows GitHub Actions to authenticate using OIDC without storing credentials:

```bash
az identity federated-credential create \
  --name "github-actions" \
  --identity-name "$MANAGED_IDENTITY_NAME" \
  --resource-group "$RESOURCE_GROUP" \
  --issuer "https://token.actions.githubusercontent.com" \
  --subject "repo:${GITHUB_REPO_OWNER}/${GITHUB_REPO_NAME}:environment:production"
```

## Step 6: Capture Output Values

Get the values needed for GitHub Secrets:

```bash
# Get Tenant ID
TENANT_ID=$(az account show --subscription "$SUBSCRIPTION_ID" --query tenantId -o tsv)

# Get Client ID (Managed Identity's Object ID)
CLIENT_ID=$(az identity show \
  --name "$MANAGED_IDENTITY_NAME" \
  --resource-group "$RESOURCE_GROUP" \
  --subscription "$SUBSCRIPTION_ID" \
  --query clientId -o tsv)

echo "SUBSCRIPTION_ID=$SUBSCRIPTION_ID"
echo "TENANT_ID=$TENANT_ID"
echo "CLIENT_ID=$CLIENT_ID"
echo "RESOURCE_GROUP=$RESOURCE_GROUP"
echo "MANAGED_IDENTITY_NAME=$MANAGED_IDENTITY_NAME"
```

## Step 7: Add GitHub Secrets

In your GitHub repository, go to **Settings** → **Secrets and variables** → **Actions** and add:

| Secret Name | Value |
|---|---|
| `AZURE_SUBSCRIPTION_ID` | (from Step 6) |
| `AZURE_TENANT_ID` | (from Step 6) |
| `AZURE_CLIENT_ID` | (from Step 6) |
| `AZURE_RESOURCE_GROUP` | `rg-really-cool-prd` |
| `AZURE_MANAGED_IDENTITY_NAME` | `mi-really-cool-prd` |

## Step 8: (Optional) Create an Environment

For additional protection, create a **production** environment in GitHub:

1. Go to **Settings** → **Environments** → **New environment**
2. Name it `production`
3. (Optional) Add required reviewers to approve deployments
4. (Optional) Set environment secrets (same ones as above)

## Verify Setup

Check that federated credentials are configured:

```bash
az identity federated-credential list \
  --identity-name "$MANAGED_IDENTITY_NAME" \
  --resource-group "$RESOURCE_GROUP"
```

You should see the GitHub token issuer configured.

## Testing

Once CI passes on your branch:

1. Go to **Actions** → **CD — Deploy to Azure**
2. Click **Run workflow** (top right)
3. Leave deployment name as `manual-deployment` or customize it
4. Click **Run workflow**
5. Watch the stages: Validation → Infrastructure → Deploy → Health Check → Swap

## Troubleshooting

**"OIDC token could not be obtained"**
- Verify the federated credential subject matches: `repo:OWNER/REPO:environment:production`
- Check that the environment variable `GITHUB_ENVIRONMENT` is set correctly in workflow

**"Managed identity not found"**
- Verify the managed identity exists: `az identity list --resource-group $RESOURCE_GROUP`
- Check the name matches what's in the secret

**"Insufficient permissions"**
- Verify role assignment: `az role assignment list --scope /subscriptions/$SUBSCRIPTION_ID/resourceGroups/$RESOURCE_GROUP`
- Should show `Contributor` for the managed identity

**"Health check failed"**
- The app may need more time to start. Check Azure Portal logs
- Verify the `/health` endpoint is reachable (not blocked by network policies)
- Check that `DEPLOYMENT_ERROR` is not set to `true` (unless testing failure scenario)

---

**Ready to deploy!** 🚀  
After CI passes, trigger deployment from the Actions tab.
