#!/usr/bin/env bash
# Layer 2: the one secret the app needs, the SQL SA password.
# A Secret is a ConfigMap with base64 values, RBAC-restricted and (on NKP) encrypted at rest in etcd.
# Created here by hand and never written to Git or baked into an image.
set -euo pipefail                                # stop on any error, unset variable, or failed pipe

# Use a password passed in via SQL_SA_PASSWORD, otherwise generate one that meets SQL Server's complexity rule
SQL_SA_PASSWORD="${SQL_SA_PASSWORD:-$(python3 -c 'import secrets,string; a=string.ascii_letters+string.digits; print("Hy"+"".join(secrets.choice(a) for _ in range(18))+"!9")')}"

# The Secret the pods read. envFrom in the web Deployment (layer 7) turns the key into an environment variable.
kubectl -n teller create secret generic teller-secret \
  --from-literal=MSSQL_SA_PASSWORD="$SQL_SA_PASSWORD" \
  --dry-run=client -o yaml | kubectl apply -f -   # dry-run + apply makes this idempotent (re-running updates, never errors)

# Flux's copy. In layer 10 Flux substitutes ${SQL_SA_PASSWORD} in Git with this value, so the handoff
# uses the identical password and SQL Server does not need re-initializing.
kubectl create namespace teller-system --dry-run=client -o yaml | kubectl apply -f -
kubectl -n teller-system create secret generic teller-secrets \
  --from-literal=SQL_SA_PASSWORD="$SQL_SA_PASSWORD" \
  --dry-run=client -o yaml | kubectl apply -f -

echo "Secret teller/teller-secret created (and mirrored to teller-system/teller-secrets for Flux)."
kubectl -n teller get secret teller-secret        # shows TYPE Opaque and DATA 1; the value is never printed
