#!/bin/zsh
# Waits for the running Unity Editor to finish reloading, then prints compile errors logged since the call started.
# Usage: scripts/unity-wait.sh [seconds-to-let-reload-start]
cd "$(dirname "$0")/.."
start=$(wc -l < Logs/Editor.log)
sleep "${1:-6}"
for i in $(seq 1 120); do
  unity pipeline list 2>/dev/null | grep -q "true	true" && break
  sleep 2
done
sleep 2
tail -n +$((start+1)) Logs/Editor.log | grep -E "error CS[0-9]+" | sort -u | tail -15
echo "unity ready"
