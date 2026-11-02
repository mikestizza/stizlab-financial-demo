#!/usr/bin/env bash
# Erase everything Teller-lite put on the cluster. Back to "cluster just deployed".
# Safe to re-run. Empties, but never deletes, the Kommander project namespace (teller-lite-6mg6s).
# Run it once per cluster:
#   KUBECONFIG=~/stizlab.conf  ./bootstrap/reset.sh
#   KUBECONFIG=~/stizlab2.conf ./bootstrap/reset.sh   (only the fleet step finds anything here)
set -uo pipefail

echo "== 1. Flux objects (deleting a Kustomization with prune:true garbage-collects what it applied)"
kubectl -n teller-system delete kustomization teller-policy-constraints teller-policy teller-prod teller-envs --ignore-not-found --wait=true --timeout=120s
kubectl -n teller-system delete gitrepository teller-lite --ignore-not-found

echo "== 1b. Multi-cluster fleet hookup in the project namespace (prune removes the app it deployed)"
kubectl -n teller-lite-6mg6s delete kustomization teller-lite-fleet --ignore-not-found --wait=true --timeout=120s
kubectl -n teller-lite-6mg6s delete gitrepository teller-lite-fleet --ignore-not-found

echo "== 2. Namespaces"
kubectl delete ns teller-system teller intruder --ignore-not-found --wait=true --timeout=180s
for ns in $(kubectl get ns -l stizlab.financial/managed=true -o jsonpath='{.items[*].metadata.name}' 2>/dev/null); do
  kubectl delete ns "$ns" --ignore-not-found --wait=true --timeout=180s
done

echo "== 3. Cluster-scoped policy"
kubectl delete constrainttemplates tellerrequiredlimits tellerdisallowlatest --ignore-not-found

echo "== 4. Anything left?"
kubectl get ns | grep -E '^teller' || echo "   no teller namespaces"
kubectl get constrainttemplates 2>/dev/null | grep -i teller || echo "   no teller policy"
kubectl get kustomizations -A 2>/dev/null | grep -i teller || echo "   no teller Flux objects (project ones excepted)"
echo
echo "Reset complete. Cluster is back to just-deployed for Teller-lite."
